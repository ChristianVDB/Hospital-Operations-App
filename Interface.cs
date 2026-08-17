using System;

namespace HospitalOperationsSystem
{
	//Interface for enttities that can be monitored for vital thresholds
	public interface IMonitorable
	{
		bool CheckVitalStatus();
	}

	//Interface for entities that can be processed for admission and discharge
	public interface IProcessable
	{
		void ProcessAdmission(string doctorID, string initialDiagnosis);
		void DischargePatient();
	}
}