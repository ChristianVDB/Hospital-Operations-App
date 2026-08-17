using System;
using System.Collections.Generic;

namespace HospitalOperationsSystem
{
    public static class DataGenerator
    {
        private static readonly Random Rnd = new();

        public static List<Employee> GenerateEmployees()
        {
            var address = new Address("123 Hospital St", "Central", "Pretoria", "Gauteng");
            return new List<Employee>
            {
                new Employee("EMP-101", "admin", "admin123", JobRoleEnum.Administrator, "Nompilo", "Mbense", "9801015000081", "0821234567", "admin@hospital.com", address),
                new Employee("EMP-102", "dr.smith", "docpass", JobRoleEnum.Doctor, "John", "Smith", "8503125000082", "0839876543", "jsmith@hospital.com", address),
                new Employee("EMP-103", "nurse.mary", "nursepass", JobRoleEnum.Nurse, "Mary", "Jane", "9207205000083", "0841112223", "mjane@hospital.com", address)
            };
        }

        public static List<Patient> GeneratePatients(int count)
        {
            var firstNames = new[] { "Sipho", "Lerato", "David", "Thabo", "Sarah", "Kagiso" };
            var lastNames = new[] { "Dlamini", "Mokoena", "Williams", "Ndlovu", "Botha", "Khumalo" };
            var cities = new[] { "Johannesburg", "Pretoria", "Durban", "Cape Town" };
            var medicalAids = new[] { "Discovery Health", "Bonitas", "Momentum", "Medshield" };

            var patients = new List<Patient>();

            for (int i = 1; i <= count; i++)
            {
                var address = new Address($"{Rnd.Next(1, 999)} Main Rd", "Suburbs", cities[Rnd.Next(cities.Length)], "Gauteng");
                var medAid = new MedicalAid(medicalAids[Rnd.Next(medicalAids.Length)], "Classic Comprehensive", $"POL-{1000 + i}", firstNames[Rnd.Next(firstNames.Length)], lastNames[Rnd.Next(lastNames.Length)], $"ID-{7000 + i}");

                var patient = new Patient(
                    medAid,
                    firstNames[Rnd.Next(firstNames.Length)],
                    lastNames[Rnd.Next(lastNames.Length)],
                    $"100200300400{i}",
                    $"071{Rnd.Next(1000000, 9999999)}",
                    $"patient{i}@gmail.com",
                    address
                    
                );

                //Patient's vitals and designated bed number
                patient.BedNumber = $"ICU-Bed-{100 + i}";    // Assigns Bed 101, Bed 102, Bed 103, etc.
                patient.HeartRate = Rnd.Next(70, 95);        // Random starting heart rate
                patient.OxygenLevel = Rnd.Next(93, 100);     // Random starting oxygen level

                // Populate randomized initial file history
                var file = new PatientFile(DateTime.Now.AddDays(-Rnd.Next(1, 30)));
                file.Diagnosis.Add("General Checkup / Observation");
                file.AssignedDoctorID.Add("EMP-102");
                file.CurrentMedication.Add("Paracetamol 500mg");
                file.BillingItems.Add(new BillingItem("Consultation Fee", 650.00, DateTime.Now));

                patient.FileHistory.Add(file);
                patients.Add(patient);
            }

            return patients;
        }
    }
}
