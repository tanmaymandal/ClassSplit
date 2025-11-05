using SplitCS.Models;
using SplitCS.Services;

namespace SplitCS;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("=== SplitCS - C# Class Splitter ===");
        Console.WriteLine("Split a C# class into multiple partial class files");
        Console.WriteLine();

        try
        {
            Console.WriteLine("Step 1: Getting user input...");
            var options = GetUserInput();
            Console.WriteLine("✓ User input collected successfully");
            
            Console.WriteLine("Step 2: Parsing C# file...");
            var parser = new ClassParser();
            var fileInfo = parser.ParseClassFile(options.InputFilePath);
            Console.WriteLine("✓ File parsed successfully");
            
            Console.WriteLine($"Found {fileInfo.Classes.Count} classes:");
            foreach (var cls in fileInfo.Classes)
            {
                Console.WriteLine($"  - {cls.ClassName} ({cls.Methods.Count} methods)");
            }
            Console.WriteLine();

            Console.WriteLine("Step 3: Splitting classes into partial files...");
            var splitter = new ClassSplitter();
            splitter.SplitFile(fileInfo, options);
            
            Console.WriteLine("✓ Split completed successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Environment.Exit(1);
        }
    }

    private static SplitOptions GetUserInput()
    {
        Console.WriteLine("  → Prompting for input file path...");
        // Get input file path
        Console.Write("Enter the input C# file path: ");
        var inputPath = Console.ReadLine()?.Trim().Trim('"');
        
        if (string.IsNullOrWhiteSpace(inputPath))
        {
            throw new ArgumentException("Input file path cannot be empty.");
        }

        Console.WriteLine($"  → Checking if file exists: {inputPath}");
        if (!File.Exists(inputPath))
        {
            throw new FileNotFoundException($"Input file not found: {inputPath}");
        }
        Console.WriteLine("  ✓ Input file found");

        Console.WriteLine("  → Prompting for output directory...");
        // Get output directory
        Console.Write("Enter the target output folder: ");
        var outputDirectory = Console.ReadLine()?.Trim().Trim('"');
        
        if (string.IsNullOrWhiteSpace(outputDirectory))
        {
            throw new ArgumentException("Output directory cannot be empty.");
        }
        Console.WriteLine($"  ✓ Output directory set: {outputDirectory}");

        Console.WriteLine("  → Prompting for split options...");
        // Get number of splits (optional)
        Console.Write("Enter number of splits (press Enter for public/private split): ");
        var splitsInput = Console.ReadLine()?.Trim();
        
        int? numberOfSplits = null;
        if (!string.IsNullOrWhiteSpace(splitsInput))
        {
            if (int.TryParse(splitsInput, out int splits) && splits > 0)
            {
                numberOfSplits = splits;
                Console.WriteLine($"  ✓ Will split into {splits} files");
            }
            else
            {
                throw new ArgumentException("Number of splits must be a positive integer.");
            }
        }
        else
        {
            Console.WriteLine("  ✓ Will split by public/private methods");
        }

        Console.WriteLine("  → Prompting for specific class (optional)...");
        Console.Write("Enter specific class name to split (press Enter to split all classes): ");
        var specificClassName = Console.ReadLine()?.Trim();
        
        if (!string.IsNullOrWhiteSpace(specificClassName))
        {
            Console.WriteLine($"  ✓ Will split only class: {specificClassName}");
        }
        else
        {
            Console.WriteLine("  ✓ Will split all classes found in file");
        }

        return new SplitOptions(inputPath, outputDirectory, numberOfSplits, specificClassName);
    }
}
