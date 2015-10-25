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
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

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

        protected void InitializeServer()
        {
            this.server = new Server();
            this.server.Start();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            
            this.graphics.PreferredBackBufferWidth = 641;
            this.graphics.PreferredBackBufferHeight = this.kinectTexture.Height;
            this.graphics.ApplyChanges();

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var client = this.server.GetClosestClient();
            //if (client != null && (this.user == null || this.user.Client.ID != client.ID))
            //{
                //this.user = new User(this, this.kinectSensor, client);
            //}
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
                this.user.Update(gameTime);
                //if (client == null)
                //{
                //    this.user.Deactivate();
                //}

                //if (this.user.Active)
                //{
                //    this.user.Update(gameTime);
                //}
                //else
                //{
                //    this.user = null;
                //}
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.White);

            var blendState = new BlendState();
            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.ColorSourceBlend = Blend.DestinationColor;
            blendState.ColorDestinationBlend = Blend.Zero;

            //this.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            this.spriteBatch.Begin(SpriteSortMode.Immediate, blendState);

            lock (this.kinectLock)
            {
                // Set video data
                this.kinectTexture.SetData(this.kinectColorPixels);
            }

            // Draw video
            this.spriteBatch.Draw(this.kinectTexture, Vector2.Zero, Color.White);
            this.spriteBatch.Draw(this.textures["whitepixel"], new Rectangle(0, 0, 640, 480), new Color(130, 20, 75));

            if (this.skeletons != null)
            {
                foreach (var Skeleton in this.skeletons)
                {
                    this.DrawSkeleton(this.spriteBatch, Skeleton);
                }
            }

            if (this.user != null)
            {
                this.user.Draw(this.spriteBatch);
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
                    var position = this.kinectSensor.CoordinateMapper.MapSkeletonPointToColorPoint(joint.Position, this.kinectSensor.ColorStream.Format);
                    spriteBatch.Draw(textures["whitepixel"], new Rectangle((int)position.X - 5, (int)position.Y - 5, 10, 10), new Color(Color.Red, 0.3f));
                }
            }
        }

        protected Vector2 GetCenteredVector(string str, SpriteFont font, Vector2 origin)
        {
            var size = font.MeasureString(str);
            return origin - size / 2;
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
