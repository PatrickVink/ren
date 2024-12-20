using System;
using System.Collections.Concurrent;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static readonly Options options = [];
    static async Task Main(string[] args)
    {

        options.SetHandler(async (context) =>
        {
            await ProcessFilesAsync(context.Map());
        });

        await options.InvokeAsync(args);
    }

    static async Task ProcessFilesAsync(Options options)
    {
        string[] files = Directory.GetFiles(options.Input!, options.Pattern!, SearchOption.AllDirectories);
        ConcurrentQueue<Task> tasks = new ConcurrentQueue<Task>();

        using SemaphoreSlim semaphore = new SemaphoreSlim(options.PoolSize);

        foreach (string file in files)
        {
            await semaphore.WaitAsync();

            tasks.Enqueue(Task.Run(async () =>
            {
                try
                {
                    await ProcessFileAsync(options, file);
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
    }

    static Task ProcessFileAsync(Options options, string inputFile)
    {
        return Task.Run(async () =>
        {
            try
            {
                string relativePath = Path.GetRelativePath(options.Input!, inputFile);
                string outputFile = Path.Combine(options.Output!, relativePath);
                string? outputFolder = Path.GetDirectoryName(outputFile);
                if (outputFolder != null) Directory.CreateDirectory(outputFolder);
                string arguments = $"-hide_banner -loglevel error -y -i \"{inputFile}\" -vn -sn -dn -acodec {options.Codec} -ab {options.Bitrate} \"{outputFile}\"";

                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = options.Ffmpeg,
                    Arguments = arguments,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                using Process? process = Process.Start(startInfo);
                if (process != null)
                {
                    Console.WriteLine($"ffmpeg {arguments}");

                    string output = await process.StandardOutput.ReadToEndAsync(); 
                    string error = await process.StandardError.ReadToEndAsync(); 

                    process.WaitForExit();

                    await Task.Delay(100); // Delay to ensure the file is fully written
                    File.SetLastWriteTime(outputFile, File.GetLastWriteTime(inputFile));
                }   
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
            }
        });
    }
}
