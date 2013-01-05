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
    public class CloudsDGC : Microsoft.Xna.Framework.DrawableGameComponent
    {
        int numParticles;
        float scale;
        Matrix world;
        Cameras.CameraInterface camera;
        Texture2D texture;
        Vector3 position;
        float density;

        VertexPositionTexture[] vertexList;
        VertexDeclaration vertexDeclaration;
        int[] vertexIndex;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        Random random;
        const float maxAlpha = 50;
        BasicEffect basicEffect;
        List<Vector3> particleRelativePosition;

        public CloudsDGC(Game game, Vector3 _position, int _numParticles, float _scale, float _density)
            : base(game)
        {
            density = _density;
            position = _position;
            scale = _scale;
            numParticles = _numParticles;
            world = Matrix.CreateTranslation(position);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            texture = generateTexture();
            //texture = textureFromAlphaMap(cloudAlphaMap);
            particleRelativePosition = new List<Vector3>();
            random = new Random();
            base.Initialize();
        }

        void placeBB()
        {
            vertexList = new VertexPositionTexture[numParticles * 4];
            vertexIndex = new int[numParticles * 6];
            Matrix bbMatrix;
            int i = 0,
                j = 0;
            Vector3 particlePosition,
                particleDirection;
            VertexPositionTexture vertexUL = new VertexPositionTexture(),
                vertexUR = new VertexPositionTexture(),
                vertexDL = new VertexPositionTexture(),
                vertexDR = new VertexPositionTexture();

            vertexUL.TextureCoordinate = new Vector2(0, 0);
            vertexUR.TextureCoordinate = new Vector2(1, 0);
            vertexDL.TextureCoordinate = new Vector2(0, 1);
            vertexDR.TextureCoordinate = new Vector2(1, 1);

            for (int h = 0; h < numParticles; h++)
            {
                //starting from cloud center
                particlePosition = position;
                //rotating a forward vector slightly on the X axis
                particleDirection = Vector3.Transform(Vector3.Forward, Matrix.CreateRotationX
                    (((float)random.NextDouble() - 0.5f) * 0.6f));
                // and freely spinning on Y axis
                particleDirection = Vector3.Transform(particleDirection, Matrix.CreateRotationY
                    ((float)random.NextDouble() * 10.0f));
                particlePosition += particleDirection * (float)random.NextDouble() * density;
                particleRelativePosition.Add(particlePosition);
                bbMatrix = Matrix.CreateBillboard(particlePosition,
                    camera.Position, camera.UpVector, camera.ForwardVector);
                vertexUL.Position = new Vector3(-scale / 2, scale / 2, 0);
                vertexUR.Position = new Vector3(scale / 2, scale / 2, 0);
                vertexDL.Position = new Vector3(-scale / 2, -scale / 2, 0);
                vertexDR.Position = new Vector3(scale / 2, -scale / 2, 0);
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

        Texture2D textureFromAlphaMap(Texture2D alphaMap)
        {
            Color[] alphaMapColors = new Color[alphaMap.Width * alphaMap.Height];
            alphaMap.GetData<Color>(alphaMapColors);
            Color currentColor = Color.White;

            for (int i = 0; i < alphaMapColors.Length; i++)
            {
                currentColor.A = alphaMapColors[i].R;
                alphaMapColors[i] = currentColor;
            }
            alphaMap.SetData<Color>(alphaMapColors);

            return alphaMap;
        }


        Texture2D generateTexture()
        {
            int lenght = 256;
            Color[] colors = new Color[lenght * lenght];
            Vector2 textureCenter = new Vector2(lenght / 2, lenght / 2);
            float radius = lenght / 2;
            float distance;
            Vector2 center = new Vector2(radius, radius);
            float alpha;

            for (int i = 0; i < lenght; i++)
            {
                for (int j = 0; j < lenght; j++)
                {
                    colors[j + i * lenght] = Color.White;
                    distance = Vector2.Distance(center, new Vector2(i, j));
                    if (Vector2.Distance(new Vector2(i, j), textureCenter) < radius)
                    {
                        alpha = (radius - distance) / radius * maxAlpha;
                        colors[j + i * lenght].A = (byte)alpha;
                    }
                    else
                        colors[j + i * lenght].A = 0;
                }
            }
            Texture2D texture = new Texture2D(Game.GraphicsDevice, lenght, lenght);
            texture.SetData<Color>(colors);

            return texture;
        }


        protected override void LoadContent()
        {
            camera = (Cameras.CameraInterface)Game.Services.GetService(typeof(Cameras.CameraInterface));
            vertexDeclaration = new VertexDeclaration(GraphicsDevice, VertexPositionTexture.VertexElements);
            placeBB();
            basicEffect = new BasicEffect(GraphicsDevice, null);

            //GraphicsDevice.RenderState.SourceBlend = Blend.SourceAlpha;
            //GraphicsDevice.RenderState.DestinationBlend = Blend.InverseSourceAlpha;

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            updateBB();
            this.DrawOrder = -(int)Vector3.Distance(camera.Position, position);

            base.Update(gameTime);
        }

        void updateBB()
        {
            Matrix bbMatrix;
            int i = 0,
                j = 0;
            VertexPositionTexture vertexUL = new VertexPositionTexture(),
                vertexUR = new VertexPositionTexture(),
                vertexDL = new VertexPositionTexture(),
                vertexDR = new VertexPositionTexture();

            vertexUL.TextureCoordinate = new Vector2(0, 0);
            vertexUR.TextureCoordinate = new Vector2(1, 0);
            vertexDL.TextureCoordinate = new Vector2(0, 1);
            vertexDR.TextureCoordinate = new Vector2(1, 1);

            //sort position base on the distance from the camera. 
            //furter position to closest so alpha is added correctly
            particleRelativePosition.Sort(delegate(Vector3 x, Vector3 y)
            {
                return Vector3.Distance(y, camera.Position).CompareTo(Vector3.Distance(x, camera.Position)) ;
            });
            for (int h = 0; h < numParticles; h++)
            {
                bbMatrix = Matrix.CreateBillboard(particleRelativePosition.ElementAt(h),
                    camera.Position, camera.UpVector, camera.ForwardVector);
                vertexUL.Position = new Vector3(-scale / 2, scale / 2, 0);
                vertexUR.Position = new Vector3(scale / 2, scale / 2, 0);
                vertexDL.Position = new Vector3(-scale / 2, -scale / 2, 0);
                vertexDR.Position = new Vector3(scale / 2, -scale / 2, 0);
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

        public override void Draw(GameTime gameTime)
        {
            basicEffect.World = Matrix.Identity;
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;
            GraphicsDevice.RenderState.AlphaBlendEnable = true;
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = false;
            //GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = true;
            GraphicsDevice.RenderState.AlphaFunction = CompareFunction.GreaterEqual;
            GraphicsDevice.RenderState.ReferenceAlpha = 1;

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

            GraphicsDevice.RenderState.AlphaBlendEnable = false;
            GraphicsDevice.RenderState.AlphaTestEnable = false;
            //GraphicsDevice.RenderState.DepthBufferWriteEnable = true;
            //GraphicsDevice.RenderState.DepthBufferEnable = true;

            base.Draw(gameTime);
        }
    }
}