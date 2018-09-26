using System;
using System.Net;
using System.Net.Sockets;

namespace CommandServer
{
    /// Occurs when a command received from a client.
    public delegate void CommandReceivedEventHandler(object sender, CommandEventArgs e);

    /// Occurs when a command had been sent to the remote client successfully.
    public delegate void CommandSentEventHandler(object sender, EventArgs e);

    /// Occurs when a command sending action had been failed.This is because disconnection or sending exception.
    public delegate void CommandSendingFailedEventHandler(object sender, EventArgs e);

    /// Occurs when a remote client had been disconnected from the server.
    public delegate void DisconnectedEventHandler(object sender, ClientEventArgs e);

    public class CommandEventArgs : EventArgs
    {
        private Command command;
        /// <summary>
        /// The received command.
        /// </summary>
        public Command Command
        {
            get { return command; }
        }

        /// <summary>
        /// Creates an instance of CommandEventArgs class.
        /// </summary>
        /// <param name="cmd">The received command.</param>
        public CommandEventArgs(Command cmd)
        {
            this.command = cmd;
        }
    }

    public class ClientEventArgs : EventArgs
    {
        private Socket socket;
        /// <summary>
        /// The ip address of remote client.
        /// </summary>
        public IPAddress IP
        {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Address; }
        }
        /// <summary>
        /// The port of remote client.
        /// </summary>
        public int Port
        {
            get { return ((IPEndPoint)this.socket.RemoteEndPoint).Port; }
        }
        /// <summary>
        /// Creates an instance of ClientEventArgs class.
        /// </summary>
        /// <param name="clientManagerSocket">The socket of server side socket that comunicates with the remote client.</param>
        public ClientEventArgs(Socket clientManagerSocket)
        {
            this.socket = clientManagerSocket;
        }
    }
}
