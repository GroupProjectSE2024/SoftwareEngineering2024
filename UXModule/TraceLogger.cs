// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UXModule;
public static class TraceLogger
{

        // Define the path for the log file
        private static readonly string s_logFilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Logs", "ApplicationTraceLog.txt");

        static TraceLogger()
        {
            // Ensure the Logs directory exists
            string logDirectory = Path.GetDirectoryName(s_logFilePath);
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // Add a text writer listener for tracing
            Trace.Listeners.Add(new TextWriterTraceListener(s_logFilePath));
            Trace.AutoFlush = true;

            // Initial log entry to confirm logger setup
            Trace.WriteLine($"[{DateTime.Now}] TraceLogger initialized. Logs are being written to {s_logFilePath}");
        }

        /// <summary>
        /// Method to retrieve the log file path.
        /// </summary>
        /// <returns>Path of the log file.</returns>
        public static string GetLogFilePath()
        {
            return s_logFilePath;
        }
    }

