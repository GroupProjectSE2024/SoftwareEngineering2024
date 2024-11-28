using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using WhiteboardGUI.Models;

namespace WhiteboardGUI.Services;

/// <summary>
/// Manages Z-indexing of shapes in an ObservableCollection, allowing for reordering based on overlap and user actions.
/// </summary>
public class MoveShapeZIndexing
{
    /// <summary>
    /// Collection of shapes managed for Z-indexing.
    /// </summary>
    private ObservableCollection<IShape> _shapes;

    /// <summary>
    /// Initializes a new instance of the <see cref="MoveShapeZIndexing"/> class.
    /// </summary>
    /// <param name="shapes">The collection of shapes to manage.</param>
    public MoveShapeZIndexing(ObservableCollection<IShape> shapes)
    {
        _shapes = shapes;
    }

    /// <summary>
    /// Moves the specified shape to the back of the collection, giving it the lowest Z-index.
    /// </summary>
    /// <param name="shape">The shape to move.</param>
    public void MoveShapeBack(IShape shape)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (shape == null || !(bool)_shapes.Any(s => (s as dynamic).ShapeId.Equals(shape.ShapeId)))
            {
                return;
            }

            _shapes.Remove(_shapes.FirstOrDefault(s => (s as dynamic).ShapeId == shape.ShapeId));
            _shapes.Insert(0, shape);
            UpdateZIndices();
        });
    }

    /// <summary>
    /// Updates the Z-index of all shapes in the collection to reflect their position.
    /// </summary>
    public void UpdateZIndices()
    {
        for (int i = 0; i < _shapes.Count; i++)
        {
            _shapes[i].ZIndex = i;
        }
    }

    /// <summary>
    /// Moves the specified shape one step backward in the Z-index, if possible, and swaps it with an overlapping shape.
    /// </summary>
    /// <param name="shape">The shape to move.</param>
    public void MoveShapeBackward(IShape shape)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (shape == null || !(bool)_shapes.Any(s => (s as dynamic).ShapeId.Equals(shape.ShapeId)))
            {
                return;
            }

            int currentIndex = _shapes.Select((s, index) => new { s, index })
                             .FirstOrDefault(item => (item.s as dynamic).ShapeId.Equals(shape.ShapeId))?.index ?? -1;
            if (currentIndex <= 0)
            {
                return; // Already at the bottom
            }

            // Iterate backward to find the first overlapping shape based on stroke
            for (int i = currentIndex - 1; i >= 0; i--)
            {
                IShape otherShape = _shapes[i];
                if (AreShapesOverlapping(shape, otherShape))
                {
                    // Swap shape with otherShape
                    _shapes.Move(currentIndex, i);
                    UpdateZIndices();
                    break;
                }
            }
        });
    }

    /// <summary>
    /// Determines if the strokes of two shapes overlap.
    /// </summary>
    /// <param name="shape1">The first shape.</param>
    /// <param name="shape2">The second shape.</param>
    /// <returns><c>true</c> if the shapes overlap; otherwise, <c>false</c>.</returns>
    private bool AreShapesOverlapping(IShape shape1, IShape shape2)
    {
        Geometry strokeGeometry1 = GetShapeStrokeGeometry(shape1);
        Geometry strokeGeometry2 = GetShapeStrokeGeometry(shape2);

        if (strokeGeometry1 == null || strokeGeometry2 == null)
        {
            return false;
        }

        // Combine the two stroke geometries to find their intersection
        // If the intersection is not empty, then the strokes overlap
        PathGeometry combinedGeometry = Geometry.Combine(
            strokeGeometry1,
            strokeGeometry2,
            GeometryCombineMode.Intersect,
            null);

        return !combinedGeometry.IsEmpty();
    }

    /// <summary>
    /// Generates the stroke geometry of a shape based on its type, stroke color, and thickness.
    /// </summary>
    /// <param name="shape">The shape to process.</param>
    /// <returns>A <see cref="Geometry"/> object representing the stroke area, or <c>null</c> if unsupported.</returns>
    private Geometry GetShapeStrokeGeometry(IShape shape)
    {
        if (shape == null)
        {
            return null;
        }

        Geometry geometry = null;

        switch (shape)
        {
            case CircleShape circle:
                {
                    double centerX = circle.Left + circle.Width / 2;
                    double centerY = circle.Top + circle.Height / 2;
                    double radiusX = circle.Width / 2;
                    double radiusY = circle.Height / 2;
                    geometry = new EllipseGeometry(new Point(centerX, centerY), radiusX, radiusY);
                    break;
                }

            case LineShape line:
                {
                    geometry = new LineGeometry(new Point(line.StartX, line.StartY), new Point(line.EndX, line.EndY));
                    break;
                }

            case ScribbleShape scribble:
                {
                    if (scribble.PointCollection == null || !scribble.PointCollection.Any())
                    {
                        return null;
                    }
                    StreamGeometry streamGeometry = new StreamGeometry();
                    using (StreamGeometryContext ctx = streamGeometry.Open())
                    {
                        ctx.BeginFigure(scribble.PointCollection.First(), false, false);
                        ctx.PolyLineTo(scribble.PointCollection.Skip(1).ToList(), true, true);
                    }
                    streamGeometry.Freeze(); // Makes the geometry immutable
                    geometry = streamGeometry;
                    break;
                }

            default:
                {
                    // Unsupported shape type
                    return null;
                }
        }

        if (geometry != null)
        {
            try
            {
                // Create a Pen with the shape's stroke color and thickness
                Color color = (Color)ColorConverter.ConvertFromString(shape.Color.ToString());
                Pen pen = new Pen(new SolidColorBrush(color), shape.StrokeThickness);
                pen.Freeze(); // Makes the pen immutable and improves performance

                // Use GetWidenedPathGeometry to create a geometry that represents the area covered by the pen's stroke
                PathGeometry strokeGeometry = geometry.GetWidenedPathGeometry(pen);
                strokeGeometry.Freeze(); // Optional: Freeze for performance if the geometry won't change

                return strokeGeometry;
            }
            catch (Exception ex)
            {
                // Handle any exceptions that might occur during geometry widening
                Debug.WriteLine($"Error widening geometry: {ex.Message}");
                return null;
            }
        }

        return null;
    }
}
