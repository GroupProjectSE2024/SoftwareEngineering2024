/******************************************************************************
 * Filename    = CircleShape.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = WhiteBoard
 *
 * Description = Implementing ShapeBase for Circle shape
 *****************************************************************************/

using System;
using System.Windows;
using System.Windows.Media;

namespace WhiteboardGUI.Models;

/// <summary>
/// Represents a circle shape derived from ShapeBase.
/// </summary>
public class CircleShape : ShapeBase
{
    /// <summary>
    /// Gets the type of the shape.
    /// </summary>
    public override string ShapeType => "Circle";

    private double _centerX;
    private double _centerY;
    private double _radiusX;
    private double _radiusY;

    /// <summary>
    /// Gets or sets the X-coordinate of the circle's center.
    /// </summary>
    public double CenterX
    {
        get => _centerX;
        set
        {
            _centerX = value;
            OnPropertyChanged(nameof(CenterX));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(TopRightHandleX));
        }
    }

    /// <summary>
    /// Gets or sets the Y-coordinate of the circle's center.
    /// </summary>
    public double CenterY
    {
        get => _centerY;
        set
        {
            _centerY = value;
            OnPropertyChanged(nameof(CenterY));
            OnPropertyChanged(nameof(Top));
            OnPropertyChanged(nameof(DownLeftHandleY));
        }
    }

    /// <summary>
    /// Gets or sets the X-radius of the circle.
    /// </summary>
    public double RadiusX
    {
        get => _radiusX;
        set
        {
            _radiusX = value;
            OnPropertyChanged(nameof(RadiusX));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(TopRightHandleX));
        }
    }

    /// <summary>
    /// Gets or sets the Y-radius of the circle.
    /// </summary>
    public double RadiusY
    {
        get => _radiusY;
        set
        {
            _radiusY = value;
            OnPropertyChanged(nameof(RadiusY));
            OnPropertyChanged(nameof(Top));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(DownLeftHandleY));
        }
    }

    // Corrected properties for binding in XAML

    public double Left => CenterX - RadiusX;
    public double Top => CenterY - RadiusY;
    public double Width => 2 * RadiusX;
    public double Height => 2 * RadiusY;
    public double HandleSize => 8;

    public double TopRightHandleX => Left + Width - HandleSize;
    public double DownLeftHandleY => Top + Height - HandleSize;
    public Brush Stroke => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));

    /// <summary>
    /// Returns the bounding rectangle of the circle.
    /// </summary>
    /// <returns>A Rect representing the bounds.</returns>
    public override Rect GetBounds()
    {
        return new Rect(Left, Top, Width, Height);
    }

    /// <summary>
    /// Creates a clone of the current circle shape.
    /// </summary>
    /// <returns>A new instance of CircleShape with copied properties.</returns>
    public override IShape Clone()
    {
        return new CircleShape
        {
            ShapeId = this.ShapeId, // Assign a new unique ID
            UserID = this.UserID,
            Color = this.Color,
            StrokeThickness = this.StrokeThickness,
            LastModifierID = this.LastModifierID,
            IsSelected = false, // New shape should not be selected by default
            CenterX = this.CenterX,
            CenterY = this.CenterY,
            RadiusX = this.RadiusX,
            RadiusY = this.RadiusY,
            ZIndex = this.ZIndex,
            UserName = this.UserName,
            ProfilePictureURL = this.ProfilePictureURL,
            LastModifiedBy = this.LastModifiedBy,
            BoundingBoxColor = this.BoundingBoxColor
            
        };
    }
}
