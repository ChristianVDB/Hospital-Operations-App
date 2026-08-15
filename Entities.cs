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
        //Patient default vitals
        public int HeartRate { get; set; } = 75;            // Standard baseline HR
        public int OxygenLevel { get; set; } = 98;          // Standard baseline O2%
        public bool IsCritical { get; set; } = false;       // Alert status flag
        public string BedNumber { get; set; } = "Bed-Unassigned";

        public Patient(MedicalAid medicalAidDetails, string firstName, string lastName,
                       string idNumber, string phoneNumber, string emailAddress, Address address)
            : base(firstName, lastName, idNumber, phoneNumber, emailAddress, address)
        {
            MedicalAidDetails = medicalAidDetails;
        }

        public override string GetSummary()
        {
            string status = IsCritical ? "[CRITICAL]" : "[STABLE]";
            return $"[Patient {status}] ID/NatID: {IDNumber} | Name: {FirstName} {LastName} | " +
                   $"Location: {BedNumber} | HR: {HeartRate} bpm | O2: {OxygenLevel}% | " +
                   $"Medical Aid: {MedicalAidDetails?.CompanyName ?? "Private"}";
        }
    }
}
