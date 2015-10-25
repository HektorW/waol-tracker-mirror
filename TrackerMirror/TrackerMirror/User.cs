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
                this._skeleton = value;
            }
        }

        public KinectSensor KinectSensor { get; set; }
        public Client Client { get; set; }
        public bool Active { get; protected set; }

        private TrackerMirror trackerMirror;

        private bool _started = false;

        // Animations
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

        private FloatAnimation heightOpacityAnimation;
        private Vector2Animation heightPositionAnimation;
        private StringAnimation heightStringAnimation;

        private FloatAnimation shoeSizeOpacityAnimation;
        private Vector2Animation shoeSizePositionAnimation;
        private StringAnimation shoeSizeStringAnimation;

        private bool AnimationsSetup => this.nameOpacityAnimation != null;

        // Strings
        private string heightString;
        private string shoeString;

        // Focus point
        private Vector2 focusPoint;
        private Rectangle overlayRectangle;


        public User(TrackerMirror trackerMirror, KinectSensor kinectSensor, Client client)
        {
            this.trackerMirror = trackerMirror;
            this.KinectSensor = kinectSensor;
            this.Client = client;

            this.Active = true;

            this.focusPoint = new Vector2();
            this.overlayRectangle = new Rectangle(0, 0, this.trackerMirror.Width, this.trackerMirror.Height);
        }

        private void SetupDataAnimations()
        {
            if (this.AnimationsSetup || this.Client.ClientData == null) return;

            var client = this.Client;
            var data = client.ClientData;

            this.heightString = $"Height: {data.Height}";
            this.shoeString = $"Shoe size: {data.ShoeSize}";


            this.overlayAnimation = new ColorAnimation(Color.White, client.Color, TimeSpan.FromSeconds(1.5));

            float alpha = 0.7f;

            this.nameOpacityAnimation = new FloatAnimation(0.0f, alpha, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2));
            this.namePositionAnimation = new Vector2Animation(new Vector2(0, -30), new Vector2(50, -30), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2));
            this.nameStringAnimation = new StringAnimation("", data.Name, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(2));

            this.surnameOpacityAnimation = new FloatAnimation(0.0f, alpha, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3));
            this.surnamePositionAnimation = new Vector2Animation(new Vector2(0, -5), new Vector2(50, -5), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3));
            this.surnameStringAnimation = new StringAnimation("", data.Surname, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(3));

            this.emailOpacityAnimation = new FloatAnimation(0.0f, alpha, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4));
            this.emailPositionAnimation = new Vector2Animation(new Vector2(0, 20), new Vector2(50, 20), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4));
            this.emailStringAnimation = new StringAnimation("", data.Email, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(4));

            this.heightOpacityAnimation = new FloatAnimation(0.0f, alpha, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(5));
            this.heightPositionAnimation = new Vector2Animation(new Vector2(-40, 0), new Vector2(-40, 120), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(5));
            this.heightStringAnimation = new StringAnimation("", heightString, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(5));

            this.shoeSizeOpacityAnimation = new FloatAnimation(0.0f, alpha, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(6));
            this.shoeSizePositionAnimation = new Vector2Animation(new Vector2(-40, 100), new Vector2(-40, 150), TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(6));
            this.shoeSizeStringAnimation = new StringAnimation("", shoeString, TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(6));
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

                this.nameOpacityAnimation.Update(time);
                this.namePositionAnimation.Update(time);
                this.nameStringAnimation.Update(time);

                this.surnameOpacityAnimation.Update(time);
                this.surnamePositionAnimation.Update(time);
                this.surnameStringAnimation.Update(time);

                this.emailOpacityAnimation.Update(time);
                this.emailPositionAnimation.Update(time);
                this.emailStringAnimation.Update(time);

                this.heightOpacityAnimation.Update(time);
                this.heightPositionAnimation.Update(time);
                this.heightStringAnimation.Update(time);

                this.shoeSizeOpacityAnimation.Update(time);
                this.shoeSizePositionAnimation.Update(time);
                this.shoeSizeStringAnimation.Update(time);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
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
                spriteBatch.DrawString(font, this.nameStringAnimation.Value, focusPoint + this.namePositionAnimation.Value, new Color(Color.White, this.nameOpacityAnimation.Value));
                spriteBatch.DrawString(font, this.surnameStringAnimation.Value, focusPoint + this.surnamePositionAnimation.Value, new Color(Color.White, this.surnameOpacityAnimation.Value));
                spriteBatch.DrawString(font, this.emailStringAnimation.Value, focusPoint + this.emailPositionAnimation.Value, new Color(Color.White, this.emailOpacityAnimation.Value));
                spriteBatch.DrawString(font, this.heightStringAnimation.Value, focusPoint + this.heightPositionAnimation.Value, new Color(Color.White, this.heightOpacityAnimation.Value));
                spriteBatch.DrawString(font, this.shoeSizeStringAnimation.Value, focusPoint + this.shoeSizePositionAnimation.Value, new Color(Color.White, this.shoeSizeOpacityAnimation.Value));
            }
            else
            {
                spriteBatch.DrawString(this.trackerMirror.fonts["main"], this.Client.ClientData == null ? "No client data" : "Has client data, no skeleton", Vector2.Zero, Color.White);
            }

            if (this.Skeleton == null)
            {
                spriteBatch.DrawString(this.trackerMirror.fonts["main"], "no skeleton", Vector2.Zero, Color.White);
            }
        }


        protected void UpdateFocusPoint()
        {
            if (this.Skeleton != null)
            {
                var head = this.Skeleton.Joints[JointType.Head];
                if (head != null)
                {
                    var position = this.trackerMirror.SkeletonPointToScreen(head.Position);
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
