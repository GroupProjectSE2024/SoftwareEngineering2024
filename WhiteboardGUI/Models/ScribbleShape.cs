/******************************************************************************
 * Filename    = ScribbleShape.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = WhiteBoard
 *
 * Description = Implementing ShapeBase for Scribble shape
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace WhiteboardGUI.Models;

/// <summary>
/// Represents a scribble shape derived from ShapeBase.
/// </summary>
public class ScribbleShape : ShapeBase
{
    /// <summary>
    /// Gets the type of the shape.
    /// </summary>
    public override string ShapeType => "Scribble";

    private List<Point> _points = new List<Point>();

    /// <summary>
    /// Gets or sets the collection of points defining the scribble.
    /// </summary>
    public List<Point> Points
    {
        get => _points;
        set
        {
            _points = value;
            OnPropertyChanged(nameof(Points));
            OnPropertyChanged(nameof(PointCollection));
            OnPropertyChanged(nameof(RelativePoints));
            OnPropertyChanged(nameof(Left));
            OnPropertyChanged(nameof(Top));
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(DownLeftHandleY));
            OnPropertyChanged(nameof(TopRightHandleX));
        }
    }

    /// <summary>
    /// Gets the point collection for binding in XAML.
    /// </summary>
    public PointCollection PointCollection => new PointCollection(Points);

    /// <summary>
    /// Gets the relative point collection for binding in XAML.
    /// </summary>
    public PointCollection RelativePoints => new PointCollection(Points.Select(p => new Point(p.X - Left, p.Y - Top)));

    /// <summary>
    /// Gets the stroke brush for the scribble.
    /// </summary>
    public Brush Stroke => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));

    /// <summary>
    /// Adds a point to the scribble and updates dependent properties.
    /// </summary>
    /// <param name="point">The point to add.</param>
    public void AddPoint(Point point)
    {
        _points.Add(point);
        OnPropertyChanged(nameof(PointCollection));
        OnPropertyChanged(nameof(RelativePoints));
        OnPropertyChanged(nameof(Left));
        OnPropertyChanged(nameof(Top));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(DownLeftHandleY));
        OnPropertyChanged(nameof(TopRightHandleX));
    }

    /// <summary>
    ///  Properties for binding in XAML
    /// </summary>
    public double Left => GetBounds().Left;
    public double Top => GetBounds().Top;
    public double Width => GetBounds().Width;
    public double Height => GetBounds().Height;
    public double HandleSize => 8;
    public double TopRightHandleX => Left + Width - HandleSize;
    public double DownLeftHandleY => Top + Height - HandleSize;

    /// <summary>
    /// Returns the bounding rectangle of the scribble.
    /// </summary>
    /// <returns>A Rect representing the bounds.</returns>
    public override Rect GetBounds()
    {
        if (Points == null || Points.Count == 0)
        {
            return Rect.Empty;
        }

        double minX = Points.Min(p => p.X);
        double minY = Points.Min(p => p.Y);
        double maxX = Points.Max(p => p.X);
        double maxY = Points.Max(p => p.Y);

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    /// <summary>
    /// Creates a clone of the current scribble shape.
    /// </summary>
    /// <returns>A new instance of ScribbleShape with copied properties.</returns>
    public override IShape Clone()
    {
        return new ScribbleShape
        {
            ShapeId = this.ShapeId, // Assign a new unique ID
            UserID = this.UserID,
            Color = this.Color,
            StrokeThickness = this.StrokeThickness,
            LastModifierID = this.LastModifierID,
            IsSelected = false, // New shape should not be selected by default
            Points = new List<Point>(this.Points), // Create a deep copy of the points
            ZIndex = this.ZIndex,
            UserName = this.UserName,
            ProfilePictureURL = this.ProfilePictureURL,
            LastModifiedBy = this.LastModifiedBy,
            BoundingBoxColor = this.BoundingBoxColor
           
        };
    }
}
