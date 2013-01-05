using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

namespace SceneGenerator
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        Cameras.CameraInterface camera;
        float fps;

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            if (Keyboard.GetState().IsKeyDown(Keys.LeftShift))
            {
                DisplayMode displayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                graphics.PreferredBackBufferFormat = displayMode.Format;
                graphics.PreferredBackBufferHeight = displayMode.Height;
                graphics.PreferredBackBufferWidth = displayMode.Width;
                graphics.ToggleFullScreen();
            }
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Components.Add(new Cameras.FPSCameraGC(this, Vector3.Up * 200 + Vector3.Right * 100+
                Vector3.Forward * 300));
            Vector2[] waveDirection = new Vector2[4];
            waveDirection[0] = new Vector2(-0.1f, -0.9f);
            waveDirection[1] = new Vector2(-0.1f, -0.9f);
            waveDirection[2] = new Vector2(-0.2f, -0.8f);
            waveDirection[3] = new Vector2(-0.2f, -0.8f);
            Components.Add(new OceanGeneratorDGC(this, Vector3.Up * 15, 256, 256, 1.0f,
                new Vector4(0.6f, 0.8f, 0.9f, 1.4f), new Vector4(3.0f, 0.0f, 0.0f, 0.0f),
                new Vector4(10.0f, 5.0f, 3.0f, 1.0f), waveDirection));
            Components.Add(new TerrainGenerator.TerrainGenerator(this, Vector3.Zero, 256, 256, 1.0f));
            Components.Add(new CloudsDGC(this, Vector3.Up * 200 + Vector3.Forward * 100 + Vector3.Right * 200, 150, 30.0f, 100.0f));
            Components.Add(new CloudsDGC(this, Vector3.Up * 600 + Vector3.Forward * 100 + Vector3.Right * 100, 1000, 100.0f, 1000.0f));
            Components.Add(new CloudsDGC(this, Vector3.Up * 200 + Vector3.Forward * 200 + Vector3.Right * 100, 150, 10.0f, 20.0f));
            Components.Add(new SceneGenerator.SkyboxHLSL_DGC(this));
            camera = (Cameras.CameraInterface)this.Services.GetService(typeof(Cameras.CameraInterface));

            fps = 0;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //check for the next division
            if (gameTime.ElapsedRealTime.Milliseconds > 0)
                // updated with 80% of the new value plus 20% of the old one
                fps = 1000/(float)gameTime.ElapsedRealTime.Milliseconds * 0.8f + fps * 0.2f;
            Window.Title = fps.ToString();

            base.Draw(gameTime);
        }
    }
}
