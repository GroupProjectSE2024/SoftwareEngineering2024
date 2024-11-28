/******************************************************************************
 * Filename    = ShapeBase.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = WhiteBoard
 *
 * Description = ShapeBase implements IShape interface
 *****************************************************************************/

using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace WhiteboardGUI.Models;

/// <summary>
/// A base class that implements the IShape interface and provides common properties and methods for shapes.
/// </summary>
public abstract class ShapeBase : IShape
{
    private Guid _shapeId;
    private string _color = "#000000";
    private double _strokeThickness;
    private double _userID;
    private double _lastModifierID;
    private string _userName;
    private string _profilePictureURL;
    private string _lastModifiedBy;
    private bool _isSelected;
    private int _zIndex;
    private bool _isLocked;
    private string _boundingBoxColor;
    private double _lockedByUserID;
    

    /// <summary>
    /// Gets or sets a value indicating whether the shape is locked.
    /// </summary>
    public bool IsLocked
    {
        get => _isLocked;
        set {
            if (_isLocked != value)
            {
                _isLocked = value;
                OnPropertyChanged(nameof(IsLocked));
            }
        }
    }

    /// <summary>
    /// Gets or sets the user ID who locked the shape.
    /// </summary>
    public double LockedByUserID
    {
        get => _lockedByUserID;
        set {
            if (_lockedByUserID != value)
            {
                _lockedByUserID = value;
                OnPropertyChanged(nameof(LockedByUserID));
            }
        }
    }

    /// <summary>
    /// Gets or sets the color of the bounding box.
    /// </summary>
    public string BoundingBoxColor
    {
        get => _boundingBoxColor;
        set {
            if (_boundingBoxColor != value)
            {
                _boundingBoxColor = value;
                OnPropertyChanged(nameof(BoundingBoxColor));
            }
        }
    }

    /// <summary>
    /// Gets or sets the Z-index of the shape.
    /// </summary>
    public int ZIndex
    {
        get => _zIndex;
        set
        {
            if (_zIndex != value)
            {
                _zIndex = value;
                OnPropertyChanged(nameof(ZIndex));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the shape is selected.
    /// </summary>
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    /// <summary>
    /// Gets or sets the unique identifier for the shape.
    /// </summary>
    public Guid ShapeId
    {
        get => _shapeId;
        set { _shapeId = value; OnPropertyChanged(nameof(ShapeId)); }
    }

    /// <summary>
    /// Gets the type of the shape.
    /// </summary>
    public abstract string ShapeType { get; }

    /// <summary>
    /// Gets or sets the color of the shape.
    /// </summary>
    public string Color
    {
        get => _color;
        set { _color = value; OnPropertyChanged(nameof(Color)); }
    }

    /// <summary>
    /// Gets or sets the thickness of the shape's stroke.
    /// </summary>
    public double StrokeThickness
    {
        get => _strokeThickness;
        set { _strokeThickness = value; OnPropertyChanged(nameof(StrokeThickness)); }
    }

    /// <summary>
    /// Gets or sets the user ID associated with the shape.
    /// </summary>
    public double UserID
    {
        get => _userID;
        set { _userID = value; OnPropertyChanged(nameof(UserID)); }
    }
    public string UserName
    {
        get => _userName;
        set { _userName = value; OnPropertyChanged(nameof(UserName)); }
    }
    
    public string ProfilePictureURL
    {
        get => _profilePictureURL;
        set { _profilePictureURL = value; OnPropertyChanged(nameof(ProfilePictureURL)); }
    }

    /// <summary>
    /// Gets or sets the ID of the last user who modified the shape.
    /// </summary>
    public double LastModifierID
    {
        get => _lastModifierID;
        set { _lastModifierID = value; OnPropertyChanged(nameof(LastModifierID)); }
    }

    public string LastModifiedBy
    {
        get => _lastModifiedBy;
        set { _lastModifiedBy = value; OnPropertyChanged(nameof(LastModifiedBy)); }
    }
    /// <summary>
    /// Gets the bounding rectangle of the shape.
    /// </summary>
    /// <returns>A Rect representing the bounds.</returns>
    public abstract Rect GetBounds();

    /// <summary>
    /// Creates a clone of the shape.
    /// </summary>
    /// <returns>A new instance of IShape with copied properties.</returns>
    public abstract IShape Clone();

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    /// <summary>
    /// Raises the PropertyChanged event for the specified property.
    /// </summary>
    /// <param name="propertyName">Name of the property that changed.</param>
    public void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
