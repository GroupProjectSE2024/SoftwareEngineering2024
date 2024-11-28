/******************************************************************************
* Filename    = DirectoryMetadataComparer.cs
*
* Author      = Garima Ranjan and Amithabh A
*
* Product     = Updater
* 
* Project     = Lab Monitoring Software
*
* Description = Compares metadata of two directories and generates differences.
*****************************************************************************/

using System.Xml.Serialization;

namespace Updater;

[Serializable]
[XmlRoot("ArrayOfMetadataDifference")]
public class DirectoryMetadataComparer
{
    [XmlElement("MetadataDifference")]
    public List<MetadataDifference> Differences { get; private set; } = [];

    // Properties for storing unique files in server and client directories
    [XmlElement("UniqueServerFiles")]
    public List<string> UniqueServerFiles { get; private set; } = [];
    [XmlElement("UniqueClientFiles")]

    public List<string> UniqueClientFiles { get; private set; } = [];
    public List<string>? InvalidSyncUpFiles { get; private set; } = [];

    // Parameterless constructor for XML serialization
    public DirectoryMetadataComparer() { }

    /// <summary>
    /// Initialize new instance. 
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="metadataB">Dir. B's metadata</param>
    public DirectoryMetadataComparer(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
    {
        CompareMetadata(metadataA, metadataB);
    }

    /// <summary>
    /// Compares and generates differences between a directory's metadata pairs.
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="metadataB">Dir. B's metadata</param>
    private void CompareMetadata(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
    {
        // Initialize differences for three cases
        Differences.Add(new MetadataDifference { Key = "-1", Value = [] }); // In B but not in A
        Differences.Add(new MetadataDifference { Key = "0", Value = [] });  // Files with same hash but different names
        Differences.Add(new MetadataDifference { Key = "1", Value = [] });    // In A but not in B

        List<KeyValuePair<string, string>> hashToFileA = CreateHashToFileDictionary(metadataA);
        List<KeyValuePair<string, string>> hashToFileB = CreateHashToFileDictionary(metadataB);

        CheckForRenamesAndMissingFiles(metadataB, hashToFileA);
        CheckForOnlyInAFiles(metadataA, hashToFileB);
        CheckForSameNameDifferentHash(metadataA, metadataB);
    }

    /// <summary>
    /// Create a map from file hash to filename.
    /// </summary>
    /// <param name="metadata">List of metadata.</param>
    /// <returns>List of key-value pairs containing the mapping.</returns>
    private static List<KeyValuePair<string, string>> CreateHashToFileDictionary(List<FileMetadata> metadata)
    {
        return metadata
    .Where(file => file.FileHash != null && file.FileName != null) // Filter out nulls
    .Select(file => new KeyValuePair<string, string>(
        file.FileHash ?? string.Empty,
        file.FileName ?? string.Empty))
    .ToList();
    }

    /// <summary>
    /// Checks for files in directory B that have been renamed or are missing in directory A.
    /// </summary>
    /// <param name="metadataB">Dir. B's metadata.</param>
    /// <param name="hashToFileA">List of key-value pairs mapping hash to filename in A.</param>
    private void CheckForRenamesAndMissingFiles(List<FileMetadata> metadataB, List<KeyValuePair<string, string>> hashToFileA)
    {
        foreach (FileMetadata fileB in metadataB)
        {
            KeyValuePair<string, string> existingFile = hashToFileA.FirstOrDefault(kvp => kvp.Key == fileB.FileHash);
            if (existingFile.Key != null)
            {
                if (existingFile.Value != fileB.FileName)
                {
                    Differences.First(d => d.Key == "0").Value.Add(new FileDetail {
                        RenameFrom = fileB.FileName,
                        RenameTo = existingFile.Value,
                        FileHash = fileB.FileHash
                    });
                }
            }
            else
            {
                Differences.First(d => d.Key == "-1").Value.Add(new FileDetail {
                    FileName = fileB.FileName,
                    FileHash = fileB.FileHash
                });
                if (!string.IsNullOrEmpty(fileB.FileName))
                {
                    UniqueClientFiles.Add(fileB.FileName);
                }
            }
        }
    }

    /// <summary>
    /// Checks for files in directory A that are missing in directory B.
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="hashToFileB">List of key-value pairs mapping hash to filename in B.</param>
    private void CheckForOnlyInAFiles(List<FileMetadata> metadataA, List<KeyValuePair<string, string>> hashToFileB)
    {
        foreach (FileMetadata fileA in metadataA)
        {
            if (!hashToFileB.Any(kvp => kvp.Key == fileA.FileHash))
            {
                Differences.First(d => d.Key == "1").Value.Add(new FileDetail {
                    FileName = fileA.FileName,
                    FileHash = fileA.FileHash
                });
                if (!string.IsNullOrEmpty(fileA.FileName))
                {
                    UniqueServerFiles.Add(fileA.FileName);
                }
            }
        }
    }

    /// <summary>
    /// Checks for files in directory A and B that have the same name but different hashes.
    /// </summary>
    /// <param name="metadataA">Dir. A's metadata</param>
    /// <param name="metadataB">Dir. B's metadata</param>
    private void CheckForSameNameDifferentHash(List<FileMetadata> metadataA, List<FileMetadata> metadataB)
    {
        // Iterate over each file in directory A
        foreach (FileMetadata fileA in metadataA)
        {
            // Find a file in directory B that has the same name as fileA
            FileMetadata? fileB = metadataB.FirstOrDefault(fb => fb.FileName == fileA.FileName);

            if (fileB != null && fileA.FileHash != fileB.FileHash)
            {
                // Found files with the same name but different hashes
                if (!string.IsNullOrEmpty(fileA.FileName))
                {
                    InvalidSyncUpFiles?.Add(fileA.FileName);
                }
            }
        }
    }

    /// <summary>
    /// Validates whether directories can be synced.
    /// </summary>
    public bool ValidateSync()
    {
        // If there are any filenames in InvalidSyncUpFiles, directories can't be synced. 
        return InvalidSyncUpFiles?.Count == 0;
    }
}

[Serializable]
public class MetadataDifference
{
    [XmlElement("Key")]
    public string? Key { get; set; }

    [XmlArray("Value")]
    [XmlArrayItem("FileDetail")] // Define individual item
    public List<FileDetail> Value { get; set; } = [];
}

[Serializable]
public class FileDetail
{
    [XmlElement("FileName")]
    public string? FileName { get; set; }

    [XmlElement("FileHash")]
    public string? FileHash { get; set; }

    [XmlElement("RenameFrom")]
    public string? RenameFrom { get; set; }

    [XmlElement("RenameTo")]
    public string? RenameTo { get; set; }
}
