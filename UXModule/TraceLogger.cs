// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UXModule;
public static class TraceLogger
{

        private static readonly string s_logFilePath;

        static TraceLogger()
        {
            // Set the log directory to a common accessible location
            string commonLogDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments),
                "MyAppLogs");

            // Ensure the Logs directory exists
            if (!Directory.Exists(commonLogDirectory))
            {
                Directory.CreateDirectory(commonLogDirectory);
            }

            // Set the full path for the log file
            s_logFilePath = Path.Combine(commonLogDirectory, "TraceLog.txt");

            // Configure the trace listener
            Trace.Listeners.Add(new TextWriterTraceListener(s_logFilePath));
            Trace.AutoFlush = true;
        }

        public static void Log(string message,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0,
            bool isErrorMessage = false)
        {
            // Add a timestamp to the log message
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string logToBeWritten = $"{timestamp} | {Path.GetFileName(filePath)}->{memberName}->{lineNumber} :: {message}";

            if (isErrorMessage)
            {
                logToBeWritten = $"ERROR : {logToBeWritten}";
            }

            Trace.WriteLine(logToBeWritten);
        }

        public static string GetLogFilePath()
        {
            // Display the log file path in a message box
            MessageBox.Show("Log file location: " + s_logFilePath, "Log File Path", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return s_logFilePath;
        }
    }



