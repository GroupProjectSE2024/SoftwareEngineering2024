namespace Networking.Queues
{
    public class Packet
    {
        // Serialized data being transmitted
        public string serializedData;

        // Destination client ID of the packet
        public string destination;

        // Module which the packet belongs to
        public string moduleOfPacket;

        // Empty constructor
        public Packet()
        {}

        public Packet(string serializedData, string destination, string moduleOfPacket)
        {
            this.serializedData = serializedData;
            this.destination = destination;
            this.moduleOfPacket = moduleOfPacket;
        }
    }
}
