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

        public StringAnimation(string from, string to, TimeSpan duration, TimeSpan delay)
            : base(from, to, duration, delay)
        {
        }

        protected override string Lerp(float step)
        {
            var length = this.To.Length - this.From.Length;
            var delta = (int)(length * step + 0.5);

            return this.To.Substring(this.From.Length, delta);
        }
    }
}
