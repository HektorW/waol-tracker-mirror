using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerMirror.Animation
{
    class StringAnimation : Animation<string>
    {
        public StringAnimation(string from, string to, TimeSpan duration)
            : base(from, to, duration)
        {
            
        }
    }
}
