using System.Net.Sockets;

namespace Networking.Communication
{
    public interface ICommunicatorClient
    {
        void AddClient(string clientId, TcpClient socket);
        void RemoveClient(string clientId);
        void Send(string serializedData, string moduleName, string? destination = null);
        string Start(string serverIP, string serverPort);
        void Stop();
        void Subscribe(string moduleName, INotificationHandler notificationHandler, bool isHighPriority);
    }
}
