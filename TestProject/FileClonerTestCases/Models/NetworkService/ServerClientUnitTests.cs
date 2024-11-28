/******************************************************************************
 * Filename    = ServerClientUnitTests.cs
 *
 * Author(s)   = Neeraj Krishna N
 * 
 * Project     = FileClonerTestCases
 *
 * Description = UnitTests for Models/Server and Models/Client
 *****************************************************************************/

using System.Text;
using FileCloner.Models;
using FileCloner.Models.NetworkService;
using Networking.Serialization;

namespace FileClonerTestCases;

[TestClass]
public class ServerClientUnitTests
{
    private Server? _server;
    private Client? _client;
    private static string s_filePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "dummy.txt"
    );

    private Action<string> _action;
    private static string s_dummyFileContent = "hello";
    private static byte[] s_bytes = Encoding.UTF8.GetBytes(s_dummyFileContent);
    private static Serializer s_serializer = new();

    private static Message s_message = new Message {
        Subject = "Request",
        RequestID = 42,
        From = "192.168.1.1",
        To = "192.168.1.2",
        MetaData = s_filePath,
        Body = "1:" + s_serializer.Serialize(s_bytes)
    };

    [TestInitialize]
    public void Setup()
    {
        _server = Server.GetServerInstance();
        _client = new(null);

        if (!File.Exists(s_filePath))
        {
            FileStream fileStream = File.Create(s_filePath);
            fileStream.Dispose(); // Explicitly close the stream
        }

        if (!Directory.Exists(Constants.ReceivedFilesFolderPath))
        {
            Directory.CreateDirectory(Constants.ReceivedFilesFolderPath);
        }
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(Constants.ReceivedFilesFolderPath))
        {
            Directory.Delete(Constants.ReceivedFilesFolderPath, true);
        }
    }

    [TestMethod]
    public void AssertServerNotNull()
    {
        Assert.IsNotNull(_server);
        Assert.IsNotNull(_client);
    }


    [TestMethod]
    public void TestOnDataReceived()
    {
        string serializedMessage = s_serializer.Serialize<Message>(s_message);
        _server?.OnDataReceived(serializedMessage);

        Message message = s_message;
        message.To = Constants.Broadcast;
        serializedMessage = s_serializer.Serialize<Message>(s_message);
        _server?.OnDataReceived(serializedMessage);
        _server?.OnDataReceived(s_serializer.Serialize<Message>(null));

    }

    [TestMethod]
    public void TestServerMethods()
    {
        _server.OnClientLeft("");
    }

    [TestMethod]
    public void TestClientMethods()
    {
        FileStream fileStream = File.Create(Path.Combine(Constants.ReceivedFilesFolderPath, "dummy.txt"));
        fileStream.Dispose(); // Explicitly close the stream
        _client?.SendRequest();

        _client?.OnResponseReceived(s_message);
        _client?.OnSummaryReceived(s_message);
        _client?.OnFileForCloningReceived(s_message);
    }

}
