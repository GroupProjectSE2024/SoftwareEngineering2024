/******************************************************************************
 * Filename    = Constants.cs
 *
 * Author      = Sai Hemanth Reddy
 *
 * Product     = PlexShare
 * 
 * Project     = FileCloner
 *
 * Description = Defines project-level constants for icon paths, file paths
 *****************************************************************************/

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace FileCloner.Models;

/// <summary>
/// A class in which all project-level constants are declared as properties.
/// </summary>
public class Constants
{
    public static readonly string BasePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "FileCloner"
    );

    // Icon Paths
    public static readonly string LoadingIconPath = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "FileCloner", "Assets", "Images", "loading.png"));
    public static readonly string FileIconPath = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "FileCloner", "Assets", "Images", "file.png"));
    public static readonly string FolderIconPath = Path.GetFullPath(Path.Combine("..", "..", "..", "..", "FileCloner", "Assets", "Images", "folder.png"));


    // File & Folder Paths
    public static readonly string DefaultFolderPath = Path.Combine(BasePath, "Temp");
    public static readonly string InputFilePath = Path.Combine(BasePath, "input.json");
    public static readonly string OutputFilePath = Path.Combine(BasePath, "output.json");
    public static readonly string ReceivedFilesFolderPath = Path.Combine(BasePath, "ReceivedFiles");
    public static readonly string SenderFilesFolderPath = Path.Combine(BasePath, "SenderFiles");

    // Network Service Constants
    public const string Success = "success";
    public const string ModuleName = "FileCloner";
    public const string Request = "request";
    public const string Response = "response";
    public const string Summary = "summary";
    public const string Cloning = "cloning";
    public const string Broadcast = "BroadCast";
    public static string IPAddress = GetIP();
    public static string UserName = Singleton.Instance.UserName;

    // Size of FileChunk to be sent over network
    public const int FileChunkSize = 13 * 1024 * 1024;
    public const int ChunkStartIndex = 1;


    private static string GetIP()
    {
        try
        {
            // Get the IP address of the machine
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            // Iterate through the IP addresses and return the IPv4 address that does not end with "1"
            foreach (IPAddress ipAddress in host.AddressList)
            {
                if (ipAddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    string address = ipAddress.ToString();
                    if (!address.EndsWith(".1"))
                    {
                        return address;
                    }
                }
            }
            return "";
        }
        catch
        {
            return "";
        }
    }
}
