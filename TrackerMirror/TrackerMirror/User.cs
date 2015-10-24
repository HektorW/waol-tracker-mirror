using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TrackerMirror.Animation;
using TrackerMirror.TrackerMirrorServer;

namespace TrackerMirror
{
    public class User
    {
        private Skeleton _skeleton;
        public Skeleton Skeleton
        {
            get { return this._skeleton; }
            set
            {
                if (this._skeleton == null && value != null)
                {
                    this.alphaAnimation.Paused = false;
                }
                if (value == null)
                {
                    this.alphaAnimation.Reset();
                    this.alphaAnimation.Paused = true;
                }

                this._skeleton = value;
            }
        }

        public KinectSensor KinectSensor { get; set; }
        public Client Client { get; set; }
        public bool Active { get; protected set; }

        // Textures
        private Dictionary<string, Texture2D> textures;


        // Animations
        private FloatAnimation alphaAnimation; 

        // Color
        private Color color = Color.Indigo;


        public User(KinectSensor kinectSensor, Dictionary<string, Texture2D> textures, Client client)
        {
            this.KinectSensor = kinectSensor;
            this.textures = textures;
            this.Client = client;

            this.Active = true;

            this.alphaAnimation = new FloatAnimation(0.0f, 1.0f, TimeSpan.FromSeconds(1.5));
            this.alphaAnimation.Reverse();
        }

        public void Deactivate()
        {
            // Animate out
            this.Active = false;
        }


        public void Update(GameTime time)
        {
            this.alphaAnimation.Update(time);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            var focusPoint = this.GetFocusPoint();
            var texture = this.textures["overlay"];
            spriteBatch.Draw(texture, focusPoint - new Vector2(texture.Width / 2.0f, texture.Height / 2.0f), new Color(this.color, this.alphaAnimation.Value));
        }


        protected Vector2 GetFocusPoint()
        {
            if (this.Skeleton != null)
            {
                var head = this.Skeleton.Joints[JointType.Head];
                if (head != null)
                {
                    var position = this.KinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, this.KinectSensor.ColorStream.Format);
                    return new Vector2(position.X, position.Y);
                }
            }

            return new Vector2(this.KinectSensor.ColorStream.FrameWidth / 2.0f, this.KinectSensor.ColorStream.FrameHeight / 2.0f);
        }
    }
}
