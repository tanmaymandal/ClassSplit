namespace SplitCS.Models;

public record MethodInfo(
    string Signature,
    string Body,
    string FullMethod,
    bool IsPublic,
    int StartLine,
    int EndLine);

public record ClassInfo(
    string ClassName,
    string Namespace,
    string AccessModifier,
    string ClassDeclaration,
    List<MethodInfo> Methods,
    List<string> UsingStatements,
    List<string> AdditionalContent);

public record FileInfo(
    List<ClassInfo> Classes,
    List<string> UsingStatements,
    string Namespace,
    List<string> AdditionalContent);

public record SplitOptions(
    string InputFilePath,
    string OutputDirectory,
    int? NumberOfSplits,
    string? SpecificClassName = null);