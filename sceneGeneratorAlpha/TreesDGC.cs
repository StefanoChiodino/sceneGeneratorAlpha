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
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TreesDGC : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public struct bb
        {
            public Vector3 position;
            public float size;
            public int textureIndex;
            public bb(Vector3 _position, float _size, int _textureIndex)
            {
                position = _position;
                size = _size;
                textureIndex = _textureIndex;
            }
        }

        BasicEffect basicEffect;
        Matrix world;
        Cameras.CameraInterface camera;
        // rappresents billboards, xyz for position and w for size
        bb[] bbList;
        Texture2D texture;
        int textureNumber;
        VertexPositionTexture[] vertexList;
        VertexDeclaration vertexDeclaration;
        int[] vertexIndex;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;

        public TreesDGC(Game game, bb[] _bbList, Texture2D _texture, Matrix _world, int _textureNumber)
            : base(game)
        {
            textureNumber = _textureNumber;
            bbList = _bbList;
            texture = _texture;
            world = _world;
        }

        void initBBVertices()
        {
            vertexList = new VertexPositionTexture[bbList.Length * 4];
            vertexIndex = new int[bbList.Length * 6];
            Matrix bbMatrix;
            int i = 0,
                j = 0;
            VertexPositionTexture vertexUL = new VertexPositionTexture(),
                vertexUR = new VertexPositionTexture(),
                vertexDL = new VertexPositionTexture(),
                vertexDR = new VertexPositionTexture();

            foreach (bb currentBB in bbList)
            {
                vertexUL.TextureCoordinate = new Vector2((float)currentBB.textureIndex / (float)textureNumber, 0);
                vertexUR.TextureCoordinate = new Vector2(((float)currentBB.textureIndex + 1) / (float)textureNumber, 0);
                vertexDL.TextureCoordinate = new Vector2((float)currentBB.textureIndex / (float)textureNumber, 1);
                vertexDR.TextureCoordinate = new Vector2(((float)currentBB.textureIndex + 1) / (float)textureNumber, 1);

                bbMatrix = Matrix.CreateConstrainedBillboard(currentBB.position,
                    camera.Position, Vector3.Up, null, null);
                vertexUL.Position = new Vector3(-texture.Width / 2 * currentBB.size / textureNumber,
                    texture.Height / 2 * currentBB.size, 0);
                vertexUR.Position = new Vector3(texture.Width / 2 * currentBB.size / textureNumber,
                    texture.Height / 2 * currentBB.size, 0);
                vertexDL.Position = new Vector3(-texture.Width / 2 * currentBB.size / textureNumber,
                    -texture.Height / 2 * currentBB.size, 0);
                vertexDR.Position = new Vector3(texture.Width / 2 * currentBB.size / textureNumber,
                    -texture.Height / 2 * currentBB.size, 0);
                vertexUL.Position = Vector3.Transform(vertexUL.Position, bbMatrix);
                vertexUR.Position = Vector3.Transform(vertexUR.Position, bbMatrix);
                vertexDL.Position = Vector3.Transform(vertexDL.Position, bbMatrix);
                vertexDR.Position = Vector3.Transform(vertexDR.Position, bbMatrix);
                //adding 4 vertices
                vertexList[i++] = vertexUL;
                vertexList[i++] = vertexUR;
                vertexList[i++] = vertexDL;
                vertexList[i++] = vertexDR;
                //first triangle
                vertexIndex[j++] = i - 4;
                vertexIndex[j++] = i - 2;
                vertexIndex[j++] = i - 3;
                //second triangle
                vertexIndex[j++] = i - 2;
                vertexIndex[j++] = i - 1;
                vertexIndex[j++] = i - 3;
            }
            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(int),
                vertexIndex.Length, BufferUsage.None);
            indexBuffer.SetData(vertexIndex);

            vertexBuffer = new VertexBuffer(GraphicsDevice,
                VertexPositionNormalTexture.SizeInBytes * vertexList.Length, BufferUsage.None);
            vertexBuffer.SetData(vertexList);
        }        

        protected override void LoadContent()
        {
            basicEffect = new BasicEffect(GraphicsDevice, null);
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);
            camera = (Cameras.CameraInterface)Game.Services.GetService(typeof(Cameras.CameraInterface));
            initBBVertices();

            GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

 	         base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            initBBVertices();
            this.DrawOrder = -(int)Vector3.Distance(camera.Position, Vector3.Transform(Vector3.Zero, world));
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            basicEffect.World = world;
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;
            //GraphicsDevice.RenderState.AlphaBlendEnable = true;
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = true;
            GraphicsDevice.RenderState.AlphaFunction = CompareFunction.GreaterEqual;
            GraphicsDevice.RenderState.ReferenceAlpha = 254;
            
            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionTexture.SizeInBytes);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.VertexDeclaration = vertexDeclaration;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexList.Length,
                    0, vertexIndex.Length / 3);
                pass.End();
            }
            basicEffect.End();
            //GraphicsDevice.RenderState.AlphaBlendEnable = false;
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            GraphicsDevice.RenderState.AlphaTestEnable = false;

            base.Draw(gameTime);
        }
    }
}