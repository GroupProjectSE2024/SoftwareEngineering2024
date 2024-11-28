/******************************************************************************
 * Filename    = ReceivedDataService.cs
 *
 * Author      = Likith Anaparty and Vishnu Nair
 *
 * Product     = WhiteBoard
 *
 * Project     = Networking for whiteboard
 *
 * Description = The methods and logic to do after a data and command is received
 *****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteboardGUI.Models;
using WhiteboardGUI.ViewModel;
namespace WhiteboardGUI.Services;

/// <summary>
/// Service for handling received data and managing shape synchronization.
/// </summary>
public class ReceivedDataService
{
    /// <summary>
    /// List of synchronized shapes.
    /// </summary>
    public List<IShape> _synchronizedShapes = new();

    /// <summary>
    /// Event triggered when a shape is received.
    /// </summary>
    public event Action<IShape, bool> ShapeReceived;

    /// <summary>
    /// Event triggered when a shape is deleted.
    /// </summary>
    public event Action<IShape> ShapeDeleted;

    public event Action<IShape> NewClientJoinedShapeReceived;

    /// <summary>
    /// Event triggered when a shape is sent to the back.
    /// </summary>
    public event Action<IShape> ShapeSendToBack;

    /// <summary>
    /// Event triggered when a shape is sent backward.
    /// </summary>
    public event Action<IShape> ShapeSendBackward;

    /// <summary>
    /// Event triggered when a shape is modified.
    /// </summary>
    public event Action<IShape> ShapeModified;

    /// <summary>
    /// Event triggered when a shape is unlocked.
    /// </summary>
    public event Action<IShape> ShapeUnlocked;

    /// <summary>
    /// Event triggered when a shape is locked.
    /// </summary>
    public event Action<IShape> ShapeLocked;

    /// <summary>
    /// Event triggered when all shapes are cleared.
    /// </summary>
    public event Action ShapesClear;

    /// <summary>
    /// The ID of the current service instance.
    /// </summary>
    private int _id;
    
    MainPageViewModel _mainPageViewModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReceivedDataService"/> class.
    /// </summary>
    /// <param name="id">The unique identifier for the instance.</param>
    public ReceivedDataService(int id, MainPageViewModel mainPageViewModel)
    {
        _id = id;
        _mainPageViewModel = mainPageViewModel;
    }

    /// <summary>
    /// Processes received data and triggers appropriate events based on the message content.
    /// </summary>
    /// <param name="receivedData">The data received as a string.</param>
    /// <returns>Returns 0 on successful processing, or -1 if the data is invalid or from the same sender.</returns>
    public int DataReceived(string receivedData)
    {
        if (receivedData == null)
        {
            return -1;
        }

        int index = receivedData.IndexOf("END");
        int senderId = int.Parse(receivedData.Substring(2, index - 2));
        receivedData = receivedData.Substring(index + "END".Length);

        if (senderId == _id)
        {
            if (!receivedData.StartsWith("LOCK:"))
            {
                return -1;
            }
        }


        if (receivedData.StartsWith("NEWCLIENT"))
        {
            _mainPageViewModel.ClientJoined(senderId.ToString());
        }
        else if (receivedData.StartsWith("SHAPEFORNEWCLIENT:"))
        {
            string data = receivedData.Substring(18);
            IShape shape = SerializationService.DeserializeShape(data);
            NewClientJoinedShapeReceived?.Invoke(shape);
        }

        else if (receivedData.StartsWith("DELETE:"))
        {
            string data = receivedData.Substring(7);
            IShape shape = SerializationService.DeserializeShape(data);

            if (shape != null)
            {
                Guid shapeId = shape.ShapeId;
                double shapeUserId = shape.UserID;

                IShape? currentShape = _synchronizedShapes
                    .Where(s => s.ShapeId == shapeId && s.UserID == shapeUserId)
                    .FirstOrDefault();
                if (currentShape != null)
                {
                    ShapeDeleted?.Invoke(currentShape); // Deletes the shape based on the received data
                }
            }
        }
        else if (receivedData.StartsWith("CLEAR:"))
        {
            ShapesClear?.Invoke(); // Clears the screen if CLEAR command was received
        }
        else if (receivedData.StartsWith("INDEX-BACK:"))
        {
            string data = receivedData.Substring(11);
            IShape shape = SerializationService.DeserializeShape(data);

            if (shape != null)
            {
                Guid shapeId = shape.ShapeId;
                double shapeUserId = shape.UserID;

                IShape? currentShape = _synchronizedShapes
                    .Where(s => s.ShapeId == shapeId && s.UserID == shapeUserId)
                    .FirstOrDefault();
                if (currentShape != null)
                {
                    ShapeSendToBack?.Invoke(currentShape); // Sends the shape to the last layer in the canvas
                }
            }
        }
        else if (receivedData.StartsWith("INDEX-BACKWARD:"))
        {
            string data = receivedData.Substring(15);
            IShape shape = SerializationService.DeserializeShape(data);

            if (shape != null)
            {
                Guid shapeId = shape.ShapeId;
                double shapeUserId = shape.UserID;

                IShape? currentShape = _synchronizedShapes
                    .Where(s => s.ShapeId == shapeId && s.UserID == shapeUserId)
                    .FirstOrDefault();
                if (currentShape != null)
                {
                    ShapeSendBackward?.Invoke(currentShape); // Sends the shape one layer back
                }
            }
        }
        else if (receivedData.StartsWith("MODIFY:"))
        {
            string data = receivedData.Substring(7);
            IShape shape = SerializationService.DeserializeShape(data);
            Debug.WriteLine($"Received shape: {shape}");
            if (shape != null)
            {
                Guid shapeId = shape.ShapeId;
                double shapeUserId = shape.UserID;

                IShape? currentShape = _synchronizedShapes
                    .Where(s => s.ShapeId == shapeId && s.UserID == shapeUserId)
                    .FirstOrDefault();
                if (currentShape != null)
                {
                    ShapeModified?.Invoke(shape); // Changes the shape identified by its shape_id
                }
            }
        }
        else if (receivedData.StartsWith("CREATE:"))
        {
            string data = receivedData.Substring(7);
            IShape shape = SerializationService.DeserializeShape(data);
            if (shape != null)
            {
                ShapeReceived?.Invoke(shape, true); // Draws the shape sent over the network
            }
        }
        else if (receivedData.StartsWith("DOWNLOAD:"))
        {
            string data = receivedData.Substring(9);
            IShape shape = SerializationService.DeserializeShape(data);
            if (shape != null)
            {
                ShapeReceived?.Invoke(shape, false); // For rendering snapshot after downloading
            }
        }
        else if (receivedData.StartsWith("UNLOCK:"))
        {
            string data = receivedData.Substring(7);
            IShape shape = SerializationService.DeserializeShape(data);
            if (shape != null)
            {
                IShape? existingShape = _synchronizedShapes
                    .FirstOrDefault(s => s.ShapeId == shape.ShapeId);
                if (existingShape != null)
                {
                    existingShape.IsLocked = false;
                    existingShape.LockedByUserID = -1;
                    ShapeUnlocked?.Invoke(existingShape); // Unlocks the shape locked by another user
                }
            }
        }
        else if (receivedData.StartsWith("LOCK:")) 
        {
            string data = receivedData.Substring(5);
            IShape shape = SerializationService.DeserializeShape(data);
            if (shape != null)
            {
                IShape? existingShape = _synchronizedShapes
                    .FirstOrDefault(s => s.ShapeId == shape.ShapeId);

                if (existingShape != null)
                {
                    if (_id == 1)
                    {
                        if (existingShape.IsLocked)
                        {
                            receivedData = "UNLOCK:" + receivedData.Substring("LOCK:".Length);
                        }
                        else
                        {
                            existingShape.IsLocked = true;
                            existingShape.LockedByUserID = senderId;
                            existingShape.LastModifiedBy = shape.LastModifiedBy;
                            
                            ShapeLocked?.Invoke(existingShape); // Locks the shape
                        }
                    }
                    else
                    {
                        existingShape.IsLocked = true;
                        existingShape.LockedByUserID = shape.LockedByUserID;
                        existingShape.LastModifiedBy = shape.LastModifiedBy;
                        
                        ShapeLocked?.Invoke(existingShape);
                    }
                }
            }
        }

        return 0;
    }
}
