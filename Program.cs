using System.Diagnostics;

const string _music = @"Z:\Downloads\NewsLazer\Top 4000 Editie 2024\320kbps";
const string _pattern = "*.mp3";
const string _bitrate = "96k";
const string _codec = "libmp3lame";

DownSample(_music, _pattern, _bitrate, _codec);

async void DownSample(string music, string pattern = _pattern, string bitrate = _bitrate, string codec = _codec)
{
    ProcessPool pool = new(5);
    const string ffmpeg = @"C:\Apps\ffmpeg\bin\ffmpeg.exe";
    foreach (var file in Directory.GetFiles(music, pattern))
    {
        string path = Path.GetDirectoryName(file) ?? string.Empty;
        string output = Path.Combine(path, "..", Path.GetFileName(file));
        string arguments = $"-i \"{file}\" -acodec {codec} -ab {bitrate} \"{output}\"";

        Process? proc = await pool.QueueAsync(ffmpeg, arguments, () => 
        {
            File.SetLastWriteTime(output, File.GetLastWriteTime(file));
        });
        if (proc is null)
            continue;

        //File.SetLastWriteTime(output, File.GetLastWriteTime(file));
    }
}
void Rename()
{
    if (args.Length < 2)
    {
        Usage();
        return;
    }

    const StringComparison comparisonType = StringComparison.InvariantCultureIgnoreCase;
    string search = args[0];
    string replace = args[1];
    string path = args.Length > 2 ? args[2] : ".";
    string pattern = args.Length > 3 ? args[3] : "*.*";
    string[] files = Directory.GetFiles(path, pattern);

    foreach (var file in files)
    {
        path = Path.GetDirectoryName(file) ?? string.Empty;
        string name = Path.GetFileNameWithoutExtension(file);
        string ext = Path.GetExtension(file);
        if (name.Contains(search, comparisonType))
        {
            string newname = name.Replace(search, replace, comparisonType);
            string newfile = Path.Combine(path, $"{newname}{ext}");
            if (File.Exists(newfile))
            {
                Warn("File '{0}' already exists.", newfile);
                continue;
            }
            try
            {
                File.Move(file, newfile);
                if (File.Exists(newfile))
                    Console.WriteLine($"{name}{ext} => {newname}{ext}");

            } catch (Exception ex)
            {
                Error(ex);
            }
        }
    }
}

void ShowCommandLineArgs()
{
    Console.WriteLine(args.Length);
    foreach (var arg in args)
        Console.WriteLine(arg);
}

void Error(Exception ex)
{
    var lastBackgroundColor = Console.BackgroundColor;
    var lastForegroundColor = Console.ForegroundColor;
    Console.BackgroundColor = ConsoleColor.Red;
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("{0}: {1}", ex.GetType(), ex.Message);
    Console.BackgroundColor = lastBackgroundColor;
    Console.ForegroundColor = lastForegroundColor;
}

void Warn(string format, params object[] args)
{
    var lastForegroundColor = Console.ForegroundColor;
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine(format, args);
    Console.ForegroundColor = lastForegroundColor;
}

void Usage()
{
    Console.WriteLine("Usage: ren {search} {replace} [{path:.}] [{pattern:*.*}]");
}