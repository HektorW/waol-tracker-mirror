using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TrackerMirror.Animation
{
    public class ColorAnimation : Animation<Color>
    {
        public ColorAnimation(Color from, Color to, TimeSpan duration)
            : base(from, to, duration)
        {
        }

        protected override Color Step(float step)
        {
            return Color.Lerp(this.From, this.To, step);
        }
    }
}
