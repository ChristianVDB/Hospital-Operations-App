using System;
using System.Threading;

namespace HospitalOperationsSystem
{
    public class OperationsEngine
    {
        // Events for completion and failures
        public event EventHandler<string>? OnTaskCompleted;
        public event EventHandler<string>? OnResourceFailed;

        // Public helper methods allowing external classes to trigger your events
        public void RaiseTaskCompleted(string message)
        {
            OnTaskCompleted?.Invoke(this, message);
        }

        public void RaiseResourceFailed(string errorMessage)
        {
            OnResourceFailed?.Invoke(this, errorMessage);
        }

    }
}