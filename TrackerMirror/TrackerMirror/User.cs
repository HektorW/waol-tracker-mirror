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
                    this._started = true;
                    //this.alphaAnimation.Paused = false;
                }
                if (value == null)
                {
                    //this.alphaAnimation.Reset();
                    //this.alphaAnimation.Paused = true;
                }

                this._skeleton = value;
            }
        }

        public KinectSensor KinectSensor { get; set; }
        public Client Client { get; set; }
        public bool Active { get; protected set; }

        private TrackerMirror trackerMirror;

        private bool _started = false;

        // Animations
        private FloatAnimation alphaAnimation;
        private ColorAnimation overlayAnimation;

        private FloatAnimation nameOpacityAnimation;
        private Vector2Animation namePositionAnimation;
        private StringAnimation nameStringAnimation;

        private FloatAnimation surnameOpacityAnimation;
        private Vector2Animation surnamePositionAnimation;
        private StringAnimation surnameStringAnimation;

        private FloatAnimation emailOpacityAnimation;
        private Vector2Animation emailPositionAnimation;
        private StringAnimation emailStringAnimation;

        private bool AnimationsSetup => this.nameOpacityAnimation != null;

        // Focus point
        private Vector2 focusPoint;
        private Rectangle overlayRectangle;


        public User(TrackerMirror trackerMirror, KinectSensor kinectSensor, Client client)
        {
            this.trackerMirror = trackerMirror;
            this.KinectSensor = kinectSensor;
            this.Client = client;

            this.Active = true;


            //this.alphaAnimation = new FloatAnimation(0.0f, 0.8f, TimeSpan.FromSeconds(1.5));

            this.focusPoint = new Vector2();
            this.overlayRectangle = new Rectangle(0, 0, this.trackerMirror.Width, this.trackerMirror.Height);
        }

        private void SetupDataAnimations()
        {
            if (this.AnimationsSetup || this.Client.ClientData == null) return;

            var client = this.Client;
            var data = client.ClientData;

            this.overlayAnimation = new ColorAnimation(Color.White, client.Color, TimeSpan.FromSeconds(1.5));

            this.nameOpacityAnimation = new FloatAnimation(0.0f, 1.0f, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2));
            this.namePositionAnimation = new Vector2Animation(new Vector2(0, -20), new Vector2(100, -20), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2));
            this.nameStringAnimation = new StringAnimation("", data.Name, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2));

            this.surnameOpacityAnimation = new FloatAnimation(0.0f, 1.0f, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3));
            this.surnamePositionAnimation = new Vector2Animation(new Vector2(0, 5), new Vector2(100, 5), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3));
            this.surnameStringAnimation = new StringAnimation("", data.Surname, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3));

            this.emailOpacityAnimation = new FloatAnimation(0.0f, 1.0f, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4));
            this.emailPositionAnimation = new Vector2Animation(new Vector2(0, 25), new Vector2(100, 25), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4));
            this.emailStringAnimation = new StringAnimation("", data.Email, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4));
        }

        public void Deactivate()
        {
            // Animate out
            this.Active = false;
        }


        public void Update(GameTime time)
        {
            this.UpdateFocusPoint();

            if (!this.AnimationsSetup)
            {
                this.SetupDataAnimations();
            }
            else
            {
                if (this.Client.Color != this.overlayAnimation.To)
                {
                    this.overlayAnimation.Set(this.overlayAnimation.Value, this.Client.Color);
                    this.overlayAnimation.Reset();
                }

                this.overlayAnimation.Update(time);
            }
            
            //if (this._started)
            //{
            //    this.alphaAnimation.Update(time);

            //    if (this.Client.ClientData != null)
            //    {
            //        this.SetupDataAnimations();

            //        this.nameOpacityAnimation.Update(time);
            //        this.namePositionAnimation.Update(time);
            //        this.nameStringAnimation.Update(time);

            //        this.surnameOpacityAnimation.Update(time);
            //        this.surnamePositionAnimation.Update(time);
            //        this.surnameStringAnimation.Update(time);

            //        this.emailOpacityAnimation.Update(time);
            //        this.emailPositionAnimation.Update(time);
            //        this.emailStringAnimation.Update(time);
            //    }
            //}
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw overlay
            //var texture = this.trackerMirror.textures["overlay"];
            //spriteBatch.Draw(texture, focusPoint - new Vector2(texture.Width / 2.0f, texture.Height / 2.0f + 0.0f), new Color(this.Client.Color, this.alphaAnimation.Value));
            //spriteBatch.Draw(texture, this.focusPoint - new Vector2(texture.Width / 2.0f, texture.Height / 2.0f + 0.0f), new Color(this.Client.Color, 1.0f));

            // Overlay
            if (this.AnimationsSetup)
            {
                spriteBatch.Draw(this.trackerMirror.textures["whitepixel"], overlayRectangle, this.overlayAnimation.Value);
            }
            else
            {
                spriteBatch.Draw(this.trackerMirror.textures["whitepixel"], overlayRectangle, Color.White);
            }
        }

        public void DrawStrings(SpriteBatch spriteBatch)
        {
            if (this.AnimationsSetup && this.Client.ClientData != null)
            {
                var data = this.Client.ClientData;
                var font = this.trackerMirror.fonts["main"];
                //spriteBatch.DrawString(font, this.nameStringAnimation.Value, focusPoint + this.namePositionAnimation.Value, new Color(Color.White, this.nameOpacityAnimation.Value));
                //spriteBatch.DrawString(font, this.surnameStringAnimation.Value, focusPoint + this.surnamePositionAnimation.Value, new Color(Color.White, this.surnameOpacityAnimation.Value));
                //spriteBatch.DrawString(font, this.emailStringAnimation.Value, focusPoint + this.emailPositionAnimation.Value, new Color(Color.White, this.emailOpacityAnimation.Value));

                spriteBatch.DrawString(font, data.Name, this.focusPoint + this.namePositionAnimation.To, Color.White);
                spriteBatch.DrawString(font, data.Surname, this.focusPoint + this.surnamePositionAnimation.To, Color.White);
                spriteBatch.DrawString(font, data.Email, this.focusPoint + this.emailPositionAnimation.To, Color.White);
            }
            else
            {
                spriteBatch.DrawString(this.trackerMirror.fonts["main"], this.Client.ClientData == null ? "No client data" : "Has client data, no skeleton", Vector2.Zero, Color.White);
            }
        }


        protected void UpdateFocusPoint()
        {
            if (this.Skeleton != null)
            {
                var head = this.Skeleton.Joints[JointType.Head];
                if (head != null)
                {
                    var position = this.KinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, this.KinectSensor.ColorStream.Format);
                    this.focusPoint.X = position.X;
                    this.focusPoint.Y = position.Y;
                    return;
                }
            }

            this.focusPoint.X = this.trackerMirror.Width / 2.0f;
            this.focusPoint.Y = this.trackerMirror.Height / 2.0f;
        }
    }
}
