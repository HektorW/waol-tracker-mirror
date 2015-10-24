using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace TrackerMirror.TrackerMirrorServer
{
    public class ClientData
    {
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string Height { get; set; }
        public string ShoeSize { get; set; }
    }
}
