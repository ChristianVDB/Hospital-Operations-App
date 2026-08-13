using System;
using System.Collections.Generic;

namespace HospitalOperationsSystem
{
    // ABSTRACT BASE CLASS: Person (Demonstrates Abstraction & Encapsulation)
  
    public abstract class Person
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string IDNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string EmailAddress { get; set; }
        public Address HomeAddress { get; set; }

        protected Person(string firstName, string lastName, string idNumber, 
                         string phoneNumber, string emailAddress, Address address)
        {
            FirstName = firstName;
            LastName = lastName;
            IDNumber = idNumber;
            PhoneNumber = phoneNumber;
            EmailAddress = emailAddress;
            HomeAddress = address;
        }

        // Abstract method enforcing Polymorphism in derived classes
        public abstract string GetSummary();
    }

    // DERIVED CLASS: Employee (Inheritance & Encapsulation)
   
    public class Employee : Person
    {
        public string EmployeeID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Encapsulated credentials
        public JobRoleEnum JobRole { get; set; }

        public Employee(string employeeID, string username, string password, JobRoleEnum jobRole,
                        string firstName, string lastName, string idNumber, 
                        string phoneNumber, string emailAddress, Address address)
            : base(firstName, lastName, idNumber, phoneNumber, emailAddress, address)
        {
            EmployeeID = employeeID;
            Username = username;
            Password = password;
            JobRole = jobRole;
        }

        public override string GetSummary()
        {
            return $"[Employee] ID: {EmployeeID} | Name: {FirstName} {LastName} | Role: {JobRole} | Username: {Username}";
        }
    }

    // DERIVED CLASS: Patient (Inheritance & Encapsulation)
   
    public class Patient : Person
    {
        public MedicalAid MedicalAidDetails { get; set; }
        public List<PatientFile> FileHistory { get; set; } = new();

        public Patient(MedicalAid medicalAidDetails, string firstName, string lastName, 
                       string idNumber, string phoneNumber, string emailAddress, Address address)
            : base(firstName, lastName, idNumber, phoneNumber, emailAddress, address)
        {
            MedicalAidDetails = medicalAidDetails;
        }

        public override string GetSummary()
        {
            return $"[Patient] ID/NatID: {IDNumber} | Name: {FirstName} {LastName} | Medical Aid: {MedicalAidDetails?.CompanyName ?? "Private"} | Files Count: {FileHistory.Count}";
        }
    }
}
