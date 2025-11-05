using System;

namespace TestNamespace
{
    public class SampleClass
    {
        private string _name;
        private int _age;

        public SampleClass(string name, int age)
        {
            _name = name;
            _age = age;
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Age
        {
            get { return _age; }
            set { _age = value; }
        }

        public void DisplayInfo()
        {
            Console.WriteLine($"Name: {_name}, Age: {_age}");
        }

        public string GetFullInfo()
        {
            return $"Person: {_name} ({_age} years old)";
        }

        private void ValidateAge(int age)
        {
            if (age < 0)
                throw new ArgumentException("Age cannot be negative");
        }

        private void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty");
        }

        private string FormatName(string name)
        {
            return name.Trim().ToTitleCase();
        }

        public static SampleClass CreateDefault()
        {
            return new SampleClass("Unknown", 0);
        }
    }
}

// Extension method for string
public static class StringExtensions
{
    public static string ToTitleCase(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToUpper(input[0]) + input.Substring(1).ToLower();
    }
}