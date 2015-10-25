using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace TrackerMirror.Animation
{
    public class Animation<T>
    {
        public T Value { get; set; }
        public T From { get; set; }
        public T To { get; set; }

        public T OriginalFrom { get; set; }
        public T OriginalTo { get; set; }

        public TimeSpan Delay { get; protected set; }
        public TimeSpan Duration { get; protected set; }
        public TimeSpan Elapsed { get; protected set; }

        public bool Reversed { get; protected set; }

        public bool Paused { get; set; } = false;
        public bool Done => this.Elapsed >= this.Duration + this.Delay;

        
        public Animation(T from, T to, TimeSpan duration)
            : this(from, to, duration, TimeSpan.Zero)
        {
        }

        public Animation(T from, T to, TimeSpan duration, TimeSpan delay)
        {
            this.Value = this.OriginalFrom = this.From = from;
            this.OriginalTo = this.To = to;

            this.Delay = delay;
            this.Duration = duration;
            this.Elapsed = TimeSpan.Zero;
        }

        public virtual void Update(GameTime time)
        {
            if (this.Paused || this.Done) return;
            
            this.Elapsed += time.ElapsedGameTime;
            if (this.Elapsed >= this.Duration + this.Delay)
            {
                this.Elapsed = this.Duration + this.Delay;
            }

            if (this.Elapsed >= this.Delay)
            {
                float step = (float)(this.Elapsed.Ticks - this.Delay.Ticks) / (float)this.Duration.Ticks;
                this.Value = this.Lerp((float)this.Swing(step));
            }
        }

        protected virtual double Swing(double t)
        {
            return 0.5 - Math.Cos(t * Math.PI) / 2.0;
        }

        protected virtual T Lerp(float step)
        {
            return From;
        }

        public virtual void Reset()
        {
            this.From = this.OriginalFrom;
            this.To = this.OriginalTo;
            this.Elapsed = TimeSpan.Zero;
        }

        public virtual void Reverse()
        {
            this.To = this.Reversed ? this.OriginalTo : this.OriginalFrom;

            this.Elapsed = this.Duration - this.Elapsed;
            this.From = this.Value;
            this.Reversed = !this.Reversed;
        }
    }
}
