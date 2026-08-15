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

    public class Patient : Person, IMonitorable, IProcessable
    {
        public MedicalAid MedicalAidDetails { get; set; }
        public List<PatientFile> FileHistory { get; set; } = new();
        //Patient default vitals
        public int HeartRate { get; set; } = 75;            // Standard baseline HR
        public int OxygenLevel { get; set; } = 98;          // Standard baseline O2%
        public bool IsCritical { get; set; } = false;       // Alert status flag
        public string BedNumber { get; set; } = "Bed-Unassigned";

        private const int MaxfileHistoryLimit = 10;         // Domain constraint

        public Patient(MedicalAid medicalAidDetails, string firstName, string lastName,
                       string idNumber, string phoneNumber, string emailAddress, Address address)
            : base(firstName, lastName, idNumber, phoneNumber, emailAddress, address)
        {
            MedicalAidDetails = medicalAidDetails;
        }

        // throws the invalid system state exception if the patient is in a critical state
        public bool CheckVitalStatus()
        {
            if (OxygenLevel < 90 || HeartRate > 120) 
            { 
                IsCritical = true;
                throw new InvalidSystemStateException($"Critical vittals detected for {FirstName} {LastName}");

            }
            return IsCritical;
        }

        //throws domain exceptions if constraints are violated, such as exceeding file history limit
        public void ProcessAdmission(string doctorID, string initialDiagnosis)
        {
            if(FileHistory.Count >= MaxfileHistoryLimit)
            {
                throw new ResourceLimitExceededException($"Cannot admit patient {FirstName} {LastName}. File history limit reached.");
            }
            var newFile = new PatientFile(DateTime.Now);
            newFile.AssignedDoctorID.Add(doctorID);
            newFile.Diagnosis.Add(initialDiagnosis);
            FileHistory.Add(newFile);
        }

        //throws an exception if there is no active file to discharge the patient from
        public void DischargePatient()
        {
            var activeFile = FileHistory.FindLast(f => f.DischargeDate == null)
            ?? throw new InvalidSystemStateException($"No active file found for patient {FirstName} {LastName} to discharge.");
            
            activeFile.DischargeDate = DateTime.Now;
            IsCritical = false;
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
