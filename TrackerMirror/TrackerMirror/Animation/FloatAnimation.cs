using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TrackerMirror.Animation
{
    public class FloatAnimation : Animation<float>
    {
        public FloatAnimation(float from, float to, TimeSpan duration)
            : base(from, to, duration)
        {
        }

        public FloatAnimation(float from, float to, TimeSpan duration, TimeSpan delay)
            : base(from, to, duration, delay)
        {
        }

        protected override float Step(float step)
        {
            return this.From + (this.To - this.From) * step;
        }
    }
}
