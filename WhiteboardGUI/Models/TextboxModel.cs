/******************************************************************************
 * Filename    = TextBoxModel.cs
 *
 * Author      = Yash Mittal
 *
 * Project     = WhiteBoard
 *
 * Description = TextBoxModel implements ShapeBase class
 *****************************************************************************/
using System;
using System.Windows;
using System.Windows.Media;

namespace WhiteboardGUI.Models;

/// <summary>
/// Represents a textbox shape derived from ShapeBase.
/// </summary>
public class TextboxModel : ShapeBase
{

    public override string ShapeType => "TextboxModel";

    private string _text;
    private double _width;
    private double _height;
    private double _x;
    private double _y;
    private double _fontSize;

    /// <summary>
    /// Gets or sets the text content of the textbox.
    /// </summary>
    public string Text
    {
        get => _text;
        set { _text = value; OnPropertyChanged(nameof(Text)); }
    }      
    public double Width
    {
        get => _width;
        set { _width = value; OnPropertyChanged(nameof(Width)); }
    }

    public double Height
    {
        get => _height;
        set { _height = value; OnPropertyChanged(nameof(Height)); }
    }

    public double X
    {
        get => _x;
        set { _x = value; OnPropertyChanged(nameof(X)); OnPropertyChanged(nameof(Left)); }
    }

    public double Y
    {
        get => _y;
        set { _y = value; OnPropertyChanged(nameof(Y)); OnPropertyChanged(nameof(Top)); }
    }

    /// <summary>
    /// Gets or sets the font size of the textbox text.
    /// </summary>
    public double FontSize
    {
        get => _fontSize;
        set { _fontSize = value; OnPropertyChanged(nameof(FontSize)); }
    }


    /// <summary>
    /// Properties for binding in XAML
    /// </summary>

    public double Left => X;

    public double Top => Y;

    public Brush Background => new SolidColorBrush(Colors.LightGray);
    public Brush BorderBrush => new SolidColorBrush(Colors.Blue);
    public Brush Foreground => new SolidColorBrush((Color)ColorConverter.ConvertFromString(Color));

    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
    }

    /// <summary>
    /// Returns the bounding rectangle of the textbox.
    /// </summary>
    /// <returns>A Rect representing the bounds.</returns>
    public override Rect GetBounds()
    {
        return new Rect(Left, Top, Width, Height);
    }

    /// <summary>
    /// Creates a clone of the current textbox model.
    /// </summary>
    /// <returns>A new instance of TextboxModel with copied properties.</returns>
    public override IShape Clone()
    {
        return new TextboxModel
        {
            ShapeId = this.ShapeId, // Assign a new unique ID
            UserID = this.UserID,
            Color = this.Color,
            StrokeThickness = this.StrokeThickness,
            LastModifierID = this.LastModifierID,
            IsSelected = false, // New shape should not be selected by default
            Text = this.Text,
            Width = this.Width,
            Height = this.Height,
            X = this.X,
            Y = this.Y,
            FontSize = this.FontSize,
            ZIndex = this.ZIndex,
            UserName = this.UserName,
            ProfilePictureURL = this.ProfilePictureURL,
            LastModifiedBy = this.LastModifiedBy,
            BoundingBoxColor = this.BoundingBoxColor
           
        };
    }
}
