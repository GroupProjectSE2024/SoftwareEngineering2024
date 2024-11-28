/******************************************************************************
* Filename    = Utils.cs
*
* Author      = Amithabh A and Garima Ranjan
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Utility class for common functions
*****************************************************************************/

using System.Diagnostics;
using Networking.Serialization;

namespace Updater;

public class Utils
{

    /// <summary>
    /// Reads the content of the specified file.
    /// </summary>
    /// <param name="filePath">Path of file to read. </param>
    /// <returns>Filecontent as string, or null if file dne</returns>
    public static string? ReadBinaryFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Trace.WriteLine("File not found. Please check the path and try again.");
            return null;
        }

        // Read all bytes from the file
        byte[] byteArray = File.ReadAllBytes(filePath);

        // Convert byte array to a base64 string
        return Convert.ToBase64String(byteArray);
    }


    /// <summary>
    /// Write/Overwrite content to file
    /// </summary>
    /// <param name="filePath">Path of file</param>
    /// <param name="content">Content to write.</param>
    public static bool WriteToFileFromBinary(string filePath, string content)
    {
        try
        {
            byte[] data;

            // Check if the content is in base64 format by attempting to decode it
            try
            {
                data = Convert.FromBase64String(content);
            }
            catch (FormatException)
            {
                // If it's not base64, write as a regular string
                File.WriteAllText(filePath, content);
                return true;
            }

            // If decoding to byte array is successful, write as binary
            File.WriteAllBytes(filePath, data);
            return true;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"An error occurred while writing to the file: {ex.Message}");
            return false;
        }
    }

    /// <summary> 
    /// Serializes an object to its string representation.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A string representation of the serialized object.</returns>
    public static string? SerializeObject<T>(T obj)
    {
        try
        {
            ISerializer serializer = new Serializer();
            return serializer.Serialize(obj);
        }
        catch (Exception ex)
        {
            Trace.WriteLine(ex.ToString());
            return null;
        }
    }

    /// <summary>
    /// Deserializes a string back to an object of specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize into.</typeparam>
    /// <param name="serializedData">The serialized string data.</param>
    /// <returns>An instance of the specified type.</returns>
    public static T DeserializeObject<T>(string serializedData)
    {
        ISerializer serializer = new Serializer();
        return serializer.Deserialize<T>(serializedData);
    }

    /// <summary>
    /// Generates serialized packet containing metadata of files in a directory.
    /// </summary>
    /// <returns>Serialized packet containing metadata of files in a directory.</returns>
    public static string? SerializedMetadataPacket(string? folderPath = null)
    {
        folderPath ??= AppConstants.ToolsDirectory;

        DirectoryMetadataGenerator metadataGenerator = new DirectoryMetadataGenerator(folderPath);

        if (metadataGenerator == null)
        {
            Trace.WriteLine("Failed to create DirectoryMetadataGenerator");
            return null;
        }

        List<FileMetadata>? metadata = metadataGenerator.GetMetadata();
        if (metadata == null)
        {
            Trace.WriteLine("Failed to get metadata");
            return null;
        }

        string? serializedMetadata = Utils.SerializeObject(metadata);
        if (serializedMetadata == null)
        {
            Trace.WriteLine("Failed to serialize metadata");
            return null;
        }
        FileContent fileContent = new FileContent("metadata.json", serializedMetadata);
        List<FileContent> fileContents = [fileContent];

        DataPacket dataPacket = new DataPacket(DataPacket.PacketType.Metadata, fileContents);
        return SerializeObject(dataPacket);
    }

    /// <summary>
    /// Generates serialized SyncUp packet
    /// </summary>
    /// <returns>Serialized SyncUp packet</returns>
    public static string? SerializedSyncUpPacket(string clientId)
    {
        List<FileContent> fileContents =
        [
            new FileContent(clientId, clientId)
        ];
        DataPacket dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, fileContents);
        if (dataPacket == null)
        {
            Trace.WriteLine("Failed to create DataPacket");
            return null;
        }
        return SerializeObject(dataPacket);
    }
}
