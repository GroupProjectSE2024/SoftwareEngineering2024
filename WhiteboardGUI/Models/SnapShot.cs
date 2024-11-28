using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteboardGUI.Services;

namespace WhiteboardGUI.Models;

/// <summary>
/// Represents an item available for snapshot download.
/// </summary>
public class SnapShotDownloadItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SnapShotDownloadItem"/> class.
    /// </summary>
    /// <param name="snapShotFileName">The file name of the snapshot.</param>
    /// <param name="dateTime">The timestamp of the snapshot.</param>
    public SnapShotDownloadItem(string snapShotFileName, DateTime dateTime)
    {
        FileName = snapShotFileName;
        Time = dateTime;
    }

    /// <summary>
    /// Gets or sets the file name of the snapshot.
    /// </summary>
    public string FileName { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the snapshot.
    /// </summary>
    public DateTime Time { get; set; }
}

/// <summary>
/// Represents a snapshot object with user and shape data.
/// </summary>
public class SnapShot
{
    /// <summary>
    /// The user ID associated with the snapshot.
    /// </summary>
    public string _userID;

    /// <summary>
    /// The file name of the snapshot.
    /// </summary>
    public string _fileName;

    /// <summary>
    /// The date and time when the snapshot was created.
    /// </summary>
    public DateTime _dateTime;

    /// <summary>
    /// A collection of shapes in the snapshot.
    /// </summary>
    [JsonConverter(typeof(ShapeConverter))]
    public ObservableCollection<IShape> _shapes;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapShot"/> class.
    /// </summary>
    public SnapShot() { }
}
