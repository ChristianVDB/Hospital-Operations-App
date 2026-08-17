using System;
using System.Collections.Generic;

namespace HospitalOperationsSystem
{
    // VALUE OBJECT: Address
    public class Address
    {
        public string Street { get; set; }
        public string Suburb { get; set; }
        public string City { get; set; }
        public string Province { get; set; }

        public Address(string street, string suburb, string city, string province)
        {
            Street = street;
            Suburb = suburb;
            City = city;
            Province = province;
        }

        public override string ToString() => $"{Street}, {Suburb}, {City}, {Province}";
    }

    // VALUE OBJECT: MedicalAid
    public class MedicalAid
    {
        public string CompanyName { get; set; }
        public string PlanName { get; set; }
        public string PolicyNumber { get; set; }
        public string MainMemberFirstName { get; set; }
        public string MainMemberLastName { get; set; }
        public string MainMemberID { get; set; }

        public MedicalAid(string companyName, string planName, string policyNumber, 
                          string mainMemberFirstName, string mainMemberLastName, string mainMemberID)
        {
            CompanyName = companyName;
            PlanName = planName;
            PolicyNumber = policyNumber;
            MainMemberFirstName = mainMemberFirstName;
            MainMemberLastName = mainMemberLastName;
            MainMemberID = mainMemberID;
        }

        public override string ToString() => $"{CompanyName} ({PlanName}) - Policy: {PolicyNumber}";
    }

    // VALUE OBJECT: BillingItem
    public class BillingItem
    {
        public string Description { get; set; }
        public double Cost { get; set; }
        public DateTime DateIncurred { get; set; }

        public BillingItem(string description, double cost, DateTime dateIncurred)
        {
            Description = description;
            Cost = cost;
            DateIncurred = dateIncurred;
        }
    }

    // DOMAIN ENTITY / VALUE OBJECT: PatientFile
    public class PatientFile
    {
        public DateTime AdmissionDate { get; set; }
        public DateTime? DischargeDate { get; set; }
        public List<string> Diagnosis { get; set; } = new();
        public List<string> AssignedDoctorID { get; set; } = new();
        public List<BillingItem> BillingItems { get; set; } = new();
        public List<string> CurrentMedication { get; set; } = new();

        // Placeholder event hook for future milestone logic
        public event EventHandler? OnStatusChange;

        public PatientFile(DateTime admissionDate)
        {
            AdmissionDate = admissionDate;
        }

        public void TriggerStatusChange()
        {
            OnStatusChange?.Invoke(this, EventArgs.Empty);
        }
    }
}
