using SplitCS.Models;

namespace SplitCS.Services;

public class ClassSplitter
{
    public void SplitFile(Models.FileInfo fileInfo, SplitOptions options)
    {
        Console.WriteLine($"  → Processing {fileInfo.Classes.Count} classes...");
        
        if (options.SpecificClassName != null)
        {
            var specificClass = fileInfo.Classes.FirstOrDefault(c => 
                c.ClassName.Equals(options.SpecificClassName, StringComparison.OrdinalIgnoreCase));
            
            if (specificClass == null)
            {
                throw new ArgumentException($"Class '{options.SpecificClassName}' not found in the file.");
            }
            
            Console.WriteLine($"  → Splitting specific class: {specificClass.ClassName}");
            SplitSingleClass(specificClass, options, fileInfo.UsingStatements);
        }
        else
        {
            Console.WriteLine("  → Splitting all classes in the file");
            foreach (var classInfo in fileInfo.Classes)
            {
                Console.WriteLine($"  → Processing class: {classInfo.ClassName}");
                SplitSingleClass(classInfo, options, fileInfo.UsingStatements);
            }
        }
        
        PrintFileSummary(fileInfo, options);
    }

    public void SplitSingleClass(ClassInfo classInfo, SplitOptions options, List<string> globalUsingStatements)
    {
        Console.WriteLine($"    → Checking output directory for {classInfo.ClassName}...");
        if (!Directory.Exists(options.OutputDirectory))
        {
            Console.WriteLine("    → Creating output directory...");
            Directory.CreateDirectory(options.OutputDirectory);
            Console.WriteLine("    ✓ Output directory created");
        }

        Console.WriteLine($"    → Grouping methods for {classInfo.ClassName}...");
        var originalFileName = Path.GetFileNameWithoutExtension(options.InputFilePath);
        var methodGroups = GroupMethods(classInfo.Methods, options.NumberOfSplits);
        Console.WriteLine($"    ✓ Methods grouped into {methodGroups.Count} files");

        Console.WriteLine($"    → Generating partial class files for {classInfo.ClassName}...");
        for (int i = 0; i < methodGroups.Count; i++)
        {
            var outputFileName = $"{classInfo.ClassName}_Part{i + 1}.cs";
            var outputPath = Path.Combine(options.OutputDirectory, outputFileName);
            
            Console.WriteLine($"      • Generating {outputFileName} ({methodGroups[i].Count} methods)...");
            GeneratePartialClassFile(classInfo, methodGroups[i], outputPath, globalUsingStatements);
            Console.WriteLine($"      ✓ {outputFileName} created");
        }

        Console.WriteLine($"    ✓ {classInfo.ClassName} split completed");
    }

    private List<List<MethodInfo>> GroupMethods(List<MethodInfo> methods, int? numberOfSplits)
    {
        if (numberOfSplits.HasValue)
        {
            Console.WriteLine($"    • Using even distribution across {numberOfSplits.Value} files");
            return SplitMethodsEvenly(methods, numberOfSplits.Value);
        }
        else
        {
            Console.WriteLine("    • Using public/private split");
            return SplitMethodsByVisibility(methods);
        }
    }

    private List<List<MethodInfo>> SplitMethodsEvenly(List<MethodInfo> methods, int numberOfSplits)
    {
        var groups = new List<List<MethodInfo>>();
        
        for (int i = 0; i < numberOfSplits; i++)
        {
            groups.Add(new List<MethodInfo>());
        }

        for (int i = 0; i < methods.Count; i++)
        {
            var groupIndex = i % numberOfSplits;
            groups[groupIndex].Add(methods[i]);
        }

        // Remove empty groups
        return groups.Where(g => g.Any()).ToList();
    }

    private List<List<MethodInfo>> SplitMethodsByVisibility(List<MethodInfo> methods)
    {
        var publicMethods = methods.Where(m => m.IsPublic).ToList();
        var privateMethods = methods.Where(m => !m.IsPublic).ToList();

        var groups = new List<List<MethodInfo>>();
        
        if (publicMethods.Any())
            groups.Add(publicMethods);
        
        if (privateMethods.Any())
            groups.Add(privateMethods);

        return groups;
    }

