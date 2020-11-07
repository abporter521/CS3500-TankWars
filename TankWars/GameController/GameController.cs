using System;
using System.Collections.Generic;
using System.Net.WebSockets;

namespace TankWars
{
    public class GameController
    {

        public delegate void EventHandler(IEnumerable<string> JsonMessage);
        public event EventHandler updateWorld;

        public delegate void ConnectedHandler();
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string err);
        public event ErrorHandler Error;

        // State representing the connection with the server
        SocketState theServer = null;

        // Connect to server
        // start event loop with socket state

    }
}
