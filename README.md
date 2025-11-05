# SplitCS - C# Class Splitter

A .NET 9 console application that reads a C# file containing a single class and splits it into multiple partial class files based on user input.

## Features

- **Multiple Class Support**: Handles files with multiple classes
- **Selective Splitting**: Split all classes or specify a particular class
- **Flexible Splitting**: Split by number of files or by method visibility (public/private)
- **Preserve Structure**: Maintains namespace, using statements, and class structure
- **Proper Formatting**: Preserves indentation and method bodies
- **Interactive CLI**: User-friendly command-line interface
- **Detailed Progress Tracking**: Shows exactly what the application is doing at each step

## Usage

Run the application:

```bash
dotnet run
```

The application will prompt you for:

1. **Input C# file path**: Path to the .cs file containing the class(es) to split
2. **Target output folder**: Directory where the partial class files will be created
3. **Number of splits** (optional): 
   - If specified: Methods are distributed evenly across that many files
   - If not specified: Methods are split into two files (public and private)
4. **Specific class name** (optional):
   - If specified: Only that class will be split
   - If not specified: All classes in the file will be split

## Examples

### Single Class File: `Person.cs`
```csharp
using System;

namespace MyApp
{
    public class Person
    {
        private string _name;
        
        public Person(string name)
        {
            _name = name;
        }
        
        public string GetName()
        {
            return _name;
        }
        
        private void ValidateName(string name)
        {
            // validation logic
        }
    }
}
```

### Output Files

**Default Split (public/private):**
- `Person_Part1.cs` - Contains public methods
- `Person_Part2.cs` - Contains private methods

**Custom Split (e.g., 2 files):**
- Methods distributed evenly across specified number of files

### Multiple Classes File: `MultipleClasses.cs`

When a file contains multiple classes (Person, Employee, Company), you can:
- Split all classes: Creates partial files for each class
- Split specific class: Specify "Employee" to only split the Employee class

### Sample Output Structure
```csharp
using System;

namespace MyApp;

public partial class Person
{
    public string GetName()
    {
        return _name;
    }
}
```

### Output File Naming
- **Single class**: `OriginalFileName_Part1.cs`, `OriginalFileName_Part2.cs`
- **Multiple classes**: `ClassName_Part1.cs`, `ClassName_Part2.cs` (one set per class)

## Requirements

- .NET 9.0 or later
- C# input files with single class definitions

## Building

```bash
dotnet build
```

## Testing

A sample test file is included in `TestFiles/SampleClass.cs` for testing the application.

## Project Structure

- `Program.cs` - Main application entry point and user interface
- `Models.cs` - Data models for method and class information
- `ClassParser.cs` - Parses C# files and extracts class/method information
- `ClassSplitter.cs` - Handles the splitting logic and file generation