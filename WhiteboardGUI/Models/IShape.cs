/******************************************************************************
 * Filename    = Ishape.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = Whiteboard
 *
 * Description = Implementing the IShape Interface
 *****************************************************************************/
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;

namespace WhiteboardGUI.Models;

/// <summary>
/// Defines the contract for shape objects.
/// </summary>
public interface IShape : INotifyPropertyChanged
{
    /// <summary>
    /// Gets or sets the unique identifier for the shape.
    /// </summary>
    Guid ShapeId { get; set; }

    /// <summary>
    /// Gets the type of the shape.
    /// </summary>
    string ShapeType { get; }
    string Color { get; set; }
    double StrokeThickness { get; set; }
    double UserID { get; set; }
    double LastModifierID { get; set; }
    string UserName { get; set; }
    string LastModifiedBy { get; set; }

    
    string ProfilePictureURL { get; set; }

    /// <summary>
    /// Gets or sets the Z-index of the shape.
    /// </summary>
    int ZIndex { get; set; }
    bool IsSelected { get; set; }
    bool IsLocked { get; set; }
    double LockedByUserID { get; set; }
    string BoundingBoxColor { get; set; }

    /// <summary>
    /// Gets the bounding rectangle of the shape.
    /// </summary>
    /// <returns>A Rect representing the bounds.</returns>
    Rect GetBounds();

    /// <summary>
    /// Creates a clone of the shape.
    /// </summary>
    /// <returns>A new instance of IShape with copied properties.</returns>
    IShape Clone();
}
