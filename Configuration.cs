namespace SplitCS.Configuration;

public class AppConfiguration
{
    public AppSettings AppSettings { get; set; } = new();
    public ParsingSettings ParsingSettings { get; set; } = new();
    public OutputSettings OutputSettings { get; set; } = new();
}

public class AppSettings
{
    public string DefaultSourceDirectory { get; set; } = "./Input";
    public string DefaultOutputDirectory { get; set; } = "./Output";
    public string DefaultSourceFile { get; set; } = "";
    public string DefaultDestinationFile { get; set; } = "";
    public bool EnableDetailedLogging { get; set; } = true;
    public int MaxFileSizeInMB { get; set; } = 50;
    public List<string> SupportedFileExtensions { get; set; } = new() { ".cs" };
    public string DefaultSplitMode { get; set; } = "PublicPrivate";
    public bool BackupOriginalFile { get; set; } = false;
    public bool CreateDirectoriesIfNotExist { get; set; } = true;
}

public class ParsingSettings
{
    public bool IncludeProperties { get; set; } = true;
    public bool IncludeConstructors { get; set; } = true;
    public bool IncludeFields { get; set; } = false;
    public int MinimumMethodsPerFile { get; set; } = 1;
    public int MaximumMethodsPerFile { get; set; } = 50;
}

public class OutputSettings
{
    public int IndentSize { get; set; } = 4;
    public bool UseSpaces { get; set; } = true;
    public bool AddGeneratedComment { get; set; } = true;
    public bool PreserveOriginalFormatting { get; set; } = true;
    public string FileNamingPattern { get; set; } = "{ClassName}_Part{PartNumber}.cs";
}