using System.Diagnostics;

public class ProcessPool(int poolsize = 5)
{
    List<Process?> _pool = new(poolsize);
    Process? Spawn(string filename, string arguments, Action? postaction = null)
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

        Process? process = Process.Start(info);
        
        if (process != null)
        {
            Console.Error.WriteLine(process.StandardError.ReadToEnd());
            Console.WriteLine(process.StandardOutput.ReadToEnd());
        }

        postaction?.Invoke();

        return process;
    }
    public Process? Queue(string filename, string arguments, Action? postaction = null)
    {
        while (_pool.All(p => p != null && p.HasExited == false))
            Thread.Sleep(1000);

        for (int i = 0; i < _pool.Count; i++)
        {
            Process? slot = _pool[i];
            if (slot == null || slot.HasExited)
            {
                slot = Spawn(filename, arguments, postaction);
                _pool[i] = slot;
                return slot;
            }
        }

        return default;
    }
}
