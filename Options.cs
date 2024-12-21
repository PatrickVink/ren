using System.CommandLine;
using System.CommandLine.Invocation;

internal class Options : RootCommand
{
    internal Options() : base("Batch process MP3 files using ffmpeg")
    {
        Add(InputArgument);
        Add(OutputArgument);
        Add(PoolSizeOption);
        Add(BitrateOption);
        Add(CodecOption);
        Add(PatternOption);
        Add(RecursiveOption);
        Add(FfmpegOption);
    }
    internal const int DefaultPoolSize = 8;
    internal const string DefaultBitrate = "96k";
    internal const string DefaultCodec = "libmp3lame";
    internal const string DefaultPattern = "*.mp3";
    internal const bool DefaultRecursive = false;
    internal const string DefaultFfmpeg = "ffmpeg";

    internal static readonly Argument<string?> InputArgument = new(
        "input", 
        "The path to the input folder containing media files");
    internal static readonly Argument<string?> OutputArgument = new(
        "output", 
        () => ".", 
        "The path to the output folder where processed files will be saved");
    internal static readonly Option<int> PoolSizeOption = new(
        ["--poolsize", "-p"], 
        () => DefaultPoolSize, 
        "The number of parallel tasks");
    internal static readonly Option<string?> BitrateOption = new(
        ["--bitrate", "-b"], 
        () => DefaultBitrate, 
        "The bitrate for the output files");
    internal static readonly Option<string?> CodecOption = new(
        ["--codec", "-c"], 
        () => DefaultCodec,
        "The codec to use for the output files");
    internal static readonly Option<string?> PatternOption = new(
        ["--pattern", "-f"],
        () => DefaultPattern,
        "The file pattern to search for in the input folder");
    internal static readonly Option<bool> RecursiveOption = new(
        ["--recursive", "-r"],
        () => DefaultRecursive,
        "Search for media in the input folder and its subfolders");
    internal static readonly Option<string?> FfmpegOption = new(
        ["--ffmpeg", "-e"],
        () => DefaultFfmpeg,
        "The location of the ffmpeg executable");

    public string? Input { get; set; }
    public string? Output { get; set; }
    public int PoolSize { get; set; } = DefaultPoolSize;
    public string? Bitrate { get; set; } = DefaultBitrate;
    public string? Codec { get; set; } = DefaultCodec;
    public string? Pattern { get; set; } = DefaultPattern;
    public bool Recursive { get; set; } = DefaultRecursive;
    public string? Ffmpeg { get; set; } = DefaultFfmpeg;
}
internal static class OptionsExtensions
{
    internal static Options Map(this InvocationContext context)
    {
        return new()
        {
            Input = context.ParseResult.GetValueForArgument<string?>(Options.InputArgument),
            Output = context.ParseResult.GetValueForArgument<string?>(Options.OutputArgument),
            PoolSize = context.ParseResult.GetValueForOption<int>(Options.PoolSizeOption),
            Bitrate = context.ParseResult.GetValueForOption<string?>(Options.BitrateOption),
            Codec = context.ParseResult.GetValueForOption<string?>(Options.CodecOption),
            Pattern = context.ParseResult.GetValueForOption<string?>(Options.PatternOption),
            Ffmpeg = context.ParseResult.GetValueForOption<string?>(Options.FfmpegOption)
        };
    }
    // internal static void SetHandler(this Options options, Func<InvocationContext, Task> handler)
    // {
    //     options.SetHandler();
    // }
}