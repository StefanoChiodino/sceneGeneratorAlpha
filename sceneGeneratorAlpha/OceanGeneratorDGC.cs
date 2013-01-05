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
    public struct VertexPosition2Texture
    {
        Vector2 Position,
            Texture;
        public VertexPosition2Texture(Vector2 position, Vector2 texture)
        {
            this.Position = position;
            this.Texture = texture;
        }
        public static readonly VertexElement[] VertexElements =
        {
            new VertexElement(0, 0, VertexElementFormat.Vector2, VertexElementMethod.Default,
                VertexElementUsage.Position, 0),
            new VertexElement(0, sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementMethod.Default,
                VertexElementUsage.TextureCoordinate, 0)
        };
        public static readonly int SizeInBytes = sizeof(float) * 4;
    }
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class OceanGeneratorDGC : DrawableGameComponent
    {
        int elementsNumX;
        int elementsNumZ;
        float elementLenght;
        const float heightFactor = 0.3f;
        const float bumpStrenght = 0.6f;
        const float bumpTextureStretch = 3.0f;
        const float fresnelTerm = 0.7f;
        const int specularPowerTerm = 5;

        Texture2D bumpMap;
        TextureCube textureCube;
        VertexPosition2Texture[] vertices;
        VertexDeclaration vertexDeclatarion;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        Vector3 textureSize;
        int[] indices;
        Random random;

        Vector4 waveSpeed,
            waveHeight,
            waveLenght;
        Vector2[] waveDirection;

        Matrix world;

        Effect effect;
        //KeyboardState keyboardState;

        Cameras.CameraInterface camera;
        
        public OceanGeneratorDGC(Game game, Vector3 start, int elementsNumX,
            int elementsNumZ, float elementLenght, Vector4 waveSpeed, Vector4 waveHeight,
            Vector4 waveLenght, Vector2[] waveDirection)
            : base(game)
        {
            this.waveDirection = waveDirection;
            this.waveSpeed = waveSpeed;
            this.waveHeight = waveHeight;
            this.waveLenght = waveLenght;
            this.elementsNumX = elementsNumX;
            this.elementsNumZ = elementsNumZ;
            this.elementLenght = elementLenght;
            this.world = Matrix.CreateTranslation(start);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            textureSize = new Vector3(elementsNumX * elementLenght, 0, elementsNumZ * elementLenght);

            base.Initialize();
        }

        private void createBuffers()
        {
            vertexBuffer = new VertexBuffer(GraphicsDevice,
                VertexPosition2Texture.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData<VertexPosition2Texture>(vertices, 0, vertices.Length);

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(int),
                indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        protected override void  LoadContent()
        {
            random = new Random();

            effect = Game.Content.Load<Effect>("oceanGenerator");
            bumpMap = Game.Content.Load<Texture2D>("waterbumps");
            textureCube = Game.Content.Load<TextureCube>("skyboxTexture");
            effect.Parameters["BumpMap"].SetValue(bumpMap);
            effect.Parameters["BumpStrength"].SetValue(bumpStrenght);
            effect.Parameters["TextureCube"].SetValue(textureCube);
            effect.Parameters["TexStretch"].SetValue(bumpTextureStretch);
            effect.Parameters["FresnelTerm"].SetValue(fresnelTerm);
            effect.Parameters["SpecularPowerTerm"].SetValue(specularPowerTerm);

            effect.Parameters["WaveSpeeds"].SetValue(waveSpeed);
            effect.Parameters["WaveHeights"].SetValue(waveHeight);
            effect.Parameters["WaveLengths"].SetValue(waveLenght);
            effect.Parameters["WaveDir0"].SetValue(waveDirection[0]);
            effect.Parameters["WaveDir1"].SetValue(waveDirection[1]);
            effect.Parameters["WaveDir2"].SetValue(waveDirection[2]);
            effect.Parameters["WaveDir3"].SetValue(waveDirection[3]);

            vertices = initVertices();
            indices = initIndices();
            createBuffers();

            camera = (Cameras.CameraInterface)Game.Content.ServiceProvider.GetService
                (typeof(Cameras.CameraInterface));

 	        base.LoadContent();
        }

        public VertexPosition2Texture[] initVertices()
        {
            vertexDeclatarion = new VertexDeclaration(GraphicsDevice, VertexPosition2Texture.VertexElements);
            int totalVertices = ((elementsNumX + 1) * (elementsNumZ + 1)),
                i = 0;
            VertexPosition2Texture[] vertices = new VertexPosition2Texture[totalVertices];
            Vector3 position;
            Vector2 txtCoord;// 0 to 1

            for (int z = 0; z < elementsNumZ + 1; z++)
            {
                for (int x = 0; x < elementsNumX + 1; x++)
                {
                    position = new Vector3(x * elementLenght, 0, - z * elementLenght);
                    txtCoord = textureCoord(position);
                    txtCoord *= bumpTextureStretch;
                    position = Vector3.Transform(position, world);
                    vertices[i++] = new VertexPosition2Texture(new Vector2(position.X, position.Z), txtCoord);
                }
            }

            return vertices;
        }

        public int[] initIndices()
        {
            int[] indices = new int[((elementsNumX + 1) * 2 + 1) * elementsNumZ];

            int i = 0,
                z = 0,
                x = 0;
            while (z < elementsNumZ)
            {
                for (x = 0; x < (elementsNumX + 1); x++)
                {
                    indices[i++] = x + z * (elementsNumX + 1);
                    indices[i++] = x + (z + 1) * (elementsNumX + 1);
                }
                x--;
                indices[i++] = x + (z + 1) * (elementsNumX + 1);
                z++;

                if (z < elementsNumZ)
                {
                    for (x = elementsNumX; x >= 0; x--)
                    {
                        indices[i++] = x + (z + 1) * (elementsNumX + 1);
                        indices[i++] = x + z * (elementsNumX + 1);
                    }
                    x++;
                    indices[i++] = x + (z + 1) * (elementsNumX + 1);
                }
                z++;
            }
            return indices;
        }

        private Vector2 textureCoord(Vector3 point)
        {
            return new Vector2(point.X / textureSize.X,
                -point.Z / textureSize.Z);
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //keyboardState = Keyboard.GetState();

            //if (keyboardState.IsKeyDown(Keys.LeftShift))
            //{
            //    if (keyboardState.IsKeyDown(Keys.G))
            //        waveHeight.X *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.V))
            //        waveHeight.X *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.H))
            //        waveHeight.Y *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.B))
            //        waveHeight.Y *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.J))
            //        waveHeight.Z *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.N))
            //        waveHeight.Z *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.K))
            //        waveHeight.W *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.M))
            //        waveHeight.W *= 0.99f;
            //}
            //else if (keyboardState.IsKeyDown(Keys.LeftAlt))
            //{
            //    if (keyboardState.IsKeyDown(Keys.G))
            //        waveLenght.X *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.V))
            //        waveLenght.X *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.H))
            //        waveLenght.Y *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.B))
            //        waveLenght.Y *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.J))
            //        waveLenght.Z *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.N))
            //        waveLenght.Z *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.K))
            //        waveLenght.W *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.M))
            //        waveLenght.W *= 0.99f;
            //}
            //else if (keyboardState.IsKeyDown(Keys.LeftControl))
            //{
            //    if (keyboardState.IsKeyDown(Keys.G))
            //        waveDirection[0] *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.V))
            //        waveDirection[0] *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.H))
            //        waveDirection[1] *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.B))
            //        waveDirection[1] *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.J))
            //        waveDirection[2] *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.N))
            //        waveDirection[2] *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.K))
            //        waveDirection[3] *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.M))
            //        waveDirection[3] *= 0.99f;
            //}
            //else
            //{
            //    if (keyboardState.IsKeyDown(Keys.G))
            //        waveSpeed.X *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.V))
            //        waveSpeed.X *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.H))
            //        waveSpeed.Y *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.B))
            //        waveSpeed.Y *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.J))
            //        waveSpeed.Z *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.N))
            //        waveSpeed.Z *= 0.99f;
            //    if (keyboardState.IsKeyDown(Keys.K))
            //        waveSpeed.W *= 1.01f;
            //    if (keyboardState.IsKeyDown(Keys.M))
            //        waveSpeed.W *= 0.99f;
            //}

            //Game.Window.Title = "speed: " + waveSpeed.ToString() + " dir: " + waveDirection[0].ToString()
            //    + waveDirection[1].ToString() + waveDirection[2].ToString() + waveDirection[3].ToString() +
            //    " height: " + waveHeight.ToString() + " lenght: " + waveLenght.ToString();

            this.DrawOrder = -(int)Vector3.Distance(camera.Position, Vector3.Transform(Vector3.Zero, world));

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalMilliseconds / 1000.0f;
            effect.CurrentTechnique = effect.Techniques["OceanGenerator"];
            effect.Parameters["World"].SetValue(world);
            effect.Parameters["View"].SetValue(camera.ViewMatrix);
            effect.Parameters["Projection"].SetValue(camera.ProjectionMatrix);
            effect.Parameters["CameraPosition"].SetValue(camera.Position);
            effect.Parameters["Time"].SetValue(time);

            GraphicsDevice.RenderState.CullMode = CullMode.None;

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPosition2Texture.SizeInBytes);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.VertexDeclaration = vertexDeclatarion;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, (elementsNumX + 1) * (elementsNumZ + 1),
                    0, indices.Length - 2);
                pass.End();
            }
            effect.End();

            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            base.Draw(gameTime);
        }
    }
}