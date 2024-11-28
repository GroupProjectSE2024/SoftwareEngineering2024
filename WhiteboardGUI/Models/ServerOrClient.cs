/**************************************************************************************************
 * Filename    = ServerOrClient.cs
 *
 * Author      = Likith Anaparty
 *
 * Product     = WhiteBoard
 * 
 * Project     = Logic for Dashboard Integration
 *
 * Description = This class implements a Singleton pattern to manage the state of a user 
 *               (either as a server or client) in the Whiteboard application. It provides 
 *               functionality to set and retrieve user details, ensuring thread safety 
 *               while maintaining a single instance throughout the application lifecycle.
 *************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhiteboardGUI.Services;
using WhiteboardGUI.ViewModel;

namespace WhiteboardGUI.Models;

/// <summary>
/// Represents a singleton class to manage server or client user details.
/// </summary>
public class ServerOrClient
{
    /// <summary>
    /// Stores the username of the user.
    /// </summary>
    public string _userName;

    /// <summary>
    /// Stores the user ID of the user.
    /// </summary>
    public int _userId;

    /// <summary>
    /// Stores the user email of the user.
    /// </summary>
    public string _userEmail;

    public string _profilePictureURL;

    /// <summary>
    /// Object used for ensuring thread safety in singleton instance creation.
    /// </summary>
    private static readonly object s_padlock = new object();

    /// <summary>
    /// Singleton instance of the ServerOrClient class.
    /// </summary>
    private static ServerOrClient s_serverOrClient;

    /// <summary>
    /// Gets the singleton instance of the ServerOrClient class.
    /// Ensures thread-safe creation of the instance.
    /// </summary>
    public static ServerOrClient ServerOrClientInstance
    {
        get
        {
            lock (s_padlock)
            {
                if (s_serverOrClient == null)
                {
                    s_serverOrClient = new ServerOrClient();
                }

                return s_serverOrClient;
            }
        }
    }

    /// <summary>
    /// Sets the user details including username and user ID.
    /// </summary>
    /// <param name="username">The username of the user.</param>
    /// <param name="userid">The user ID as a string, which will be parsed into an integer.</param>
    /// <exception cref="FormatException">Thrown if the userid cannot be parsed into an integer.</exception>
    public void SetUserDetails(string username, string userid, string useremail, string profileURL)
    {
        _userName = username;
        _userId = int.Parse(userid);
        _userEmail = useremail;
        _profilePictureURL = profileURL;

    }
}
