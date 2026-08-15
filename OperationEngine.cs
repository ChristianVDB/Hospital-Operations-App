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

        // Standard task runner method
        public void ExecuteTask(Action taskAction, string taskName)
        {
            try
            {
                taskAction();
                RaiseTaskCompleted($"Operation '{taskName}' completed successfully.");
            }
            catch (ResourceLimitExceededException ex)
            {
                RaiseResourceFailed($"[RESOURCE LIMIT REACHED in '{taskName}']: {ex.Message}");
            }
            catch (InvalidSystemStateException ex)
            {
                RaiseResourceFailed($"[RULE VIOLATION in '{taskName}']: {ex.Message}");
            }
            catch (Exception ex)
            {
                RaiseResourceFailed($"[SYSTEM ERROR in '{taskName}']: {ex.Message}");
            }
        }

        // Concrete Background Task Example
        public void ExecuteBatchFileSync(int recordCount)
        {
            try
            {
                if (recordCount > 50)
                {
                    throw new ResourceLimitExceededException("Cannot sync more than 50 records per batch.");
                }

                Thread.Sleep(1000);
                RaiseTaskCompleted($"Batch file sync for {recordCount} patient records completed successfully.");
            }
            catch (Exception ex)
            {
                RaiseResourceFailed($"Batch Sync Failed: {ex.Message}");
            }
        }
    }
}