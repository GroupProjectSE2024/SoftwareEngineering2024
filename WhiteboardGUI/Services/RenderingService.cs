/**************************************************************************************************
 * Filename    = RenderingService.cs
 *
 * Authors     = Likith Anaparty
 *
 * Product     = WhiteBoard
 * 
 * Project     = Rendering shapes
 *
 * Description = Helps in local rendering of shapes and broadcasts shape data over the network to other users.
 *************************************************************************************************/


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteboardGUI.Models;

namespace WhiteboardGUI.Services
{
    /// <summary>
    /// Manages rendering operations for the whiteboard application, including shape creation,
    /// modification, deletion, and synchronization across clients.
    /// </summary>
    public class RenderingService
    {
        /// <summary>
        /// Service responsible for network communications and shape synchronization.
        /// </summary>
        NetworkingService _networkingService;

        /// <summary>
        /// Service that manages undo and redo operations for shape manipulations.
        /// </summary>
        UndoRedoService _undoRedoService;

        /// <summary>
        /// Collection of shapes currently present on the whiteboard.
        /// </summary>
        ObservableCollection<IShape> Shapes;

        /// <summary>
        /// Unique identifier for the current user.
        /// </summary>
        double _userId;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderingService"/> class.
        /// </summary>
        /// <param name="networkingService">The networking service for communication.</param>
        /// <param name="undoRedoService">The service managing undo and redo operations.</param>
        /// <param name="shapes">The collection of shapes on the whiteboard.</param>
        /// <param name="UserID">The unique identifier of the user.</param>
        public RenderingService(NetworkingService networkingService, UndoRedoService undoRedoService, ObservableCollection<IShape> shapes, double UserID)
        {
            _networkingService = networkingService;
            _undoRedoService = undoRedoService;
            Shapes = shapes;
            _userId = UserID;
        }

        /// <summary>
        /// Updates the synchronized shapes by replacing the existing shape with the new one.
        /// </summary>
        /// <param name="shape">The new shape to synchronize.</param>
        /// <returns>The previous version of the shape, if it existed; otherwise, null.</returns>
        private IShape UpdateSynchronizedShapes(IShape shape)
        {
            var prevShape = _networkingService._synchronizedShapes.Where(s => s.ShapeId == shape.ShapeId && s.UserID == shape.UserID).FirstOrDefault();
            _networkingService._synchronizedShapes.Remove(prevShape);
            _networkingService._synchronizedShapes.Add(shape);
            return prevShape;
        }

        /// <summary>
        /// Renders the specified shape based on the given command.
        /// </summary>
        /// <param name="currentShape">The shape to render or manipulate.</param>
        /// <param name="command">The command indicating the action to perform (e.g., CREATE, MODIFY, DELETE).</param>
        public void RenderShape(IShape currentShape, string command)
        {

            if (command == "CREATE")
            {
                var newShape = currentShape.Clone();
                _networkingService._synchronizedShapes.Add(newShape);
                newShape.IsSelected = false;
                _undoRedoService.UpdateLastDrawing(newShape, null);
            }

            else if (command.StartsWith("INDEX"))
            {
                _networkingService._synchronizedShapes.Clear();
                foreach (IShape shape in Shapes)
                {
                    _networkingService._synchronizedShapes.Add(shape);
                }
            }

            else if (command == "DOWNLOAD")
            {
                var newShape = currentShape.Clone();
                _networkingService._synchronizedShapes.Add(newShape);
                newShape.IsSelected = false;
            }

            else if (command == "MODIFY")
            {
                var newShape = currentShape.Clone();
                var prevShape = UpdateSynchronizedShapes(newShape);
                newShape.IsSelected = false;
                _undoRedoService.UpdateLastDrawing(newShape, prevShape);
            }

            else if (command == "CLEAR")
            {
                Shapes.Clear();
                _undoRedoService.UndoList.Clear();
                _undoRedoService.RedoList.Clear();
                _networkingService._synchronizedShapes.Clear();
                string clearMessage = $"ID{_userId}{command}:";
                Debug.WriteLine(clearMessage);
                _networkingService.BroadcastShapeData(clearMessage);
                return;
            }

            else if (command == "UNDO")
            {
                var prevShape = _undoRedoService.UndoList[_undoRedoService.UndoList.Count - 1].Item1;
                var _currentShape = _undoRedoService.UndoList[_undoRedoService.UndoList.Count - 1].Item2;
                if (_currentShape == null)
                {
                    currentShape = prevShape;
                    foreach (var s in Shapes)
                    {
                        if (s.ShapeId == currentShape.ShapeId)
                        {
                            Shapes.Remove(s);
                            break;
                        }
                    }
                    _networkingService._synchronizedShapes.Remove(currentShape);
                    prevShape.LastModifierID = _networkingService._clientID;
                    command = "DELETE";
                }

                else if (prevShape == null)
                {
                    currentShape = _currentShape;
                    var newShape = currentShape.Clone();
                    Shapes.Add(newShape);
                    _networkingService._synchronizedShapes.Add(newShape);
                    command = "CREATE";
                }

                else
                {
                    currentShape = _currentShape;
                    var newShape = currentShape.Clone();
                    UpdateSynchronizedShapes(newShape);
                    foreach (var s in Shapes)
                    {
                        if (s.ShapeId == prevShape.ShapeId)
                        {
                            Shapes.Remove(s);
                            break;
                        }
                    }
                    Shapes.Add(currentShape);
                    _currentShape.LastModifierID = _networkingService._clientID;
                    command = "MODIFY";

                }
                _undoRedoService.Undo();
            }

            else if (command == "REDO")
            {
                var prevShape = _undoRedoService.RedoList[_undoRedoService.RedoList.Count - 1].Item1;
                var _currentShape = _undoRedoService.RedoList[_undoRedoService.RedoList.Count - 1].Item2;
                if (_currentShape == null)
                {
                    currentShape = prevShape;
                    foreach (var s in Shapes)
                    {
                        if (s.ShapeId == prevShape.ShapeId)
                        {
                            Shapes.Remove(s);
                            break;
                        }
                    }
                    _networkingService._synchronizedShapes.Remove(currentShape);
                    prevShape.LastModifierID = _networkingService._clientID;
                    command = "DELETE";
                }
                else if (prevShape == null)
                {
                    currentShape = _currentShape;
                    var newShape = currentShape.Clone();
                    Shapes.Add(newShape);
                    _networkingService._synchronizedShapes.Add(newShape);
                    command = "CREATE";
                }
                else
                {
                    currentShape = _currentShape;
                    var newShape = currentShape.Clone();
                    UpdateSynchronizedShapes(newShape);
                    foreach (var s in Shapes)
                    {
                        if (s.ShapeId == currentShape.ShapeId)
                        {
                            Shapes.Remove(s);
                            break;
                        }
                    }
                    Shapes.Add(currentShape);
                    _currentShape.LastModifierID = _networkingService._clientID;
                    command = "MODIFY";

                }
                _undoRedoService.Redo();
            }

            else if (command == "DELETE")
            {
                Shapes.Remove(currentShape);
                _networkingService._synchronizedShapes.Remove(currentShape);
                _undoRedoService.UpdateLastDrawing(null, currentShape);
            }

            currentShape.LastModifierID = _networkingService._clientID;
            string serializedShape = SerializationService.SerializeShape(currentShape);
            string serializedMessage = $"ID{_userId}END{command}:{serializedShape}";
            Debug.WriteLine(serializedMessage);
            _networkingService.BroadcastShapeData(serializedMessage);
        }
    }
}
