using System.Diagnostics;
using System.Threading.Tasks;

public class ProcessPool
{
    private List<Process?> _pool;

    public ProcessPool(int poolSize)
    {
        _pool = new List<Process?>(new Process?[poolSize]);
    }

    public async Task<Process?> QueueAsync(string filename, string arguments, Action? postaction = null)
    {
        while (_pool.All(p => p != null && !p.HasExited))
            await Task.Delay(1000);

        for (int i = 0; i < _pool.Count; i++)
        {
            Process? slot = _pool[i];
            if (slot == null || slot.HasExited)
            {
                ProcessStartInfo info = new()
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    FileName = filename,
                    Arguments = arguments
                };

                try
                {
                    Process? process = Process.Start(info);

                    if (process != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            string output = await process.StandardOutput.ReadToEndAsync();
                            string error = await process.StandardError.ReadToEndAsync();
                            Console.WriteLine(output);
                            Console.Error.WriteLine(error);
                        });

                        postaction?.Invoke();
                        _pool[i] = process;
                        return process;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Failed to start process: {ex.Message}");
                }
            }
        }

        return null;
    }
}
