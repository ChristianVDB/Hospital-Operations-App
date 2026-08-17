using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HospitalOperationsSystem
{
    public class HospitalManager
    {
        public List<Patient> Patients { get; set; } = new();
        public List<Employee> Employees { get; set; } = new();
        private Employee? _currentUser = null;

        private readonly OperationsEngine _engine = new();

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
                Console.WriteLine("[CRITICAL EVENT DETECTED - Continue input here]");
                Console.Beep(); // Audible alert for critical event
                Console.WriteLine("");
                Console.ResetColor();
            };

            manager._engine.OnTaskCompleted += (sender, message) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n[EVENT: TASK SUCCESS] {message}");
                Console.ResetColor();
            };

            manager._engine.OnResourceFailed += (sender, errorMessage) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[EVENT: TASK FAILED] {errorMessage}");
                Console.ResetColor();
            };

            // Instantiating a Thread
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

                switch ((MenuOption)int.Parse(choice))
                {
                    case MenuOption.AddNewPatient:
                        AddNewPatient();
                        break;
                    case MenuOption.DeletePatient:
                        DeletePatient();
                        break;
                    case MenuOption.UpdatePatientRecord:
                        UpdatePatientRecord();
                        break;
                    case MenuOption.ViewPatientRecords:
                        ViewPatientRecords();
                        break;
                    case MenuOption.SearchPatientFile:
                        SearchPatientFile();
                        break;
                    case MenuOption.Exit:
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

        // Handles domain exceptions and prevents application crashes
        private void ExecuteSafeOperation(Action action, string taskName = "Operation")
        {
            try
            {
                action();
                // Trigger Task Success Event
                _engine.RaiseTaskCompleted($"'{taskName}' processed without errors.");
            }
            catch (ResourceLimitExceededException ex)
            {
                // Trigger Resource Failure Event
                _engine.RaiseResourceFailed($"[Resource Limit Exceeded] {ex.Message}");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[Resource Limit Has Been Reached] {ex.Message}");
                Console.ResetColor();
            }
            catch (InvalidSystemStateException ex)
            {
                // Trigger Domain Rule Failure Event
                _engine.RaiseResourceFailed($"[Domain Rule Violation] {ex.Message}");

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n[Domain Rule Violation] {ex.Message}");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                // Trigger Unexpected Exception Event
                _engine.RaiseResourceFailed($"[Unexpected Error] {ex.Message}");

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[UNEXPECTED ERROR] {ex.Message}");
                Console.ResetColor();
            }
            finally
            {
                Console.ResetColor();
                Console.WriteLine("\nPress Enter to return to main menu...");
                Console.ReadLine();
            }
        }

        // FUNCTION 2: Add New Patients
        private void AddNewPatient()
        {
            Console.Clear();
        returnFirstName: //Label to return to if validation fails
            Console.WriteLine("--- ADD NEW PATIENT ---");
            Console.Write("Enter First Name: ");
            string firstName = Console.ReadLine() ?? "";

            // Validadting that the user entered a value and not NULL
            if (string.IsNullOrWhiteSpace(firstName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("First name is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnFirstName; // Returning to the label to re-prompt for first name
            }
            Console.Clear();

        returnLastName:
            Console.WriteLine("--- ADD NEW PATIENT ---");
            Console.Write("Enter Last Name: ");
            string lastName = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(lastName))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Last name is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnLastName;
            }
            Console.Clear();

        returnIDNum:
            Console.WriteLine("--- ADD NEW PATIENT ---");
            Console.Write("Enter ID Number: ");
            string idNumber = Console.ReadLine() ?? "";

            //Validating that the value entered is not NUll, is 13 digits long, and is numbers
            if (string.IsNullOrWhiteSpace(idNumber) || idNumber.Length != 13 || !idNumber.All(char.IsDigit)) 
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid ID number. Please enter a 13-digit.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnIDNum;
            }
            Console.Clear();

        returnPhoneNum:
            Console.WriteLine("--- ADD NEW PATIENT ---");
            Console.Write("Enter Phone Number: ");
            string phoneNum = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(phoneNum) || phoneNum.Length != 10 || !phoneNum.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid phone number. Please enter a 10-digit number.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnPhoneNum;
            }
            Console.Clear();

        returnEmailAdd:
            Console.WriteLine("--- ADD NEW PATIENT ---");
            Console.Write("Enter Email Address: ");
            string email = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(email))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Email Address is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnEmailAdd;
            }
            Console.Clear();

        returnStreet:
            Console.WriteLine("\n-- Address Details --");
            Console.Write("Street: ");
            string street = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(street))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Street is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnStreet;
            }
            Console.Clear();

        returnSuburb:
            Console.WriteLine("\n-- Address Details --");
            Console.Write("Suburb: ");
            string suburb = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(suburb))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Suburb is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnSuburb;
            }
            Console.Clear();

        returnCity:
            Console.WriteLine("\n-- Address Details --");
            Console.Write("City: ");
            string city = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(city))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("City is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnCity;
            }
            Console.Clear();

        returnProvince:
            Console.WriteLine("\n-- Address Details --");
            Console.Write("Province: ");
            string province = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(province))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Province is required.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                Console.Clear();
                goto returnProvince;
            }
            Console.Clear();

            var address = new Address(street, suburb, city, province);

            Console.Write("Does the patient have a medical aid? (y/n): ");
            string hasMedAid = Console.ReadLine() ?? "";

            // Default to Private payer if user indicates no medical aid
            MedicalAid medAid = new MedicalAid("Private", "N/A", "N/A", firstName, lastName, idNumber);

            if (hasMedAid.Trim().ToLower() == "y")
            {
            returnCumpanyName:
                Console.WriteLine("\n-- Medical Aid Details --");
                Console.Write("Company Name: ");
                string medCompany = Console.ReadLine() ?? "Private";

                if (string.IsNullOrWhiteSpace(medCompany))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Medical Aid Company Name is required.");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to return...");
                    Console.ReadLine();
                    Console.Clear();
                    goto returnCumpanyName;
                }
                Console.Clear();
                
            returnPlanName:
                Console.WriteLine("\n-- Medical Aid Details --");
                Console.Write("Plan Name: ");
                string planName = Console.ReadLine() ?? "N/A";

                if (string.IsNullOrWhiteSpace(planName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Plan Name is required.");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to return...");
                    Console.ReadLine();
                    Console.Clear();
                    goto returnPlanName;
                }
                Console.Clear();

            returnPolicyName:
                Console.WriteLine("\n-- Medical Aid Details --");
                Console.Write("Policy Number: ");
                string policyNum = Console.ReadLine() ?? "N/A";

                if (string.IsNullOrWhiteSpace(policyNum))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Policy Number is required.");
                    Console.ResetColor();
                    Console.WriteLine("Press Enter to return...");
                    Console.ReadLine();
                    Console.Clear();
                    goto returnPolicyName;
                }
                Console.Clear();

                medAid = new MedicalAid(medCompany, planName, policyNum, firstName, lastName, idNumber);
            }

            var newPatient = new Patient(medAid, firstName, lastName, idNumber, phoneNum, email, address);

            newPatient.BedNumber = $"Ward-Bed-{Patients.Count + 1}"; //Auto generated bednumber

            ExecuteSafeOperation(() =>
            {
                //creates the initial file safely and checks domain rules
                newPatient.ProcessAdmission(_currentUser?.EmployeeID ?? "EMP-101", "Initial Admission");

                Patients.Add(newPatient);
                SaveState();
                // HERE IS THE LOGGING CALL
                // We just call the static method and pass a descriptive message.
                Logger.Log($"Added patient ID: {patient.IDNumber}");

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\nPatient '{firstName} {lastName}' added successfully!");
                Console.ResetColor();

            }, "Patient Admission");
        }

        // FUNCTION 3: Delete Patients
        private void DeletePatient()
        {
            Console.Clear();
            Console.WriteLine("--- DELETE PATIENT RECORD (DECEASED / REMOVAL) ---");
        returnIDNum:
            Console.Write("Enter Patient ID Number to delete: ");
            string idNum = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(idNum) || idNum.Length != 13 || !idNum.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid ID number. Please enter a 13-digit.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                goto returnIDNum;
            }

            var patient = Patients.FirstOrDefault(p => p.IDNumber.Equals(idNum, StringComparison.OrdinalIgnoreCase));

            if (patient == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nPatient not found.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return to main menu...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine($"Found Record: {patient.GetSummary()}");
                Console.Write("Are you sure you want to permanently delete this record? (y/n): ");
                string confirm = Console.ReadLine() ?? "";

                if (confirm.ToLower() == "y")
                {
                    ExecuteSafeOperation(() =>
                    {
                        patient.DischargePatient(); // Attempt to discharge before deletion
                        Patients.Remove(patient);

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("\nPatient record removed successfully.");
                        Console.ResetColor();

                    }, "Patient Removal & Discharge");
                }
                else
                {
                    Console.WriteLine("\nDeletion cancelled.");
                    Console.WriteLine("Press Enter to return to main menu...");
                    Console.ReadLine();
                }
            }
        }

        // FUNCTION 4: Update Patient Records
        private void UpdatePatientRecord()
        {
            Console.Clear();
            Console.WriteLine("--- UPDATE PATIENT RECORD ---");
        returnIDNum:
            Console.Write("Enter Patient ID Number: ");
            string idNum = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(idNum) || idNum.Length != 13 || !idNum.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid ID number. Please enter a 13-digit.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                goto returnIDNum;
            }

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
            returnPhoneNum:
                Console.Write("Enter New Phone Number (leave blank to keep current): ");
                string phoneNum = Console.ReadLine() ?? "";

                if (!string.IsNullOrWhiteSpace(phoneNum)) patient.PhoneNumber = phoneNum;

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
        returnIDNum:
            Console.Write("Enter Patient National ID Number: ");
            string idNum = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(idNum) || idNum.Length != 13 || !idNum.All(char.IsDigit))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Invalid ID number. Please enter a 13-digit.");
                Console.ResetColor();
                Console.WriteLine("Press Enter to return...");
                Console.ReadLine();
                goto returnIDNum;
            }

            var patient = Patients.FirstOrDefault(p => p.IDNumber.Equals(idNum, StringComparison.OrdinalIgnoreCase));

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
