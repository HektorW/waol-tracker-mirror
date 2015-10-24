using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TrackerMirror.TrackerMirrorServer
{
    public class Server
    {
        public object AccessLock = new object();

        // Server
        private int port;
        private TcpListener listener;

        // Threads
        private Thread incommingThread;
        private bool runningServer = false;

        // Clients
        private List<Client> clients = new List<Client>();
        private long _clientguid = 1;
        private object clientsLock = new object();

        public Server(int port = 8004)
        {
            this.port = port;

            this.listener = new TcpListener(IPAddress.Any, this.port);
            this.incommingThread = new Thread(new ParameterizedThreadStart(StartServer));
        }

        public void Start()
        {
            if (!runningServer)
            {
                this.listener.Start();

                this.runningServer = true;
                this.incommingThread.Start();
            }
        }

        public void Stop()
        {
            this.runningServer = false;

            this.listener.Stop();

            this.incommingThread.Abort();
        }

        protected void StartServer(object server)
        {
            while (this.runningServer)
            {
                var socket = listener.AcceptSocket();

                lock (this.clientsLock)
                {
                    this.clients.Add(new Client(this, socket, _clientguid++));
                }
            }
        }



        public Client GetClosestClient()
        {
            lock (this.AccessLock)
            {
                return this.clients.OrderBy(c => c.Distance).FirstOrDefault();
            }
        }
    }
}
