using System;

        // =========================
        // REQUIREMENT: Logging System
        // =========================
public static class Logger
    {
        // 1. DEFINE THE FILE PATH
        // We determine where the log file should sit.
        // AppDomain.CurrentDomain.BaseDirectory finds the folder where your .exe is running.
        // We combine that with the filename "hospital.log".
        private static string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hospital.log");

        // 2. THE LOG METHOD
        // Other classes call this method. They provide the 'message' and an optional 'level'.
        // Levels help categorize logs: "INFO" (normal stuff), "WARN" (something fishy), "ERROR" (crashes).
        public static void Log(string message, string level = "INFO")
        {
            // 3. FORMAT THE ENTRY
            // We add a timestamp so we know exactly when it happened.
            // Format: [YYYY-MM-DD HH:MM:SS] [LEVEL] - Message
            string logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] - {message}{Environment.NewLine}";

            try
            {
                // 4. WRITE TO FILE
                // File.AppendAllText is great because if the file doesn't exist, it creates it.
                // If it does exist, it just adds the new line to the end.
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // 5. SILENT FAILURE
                // Crucial rule: The logging system itself should never crash the app.
                // If we can't write to the log file (e.g., hard drive full), we just ignore it.
            }
        }
    }

