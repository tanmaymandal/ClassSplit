using System;
using System.Collections.Generic;

namespace TestNamespace
{
    public class Person
    {
        private string _name;
        private int _age;

        public Person(string name, int age)
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
    }

    public class Employee
    {
        private string _employeeId;
        private string _department;
        private decimal _salary;

        public Employee(string employeeId, string department, decimal salary)
        {
            _employeeId = employeeId;
            _department = department;
            _salary = salary;
        }

        public string EmployeeId
        {
            get { return _employeeId; }
            set { _employeeId = value; }
        }

        public string Department
        {
            get { return _department; }
            set { _department = value; }
        }

        public decimal Salary
        {
            get { return _salary; }
            set { _salary = value; }
        }

        public void DisplayEmployeeInfo()
        {
            Console.WriteLine($"Employee: {_employeeId} in {_department}, Salary: ${_salary}");
        }

        public decimal CalculateAnnualSalary()
        {
            return _salary * 12;
        }

        public void GiveRaise(decimal percentage)
        {
            _salary += _salary * (percentage / 100);
        }

        private void ValidateEmployeeId(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("Employee ID cannot be empty");
        }

        private void ValidateSalary(decimal salary)
        {
            if (salary < 0)
                throw new ArgumentException("Salary cannot be negative");
        }

        private string FormatSalary(decimal salary)
        {
            return $"${salary:N2}";
        }
    }

    internal class Company
    {
        private List<Employee> _employees;
        private string _companyName;

        public Company(string companyName)
        {
            _companyName = companyName;
            _employees = new List<Employee>();
        }

        public string CompanyName
        {
            get { return _companyName; }
            set { _companyName = value; }
        }

        public void AddEmployee(Employee employee)
        {
            _employees.Add(employee);
        }

        public void RemoveEmployee(string employeeId)
        {
            _employees.RemoveAll(e => e.EmployeeId == employeeId);
        }

        public Employee FindEmployee(string employeeId)
        {
            return _employees.Find(e => e.EmployeeId == employeeId);
        }

        public List<Employee> GetAllEmployees()
        {
            return new List<Employee>(_employees);
        }

        private void ValidateCompanyName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Company name cannot be empty");
        }

        private decimal CalculateTotalPayroll()
        {
            decimal total = 0;
            foreach (var employee in _employees)
            {
                total += employee.Salary;
            }
            return total;
        }
    }
}