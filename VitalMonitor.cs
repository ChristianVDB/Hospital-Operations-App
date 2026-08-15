using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace HospitalOperationsSystem
{
    public class PatientAlertEventArgs : EventArgs
    {
        public string PatientId { get; }
        public string BedNumber { get; }
        public string Message { get; }

        public PatientAlertEventArgs(string patientId, string bedNumber, string message)
        {
            PatientId = patientId;
            BedNumber = bedNumber;
            Message = message;
        }
    }
    public delegate void PatientAlertHandler(object sender, PatientAlertEventArgs e);

    public class PatientMonitor
    {
        // Custom Event
        public event PatientAlertHandler? OnCriticalVitalsDetected;

        // Synchronous void method designed to run on a dedicated Thread
        public void StartMonitoring(List<Patient> patients, CancellationToken token)
        {
            Random rand = new Random();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Smart Sleep: Pauses thread for 3000ms OR wakes up instantly if canceled
                    bool isCanceled = token.WaitHandle.WaitOne(3000);
                    if (isCanceled)
                    {
                        break; // Exit loop immediately if app is closing
                    }

                    // Guard clause for empty patient list
                    if (patients == null || patients.Count == 0)
                        continue;

                    // Thread-safe snapshot pattern (.ToList())
                    var patientSnapshot = patients.ToList();

                    foreach (var patient in patientSnapshot)
                    {
                        // Fluctuate vitals in C# memory
                        patient.HeartRate += rand.Next(-3, 4);
                        patient.OxygenLevel -= rand.Next(0, 3);

                        // Threshold check
                        if (patient.OxygenLevel < 90 || patient.HeartRate > 120)
                        {
                            patient.IsCritical = true;

                            // Fire custom event safely
                            OnCriticalVitalsDetected?.Invoke(this, new PatientAlertEventArgs(
                                patient.IDNumber,
                                patient.BedNumber,
                                $"Critical Vitals Alert! O2 Level: {patient.OxygenLevel}%, HR: {patient.HeartRate} bpm"
                            ));
                        }
                    }
                }
                catch (Exception ex)//if existing patients lists gets removed or modified while thread is running
                {
                    Console.WriteLine($"[Monitoring Warning] {ex.Message}");
                }
            }

            Console.WriteLine("\n[Background Thread] Patient vital monitoring stopped safely.");
        }
    }
}