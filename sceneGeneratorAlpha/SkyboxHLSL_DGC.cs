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
    public struct VertexPosition
    {
        Vector3 Position;
        public VertexPosition(Vector3 position)
        {
            this.Position = position;
        }
        public static readonly VertexElement[] VertexElements =
            {new VertexElement(0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default,
            VertexElementUsage.Position, 0)};
        public static readonly int SizeInBytes = sizeof(float) * 3;
    }
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SkyboxHLSL_DGC : Microsoft.Xna.Framework.DrawableGameComponent
    {
        Cameras.CameraInterface camera;
        TextureCube textureCube;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        Effect effect;
        VertexDeclaration vertexDeclaration;

        public SkyboxHLSL_DGC(Game game)
            : base(game)
        {
            // TODO: Construct any child components here
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            camera = (Cameras.CameraInterface)Game.Services.GetService(typeof(Cameras.CameraInterface));
            this.DrawOrder = int.MinValue;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            textureCube = Game.Content.Load<TextureCube>("skyboxTexture");
            effect = Game.Content.Load<Effect>("skybox");
            initCube();

            base.LoadContent();
        }

        private void initCube()
        {
            VertexPosition[] vertices = new VertexPosition[8];
            int[] index = new int[36];
            int i = 0,
                j = 0;

            vertices[i++] = new VertexPosition(new Vector3(-1, 1, -1));
            vertices[i++] = new VertexPosition(new Vector3(1, 1, -1));
            vertices[i++] = new VertexPosition(new Vector3(-1, -1, -1));
            vertices[i++] = new VertexPosition(new Vector3(1, -1, -1));

            vertices[i++] = new VertexPosition(new Vector3(-1, 1, 1));
            vertices[i++] = new VertexPosition(new Vector3(1, 1, 1));
            vertices[i++] = new VertexPosition(new Vector3(-1, -1, 1));
            vertices[i++] = new VertexPosition(new Vector3(1, -1, 1));

            // front face
            index[j++] = 2;
            index[j++] = 0;
            index[j++] = 1;
            index[j++] = 2;
            index[j++] = 1;
            index[j++] = 3;
            // back face
            index[j++] = 7;
            index[j++] = 5;
            index[j++] = 4;
            index[j++] = 7;
            index[j++] = 4;
            index[j++] = 6;
            // upper face
            index[j++] = 0;
            index[j++] = 4;
            index[j++] = 5;
            index[j++] = 0;
            index[j++] = 5;
            index[j++] = 1;
            // lower face
            index[j++] = 6;
            index[j++] = 2;
            index[j++] = 3;
            index[j++] = 6;
            index[j++] = 3;
            index[j++] = 7;
            // right face
            index[j++] = 3;
            index[j++] = 1;
            index[j++] = 5;
            index[j++] = 3;
            index[j++] = 5;
            index[j++] = 7;
            // left face
            index[j++] = 6;
            index[j++] = 4;
            index[j++] = 0;
            index[j++] = 6;
            index[j++] = 0;
            index[j++] = 2;

            vertexBuffer = new VertexBuffer(GraphicsDevice,
                VertexPosition.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(int),
                index.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(index);

            vertexDeclaration = new VertexDeclaration(GraphicsDevice,
                VertexPosition.VertexElements);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // TODO: Add your update code here

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            effect.CurrentTechnique = effect.Techniques["Skybox"];
            effect.Parameters["World"].SetValue(Matrix.CreateTranslation(camera.Position));
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["TextureCube"].SetValue(textureCube);

            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPosition.SizeInBytes);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.VertexDeclaration = vertexDeclaration;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    8, 0, 12);
                pass.End();
            }
            effect.End();
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            base.Draw(gameTime);
        }
    }
}