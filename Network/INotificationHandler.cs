using System.Net.Sockets;

namespace Networking
{
    public interface INotificationHandler
    {
        /// <summary>
        /// Called when data of a particular module appears in the receiving queue
        /// </summary>
        public void OnDataReceived(string serializedData);

        /// <summary>
        /// Called on the server when a new client joins
        /// </summary>
        public void OnClientJoined(TcpClient socket)
        { }

        /// <summary>
        /// Called on the server when a client leaves
        /// </summary>
        public void OnClientLeft(string clientId)
        { }
    }
}
