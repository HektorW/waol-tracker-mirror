using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TrackerMirror.TrackerMirrorServer
{
    public class Client
    {
        private Server server;
        private Socket socket;

        public long ID { get; set; }

        private NetworkStream stream;
        private StreamReader streamReader;
        private StreamWriter streamWriter;

        private Thread thread;
        private bool running = false;


        // Data
        public double Distance { get; set; }
        public Color Color { get; set; }
        public ClientData ClientData { get; set; }


        public Client(Server server, Socket socket, long id)
        {
            this.server = server;
            this.socket = socket;
            this.ID = id;

            this.stream = new NetworkStream(this.socket);
            this.streamReader = new StreamReader(this.stream);
            this.streamWriter = new StreamWriter(this.stream);
            this.streamWriter.AutoFlush = true;

            this.thread = new Thread(new ParameterizedThreadStart(Listen));
            this.running = true;
            this.thread.Start();
        }

        public void Destroy()
        {
            this.running = false;

            this.streamWriter.Close();
            this.streamWriter.Dispose();
            
            this.streamReader.Close();
            this.streamReader.Dispose();

            this.stream.Close();
            this.stream.Dispose();

            this.socket.Close();
            this.socket.Dispose();

            this.thread.Abort();
        }

        private void Disconnect()
        {
            this.server.RemoveClient(this);
        }

        private void Listen(object sender)
        {
            this.Send("Started listening");

            while (this.running)
            {
                string streamBuffer = null;
                byte[] byteList = new byte[1024];
                int dataSize = 0;
                try
                {
                    dataSize = socket.Receive(byteList);
                }
                catch (Exception e)
                {
                    // Handle disconnects
                    this.Disconnect();
                }

                streamBuffer = Encoding.UTF8.GetString(byteList, 0, dataSize);
                this.ParseMessage(streamBuffer);
            }
        }

        private void ParseMessage(string message)
        {
            JObject data = null;
            try
            {
                data = JObject.Parse(message);
            }
            catch (Exception ex)
            {
                this.Disconnect();
                return;
            }

            var distance = data["distance"];
            var color = data["color"];
            var info = data["info"];

            if (distance != null)
            {
                this.Distance = distance.Value<double>();
            }

            if (color != null)
            {
                this.Color = JsonConvert.DeserializeObject<Color>(color.ToString());
            }

            if (info != null)
            {
                this.ClientData = JsonConvert.DeserializeObject<ClientData>(info.ToString());
            }
        }

        public void Send(string message)
        {
            this.streamWriter.Write(message);
        }
    }
}
