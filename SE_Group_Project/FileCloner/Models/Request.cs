namespace FileCloner.Models
{
    //<Summary>
    // A request can have only 1 state i.e onhold
    // Once a request is accepted/rejected it's deleted forever 
    //<Summary>

    public class Request
    {
        public string senderAddress;
        public string receiverAddress;
        public DateTime createdOn;

        public Request(string senderAddress, string receiverAddress)
        {
            this.senderAddress = senderAddress;
            this.receiverAddress = receiverAddress;
            this.createdOn = DateTime.Now;
        }
    }
}
