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
public class FileTraceListener : TraceListener
{
    private readonly string _logFilePath;

    public FileTraceListener(string filePath)
    {
        _logFilePath = filePath;
    }

    public override void Write(string message)
    {
        WriteMessage(message);
    }

    public override void WriteLine(string message)
    {
        WriteMessage(message + Environment.NewLine);
    }

    private void WriteMessage(string message)
    {
        try
        {
            File.AppendAllText(_logFilePath, $"{DateTime.Now}: {message}");
        }
        catch (Exception ex)
        {
            // Handle logging errors gracefully
            Console.WriteLine($"Failed to write log: {ex.Message}");
        }
    }
}

