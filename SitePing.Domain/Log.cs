using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;

namespace Matrix.SitePing.Domain
{
    public enum LogErrorLevel
    {
        Highlight,
        Information,
        Warning,
        Error
    }

    public interface ILog
    {
        bool EchoToConsole { get; set; }

        void AddLogEntry(string text);
        void AddLogEntry(string text, LogErrorLevel level);
        void LogError(Exception ex, string errorInfo);
        void NewLine();
        void SeparatorLine();
        void Close(bool success);
        string GetFormattedLogText(string text, bool addTime, LogErrorLevel level);
    }

    public class Log : ILog
    {
        private ConsoleColor mOriginalConsoleColor;

        private StringBuilder mLatestLog = new StringBuilder();
        /// <summary>
        /// Returns the current complete contents of the log
        /// </summary>
        public string LogText
        {
            get { return mLatestLog.ToString(); }
        }

        private FileInfo mLogFile = null;
        public FileInfo LogFile
        {
            get
            {
                if (mLogFile == null)
                {
                    string actualFilename = String.Format("{0}{1:yyyyMMdd-HHmmss}.txt", mLogFilenamePrefix, DateTime.Now);
                    mLogFile = new FileInfo(Path.Combine(this.LogFilePath, actualFilename));
                }
                return mLogFile;
            }
        }

        private string mLogFilePath = String.Empty;
        public string LogFilePath
        {
            get
            {
                if (String.IsNullOrEmpty(mLogFilePath))
                {
                    return Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);
                }
                else
                {
                    return mLogFilePath;
                }
            }

            set { mLogFilePath = value; }
        }


        private string mLogFilenamePrefix = String.Empty;
        /// <summary>
        /// A string that is prepended to the log filename
        /// </summary>
        public string LogFilenamePrefix
        {
            get { return mLogFilenamePrefix; }
            set { mLogFilenamePrefix = value; }
        }

        private int mKeepLogsForDays = 180;
        /// <summary>
        /// Log files older than this number of days are automatically deleted 
        /// </summary>
        public int KeepLogsForDays
        {
            get { return mKeepLogsForDays; }
            set { mKeepLogsForDays = value; }
        }

        private bool mEchoToConsole = true;
        public bool EchoToConsole
        {
            get { return mEchoToConsole; }
            set { mEchoToConsole = value; }
        }

        public Log()
        {
            //this.mLogFilenamePrefix = Config.Instance.LoggingFilenamePrefix;
            this.mOriginalConsoleColor = Console.ForegroundColor;

            CleanupOldLogs();
        }

        private void CleanupOldLogs()
        {
            DirectoryInfo di = new DirectoryInfo(this.LogFilePath);
            FileInfo[] logs = di.GetFiles(this.LogFilenamePrefix + "*");
            foreach (FileInfo fi in logs)
            {
                TimeSpan age = DateTime.Now - fi.LastWriteTime;
                if (age.TotalDays > this.KeepLogsForDays)
                {
                    try
                    {
                        fi.Delete();
                    }
                    catch (Exception)
                    {
                        // causes problems when called from web app as logs are stored outside
                        //throw;
                    }

                }
            }
        }

        public void AddLogEntry(string text)
        {
            AddLogEntry(text, LogErrorLevel.Information);
        }

        public void AddLogEntry(string text, LogErrorLevel level)
        {
            bool addDateTime = !text.StartsWith(Environment.NewLine) && (!String.IsNullOrEmpty(text));
            string logEntry = GetFormattedLogText(text, addDateTime, level);

            if (this.EchoToConsole)
            {
                Console.ForegroundColor = GetForegroundColor(level);
                Console.WriteLine(logEntry);  //screen - only add the time               
                Console.ForegroundColor = this.mOriginalConsoleColor;
            }

            if (addDateTime)
            {
                // add the date to the file / email log entry
                logEntry = String.Format("{0} {1}", DateTime.Now.ToShortDateString(), logEntry);
            }
            WriteToLogFile(logEntry); // file
            mLatestLog.AppendLine(logEntry); // email            
        }

        public void LogError(Exception ex, string errorInfo)
        {
            string entry;
            if (String.IsNullOrEmpty(errorInfo))
            {
                entry = String.Format("{0}", ex == null ? String.Empty : ex.Message);
            }
            else
            {
                entry = String.Format("{0} {1}", errorInfo, ex == null ? String.Empty : ex.Message);
            }

            AddLogEntry(entry, LogErrorLevel.Error);
        }

        public string GetFormattedLogText(string text, bool addTime, LogErrorLevel level)
        {
            string outputText = String.Empty;

            if (!String.IsNullOrEmpty(text))
            {
                outputText = String.Format("{0}{1}", GetEntryPrefix(level), text);

                if (addTime)
                {
                    outputText = String.Format("{0} - {1}", DateTime.Now.ToShortTimeString(), outputText);
                }
            }

            return outputText;
        }

        private void WriteToLogFile(string text)
        {
            StreamWriter sw = this.LogFile.AppendText();
            sw.WriteLine(text);
            sw.Close();
        }

        public void NewLine()
        {
            AddLogEntry("");
        }

        public void SeparatorLine()
        {
            int width;
            try
            {
                width = Console.BufferWidth - 4;
            }
            catch (IOException)
            {
                // is there a better way to tell if console handle is not valid?
                width = 50;
            }
            string line = new string('-', width);

            AddLogEntry(String.Format("{0}  {1}  ", Environment.NewLine, line),
                LogErrorLevel.Information);
        }

        public void Close(bool success)
        {
            NewLine();
            AddLogEntry(success ? "Done - success!" : "Done - errors occurred (review log)");
        }

        private ConsoleColor GetForegroundColor(LogErrorLevel level)
        {
            switch (level)
            {
                case LogErrorLevel.Highlight:
                    return ConsoleColor.White;

                case LogErrorLevel.Warning:
                    return ConsoleColor.Yellow;

                case LogErrorLevel.Error:
                    return ConsoleColor.Red;

                case LogErrorLevel.Information:
                default:
                    return mOriginalConsoleColor;
            }
        }

        private string GetEntryPrefix(LogErrorLevel level)
        {
            switch (level)
            {
                case LogErrorLevel.Warning:
                case LogErrorLevel.Error:
                    return level.ToString().ToUpperInvariant() + ": ";

                case LogErrorLevel.Information:
                case LogErrorLevel.Highlight:
                default:
                    return String.Empty;
            }
        }
    }
}
