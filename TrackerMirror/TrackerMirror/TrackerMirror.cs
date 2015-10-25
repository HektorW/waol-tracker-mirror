using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Kinect;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TrackerMirror.TrackerMirrorServer;


namespace TrackerMirror
{
    public class TrackerMirror : Game
    {
        public int Width { get; private set; } = 1600;
        public int Height { get; private set; } = 900;
        public Rectangle ScreenRectangle;

        // Graphics
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;

        // Blendstate
        private BlendState blendMultiply;

        // Fonts
        public Dictionary<string, SpriteFont> fonts;

        // Textures
        public Dictionary<string, Texture2D> textures; 

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
        private Skeleton firstSkeleton;


        // Server
        private Server server;

        // User
        private User user;


        public TrackerMirror()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            this.InitializeKinect();
            this.InitializeServer();
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

            // Streams
            this.kinectSensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
            this.kinectSensor.SkeletonStream.Enable();

            // Events
            this.kinectSensor.ColorFrameReady += this.OnKinectColorFrame;
            this.kinectSensor.SkeletonFrameReady += this.OnKinectSkeletonFrame;

            // Texture
            this.kinectTexture = new Texture2D(graphics.GraphicsDevice, kinectSensor.ColorStream.FrameWidth, kinectSensor.ColorStream.FrameHeight);
            this.kinectColorPixels = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];
            this.kinectColorPixelsRgba = new byte[this.kinectSensor.ColorStream.FramePixelDataLength];

            this.kinectSensor.Start();
        }

        protected void InitializeServer()
        {
            this.server = new Server();
            this.server.Start();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            this.graphics.PreferredBackBufferWidth = this.Width;
            this.graphics.PreferredBackBufferHeight = this.Height;
            this.graphics.IsFullScreen = true;
            this.graphics.ApplyChanges();

            this.ScreenRectangle = new Rectangle(0, 0, this.Width, this.Height);
            
            // Blendstate
            this.blendMultiply = new BlendState();
            this.blendMultiply.ColorBlendFunction = BlendFunction.Add;
            this.blendMultiply.ColorSourceBlend = Blend.DestinationColor;
            this.blendMultiply.ColorDestinationBlend = Blend.Zero;

            // Textures
            this.textures = new Dictionary<string, Texture2D>();
            this.textures["whitepixel"] = new Texture2D(this.GraphicsDevice, 1, 1);
            this.textures["whitepixel"].SetData(new byte[] { (byte)255, (byte)255, (byte)255, (byte)255 });
            this.textures["overlay"] = Content.Load<Texture2D>(@"overlay-200");

            // Fonts
            this.fonts = new Dictionary<string, SpriteFont>();
            this.fonts["main"] = Content.Load<SpriteFont>(@"Font");
        }

        protected override void UnloadContent()
        {
            this.kinectSensor.Stop();
            this.kinectSensor.Dispose();

            this.server.Stop();
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();


            var client = this.server.GetClosestClient();
            if (client == null)
            {
                this.user = null;
            }
            else
            {
                if (this.user == null || this.user.Client.ID != client.ID)
                {
                    this.user = new User(this, this.kinectSensor, client);
                }
            }

            if (this.user != null)
            {
                this.user.Skeleton = this.firstSkeleton;
                this.user.Update(gameTime);
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            
            lock (this.kinectLock)
            {
                // Set video data
                this.kinectTexture.SetData(this.kinectColorPixels);
            }

            //var b = new BlendState();


            //this.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            this.spriteBatch.Begin(SpriteSortMode.Immediate, this.blendMultiply);
            //this.spriteBatch.Begin(SpriteSortMode.Immediate);

            // Draw video
            this.spriteBatch.Draw(this.kinectTexture, this.ScreenRectangle, Color.White);

            //this.spriteBatch.End();
            //this.spriteBatch.Begin(SpriteSortMode.Immediate, this.blendMultiply);

            //this.spriteBatch.Draw(this.textures["whitepixel"], this.ScreenRectangle, new Color(130, 20, 75));


            if (this.skeletons != null)
            {
                //    foreach (var Skeleton in this.skeletons)
                //    {
                //        this.DrawSkeleton(this.spriteBatch, Skeleton);
                //    }
            }

            if (this.user != null)
            {
                this.user.Draw(this.spriteBatch);
            }

            this.spriteBatch.End(); // Finish blend drawing


            // Draw strings
            this.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            if (this.user != null)
            {
                this.user.DrawStrings(this.spriteBatch);
            }
            else
            {
                this.spriteBatch.DrawString(this.fonts["main"], "No user", Vector2.Zero, Color.White);
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
                    var position = this.SkeletonPointToScreen(joint.Position);
                    spriteBatch.Draw(textures["whitepixel"], new Rectangle((int)position.X - 5, (int)position.Y - 5, 10, 10), Color.Black);
                }
            }
        }


        public Vector2 SkeletonPointToScreen(SkeletonPoint point)
        {
            //var position = this.kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(point, this.kinectSensor.ColorStream.Format);
            //return new Vector2(position.X, position.Y);
            return new Vector2((((0.5f * point.X) + 0.5f) * (this.Width)), (((-0.5f * point.Y) + 0.5f) * (Height)));
        }

        protected void OnKinectColorFrame(object sender, ColorImageFrameReadyEventArgs frameArgs)
        {
            using (var frame = frameArgs.OpenColorImageFrame())
            {
                if (frame == null) return;

                frame.CopyPixelDataTo(this.kinectColorPixelsRgba);

                lock (this.kinectLock)
                {
                    // Convert RGBA -> BGRA
                    for (int i = 0; i < this.kinectColorPixels.Length; i += 4)
                    {
                        this.kinectColorPixels[i + 0] = this.kinectColorPixelsRgba[i + 2];
                        this.kinectColorPixels[i + 1] = this.kinectColorPixelsRgba[i + 1];
                        this.kinectColorPixels[i + 2] = this.kinectColorPixelsRgba[i];
                        this.kinectColorPixels[i + 3] = (byte)255;
                    }
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

                if (this.skeletonData != null)
                {
                    for (var i = 0; i < skeletonData.Length; i++) skeletonData[i] = null;
                }
                if (this.skeletons != null)
                {
                    for (var i = 0; i < this.skeletons.Length; i++) this.skeletons[i] = null;
                }

                frame.CopySkeletonDataTo(this.skeletonData);

                if (this.skeletonData == null) return;
                
                for (int i = 0; i < this.skeletonData.Length; i++)
                {
                    var skeleton = this.skeletonData[i];
                    if (skeleton != null && skeleton.TrackingState == SkeletonTrackingState.Tracked)
                    {
                        this.skeletons[i] = this.skeletonData[i];
                    }
                }

                lock (this.kinectLock)
                {
                    this.firstSkeleton = this.skeletons.FirstOrDefault(n => n != null);

                    if (this.user != null)
                    {
                        this.user.Skeleton = firstSkeleton;
                    }
                }
            }
        }
    }
}
