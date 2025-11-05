using System.Text.RegularExpressions;
using SplitCS.Models;

namespace SplitCS.Services;

public class ClassParser
{
    public Models.FileInfo ParseClassFile(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"File not found: {filePath}");

        Console.WriteLine("  → Reading file content...");
        var content = File.ReadAllText(filePath);
        var lines = File.ReadAllLines(filePath);
        Console.WriteLine($"  ✓ File read successfully ({lines.Length} lines)");

        Console.WriteLine("  → Extracting using statements...");
        var usingStatements = ExtractUsingStatements(lines);
        Console.WriteLine($"  ✓ Found {usingStatements.Count} using statements");

        Console.WriteLine("  → Extracting namespace information...");
        var namespaceInfo = ExtractNamespace(content);
        Console.WriteLine($"  ✓ Namespace: {(string.IsNullOrEmpty(namespaceInfo) ? "(none)" : namespaceInfo)}");

        Console.WriteLine("  → Extracting all classes...");
        var classes = ExtractAllClasses(content, lines);
        Console.WriteLine($"  ✓ Found {classes.Count} classes");

        return new Models.FileInfo(
            Classes: classes,
            UsingStatements: usingStatements,
            Namespace: namespaceInfo,
            AdditionalContent: new List<string>()
        );
    }

    private List<ClassInfo> ExtractAllClasses(string content, string[] lines)
    {
        var classes = new List<ClassInfo>();
        
        // Find all class declarations
        var classPattern = @"(?:public|private|protected|internal)?\s*(?:static|abstract|sealed)?\s*class\s+(\w+)(?:[^{]*)?{";
        var classMatches = Regex.Matches(content, classPattern, RegexOptions.Multiline);
        
        Console.WriteLine($"    • Found {classMatches.Count} class declarations");
        int i = 1;
        foreach (Match classMatch in classMatches)
        {
            Console.WriteLine($" Processing class {i}");
            try
            {
                var classInfo = ExtractSingleClass(content, lines, classMatch);
                if (classInfo != null)
                {
                    classes.Add(classInfo);
                    Console.WriteLine($"    ✓ Parsed class: {classInfo.ClassName} ({classInfo.Methods.Count} methods)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    ⚠ Warning: Could not parse class at position {classMatch.Index}: {ex.Message}");
            }
            i++;
        }
        
        return classes;
    }

    private ClassInfo? ExtractSingleClass(string content, string[] lines, Match classMatch)
    {
        var startIndex = classMatch.Index;
        var className = classMatch.Groups[1].Value;
        Console.WriteLine($"    → Extracting class: {className}");
        
        // Find the class body boundaries
        Console.WriteLine($"    → Finding class boundaries for {className}...");
        var openBraceIndex = content.IndexOf('{', startIndex);
        if (openBraceIndex == -1)
        {
            Console.WriteLine($"    ⚠ No opening brace found for class {className}");
            return null;
        }
        
        Console.WriteLine($"    → Matching braces for {className}...");
        var braceCount = 1;
        var endIndex = openBraceIndex + 1;
        
        while (endIndex < content.Length && braceCount > 0)
        {
            if (content[endIndex] == '{') braceCount++;
            else if (content[endIndex] == '}') braceCount--;
            endIndex++;
        }
        
        if (braceCount != 0)
        {
            Console.WriteLine($"    ⚠ Unmatched braces for class {className}");
            return null;
        }
        Console.WriteLine($"    ✓ Class boundaries found for {className}");
        
        // Extract class content
        Console.WriteLine($"    → Extracting class content for {className}...");
        var classContent = content.Substring(startIndex, endIndex - startIndex);
        var classStartLine = GetLineNumber(content, startIndex);
        var classEndLine = GetLineNumber(content, endIndex - 1);
        Console.WriteLine($"    ✓ Class {className} spans lines {classStartLine}-{classEndLine}");
        
        // Extract class declaration
        Console.WriteLine($"    → Extracting class declaration for {className}...");
        var declarationEnd = classContent.IndexOf('{');
        var classDeclaration = classContent.Substring(0, declarationEnd).Trim();
        Console.WriteLine($"    ✓ Declaration: {classDeclaration}");
        
        // Extract access modifier
        Console.WriteLine($"    → Determining access modifier for {className}...");
        var accessModifier = "internal"; // default
        if (classDeclaration.Contains("public")) accessModifier = "public";
        else if (classDeclaration.Contains("private")) accessModifier = "private";
        else if (classDeclaration.Contains("protected")) accessModifier = "protected";
        Console.WriteLine($"    ✓ Access modifier: {accessModifier}");
        
        // Extract methods within this specific class
        Console.WriteLine($"    → Extracting methods for {className}...");
        var classMethods = ExtractMethodsInRange(content, lines, openBraceIndex, endIndex - 1);
        Console.WriteLine($"    ✓ Found {classMethods.Count} methods in {className}");
        
        Console.WriteLine($"    ✓ Successfully extracted class {className}");
        return new ClassInfo(
            ClassName: className,
            Namespace: ExtractNamespace(content),
            AccessModifier: accessModifier,
            ClassDeclaration: classDeclaration,
            Methods: classMethods,
            UsingStatements: new List<string>(), // Will be set at file level
            AdditionalContent: new List<string>()
        );
    }

    private List<MethodInfo> ExtractMethodsInRange(string content, string[] lines, int startIndex, int endIndex)
    {
        var methods = new List<MethodInfo>();
        Console.WriteLine($"      → Extracting range content ({endIndex - startIndex} characters)...");
        var rangeContent = content.Substring(startIndex, endIndex - startIndex);
        
        // Pattern to match method declarations within the range
        Console.WriteLine($"      → Searching for methods and properties...");
        var methodPattern = @"(?:public|private|protected|internal|static|\s)*\s*(?:virtual|override|abstract|\s)*\s*(?:\w+(?:<[^>]*>)?|\w+\[\]|\w+\?)\s+(\w+)\s*\([^{]*\)\s*{";
        var propertyPattern = @"(?:public|private|protected|internal|static|\s)*\s*(?:virtual|override|abstract|\s)*\s*(?:\w+(?:<[^>]*>)?|\w+\[\]|\w+\?)\s+(\w+)\s*{\s*(?:get|set)";
        
        var methodMatches = Regex.Matches(rangeContent, methodPattern, RegexOptions.Multiline);
        var propertyMatches = Regex.Matches(rangeContent, propertyPattern, RegexOptions.Multiline);
        Console.WriteLine($"      ✓ Found {methodMatches.Count} methods and {propertyMatches.Count} properties");
        
        var allMatches = methodMatches.Cast<Match>()
            .Concat(propertyMatches.Cast<Match>())
            .OrderBy(m => m.Index)
            .ToList();

        Console.WriteLine($"      → Processing {allMatches.Count} total matches...");
        int matchIndex = 1;
        foreach (var match in allMatches)
        {
            Console.WriteLine($"        • Processing match {matchIndex}/{allMatches.Count}...");
            try
            {
                // Adjust match index to global content position
                var adjustedMatch = new
                {
                    Index = startIndex + match.Index,
                    Value = match.Value,
                    Groups = match.Groups
                };
                
                var methodInfo = ExtractMethodBodyInRange(content, lines, adjustedMatch.Index, startIndex, endIndex);
                if (methodInfo != null)
                {
                    methods.Add(methodInfo);
                    Console.WriteLine($"        ✓ Successfully parsed match {matchIndex}");
                }
                else
                {
                    Console.WriteLine($"        ⚠ Match {matchIndex} returned null");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"      ⚠ Warning: Could not parse method at position {match.Index}: {ex.Message}");
            }
            matchIndex++;
        }

        return methods;
    }

    private MethodInfo? ExtractMethodBodyInRange(string content, string[] lines, int methodStartIndex, int classStartIndex, int classEndIndex)
    {
        var startLine = GetLineNumber(content, methodStartIndex);
        
        // Find the opening brace for this method
        var braceIndex = content.IndexOf('{', methodStartIndex);
        if (braceIndex == -1 || braceIndex >= classEndIndex) return null;

        // Find matching closing brace
        var braceCount = 1;
        var endIndex = braceIndex + 1;
        
        while (endIndex < classEndIndex && braceCount > 0)
        {
            if (content[endIndex] == '{') braceCount++;
            else if (content[endIndex] == '}') braceCount--;
            endIndex++;
        }

        if (braceCount != 0) return null;

        var endLine = GetLineNumber(content, endIndex - 1);
        
        // Extract the complete method
        var methodLines = lines.Skip(startLine - 1).Take(endLine - startLine + 1).ToList();
        var fullMethod = string.Join(Environment.NewLine, methodLines);
        
        // Extract signature (everything before first {)
        var signatureEndIndex = fullMethod.IndexOf('{');
        var signature = fullMethod.Substring(0, signatureEndIndex).Trim();
        
        // Extract body (everything inside braces)
        var body = fullMethod.Substring(signatureEndIndex);
        
        // Determine if method is public
        var isPublic = signature.Contains("public");

        return new MethodInfo(
            Signature: signature,
            Body: body,
            FullMethod: fullMethod,
            IsPublic: isPublic,
            StartLine: startLine,
            EndLine: endLine
        );
    }

    private List<string> ExtractUsingStatements(string[] lines)
    {
        var usingStatements = new List<string>();
        
        foreach (var line in lines)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("using ") && trimmed.EndsWith(";"))
            {
                usingStatements.Add(line);
            }
            else if (!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("//"))
            {
                // Stop at first non-using, non-comment, non-empty line
                break;
            }
        }

        return usingStatements;
    }

    private string ExtractNamespace(string content)
    {
        // Match namespace declaration
        var namespaceMatch = Regex.Match(content, @"namespace\s+([^;\s\{]+)", RegexOptions.Multiline);
        return namespaceMatch.Success ? namespaceMatch.Groups[1].Value.Trim() : string.Empty;
    }

    private (string Name, string AccessModifier, string Declaration) ExtractClassInfo(string content)
    {
        // Match class declaration
        var classPattern = @"(?:public|private|protected|internal)?\s*(?:static|abstract|sealed)?\s*class\s+(\w+)(?:[^{]*)?";
        var classMatch = Regex.Match(content, classPattern, RegexOptions.Multiline);
        
        if (!classMatch.Success)
            throw new InvalidOperationException("No class declaration found in the file.");

        var fullDeclaration = classMatch.Value.Trim();
        var className = classMatch.Groups[1].Value;
        
        // Extract access modifier
        var accessModifier = "internal"; // default
        if (fullDeclaration.Contains("public")) accessModifier = "public";
        else if (fullDeclaration.Contains("private")) accessModifier = "private";
        else if (fullDeclaration.Contains("protected")) accessModifier = "protected";

        return (className, accessModifier, fullDeclaration);
    }

    private List<MethodInfo> ExtractMethods(string content, string[] lines)
    {
        var methods = new List<MethodInfo>();
        
        // Pattern to match method declarations (including properties with bodies)
        var methodPattern = @"(?:public|private|protected|internal|static|\s)*\s*(?:virtual|override|abstract|\s)*\s*(?:\w+(?:<[^>]*>)?|\w+\[\]|\w+\?)\s+(\w+)\s*\([^{]*\)\s*{";
        var propertyPattern = @"(?:public|private|protected|internal|static|\s)*\s*(?:virtual|override|abstract|\s)*\s*(?:\w+(?:<[^>]*>)?|\w+\[\]|\w+\?)\s+(\w+)\s*{\s*(?:get|set)";
        
        var methodMatches = Regex.Matches(content, methodPattern, RegexOptions.Multiline);
        var propertyMatches = Regex.Matches(content, propertyPattern, RegexOptions.Multiline);
        
        var allMatches = methodMatches.Cast<Match>()
            .Concat(propertyMatches.Cast<Match>())
            .OrderBy(m => m.Index)
            .ToList();

        foreach (var match in allMatches)
        {
            try
            {
                var methodInfo = ExtractMethodBody(content, lines, match);
                if (methodInfo != null)
                    methods.Add(methodInfo);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Warning: Could not parse method at position {match.Index}: {ex.Message}");
            }
        }

        return methods;
    }

    private MethodInfo? ExtractMethodBody(string content, string[] lines, Match methodMatch)
    {
        var startIndex = methodMatch.Index;
        var startLine = GetLineNumber(content, startIndex);
        
        // Find the opening brace
        var braceIndex = content.IndexOf('{', startIndex);
        if (braceIndex == -1) return null;

        // Find matching closing brace
        var braceCount = 1;
        var endIndex = braceIndex + 1;
        
        while (endIndex < content.Length && braceCount > 0)
        {
            if (content[endIndex] == '{') braceCount++;
            else if (content[endIndex] == '}') braceCount--;
            endIndex++;
        }

        if (braceCount != 0) return null;

        var endLine = GetLineNumber(content, endIndex - 1);
        
        // Extract the complete method
        var methodLines = lines.Skip(startLine - 1).Take(endLine - startLine + 1).ToList();
        var fullMethod = string.Join(Environment.NewLine, methodLines);
        
        // Extract signature (everything before first {)
        var signatureEndIndex = fullMethod.IndexOf('{');
        var signature = fullMethod.Substring(0, signatureEndIndex).Trim();
        
        // Extract body (everything inside braces)
        var body = fullMethod.Substring(signatureEndIndex);
        
        // Determine if method is public
        var isPublic = signature.Contains("public");

        return new MethodInfo(
            Signature: signature,
            Body: body,
            FullMethod: fullMethod,
            IsPublic: isPublic,
            StartLine: startLine,
            EndLine: endLine
        );
    }

    private int GetLineNumber(string content, int charIndex)
    {
        return content.Substring(0, charIndex).Count(c => c == '\n') + 1;
    }
}