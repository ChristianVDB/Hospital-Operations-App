using System;
using System.Collections.Generic;
using System.Linq;

namespace HospitalOperationsSystem
{
    public class HospitalManager
    {
        public List<Patient> Patients { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
        private Employee? _currentUser = null;

        public static void Main(string[] args)
        {
            var manager = new HospitalManager();

            // Seed initial system state
            manager.Employees.AddRange(DataGenerator.GenerateEmployees());
            manager.Patients.AddRange(DataGenerator.GeneratePatients(5));

            using var cts = new CancellationTokenSource();
            var monitor = new PatientMonitor();

            // Subscribe event handler
            monitor.OnCriticalVitalsDetected += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n\n[CRITICAL EVENT] Patient {e.PatientId} at {e.BedNumber}: {e.Message}");
                Console.ResetColor();
                Console.Write("Press Enter to continue...");
            };

            // Instantiate a standard C# Thread
            Thread monitorThread = new Thread(() => monitor.StartMonitoring(manager.Patients, cts.Token))
            {
                IsBackground = true // Ensures thread terminates automatically if the process shuts down
            };

            // Start the thread independently of user menu input
            monitorThread.Start();

            Console.Title = "Hospital Management & Operations System";

            // Enforce Authentication before entering system
            if (manager.Authentication())
            {
                manager.MenuNavigation();
            }
            else
            {
                Console.WriteLine("\nAccess Denied. Exiting application.");
            }

            cts.Cancel();           // Signal thread to stop
            monitorThread.Join(1000); // Give thread up to 1 second to exit gracefully
        }


        // FUNCTION 1: Authentication

        public bool Authentication()
        {
            int attempts = 0;
            while (attempts < 3)
            {
                Console.Clear();
                Console.WriteLine("---------------------------------------------------");
                Console.WriteLine("      HOSPITAL MANAGEMENT SYSTEM - LOGIN         ");
                Console.WriteLine("---------------------------------------------------");
                Console.Write("Username: ");
                string username = Console.ReadLine() ?? "";
                Console.Write("Password: ");
                string password = Console.ReadLine() ?? "";

                var emp = Employees.FirstOrDefault(e =>
                    e.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && e.Password == password);

                if (emp != null)
                {
                    _currentUser = emp;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"\nLogin Successful! Welcome, {emp.FirstName} {emp.LastName} ({emp.JobRole}).");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to continue to main menu...");
                    Console.ReadLine();
                    return true;
                }

                attempts++;
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nInvalid username or password. Attempts remaining: {3 - attempts}");
                Console.ResetColor();
                Console.WriteLine("Press Enter to try again...");
                Console.ReadLine();
            }
            return false;
        }

        // MENU NAVIGATION

        public void MenuNavigation()
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine($"   HOSPITAL MANAGEMENT SYSTEM | User: {_currentUser?.FirstName} ({_currentUser?.JobRole})");
                Console.WriteLine("----------------------------------------------------");
                Console.WriteLine("1. Add New Patient");
                Console.WriteLine("2. Delete Patient Record (Deceased / Removed)");
                Console.WriteLine("3. Update Patient Record");
                Console.WriteLine("4. View All Patient Records");
                Console.WriteLine("5. Search Patient File (by Patient ID / ID Number)");
                Console.WriteLine("6. Logout & Exit");
                Console.WriteLine("--------------------------------------------------");
                Console.Write("Select an option (1-6): ");

                string choice = Console.ReadLine() ?? "";