    private void GeneratePartialClassFile(ClassInfo classInfo, List<MethodInfo> methods, string outputPath, List<string> globalUsingStatements)
    {
        var content = new List<string>();

        // Add using statements (prefer global ones, fall back to class-specific)
        var usingStatements = globalUsingStatements.Any() ? globalUsingStatements : classInfo.UsingStatements;
        if (usingStatements.Any())
        {
            content.AddRange(usingStatements);
            content.Add("");
        }

        // Add namespace declaration
        var hasNamespace = !string.IsNullOrWhiteSpace(classInfo.Namespace);
        if (hasNamespace)
        {
            content.Add($"namespace {classInfo.Namespace};");
            content.Add("");
        }

        // Add partial class declaration
        var partialClassDeclaration = classInfo.ClassDeclaration.Replace("class ", "partial class ");
        content.Add(partialClassDeclaration);
        content.Add("{");

        // Add methods with proper indentation
        for (int i = 0; i < methods.Count; i++)
        {
            var method = methods[i];
            var methodLines = method.FullMethod.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            
            // Add indentation to each line
            foreach (var line in methodLines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    content.Add("");
                }
                else
                {
                    // Ensure proper indentation (4 spaces for class content)
                    var trimmedLine = line.TrimStart();
                    if (trimmedLine.Length > 0)
                    {
                        content.Add("    " + trimmedLine);
                    }
                    else
                    {
                        content.Add("");
                    }
                }
            }

            // Add spacing between methods (except for the last one)
            if (i < methods.Count - 1)
            {
                content.Add("");
            }
        }

        content.Add("}");

        File.WriteAllLines(outputPath, content);
    }

    private void PrintFileSummary(Models.FileInfo fileInfo, SplitOptions options)
    {
        Console.WriteLine();
        Console.WriteLine("=== SPLIT SUMMARY ===");
        Console.WriteLine($"Source file: {Path.GetFileName(options.InputFilePath)}");
        Console.WriteLine($"Total classes processed: {fileInfo.Classes.Count}");
        
        if (options.SpecificClassName != null)
        {
            Console.WriteLine($"Specific class split: {options.SpecificClassName}");
        }
        else
        {
            Console.WriteLine("All classes split");
        }

        Console.WriteLine($"Output directory: {Path.GetFullPath(options.OutputDirectory)}");
        Console.WriteLine();

        foreach (var classInfo in fileInfo.Classes)
        {
            if (options.SpecificClassName != null && 
                !classInfo.ClassName.Equals(options.SpecificClassName, StringComparison.OrdinalIgnoreCase))
                continue;

            Console.WriteLine($"Class: {classInfo.ClassName}");
            Console.WriteLine($"  Total methods: {classInfo.Methods.Count}");
            
            var methodGroups = GroupMethods(classInfo.Methods, options.NumberOfSplits);
            Console.WriteLine($"  Split into: {methodGroups.Count} files");
            
            for (int i = 0; i < methodGroups.Count; i++)
            {
                var group = methodGroups[i];
                var fileName = $"{classInfo.ClassName}_Part{i + 1}.cs";
                
                Console.WriteLine($"    File: {fileName}");
                Console.WriteLine($"      Methods: {group.Count}");
                
                if (options.NumberOfSplits == null)
                {
                    var publicCount = group.Count(m => m.IsPublic);
                    var privateCount = group.Count - publicCount;
                    Console.WriteLine($"      Public: {publicCount}, Private: {privateCount}");
                }
                
                Console.WriteLine($"      Method names: {string.Join(", ", group.Select(GetMethodName))}");
            }
            Console.WriteLine();
        }
    }

    private void PrintSummary(ClassInfo classInfo, List<List<MethodInfo>> methodGroups, SplitOptions options)
    {
        Console.WriteLine();
        Console.WriteLine("=== SPLIT SUMMARY ===");
        Console.WriteLine($"Original class: {classInfo.ClassName}");
        Console.WriteLine($"Total methods: {classInfo.Methods.Count}");
        Console.WriteLine($"Split into: {methodGroups.Count} files");
        Console.WriteLine();

        var originalFileName = Path.GetFileNameWithoutExtension(options.InputFilePath);
        
        for (int i = 0; i < methodGroups.Count; i++)
        {
            var group = methodGroups[i];
            var fileName = $"{originalFileName}_Part{i + 1}.cs";
            var filePath = Path.Combine(options.OutputDirectory, fileName);
            
            Console.WriteLine($"File: {fileName}");
            Console.WriteLine($"  Methods: {group.Count}");
            Console.WriteLine($"  Path: {filePath}");
            
            if (options.NumberOfSplits == null)
            {
                // Show visibility breakdown for default split
                var publicCount = group.Count(m => m.IsPublic);
                var privateCount = group.Count - publicCount;
                Console.WriteLine($"  Public: {publicCount}, Private: {privateCount}");
            }
            
            Console.WriteLine($"  Method names: {string.Join(", ", group.Select(GetMethodName))}");
            Console.WriteLine();
        }

        Console.WriteLine($"All files saved to: {Path.GetFullPath(options.OutputDirectory)}");
    }

    private string GetMethodName(MethodInfo method)
    {
        // Extract method name from signature
        var signature = method.Signature.Trim();
        var parenIndex = signature.IndexOf('(');
        if (parenIndex == -1) return "Unknown";

        var beforeParen = signature.Substring(0, parenIndex).Trim();
        var parts = beforeParen.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        return parts.LastOrDefault() ?? "Unknown";
    }
}