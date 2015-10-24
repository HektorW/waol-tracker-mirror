using System;
using System.Collections.Generic;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;



namespace TrackerMirror
{
    public class TrackerMirror : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        // Fonts
        private Dictionary<string, SpriteFont> fonts;

        // Textures
        private Dictionary<string, Texture2D> textures; 

        // Kinect
        private KinectSensor kinectSensor;
        private object kinectLock = new object();
        //// Colorstream
        private Texture2D kinectTexture;
        private byte[] kinectColorPixels;
        private byte[] kinectColorPixelsRgba;
        //// SkeletonStream
        private Skeleton[] skeletons;
        private Skeleton[] skeletonData;

        public TrackerMirror()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.InitializeKinect();

            this.graphics.PreferredBackBufferWidth = this.kinectTexture.Width;
            this.graphics.PreferredBackBufferHeight = this.kinectTexture.Height;
            this.graphics.ApplyChanges();

            base.Initialize();
        }

        protected void InitializeKinect()
        {
            foreach (var sensor in KinectSensor.KinectSensors)
            {
                if (sensor.Status == KinectStatus.Connected)
                {
                    this.kinectSensor = sensor;
                    break;
                }
            }

            if (this.kinectSensor == null)
            {
                throw new Exception("No kinect device could be found");
            }

            this.kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.kinectSensor.SkeletonStream.Enable();
            this.kinectSensor.ColorFrameReady += this.OnKinectColorFrame;
            this.kinectSensor.SkeletonFrameReady += this.OnKinectSkeletonFrame;

            // Texture
            this.kinectTexture = new Texture2D(graphics.GraphicsDevice, kinectSensor.ColorStream.FrameWidth, kinectSensor.ColorStream.FrameHeight);
            this.kinectColorPixels = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];
            this.kinectColorPixelsRgba = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];

            this.kinectSensor.Start();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Textures
            this.textures = new Dictionary<string, Texture2D>();
            this.textures["whitepixel"] = new Texture2D(this.GraphicsDevice, 1, 1);
            this.textures["whitepixel"].SetData(new byte[] { (byte)255, (byte)255, (byte)255, (byte)255 });

            // Fonts
            this.fonts = new Dictionary<string, SpriteFont>();
            this.fonts["main"] = Content.Load<SpriteFont>(@"Font");
        }

        protected override void UnloadContent()
        {
            this.kinectSensor.Stop();
            this.kinectSensor.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            this.spriteBatch.Begin();

            lock (this.kinectLock)
            {
                this.spriteBatch.Draw(this.kinectTexture, Vector2.Zero, Color.White);

                if (this.skeletons != null)
                {
                    foreach (var skeleton in this.skeletons)
                    {
                        this.DrawSkeleton(this.spriteBatch, skeleton);
                    }
                }
            }
            
            this.spriteBatch.End();

            base.Draw(gameTime);
        }

        protected void DrawSkeleton(SpriteBatch spriteBatch, Skeleton skeleton)
        {
            if (skeleton != null)
            {
                foreach (Joint joint in skeleton.Joints)
                {
                    var position = this.kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, this.kinectSensor.ColorStream.Format);
                    spriteBatch.Draw(textures["whitepixel"], new Rectangle((int)position.X - 5, (int)position.Y - 5, 10, 10), Color.Red);
                }

                var head = skeleton.Joints[JointType.Head];
                if (head != null)
                {
                    var headPosition = this.kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(head.Position, this.kinectSensor.ColorStream.Format);
                    spriteBatch.DrawString(this.fonts["main"], "HEAD", new Vector2(headPosition.X, headPosition.Y - 20), Color.Red);
                }
                
            }
        }


        protected void OnKinectColorFrame(object sender, ColorImageFrameReadyEventArgs frameArgs)
        {
            using (var frame = frameArgs.OpenColorImageFrame())
            {
                if (frame == null) return;

                frame.CopyPixelDataTo(this.kinectColorPixelsRgba);

                // Convert RGBA -> BGRA
                for (int i = 0; i < this.kinectColorPixels.Length; i += 4)
                {
                    this.kinectColorPixels[i + 0] = this.kinectColorPixelsRgba[i + 2];
                    this.kinectColorPixels[i + 1] = this.kinectColorPixelsRgba[i + 1];
                    this.kinectColorPixels[i + 2] = this.kinectColorPixelsRgba[i];
                    this.kinectColorPixels[i + 3] = (byte) 255;
                }

                lock (this.kinectLock)
                {
                    this.kinectTexture.SetData(this.kinectColorPixels);
                }
            }
        }

        protected void OnKinectSkeletonFrame(object sender, SkeletonFrameReadyEventArgs frameArgs)
        {
            using (var frame = frameArgs.OpenSkeletonFrame())
            {
                if (frame == null) return;

                if (this.skeletonData == null || this.skeletonData.Length != frame.SkeletonArrayLength)
                {
                    this.skeletonData = new Skeleton[frame.SkeletonArrayLength];
                }
                if (this.skeletons == null || this.skeletons.Length != frame.SkeletonArrayLength)
                {
                    this.skeletons = new Skeleton[frame.SkeletonArrayLength];
                }

                frame.CopySkeletonDataTo(this.skeletonData);

                if (this.skeletonData == null) return;

                lock (this.kinectLock)
                {
                    for (int i = 0; i < this.skeletonData.Length; i++)
                    {
                        var skeleton = this.skeletonData[i];
                        if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
                        {
                            this.skeletons[i] = this.skeletonData[i];
                        }
                    }
                }
            }
        }
    }
}
