/******************************************************************************
* Filename    = AppConstants.cs
*
* Author      = Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = This class contains constant values used for configuration within the Updater product
*****************************************************************************/

namespace Updater;

/// <summary>
/// This class contains constant values and utility methods used across the 
/// Updater product to handle directory paths, server configuration, etc.
/// </summary>
public class AppConstants
{
    public static readonly string ToolsDirectory = Path.Combine(GetSystemDirectory(), "Updater");

    /// <summary>
    /// Retrieves the system's application data directory.
    /// This is used to get a system-wide location for storing application data.
    /// </summary>
    /// <returns>
    /// The path to the system's common application data directory.
    /// </returns>
    private static string GetSystemDirectory()
    {
        // Use the system's application data folder based on the OS
        return Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
    }
}
