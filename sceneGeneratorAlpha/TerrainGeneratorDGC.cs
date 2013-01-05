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


namespace TerrainGenerator
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class TerrainGenerator : DrawableGameComponent
    {
        int numVertices;

        int mapHeight;
        int mapWidth;

        int elementsNumX;
        int elementsNumZ;
        float elementLenght;
        const float cosRotationFactor = 0.02f;
        const float cosRotationSpeed = 0.07f;
        const float textureFactor = 10.0f;

        Texture2D texture;
        VertexPositionNormalTexture[] vertices;
        VertexDeclaration vertexDeclatarion;
        VertexBuffer vertexBuffer;
        IndexBuffer indexBuffer;
        BasicEffect basicEffect;
        Vector3 textureSize;
        Texture2D heightTexture;
        float[,] heightData;
        const float heightFactor = 0.3f;
        //double time;
        //double rotation;
        int[] indices;

        Texture2D treeTexture;
        Texture2D treeMap;
        List<SceneGenerator.TreesDGC.bb> treeBB;
        bool[,] boolTreeMap;
        const float treeScale = 0.1f;
        const float treeScaleVariable = 1.0f;
        const int treeTexturesNum = 3;

        Random random;

        //Matrix CosRotationMatrix;
        Matrix world;

        //KeyboardState kayboardState;

        Cameras.CameraInterface camera;
        
        public TerrainGenerator(Game game, Vector3 start, int elementsNumX,
            int elementsNumZ, float elementLenght) : base(game)
        {
            this.elementsNumX = elementsNumX;
            this.elementsNumZ = elementsNumZ;
            this.elementLenght = elementLenght;
            world = Matrix.CreateTranslation (start);
        }

        private void createBuffers(VertexPositionNormalTexture[] vertices, int[] indices)
        {
            vertexBuffer = new VertexBuffer(GraphicsDevice,
                VertexPositionNormalTexture.SizeInBytes * vertices.Length, BufferUsage.WriteOnly);
            vertexBuffer.SetData(vertices);

            indexBuffer = new IndexBuffer(GraphicsDevice, typeof(int),
                indices.Length, BufferUsage.WriteOnly);
            indexBuffer.SetData(indices);
        }

        protected override void  LoadContent()
        {
            treeBB = new List<SceneGenerator.TreesDGC.bb>();
            //CosRotationMatrix = Matrix.Identity;
            random = new Random();
            basicEffect = new BasicEffect(this.GraphicsDevice, null);
            texture = Game.Content.Load<Texture2D>("grass");
            heightTexture = Game.Content.Load<Texture2D>("heightmap3");

            treeMap = Game.Content.Load<Texture2D>("treeMap");
            treeTexture = Game.Content.Load<Texture2D>("3treeTexture");
            boolTreeMap = loadTreeData(treeMap);

            mapHeight = heightTexture.Height;
            mapWidth = heightTexture.Width;
            heightData = loadHeightData(heightTexture);

            vertices = initVertices();
            indices = initIndices();
            vertices = normalize(vertices, indices);
            createBuffers(vertices, indices);

            camera = (Cameras.CameraInterface)Game.Content.ServiceProvider.GetService
                (typeof(Cameras.CameraInterface));

            Game.Components.Add(new SceneGenerator.TreesDGC(Game, treeBB.ToArray(), treeTexture, world, treeTexturesNum));

 	        base.LoadContent();
        }

        float[,] loadHeightData(Texture2D heightMap)
        {
            Color[] heightMapColors = new Color[mapHeight * mapWidth];
            heightMap.GetData<Color>(heightMapColors);
            float[,] data = new float[mapWidth, mapHeight];

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    data[x, y] = heightMapColors[x + y * mapWidth].R
                        + heightMapColors[x + y * mapWidth].G
                        + heightMapColors[x + y * mapWidth].B;
                    data[x, y] *= heightFactor;
                }
            }
            return data;
        }


        bool[,] loadTreeData(Texture2D treeMap)
        {
            Color[] treeMapColors = new Color[treeMap.Width * treeMap.Height];
            bool[,] boolTreeMap = new bool[treeMap.Width, treeMap.Height];
            treeMap.GetData<Color>(treeMapColors);

            for (int x = 0; x < treeMap.Width; x++)
            {
                for (int y = 0; y < treeMap.Height; y++)
                {
                    if (treeMapColors[x + y * treeMap.Width].R > 0)
                        boolTreeMap[x, y] = true;
                    else
                        boolTreeMap[x, y] = false;
                }
            }
            return boolTreeMap;
        }

        // position è compreso tra 0 ed 1
        public float getHeight(Vector2 position)
        {
            float height;
            float positionX = position.X * (heightTexture.Width - 1);
            float positionY = position.Y * (heightTexture.Height - 1);
            int lowX = (int)positionX;
            int lowY = (int)positionY;
            int highX = lowX + 1;
            int highY = lowY + 1;
            float xRelative = positionX - lowX;
            float yRelative = positionY - lowY;

            float height_xy = heightData[lowX, lowY];
            height = height_xy;
            if (highX < heightData.GetLength(0)
                && highY < heightData.GetLength(1))
            {
                height_xy *= (1 - xRelative) * (1 - yRelative);
                float height_Xy = heightData[highX, lowY];
                float height_xY = heightData[lowX, highY];
                float height_XY = heightData[highX, highY];
                height += xRelative * (1 - yRelative);
                height += yRelative * (1 - xRelative);
                height += xRelative * yRelative;
                }
            return height;
        }

        public VertexPositionNormalTexture[] initVertices()
        {
            vertexDeclatarion = new VertexDeclaration(GraphicsDevice, VertexPositionNormalTexture.VertexElements);
            int totalVertices = ((elementsNumX + 1) * (elementsNumZ + 1));
            VertexPositionNormalTexture[] vertices = new VertexPositionNormalTexture[totalVertices];
            Vector3 _position,
                _normal;
            Vector2 _txtCoord;// 0 to 1

            numVertices = 0;
            for (int z = 0; z < elementsNumZ + 1; z++)
            {
                for (int x = 0; x < elementsNumX + 1; x++)
                {
                    _position = new Vector3(x * elementLenght, 0, - z * elementLenght);
                    // dovrebbe rimanere zero prima di passare per la normalizzazione
                    _normal = Vector3.Zero;
                    _txtCoord = textureCoord(_position);
                    _position = Vector3.Transform(_position, world);
                    _position.Y = getHeight(_txtCoord);
                    vertices[numVertices++] = new VertexPositionNormalTexture(_position, _normal, _txtCoord * textureFactor);

                    if (boolTreeMap[(int)(_txtCoord.X * (treeMap.Width - 1)), (int)(_txtCoord.Y * (treeMap.Height - 1))] == true)
                    {
                        float scale = treeScale * (float)random.NextDouble() * treeScaleVariable;
                        _position.Y += treeTexture.Height / 2 * scale;
                        treeBB.Add(new SceneGenerator.TreesDGC.bb(_position, scale, (int)random.Next(treeTexturesNum)));
                    }
                }
            }

            return vertices;
        }

        public int[] initIndices()
        {
            int[] _indices = new int[((elementsNumX + 1) * 2 + 1) * elementsNumZ];

            int i = 0,
                z = 0,
                x = 0;
            while (z < elementsNumZ)
            {
                for (x = 0; x < (elementsNumX + 1); x++)
                {
                    _indices[i++] = x + z * (elementsNumX + 1);
                    _indices[i++] = x + (z + 1) * (elementsNumX + 1);
                }
                x--;
                _indices[i++] = x + (z + 1) * (elementsNumX + 1);
                z++;

                if (z < elementsNumZ)
                {
                    for (x = elementsNumX; x >= 0; x--)
                    {
                        _indices[i++] = x + (z + 1) * (elementsNumX + 1);
                        _indices[i++] = x + z * (elementsNumX + 1);
                    }
                    x++;
                    _indices[i++] = x + (z + 1) * (elementsNumX + 1);
                }
                z++;
            }
            return _indices;
        }

        public VertexPositionNormalTexture[] normalize(VertexPositionNormalTexture[] _vertices, int[] _indices)
        {
            Vector3 vector1, vector2, normal;
            bool parity = false ;

            for (int i = 2; i < _indices.Length; i++)
            {
                vector1 = Vector3.Subtract(_vertices[_indices[i - 1]].Position, _vertices[_indices[i]].Position);
                vector2 = Vector3.Subtract(_vertices[_indices[i - 2]].Position, _vertices[_indices[i]].Position);
                if (vector1 != Vector3.Zero
                    && vector2 != Vector3.Zero
                    && vector2 != vector1)
                {
                    normal = Vector3.Cross(vector1, vector2);
                    normal.Normalize();
                    if (parity)
                    {
                        normal *= -1.0f;
                    }
                    parity = !parity;

                    if (float.IsNaN(normal.X))
                    {
                        normal = Vector3.Up;
                    }
                    _vertices[_indices[i]].Normal += normal;
                    _vertices[_indices[i - 1]].Normal += normal;
                    _vertices[_indices[i - 2]].Normal += normal;
                }
            }
            for (int i = 0; i < numVertices; i++)
            {
                _vertices[i].Normal.Normalize();
            }
            return _vertices;
        }

        private Vector2 textureCoord(Vector3 point)
        {
            return new Vector2(point.X / textureSize.X,
                -point.Z / textureSize.Z);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            textureSize = new Vector3(elementsNumX * elementLenght, 0, elementsNumZ * elementLenght);

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            //kayboardState = Keyboard.GetState();
            //if (kayboardState.IsKeyDown(Keys.P))
            //{
            //    time += gameTime.ElapsedGameTime.Milliseconds * cosRotationSpeed;
            //    rotation = Math.Cos(MathHelper.ToRadians((float)time)) * cosRotationFactor;
            //    CosRotationMatrix *= Matrix.CreateRotationZ((float)rotation);
            //}

            this.DrawOrder = -(int)Vector3.Distance(camera.Position, Vector3.Transform(Vector3.Zero, world));

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.RenderState.CullMode = CullMode.None;
            //GraphicsDevice.RenderState.FillMode = FillMode.WireFrame;

            basicEffect.World = world;
            basicEffect.View = camera.ViewMatrix;
            basicEffect.Projection = camera.ProjectionMatrix;
            basicEffect.TextureEnabled = true;
            basicEffect.Texture = texture;

            basicEffect.EnableDefaultLighting();
            //basicEffect.DirectionalLight0.Direction = Vector3.Transform(Vector3.Down, CosRotationMatrix);
            basicEffect.DirectionalLight0.Direction = Vector3.Down;
            basicEffect.DirectionalLight0.Enabled = true;
            basicEffect.AmbientLightColor = new Vector3(0.0f, 0.0f, 0.0f);
            basicEffect.DirectionalLight1.Enabled = false;
            basicEffect.DirectionalLight2.Enabled = false;
            //basicEffect.SpecularColor = new Vector3(0.1f, 0.1f, 0.1f);
            basicEffect.SpecularColor = new Vector3(0, 0, 0);
            GraphicsDevice.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            GraphicsDevice.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            basicEffect.Begin();
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Begin();
                GraphicsDevice.Vertices[0].SetSource(vertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.VertexDeclaration = vertexDeclatarion;
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, (elementsNumX + 1) * (elementsNumZ + 1),
                    0, ((elementsNumX + 1) * 2 + 1) * elementsNumZ - 2);
                pass.End();
            }
            basicEffect.End();

            GraphicsDevice.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

 	         base.Draw(gameTime);
        }
    }
}