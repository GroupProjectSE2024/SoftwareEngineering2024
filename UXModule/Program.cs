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
class Program
{
    static void TMain(string[] args)
    {
        // Create a Logs directory if it doesn't exist
        string logsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
        Directory.CreateDirectory(logsDir);

        // Set up the log file path
        string logFilePath = Path.Combine(logsDir, "application.log");

        // Add the custom trace listener
        Trace.Listeners.Add(new FileTraceListener(logFilePath));
        Trace.AutoFlush = true; // Ensure logs are written immediately

        // Example usage of Trace
        Trace.WriteLine("Application has started.");
        Trace.WriteLine("Performing some operations...");

        // Simulate application work
        Console.WriteLine("Hello, world!");

        // Log application end
        Trace.WriteLine("Application is shutting down.");
    }
}

