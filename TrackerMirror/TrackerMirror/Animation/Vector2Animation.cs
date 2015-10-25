using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TrackerMirror.Animation
{
    public class Vector2Animation : Animation<Vector2>
    {
        public Vector2Animation(Vector2 from, Vector2 to, TimeSpan duration)
            : base(from, to, duration)
        {
        }
        public Vector2Animation(Vector2 from, Vector2 to, TimeSpan duration, TimeSpan delay)
            : base(from, to, duration, delay)
        {
        }

        protected override Vector2 Lerp(float step)
        {
            return Vector2.Lerp(this.From, this.To, step);
        }
    }
}