                switch (choice)
                {
                    case "1":
                        AddNewPatient();
                        break;
                    case "2":
                        DeletePatient();
                        break;
                    case "3":
                        UpdatePatientRecord();
                        break;
                    case "4":
                        ViewPatientRecords();
                        break;
                    case "5":
                        SearchPatientFile();
                        break;
                    case "6":
                        running = false;
                        Console.WriteLine("\nLogging out... Goodbye!");
                        break;
                    default:
                        Console.WriteLine("\nInvalid option! Press Enter to try again.");
                        Console.ReadLine();
                        break;
                }
            }
        }

        // FUNCTION 2: Add New Patients

        private void AddNewPatient()
        {
            Console.Clear();
            Console.WriteLine("--- ADD NEW PATIENT ---");

            Console.Write("Enter First Name: ");
            string firstName = Console.ReadLine() ?? "";

            Console.Write("Enter Last Name: ");
            string lastName = Console.ReadLine() ?? "";

            Console.Write("Enter ID Number: ");
            string idNumber = Console.ReadLine() ?? "";

            Console.Write("Enter Phone Number: ");
            string phone = Console.ReadLine() ?? "";

            Console.Write("Enter Email Address: ");
            string email = Console.ReadLine() ?? "";

            Console.WriteLine("\n-- Address Details --");
            Console.Write("Street: "); string street = Console.ReadLine() ?? "";
            Console.Write("Suburb: "); string suburb = Console.ReadLine() ?? "";
            Console.Write("City: "); string city = Console.ReadLine() ?? "";
            Console.Write("Province: "); string province = Console.ReadLine() ?? "";
            var address = new Address(street, suburb, city, province);

            Console.WriteLine("\n-- Medical Aid Details --");
            Console.Write("Company Name: "); string medCompany = Console.ReadLine() ?? "Private";
            Console.Write("Plan Name: "); string planName = Console.ReadLine() ?? "N/A";
            Console.Write("Policy Number: "); string policyNum = Console.ReadLine() ?? "N/A";
            var medAid = new MedicalAid(medCompany, planName, policyNum, firstName, lastName, idNumber);

            var newPatient = new Patient(medAid, firstName, lastName, idNumber, phone, email, address);

            newPatient.BedNumber = $"Ward-Bed-{Patients.Count + 1}"; //Auto generated bednumber

            // Add initial active patient file
            var initialFile = new PatientFile(DateTime.Now);
            initialFile.Diagnosis.Add("Initial Admission");
            newPatient.FileHistory.Add(initialFile);

            Patients.Add(newPatient);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nPatient '{firstName} {lastName}' added successfully!");
            Console.ResetColor();
            Console.WriteLine("Press Enter to return to main menu...");
            Console.ReadLine();
        }

        // FUNCTION 3: Delete Patients

        private void DeletePatient()
        {
            Console.Clear();
            Console.WriteLine("--- DELETE PATIENT RECORD (DECEASED / REMOVAL) ---");
            Console.Write("Enter Patient ID Number to delete: ");
            string idNum = Console.ReadLine() ?? "";

            var patient = Patients.FirstOrDefault(p => p.IDNumber.Equals(idNum, StringComparison.OrdinalIgnoreCase));

            if (patient == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nPatient not found.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Found Record: {patient.GetSummary()}");
                Console.Write("Are you sure you want to permanently delete this record? (y/n): ");
                string confirm = Console.ReadLine() ?? "";

                if (confirm.ToLower() == "y")
                {
                    Patients.Remove(patient);
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("\nPatient record removed successfully.");
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine("\nDeletion cancelled.");
                }
            }

            Console.WriteLine("Press Enter to return to main menu...");
            Console.ReadLine();
        }

        // FUNCTION 4: Update Patient Records

        private void UpdatePatientRecord()
        {
            Console.Clear();
            Console.WriteLine("--- UPDATE PATIENT RECORD ---");
            Console.Write("Enter Patient ID Number: ");
            string idNum = Console.ReadLine() ?? "";

            var patient = Patients.FirstOrDefault(p => p.IDNumber.Equals(idNum, StringComparison.OrdinalIgnoreCase));

            if (patient == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nPatient not found.");
                Console.ResetColor();
            }
            else
            {
                Console.WriteLine($"Updating Patient: {patient.FirstName} {patient.LastName}");
                Console.Write("Enter New Phone Number (leave blank to keep current): ");
                string phone = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(phone)) patient.PhoneNumber = phone;

                Console.Write("Enter New Email (leave blank to keep current): ");
                string email = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(email)) patient.EmailAddress = email;

                Console.Write("Add new diagnosis to current active file? (y/n): ");
                string addDiag = Console.ReadLine() ?? "";
                if (addDiag.ToLower() == "y")
                {
                    Console.Write("Enter Diagnosis text: ");
                    string diagText = Console.ReadLine() ?? "";
                    if (!string.IsNullOrWhiteSpace(diagText) && patient.FileHistory.Count > 0)
                    {
                        patient.FileHistory.Last().Diagnosis.Add(diagText);
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\nPatient record updated successfully!");
                Console.ResetColor();
            }

            Console.WriteLine("Press Enter to return to main menu...");
            Console.ReadLine();
        }

        // FUNCTION 5: View Patient Records

        private void ViewPatientRecords()
        {
            Console.Clear();
            Console.WriteLine("--- VIEW ALL PATIENT RECORDS ---");
            if (Patients.Count == 0)
            {
                Console.WriteLine("No patient records available.");
            }
            else
            {
                foreach (var p in Patients)
                {
                    Console.WriteLine(p.GetSummary());
                    Console.WriteLine($"  Contact: {p.PhoneNumber} | {p.EmailAddress}");
                    Console.WriteLine($"  Address: {p.HomeAddress}");
                    Console.WriteLine("  ------------------------------------------------");
                }
            }

            Console.WriteLine("\nPress Enter to return to main menu...");
            Console.ReadLine();
        }

        // FUNCTION 6: Search Patient File

        private void SearchPatientFile()
        {
            Console.Clear();
            Console.WriteLine("--- SEARCH PATIENT FILE ---");
            Console.Write("Enter Patient National ID Number: ");
            string query = Console.ReadLine() ?? "";

            var patient = Patients.FirstOrDefault(p => p.IDNumber.Equals(query, StringComparison.OrdinalIgnoreCase));

            if (patient == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nNo matching patient file found.");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("\n=== PATIENT FILE FOUND ===");
                Console.ResetColor();
                Console.WriteLine($"Name: {patient.FirstName} {patient.LastName}");
                Console.WriteLine($"ID Number: {patient.IDNumber}");
                Console.WriteLine($"Medical Aid: {patient.MedicalAidDetails}");
                Console.WriteLine($"Address: {patient.HomeAddress}");

                Console.WriteLine("\n--- Admission File History ---");
                int fileIndex = 1;
                foreach (var file in patient.FileHistory)
                {
                    Console.WriteLine($"\n[File #{fileIndex++}] Admission Date: {file.AdmissionDate}");
                    Console.WriteLine($"  Diagnoses: {string.Join(", ", file.Diagnosis)}");
                    Console.WriteLine($"  Current Medications: {string.Join(", ", file.CurrentMedication)}");
                    Console.WriteLine($"  Assigned Doctor IDs: {string.Join(", ", file.AssignedDoctorID)}");
                    Console.WriteLine("  Billing Items:");
                    foreach (var item in file.BillingItems)
                    {
                        Console.WriteLine($"    - {item.Description}: R{item.Cost:F2} (Date: {item.DateIncurred:yyyy-MM-dd})");
                    }
                }
            }

            Console.WriteLine("\nPress Enter to return to main menu...");
            Console.ReadLine();
        }
    }
}
