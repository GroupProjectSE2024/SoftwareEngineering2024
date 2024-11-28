/******************************************************************************
 * Filename    = LineShape.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = WhiteBoard
 *
 * Description = Implementing ShapeBase for Line shape
 *****************************************************************************/
using System;
using System.Windows;
using System.Windows.Media;

namespace WhiteboardGUI.Models;

/// <summary>
/// Represents a line shape derived from ShapeBase.
/// </summary>
public class LineShape : ShapeBase
{
    /// <summary>
    /// Gets the type of the shape.
    /// </summary>
    public override string ShapeType => "Line";

    private double _startX;
    private double _startY;
    private double _endX;
    private double _endY;

    /// <summary>
    /// Gets or sets the starting X-coordinate of the line.
    /// </summary>
    public double StartX
    {
        get => _startX;
        set
        {
            if (_startX != value)
            {
                _startX = value;
                OnPropertyChanged(nameof(StartX));
                OnCoordinateChanged();
                OnPropertyChanged(nameof(Left));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(RelativeStartX));
                // When StartX changes, RelativeEndX might change if Left changes
                OnPropertyChanged(nameof(RelativeEndX));
                OnPropertyChanged(nameof(Bottomleft));
            }
        }
    }

    /// <summary>
    /// Gets or sets the starting Y-coordinate of the line.
    /// </summary>
    public double StartY
    {
        get => _startY;
        set
        {
            if (_startY != value)
            {
                _startY = value;
                OnPropertyChanged(nameof(StartY));
                OnCoordinateChanged();
                OnPropertyChanged(nameof(Top));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(RelativeStartY));
                OnPropertyChanged(nameof(RelativeEndY));
                OnPropertyChanged(nameof(Bottomleft));
            }
        }
    }

    /// <summary>
    /// Gets or sets the ending X-coordinate of the line.
    /// </summary>
    public double EndX
    {
        get => _endX;
        set
        {
            if (_endX != value)
            {
                _endX = value;
                OnPropertyChanged(nameof(EndX));
                OnCoordinateChanged();
                OnPropertyChanged(nameof(Left));
                OnPropertyChanged(nameof(Width));
                OnPropertyChanged(nameof(RelativeEndX));
                OnPropertyChanged(nameof(RelativeStartX));
                OnPropertyChanged(nameof(Bottomleft));
            }
        }
    }

    /// <summary>
    /// Gets or sets the ending Y-coordinate of the line.
    /// </summary>
    public double EndY
    {
        get => _endY;
        set
        {
            if (_endY != value)
            {
                _endY = value;
                OnPropertyChanged(nameof(EndY));
                OnCoordinateChanged();
                OnPropertyChanged(nameof(Top));
                OnPropertyChanged(nameof(Height));
                OnPropertyChanged(nameof(RelativeEndY));
                OnPropertyChanged(nameof(RelativeStartY));
                OnPropertyChanged(nameof(Bottomleft));
            }
        }
    }

    /// <summary>
    /// Handles changes to the coordinates by updating dependent properties.
    /// </summary>
    private void OnCoordinateChanged()
    {
        OnPropertyChanged(nameof(MidX));
        OnPropertyChanged(nameof(MidY));
        OnPropertyChanged(nameof(Left));
        OnPropertyChanged(nameof(Top));
        OnPropertyChanged(nameof(Width));
        OnPropertyChanged(nameof(Height));
        OnPropertyChanged(nameof(StartHandleX));
        OnPropertyChanged(nameof(StartHandleY));
        OnPropertyChanged(nameof(EndHandleX));
        OnPropertyChanged(nameof(EndHandleY));
    }

    public double MidX => (StartX + EndX) / 2;
    public double MidY => (StartY + EndY) / 2;
    public Brush Stroke => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));
    public double Left => Math.Min(StartX, EndX) - HandleSize / 2;
    public double Top => Math.Min(StartY, EndY) - HandleSize / 2;
    public double Width => Math.Abs(EndX - StartX) + HandleSize;
    public double Height => Math.Abs(EndY - StartY) + HandleSize;
    public double HandleSize => 8;
   
    /// <summary>
    /// Properties for handle positions
    /// </summary>
    public double StartHandleX => StartX - HandleSize / 2;
    public double StartHandleY => StartY - HandleSize / 2;
    public double EndHandleX => EndX - HandleSize / 2;
    public double EndHandleY => EndY - HandleSize / 2;
    public double RelativeStartX => StartX - Left;      
    public double RelativeStartY => StartY - Top - Height;

    /// <summary>
    /// Gets the relative X-coordinate of the end point.
    /// </summary>
    public double RelativeEndX => EndX - Left;

    /// <summary>
    /// Gets the relative Y-coordinate of the end point.
    /// </summary>
    public double RelativeEndY => EndY - Top - Height;

    /// <summary>
    /// Gets the bottom-left position of the line.
    /// </summary>
    public double Bottomleft => Top + Height;

    /// <summary>
    /// Returns the bounding rectangle of the line.
    /// </summary>
    /// <returns>A Rect representing the bounds.</returns>
    public override Rect GetBounds()
    {
        // Return the axis-aligned bounding box of the line
        return new Rect(Left, Top, Width, Height);
    }

    /// <summary>
    /// Creates a clone of the current line shape.
    /// </summary>
    /// <returns>A new instance of LineShape with copied properties.</returns>
    public override IShape Clone()
    {
        return new LineShape
        {
            ShapeId = this.ShapeId,
            UserID = this.UserID,
            Color = this.Color,
            StrokeThickness = this.StrokeThickness,
            LastModifierID = this.LastModifierID,
            IsSelected = false,
            StartX = this.StartX,
            StartY = this.StartY,
            EndX = this.EndX,
            EndY = this.EndY,
            ZIndex = this.ZIndex,
            UserName = this.UserName,
            ProfilePictureURL = this.ProfilePictureURL,
            LastModifiedBy = this.LastModifiedBy,
            BoundingBoxColor = this.BoundingBoxColor
            
        };
    }
}
