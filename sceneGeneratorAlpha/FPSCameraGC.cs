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

namespace Cameras
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    class FPSCameraGC : GameComponent, CameraInterface
    {
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Matrix rotation;
        Vector3 position;

        BoundingFrustum boundingFrustum;

        float pitch;
        float yaw;

        int middleViewportX,
            middleViewportY;

        const float nearPlane = 0.1f;
        const float farPlane = 10000.0f;
        const float movementFactor = 1.0f;
        const float rotationFactor = 0.05f;
        const float maxPitch = 90;

        MouseState mouseState,
            prevMouseState = Mouse.GetState();
        KeyboardState keyboardState,
            prevKeyboardState = Keyboard.GetState();

        public FPSCameraGC(Game game, Vector3 position)
            : base(game)
        {
            this.position = position;
            game.Services.AddService(typeof(CameraInterface), this);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            yaw = 0.0f;
            pitch = 0.0f;
            middleViewportX = Game.GraphicsDevice.Viewport.Width / 2;
            middleViewportY = Game.GraphicsDevice.Viewport.Height / 2;

            rotation = Matrix.Identity;

            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4,
                Game.GraphicsDevice.Viewport.Width / Game.GraphicsDevice.Viewport.Height,
                nearPlane, farPlane);

            updateViewMatrix();
            // TODO: Add your initialization logic here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            mouseState = Mouse.GetState();
            keyboardState = Keyboard.GetState();
            if (mouseState != prevMouseState && keyboardState.IsKeyDown(Keys.LeftShift))
            {
                yaw += (prevMouseState.X - mouseState.X) * rotationFactor;
                pitch += (prevMouseState.Y - mouseState.Y) * rotationFactor;
                if (yaw >= 360.0f)
                    yaw -= 360.0f;
                else if (yaw < 0 )
                    yaw += 360.0f;
                if (pitch >= maxPitch)
                    pitch = maxPitch;
                else if (pitch < - maxPitch)
                    pitch = - maxPitch;
                Mouse.SetPosition(middleViewportX, middleViewportY);
            }

            if (keyboardState.IsKeyDown(Keys.Up) || keyboardState.IsKeyDown(Keys.W))      //Forward
                move(Vector3.Forward);
            if (keyboardState.IsKeyDown(Keys.Down) || keyboardState.IsKeyDown(Keys.S))    //Backward
                move(Vector3.Backward);
            if (keyboardState.IsKeyDown(Keys.Right) || keyboardState.IsKeyDown(Keys.D))   //Right
                move(Vector3.Right);
            if (keyboardState.IsKeyDown(Keys.Left) || keyboardState.IsKeyDown(Keys.A))    //Left
                move(Vector3.Left);
            if (keyboardState.IsKeyDown(Keys.Q))                                     //Up
                move(Vector3.Up);
            if (keyboardState.IsKeyDown(Keys.Z))                                     //Down
                move(Vector3.Down);

            prevMouseState = Mouse.GetState();
            //prevKeyboardState = Keyboard.GetState();
            updateViewMatrix();
        }

        private void move(Vector3 direction)
        {
            position += movementFactor * Vector3.Transform(direction, rotation);
            updateViewMatrix();
        }

        public void updateViewMatrix()
        {
            rotation = Matrix.CreateRotationX(MathHelper.ToRadians(pitch))
                * Matrix.CreateRotationY(MathHelper.ToRadians(yaw));

            viewMatrix = Matrix.CreateLookAt(position, position + Vector3.Transform(Vector3.Forward, rotation),
                 Vector3.Transform(Vector3.Up, rotation));
            boundingFrustum = new BoundingFrustum(viewMatrix * projectionMatrix);
        }

        public Matrix ProjectionMatrix
        {
            get { return projectionMatrix; }
        }

        public Matrix ViewMatrix
        {
            get { return viewMatrix; }
        }

        public Vector3 Position
        {
            get { return position; }
        }

        public Vector3 ForwardVector
        {
            get
            {
                return Vector3.Transform(Vector3.Forward, rotation);
            }
        }

        public Vector3 UpVector
        {
            get
            {
                return Vector3.Transform(Vector3.Up, rotation);
            }
        }

        public BoundingFrustum BoundingFrustum
        {
            get
            {
                return boundingFrustum;
            }
        }

    }
}
