using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace FileCloner.Models
{
    public class RequestList
    {
        // List to store the requests.
        public List<Request> requests;

        // Constructor to initialize the request list.
        public RequestList()
        {
            requests = [];
        }

        public void AddRequest(Request request)
        {
            requests.Add(request);
        }

        public void RemoveRequest(Request request)
        {
            requests.Remove(request);
        }
    }
}
