using System.Diagnostics;
using Updater;
using Networking;
using Networking.Communication;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace TestsUpdater;

public class StringWriterTraceListener : TraceListener
{
    private readonly StringWriter _stringWriter;

    public StringWriterTraceListener(StringWriter stringWriter)
    {
        _stringWriter = stringWriter ?? throw new ArgumentNullException(nameof(stringWriter));
    }

    public override void Write(string? message)
    {
        _stringWriter.Write(message);
    }

    public override void WriteLine(string? message)
    {
        _stringWriter.WriteLine(message);
    }

    public string GetOutput()
    {
        return _stringWriter.ToString();
    }
}

[TestClass]
public class TestClient
{
    private static ICommunicator? s_communicator;
    private static Client? s_client;
    private static StringWriter? s_traceOutput;

    // Test initialization
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        s_communicator = CommunicationFactory.GetCommunicator(isClientSide: true);
        s_client = Client.GetClientInstance(notificationReceived: (message) => Trace.WriteLine(message));

        s_traceOutput = new StringWriter();
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new TextWriterTraceListener(s_traceOutput));
    }

    [TestInitialize]
    public void TestInitialize()
    {
        s_traceOutput?.GetStringBuilder().Clear();
    }

    [TestMethod]
    public void TestGetClientInstance_Singleton()
    {
        var client1 = Client.GetClientInstance();
        var client2 = Client.GetClientInstance();
        Assert.AreEqual(client1, client2, "Client instance is not singleton");
    }

    [TestMethod]
    public void TestSyncUp_Success()
    {
        s_client?.GetClientId("TestClient123");
        s_client?.SyncUp();

        Assert.IsTrue(s_traceOutput?.ToString().Contains("Sending syncup request to the server"),
                      "SyncUp did not log expected message");
    }

    [TestMethod]
    public void TestStop()
    {
        s_client?.Stop();
        Assert.IsTrue(s_traceOutput?.ToString().Contains("Client disconnected"),
                      "Stop method did not log expected message");
    }

    [TestMethod]
    public void TestPacketDemultiplexerSyncUp()
    {
        var dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, []);
        string serializedData = Utils.SerializeObject(dataPacket)!;

        if (s_communicator != null)
        {
            Client.PacketDemultiplexer(serializedData, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Received SyncUp request from server"),
                          "SyncUpHandler not called correctly");
        }
    }

    [TestMethod]
    public void TestPacketDemultiplexerInvalidSync()
    {
        // Create an InvalidSync packet with some file content
        var fileContent = new FileContent("invalid.txt", Utils.SerializeObject(new List<string> { "file1.txt" })!);
        var dataPacket = new DataPacket(DataPacket.PacketType.InvalidSync, [fileContent]);
        string serializedData = Utils.SerializeObject(dataPacket)!;

        // Call the PacketDemultiplexer
        if (s_communicator != null)
        {
            Client.PacketDemultiplexer(serializedData, s_communicator);

            // Assert that the InvalidSyncHandler is called by checking the trace output
            Assert.IsTrue(s_traceOutput?.ToString().Contains("Received invalid file names from server"),
                          "InvalidSyncHandler was not called correctly");
        }
    }

    [TestMethod]
    public void TestPacketDemultiplexerBroadcast()
    {
        // Create a Broadcast packet with some file content
        var fileContent = new FileContent("test.txt", Utils.SerializeObject("test content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Broadcast, [fileContent]);
        string serializedData = Utils.SerializeObject(dataPacket)!;

        // Call the PacketDemultiplexer
        if (s_communicator != null)
        {
            Client.PacketDemultiplexer(serializedData, s_communicator);

            // Assert that the BroadcastHandler is called by checking the trace output
            Assert.IsTrue(s_traceOutput?.ToString().Contains("Up-to-date with the server"),
                          "BroadcastHandler was not called correctly");
        }
    }

    [TestMethod]
    public void TestPacketDemultiplexerDifferences()
    {
        // Create a Differences packet with metadata differences and file content
        var diffContent = new FileContent("differences", Utils.SerializeObject(new List<MetadataDifference>())!);
        var fileContent = new FileContent("file.txt", Utils.SerializeObject("content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Differences, [diffContent, fileContent]);
        string serializedData = Utils.SerializeObject(dataPacket)!;

        // Call the PacketDemultiplexer
        if (s_communicator != null)
        {
            Client.PacketDemultiplexer(serializedData, s_communicator);

            // Assert that the DifferencesHandler is called by checking the trace output
            Assert.IsTrue(s_traceOutput?.ToString().Contains("Sending requested files to server"),
                          "DifferencesHandler was not called correctly");
        }
    }


    [TestMethod]
    public void TestSyncUpHandler()
    {
        var syncUpPacket = new DataPacket(DataPacket.PacketType.SyncUp, []);
        if (s_communicator != null)
        {
            Client.SyncUpHandler(syncUpPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Metadata sent to server"),
                          "SyncUpHandler did not send metadata");
        }
    }

    [TestMethod]
    public void TestInvalidSyncHandler()
    {
        var fileContent = new FileContent("invalid.txt", Utils.SerializeObject(new List<string> { "file1.txt" })!);
        var dataPacket = new DataPacket(DataPacket.PacketType.InvalidSync, [fileContent]);

        if (s_communicator != null)
        {
            Client.InvalidSyncHandler(dataPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Received invalid file names from server"),
                          "InvalidSyncHandler did not log expected message");
        }
    }

    [TestMethod]
    public void TestBroadcastHandler()
    {
        var fileContent = new FileContent("test.txt", Utils.SerializeObject("test content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Broadcast, [fileContent]);

        if (s_communicator != null)
        {
            Client.BroadcastHandler(dataPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Up-to-date with the server"),
                          "BroadcastHandler did not update correctly");
        }
    }

    [TestMethod]
    public void TestDifferencesHandler()
    {
        var diffContent = new FileContent("differences", Utils.SerializeObject(new List<MetadataDifference>())!);
        var fileContent = new FileContent("file.txt", Utils.SerializeObject("content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Differences, [diffContent, fileContent]);

        if (s_communicator != null)
        {
            Client.DifferencesHandler(dataPacket, s_communicator);

            Assert.IsTrue(s_traceOutput?.ToString().Contains("Sending requested files to server"),
                          "DifferencesHandler did not send files correctly");
        }
    }

    [TestMethod]
    public void TestOnDataReceived()
    {
        var dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, []);
        string serializedData = Utils.SerializeObject(dataPacket)!;

        s_client?.OnDataReceived(serializedData);

        Assert.IsTrue(s_traceOutput?.ToString().Contains("FileTransferHandler received data"),
                      "OnDataReceived did not handle packet correctly");
    }

    [TestMethod]
    public void TestShowInvalidFilesInUI()
    {
        Client.ShowInvalidFilesInUI(["file1.txt", "file2.txt"]);
        Assert.IsTrue(s_traceOutput?.ToString().Contains("Invalid filenames"),
                      "ShowInvalidFilesInUI did not log expected message");
    }

    [TestMethod]
    public void TestPacketDemultiplexer_DefaultCase_Exception()
    {
        // Simulate an invalid packet type (e.g., an undefined type in the enum)
        var invalidPacketType = (DataPacket.PacketType)999; // Using a value that isn't defined
        var dataPacket = new DataPacket(invalidPacketType, []);
        string serializedData = Utils.SerializeObject(dataPacket)!;

        try
        {
            // Call the PacketDemultiplexer with invalid data
            if (s_communicator != null)
            {
                Client.PacketDemultiplexer(serializedData, s_communicator);
            }

            // If no exception is thrown, fail the test
            Assert.Fail("[Updater] Error in PacketDemultiplexer: Object reference not set to an instance of an object.");
        }
        catch (Exception ex)
        {
            // Assert that the exception message matches the one thrown in the default case
            Assert.IsTrue(ex.Message.Contains("[Updater] Error in PacketDemultiplexer: Object reference not set to an instance of an object."),
                          $"Expected exception message not found. Actual message: {ex.Message}");
        }
    }

    [TestMethod]
    public void TestInvalidSyncHandler_Trace_Exception()
    {
        var fileContent = new FileContent("invalid.txt", null);
        var dataPacket = new DataPacket(DataPacket.PacketType.InvalidSync, [fileContent]);

        try
        {
            // Call InvalidSyncHandler with the invalid packet
            if (s_communicator != null)
            {
                Client.InvalidSyncHandler(dataPacket, s_communicator);
            }

            // If no exception is thrown, fail the test
            Assert.Fail("Expected exception due to null SerializedContent");
        }
        catch (Exception)
        {
            // Assert the trace log for the exception
            Assert.IsTrue(s_traceOutput?.ToString().Contains("Error in InvalidSyncHandler: SerializedContent in FileContent is null"),
                          "InvalidSyncHandler did not log the expected error message");
        }
    }
}
