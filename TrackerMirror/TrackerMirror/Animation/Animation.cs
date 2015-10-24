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

        public TimeSpan Duration { get; protected set; }
        public TimeSpan Elapsed { get; protected set; }

        public bool Reversed { get; protected set; }

        public bool Paused { get; set; } = false;
        public bool Done => Elapsed >= Duration;

        public Animation(T from, T to, TimeSpan duration)
        {
            this.OriginalFrom = this.From = from;
            this.OriginalTo = this.To = to;

            this.Duration = duration;
            this.Elapsed = TimeSpan.Zero;
        }

        public virtual void Update(GameTime time)
        {
            if (this.Paused || this.Done) return;

            this.Elapsed += time.ElapsedGameTime;
            if (this.Elapsed > this.Duration)
            {
                this.Elapsed = this.Duration;
            }

            float step = (float)this.Elapsed.Ticks / (float)this.Duration.Ticks;
            this.Value = this.Lerp(step);
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
