using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace TrackerMirror
{
    public class Server
    {
        protected int Port { get; set; }
        protected HttpListener Listener { get; set; }

        protected Thread incommingThread;
        protected bool runningServer = false;

        public Server()
        {
            this.Listener = new HttpListener();
            this.Listener.Prefixes.Add("http://*:8004");

            this.incommingThread = new Thread(new ParameterizedThreadStart(StartServer));
        }

        public void start()
        {
            if (!runningServer)
            {
                this.incommingThread.Start();
                this.runningServer = true;
            }
        }

        protected void StartServer(object server)
        {
            while (this.runningServer)
            {
                var context = Listener.GetContext();

                var data = new StreamReader(context.Request.InputStream, context.Request.ContentEncoding).ReadToEnd();

                context.Response.Close();
            }
        }
    }
}
