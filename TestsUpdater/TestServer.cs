// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System.Diagnostics;
using Updater;
using Networking;
using Networking.Communication;
using Moq;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.IO;


namespace TestsUpdater;

[TestClass]
public class TestServer
{
    private static ICommunicator s_communicator;
    private static Server s_server;
    private static StringWriter s_traceOutput;
    private static BinarySemaphore s_semaphore;
    private static string s_clientId;
    static event Action<string>? NotificationReceived; // Event to notify the view model
    private static string s_testDirectory;

    // Test initialization
    [ClassInitialize]
    public static void ClassInit(TestContext context)
    {
        s_communicator = CommunicationFactory.GetCommunicator(isClientSide: false);
        s_server = Server.GetServerInstance(notificationReceived: (message) => Trace.WriteLine(message));

        s_traceOutput = new StringWriter();
        Trace.Listeners.Clear();
        Trace.Listeners.Add(new TextWriterTraceListener(s_traceOutput));
        s_semaphore = new BinarySemaphore();

        s_clientId = "serverId";
        s_testDirectory = Path.Combine(Path.GetTempPath(), "ServerTestDirectory");

        if (Directory.Exists(s_testDirectory))
        {
            Directory.Delete(s_testDirectory, true);
        }
        Directory.CreateDirectory(s_testDirectory);

    }

    [TestInitialize]
    public void TestInitialize()
    {
        s_traceOutput.GetStringBuilder().Clear();
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(s_testDirectory))
        {
            Directory.Delete(s_testDirectory, true);
        }
    }
    [TestMethod]
    public void TestGetServerInstance_Singleton()
    {
        var client1 = Server.GetServerInstance();
        var client2 = Server.GetServerInstance();
        Assert.AreEqual(client1, client2, "Server instance is not singleton");
    }

    [TestMethod]
    public void TestBroadcasting()
    {
        var fileContent = new FileContent("test.txt", Utils.SerializeObject("test content")!);
        var dataPacket = new DataPacket(DataPacket.PacketType.Broadcast, new List<FileContent> { fileContent });
        string? serializedData = Utils.SerializeObject(dataPacket);

        if (serializedData == null)
        {
            Assert.Fail("Failed to serialize data packet");
        }

        s_server.Broadcasting(serializedData);

        Assert.IsTrue(s_traceOutput.ToString().Contains("[Updater] Broadcasting the new files"),
                      "BroadcastHandler did not update correctly");
    }

    [TestMethod]
    public void TestOnDataReceived()
    {
        var dataPacket = new DataPacket(DataPacket.PacketType.SyncUp, new List<FileContent>());
        string serializedData = Utils.SerializeObject(dataPacket)!;

        s_server.OnDataReceived(serializedData);

        Assert.IsTrue(s_traceOutput.ToString().Contains("[Updater] Read received data Successfully"),
                      "OnDataReceived did not handle packet correctly");
    }

    [TestMethod]
    public void TestUpdateUILogsShouldInvokeNotificationReceivedEvent()
    {
        string logMessage = "Test log message";
        bool eventInvoked = false;

        Server.NotificationReceived += (message) => {
            if (message == logMessage)
            {
                eventInvoked = true;
            }
        };

        Server.UpdateUILogs(logMessage);

        Assert.IsTrue(eventInvoked);
    }

    [TestMethod]
    public void TestSetUserAddsClientToConnections()
    {
        string clientId = "client123";
        var mockTcpClient = new Mock<TcpClient>();
        FieldInfo? clientConnectionsField = typeof(Server).GetField("_clientConnections", BindingFlags.NonPublic | BindingFlags.Instance);
        var clientConnections = (Dictionary<string, TcpClient>?)clientConnectionsField?.GetValue(s_server);

        s_server?.SetUser(clientId, mockTcpClient.Object);

        Assert.IsTrue(clientConnections?.ContainsKey(clientId) ?? false, "Client should be added to connections.");
    }

    [TestMethod]
    public void TestOnClientLeftRemovesClientFromConnections()
    {
        string clientId = "client123";
        var mockTcpClient = new Mock<TcpClient>();
        FieldInfo? clientConnectionsField = typeof(Server).GetField("_clientConnections", BindingFlags.NonPublic | BindingFlags.Instance);
        var clientConnections = (Dictionary<string, TcpClient>?)clientConnectionsField?.GetValue(s_server);

        s_server?.SetUser(clientId, mockTcpClient.Object);

        s_server?.OnClientLeft(clientId);

        Assert.IsFalse(clientConnections?.ContainsKey(clientId) ?? true, "Client should be removed from connections.");
    }

    [TestMethod]
    public void TestCompleteSyncShouldSignalSemaphore()
    {
        var binarySemaphore = new BinarySemaphore(); // Adjust constructor as per your implementation
        FieldInfo? semaphoreField = typeof(Server).GetField("s_semaphore", BindingFlags.NonPublic | BindingFlags.Instance);
        semaphoreField?.SetValue(s_server, binarySemaphore);

        s_server?.CompleteSync();

        bool signaled = Task.Run(binarySemaphore.Wait).Wait(1000); // Timeout after 1 second
        Assert.IsTrue(signaled, "CompleteSync did not signal the semaphore as expected.");
    }

    [TestMethod]
    public void TestSyncUpSuccess()
    {
        string clientId = s_clientId;

        s_server.SyncUp(clientId);

        Assert.IsTrue(s_traceOutput.ToString().Contains($"[Updater] Sending SyncUp request dataPacket to client: {clientId}"),
                      "SyncUp did not send the sync-up request.");
    }

    [TestMethod]
    public void TestUpdateUILogsNotificationReceived()
    {
        string message = "Test log message";
        bool eventTriggered = false;

        Server.NotificationReceived += (msg) => {
            if (msg == message)
            {
                eventTriggered = true;
            }
        };

        Server.UpdateUILogs(message);

        Assert.IsTrue(eventTriggered, "UpdateUILogs did not trigger the NotificationReceived event.");
    }
}