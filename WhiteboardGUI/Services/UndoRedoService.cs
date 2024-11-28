/**************************************************************************************************
 * Filename    = UndoRedoService.cs
 *
 * Authors     = Kshitij Ghodake
 *
 * Product     = WhiteBoard
 * 
 * Project     = Undo redo feature
 *
 * Description = Implementation for undo redo feature for the actions performed on the canvas
 *************************************************************************************************/

using WhiteboardGUI.Models;

namespace WhiteboardGUI.Services;

/// <summary>
/// Service for managing undo and redo operations for shapes on the whiteboard.
/// Maintains a history of shape modifications to allow reversing or reapplying changes.
/// </summary>
public class UndoRedoService
{
    /// <summary>
    /// List of actions that can be undone.
    /// Each entry is a tuple containing the current state of the shape and its previous state.
    /// </summary>
    public List<(IShape, IShape?)> _undoList = new();

    /// <summary>
    /// List of actions that can be redone.
    /// Each entry is a tuple containing the state of the shape before the undo and its state after the undo.
    /// </summary>
    public List<(IShape, IShape?)> _redoList = new();

    /// <summary>
    /// Removes all modifications of a specific shape from the undo and redo lists.
    /// Ensures that any existing history for the shape is cleared.
    /// </summary>
    /// <param name="networkingService">The networking service associated with the shape.</param>
    /// <param name="shape">The shape whose modifications are to be removed.</param>
    public void RemoveLastModified(IShape shape)
    {
        _undoList.RemoveAll(item =>
            item.Item1 != null &&
            item.Item1.ShapeId == shape.ShapeId &&
            item.Item1.UserID == shape.UserID);

        _redoList.RemoveAll(item =>
            item.Item1 != null &&
            item.Item1.ShapeId == shape.ShapeId &&
            item.Item1.UserID == shape.UserID);
    }

    /// <summary>
    /// Adds a new modification to the undo list, representing the current and previous states of a shape.
    /// Limits the undo list to a maximum of 5 entries.
    /// </summary>
    /// <param name="currentShape">The current state of the shape.</param>
    /// <param name="previousShape">The previous state of the shape.</param>
    public void UpdateLastDrawing(IShape currentShape, IShape previousShape)
    {
        _undoList.Add((currentShape, previousShape));
        if (_undoList.Count > 5)
        {
            // Removes the oldest modification if the undo list exceeds 5 entries.
            _undoList.RemoveAt(0);
        }
    }

    /// <summary>
    /// Undoes the last modification, moving it to the redo list.
    /// Limits the redo list to a maximum of 5 entries.
    /// </summary>
    public void Undo()
    {
        if (_undoList.Count > 0)
        {
            _redoList.Add((_undoList[_undoList.Count - 1].Item2, _undoList[_undoList.Count - 1].Item1));
            if (_redoList.Count > 5)
            {
                // Removes the oldest redo action if the redo list exceeds 5 entries.
                _redoList.RemoveAt(0);
            }
            _undoList.RemoveAt(_undoList.Count - 1);
        }
    }

    /// <summary>
    /// Redoes the last undone modification, moving it back to the undo list.
    /// </summary>
    public void Redo()
    {
        if (_redoList.Count > 0)
        {
            _undoList.Add((_redoList[_redoList.Count - 1].Item2, _redoList[_redoList.Count - 1].Item1));
            _redoList.RemoveAt(_redoList.Count - 1);
        }
    }
}
