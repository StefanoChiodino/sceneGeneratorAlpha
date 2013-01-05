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
    class QuaternionCameraGC : GameComponent, CameraInterface
    {
        Matrix viewMatrix;
        Matrix projectionMatrix;
        Quaternion rotation;
        Vector3 position;

        Quaternion [] quaternionArray ;
        Vector3 [] positionArray;
        Vector3 bezMidPos;
        Vector3 bezStartPos;
        Vector3 bezEndPos;
        Quaternion bezStartTargetVector;
        Quaternion bezEndTargetVector;
        float bezTime;
        const float bezStep = 0.01f;

        BoundingFrustum boundingFrustum;

        int middleViewportX,
            middleViewportY;

        const float nearPlane = 1.0f;
        const float farPlane = 1000.0f;
        const float movementFactor = 2.0f;
        const float rotationFactor = 0.03f;

        MouseState mouseState,
            prevMouseState = Mouse.GetState();
        KeyboardState keyboardState,
            prevKeyboardState = Keyboard.GetState();

        public QuaternionCameraGC(Game game, Vector3 _position)
            : base(game)
        {
            position = _position;
            bezEndPos = _position;
            bezEndTargetVector = Quaternion.Identity;
            game.Services.AddService(typeof(CameraInterface), this);
        }

        public void bezInitialization(Vector3 _position, Quaternion _quaternion)
        {
            bezStartPos = position;
            bezStartTargetVector = rotation;
            bezEndPos = _position;
            bezEndTargetVector = _quaternion;

            bezMidPos = (bezStartPos + bezStartPos) / 2.0f;
            bezMidPos += Vector3.Transform((bezEndPos - bezMidPos), Matrix.CreateRotationX(MathHelper.PiOver2));
            bezTime = 0.0f;
        }

        public void bezUpdate()
        {
            bezTime += bezStep;
            if (bezTime <= 1.0f)
            {
                position = Bezier(bezStartPos, bezMidPos, bezEndPos, MathHelper.SmoothStep(0, 1, bezTime));
                rotation = Quaternion.Lerp(bezStartTargetVector, bezEndTargetVector, MathHelper.SmoothStep(0, 1, bezTime));
            }
        }

        private Vector3 Bezier(Vector3 bezStartPos, Vector3 bezMidPos, Vector3 bezEndPos, float bezTime)
        {
            return (bezStartPos * (1.0f - bezTime) * (1.0f - bezTime))
                + (2 * bezMidPos * (1.0f - bezTime) * bezTime)
                + (bezEndPos * bezTime * bezTime);
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        public override void Initialize()
        {
            rotation = Quaternion.Identity;

            position = new Vector3 (0.0f, 0.0f, 100.0f);

            quaternionArray = new Quaternion[5];
            positionArray = new Vector3[5];
            for (int i = 0; i < 5; i++)
            {
                quaternionArray[i] = rotation;
                positionArray[i] = position;
            }

            middleViewportX = Game.GraphicsDevice.Viewport.Width / 2;
            middleViewportY = Game.GraphicsDevice.Viewport.Height / 2;

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
            if (bezTime > 1.0f)
            {
                mouseState = Mouse.GetState();
                keyboardState = Keyboard.GetState();
                if (mouseState != prevMouseState)
                {
                    rotation *= Quaternion.CreateFromAxisAngle(Vector3.Up,
                        MathHelper.ToRadians((prevMouseState.X - mouseState.X) * rotationFactor))
                        * Quaternion.CreateFromAxisAngle(Vector3.Right,
                        MathHelper.ToRadians((prevMouseState.Y - mouseState.Y) * rotationFactor));
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
                if (keyboardState.IsKeyDown(Keys.D1))                                     //Down
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        positionArray[0] = position;
                        quaternionArray[0] = rotation;
                    }
                    else
                    {
                        bezInitialization(positionArray[0], quaternionArray[0]);
                    }
                else if (keyboardState.IsKeyDown(Keys.D2))                                     //Down
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        positionArray[1] = position;
                        quaternionArray[1] = rotation;
                    }
                    else
                    {
                        bezInitialization(positionArray[1], quaternionArray[1]);
                    }
                else if (keyboardState.IsKeyDown(Keys.D3))                                     //Down
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        positionArray[2] = position;
                        quaternionArray[2] = rotation;
                    }
                    else
                    {
                        bezInitialization(positionArray[2], quaternionArray[2]);
                    }
                else if (keyboardState.IsKeyDown(Keys.D4))                                     //Down
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        positionArray[3] = position;
                        quaternionArray[3] = rotation;
                    }
                    else
                    {
                        bezInitialization(positionArray[3], quaternionArray[3]);
                    }
                else if (keyboardState.IsKeyDown(Keys.D5))                                     //Down
                    if (keyboardState.IsKeyDown(Keys.LeftControl))
                    {
                        positionArray[4] = position;
                        quaternionArray[4] = rotation;
                    }
                    else
                    {
                        bezInitialization(positionArray[4], quaternionArray[4]);
                    }
                else 

                Mouse.SetPosition(middleViewportX, middleViewportY);
                prevMouseState = Mouse.GetState();
                //prevKeyboardState = Keyboard.GetState();
            }
            else
                bezUpdate();
            updateViewMatrix();
        }

        private void move(Vector3 direction)
        {
            position += movementFactor * Vector3.Transform(direction, rotation);
            updateViewMatrix();
        }

        public void updateViewMatrix()
        {
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
