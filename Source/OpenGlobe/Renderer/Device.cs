#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Diagnostics;
using OpenGlobe.Core;
using OpenGlobe.Renderer.GL3x;
using OpenTK.Graphics.OpenGL;

namespace OpenGlobe.Renderer
{
    public enum WindowType
    {
        Default = 0,
        FullScreen = 1
    }

    public class Device
    {
        private Context m_Context;
        public Device(Context context)
        {
            m_Context = context;
            context.Device = this;

            GL.GetInteger(GetPName.MaxVertexAttribs, out m_MaximumNumberOfVertexAttributes);
            GL.GetInteger(GetPName.MaxCombinedTextureImageUnits, out m_NumberOfTextureUnits);
            GL.GetInteger(GetPName.MaxColorAttachments, out m_MaximumNumberOfColorAttachments);

            ///////////////////////////////////////////////////////////////

            m_Extensions = new ExtensionsGL3x();

            ///////////////////////////////////////////////////////////////

            LinkAutomaticUniformCollection linkAutomaticUniforms = new LinkAutomaticUniformCollection();

            for (int i = 0; i < context.TextureUnits.Count; ++i)
            {
                linkAutomaticUniforms.Add(new TextureUniform(i));
            }

            m_LinkAutomaticUniforms = linkAutomaticUniforms;

            ///////////////////////////////////////////////////////////////

            DrawAutomaticUniformFactoryCollection drawAutomaticUniformFactories = new DrawAutomaticUniformFactoryCollection();

            drawAutomaticUniformFactories.Add(new SunPositionUniformFactory());
            drawAutomaticUniformFactories.Add(new LightPropertiesUniformFactory());
            drawAutomaticUniformFactories.Add(new CameraLightPositionUniformFactory());
            drawAutomaticUniformFactories.Add(new CameraEyeUniformFactory());
            drawAutomaticUniformFactories.Add(new CameraEyeHighUniformFactory());
            drawAutomaticUniformFactories.Add(new CameraEyeLowUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewPerspectiveMatrixRelativeToEyeUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewMatrixRelativeToEyeUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewPerspectiveMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewOrthographicMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelViewMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new OrthographicMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportOrthographicMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportUniformFactory());
            drawAutomaticUniformFactories.Add(new InverseViewportDimensionsUniformFactory());
            drawAutomaticUniformFactories.Add(new ViewportTransformationMatrixUniformFactory());
            drawAutomaticUniformFactories.Add(new ModelZToClipCoordinatesUniformFactory());
            drawAutomaticUniformFactories.Add(new WindowToWorldNearPlaneUniformFactory());
            drawAutomaticUniformFactories.Add(new Wgs84HeightUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveNearPlaneDistanceUniformFactory());
            drawAutomaticUniformFactories.Add(new PerspectiveFarPlaneDistanceUniformFactory());
            drawAutomaticUniformFactories.Add(new HighResolutionSnapScaleUniformFactory());
            drawAutomaticUniformFactories.Add(new PixelSizePerDistanceUniformFactory());

            m_DrawAutomaticUniformFactories = drawAutomaticUniformFactories;

            ///////////////////////////////////////////////////////////////

            m_TextureSamplers = new TextureSamplers(m_Context);
        }

        //public static GraphicsWindow CreateWindow(int width, int height)
        //{
        //    return CreateWindow(width, height, "");
        //}

        //public static GraphicsWindow CreateWindow(int width, int height, string title)
        //{
        //    return CreateWindow(width, height, title, WindowType.Default);
        //}

        //public static GraphicsWindow CreateWindow(int width, int height, string title, WindowType windowType)
        //{
        //    return new GraphicsWindowGL3x(width, height, title, windowType);
        //}

        public ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL3x(m_Context, vertexShaderSource, fragmentShaderSource);
        }

        public ShaderProgram CreateShaderProgram(
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            return new ShaderProgramGL3x(m_Context, vertexShaderSource, geometryShaderSource, fragmentShaderSource);
        }

        public VertexBuffer CreateVertexBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new VertexBufferGL3x(usageHint, sizeInBytes);
        }

        public IndexBuffer CreateIndexBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new IndexBufferGL3x(usageHint, sizeInBytes);
        }

        public MeshBuffers CreateMeshBuffers(Mesh mesh, ShaderVertexAttributeCollection shaderAttributes, BufferHint usageHint)
        {
            if (mesh == null)
            {
                throw new ArgumentNullException("mesh");
            }

            if (shaderAttributes == null)
            {
                throw new ArgumentNullException("shaderAttributes");
            }

            var meshBuffers = new MeshBuffers(m_Context);

            if (mesh.Indices != null)
            {
                if (mesh.Indices.Datatype == IndicesType.UnsignedShort)
                {
                    IList<ushort> meshIndices = ((IndicesUnsignedShort)mesh.Indices).Values;

                    ushort[] indices = new ushort[meshIndices.Count];
                    for (int j = 0; j < meshIndices.Count; ++j)
                    {
                        indices[j] = meshIndices[j];
                    }

                    IndexBuffer indexBuffer = CreateIndexBuffer(usageHint, indices.Length * sizeof(ushort));
                    indexBuffer.CopyFromSystemMemory(indices);
                    meshBuffers.IndexBuffer = indexBuffer;
                }
                else if (mesh.Indices.Datatype == IndicesType.UnsignedInt)
                {
                    IList<uint> meshIndices = ((IndicesUnsignedInt)mesh.Indices).Values;

                    uint[] indices = new uint[meshIndices.Count];
                    for (int j = 0; j < meshIndices.Count; ++j)
                    {
                        indices[j] = meshIndices[j];
                    }

                    IndexBuffer indexBuffer = CreateIndexBuffer(usageHint, indices.Length * sizeof(uint));
                    indexBuffer.CopyFromSystemMemory(indices);
                    meshBuffers.IndexBuffer = indexBuffer;
                }
                else
                {
                    throw new NotSupportedException("mesh.Indices.Datatype " +
                        mesh.Indices.Datatype.ToString() + " is not supported.");
                }
            }

            //
            // Emulated double precision vectors are a special case:  one mesh vertex attribute
            // yields two shader vertex attributes.  As such, these are handled separately before
            // normal attributes.
            //
            HashSet<string> ignoreAttributes = new HashSet<string>();

            foreach (VertexAttribute attribute in mesh.Attributes)
            {
                if (attribute is VertexAttributeDoubleVector3)
                {
                    VertexAttributeDoubleVector3 emulated = (VertexAttributeDoubleVector3)attribute;

                    int highLocation = -1;
                    int lowLocation = -1;

                    foreach (ShaderVertexAttribute shaderAttribute in shaderAttributes)
                    {
                        if (shaderAttribute.Name == emulated.Name + "High")
                        {
                            highLocation = shaderAttribute.Location;
                        }
                        else if (shaderAttribute.Name == emulated.Name + "Low")
                        {
                            lowLocation = shaderAttribute.Location;
                        }

                        if ((highLocation != -1) && (lowLocation != -1))
                        {
                            break;
                        }
                    }

                    if ((highLocation == -1) && (lowLocation == -1))
                    {
                        //
                        // The shader did not have either attribute.  No problem.
                        //
                        continue;
                    }
                    else if ((highLocation == -1) || (lowLocation == -1))
                    {
                        throw new ArgumentException("An emulated double vec3 mesh attribute requires both " + emulated.Name + "High and " + emulated.Name + "Low vertex attributes, but the shader only contains one matching attribute.");
                    }

                    //
                    // Copy both high and low parts into a single vertex buffer.
                    //
                    IList<Vector3D> values = ((VertexAttribute<Vector3D>)attribute).Values;

                    Vector3F[] vertices = new Vector3F[2 * values.Count];

                    int j = 0;
                    for (int i = 0; i < values.Count; ++i)
                    {
                        EmulatedVector3D v = new EmulatedVector3D(values[i]);
                        vertices[j++] = v.High;
                        vertices[j++] = v.Low;
                    }

                    VertexBuffer vertexBuffer = CreateVertexBuffer(usageHint, ArraySizeInBytes.Size(vertices));
                    vertexBuffer.CopyFromSystemMemory(vertices);

                    int stride = 2 * SizeInBytes<Vector3F>.Value;
                    meshBuffers.Attributes[highLocation] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3, false, 0, stride);
                    meshBuffers.Attributes[lowLocation] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3, false, SizeInBytes<Vector3F>.Value, stride);

                    ignoreAttributes.Add(emulated.Name + "High");
                    ignoreAttributes.Add(emulated.Name + "Low");
                }
            }

            // TODO:  Not tested exhaustively
            foreach (ShaderVertexAttribute shaderAttribute in shaderAttributes)
            {
                if (ignoreAttributes.Contains(shaderAttribute.Name))
                {
                    continue;
                }

                if (!mesh.Attributes.Contains(shaderAttribute.Name))
                {
                    throw new ArgumentException("Shader requires vertex attribute \"" + shaderAttribute.Name + "\", which is not present in mesh.");
                }

                VertexAttribute attribute = mesh.Attributes[shaderAttribute.Name];


                if (attribute.Datatype == VertexAttributeType.EmulatedDoubleVector3)
                {
                    IList<Vector3D> values = ((VertexAttribute<Vector3D>)attribute).Values;

                    Vector3F[] valuesArray = new Vector3F[values.Count];
                    for (int i = 0; i < values.Count; ++i)
                    {
                        valuesArray[i] = values[i].ToVector3F();
                    }

                    VertexBuffer vertexBuffer = CreateVertexBuffer(usageHint, ArraySizeInBytes.Size(valuesArray));
                    vertexBuffer.CopyFromSystemMemory(valuesArray);
                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloat)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Core.Half>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector2H>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector3H>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.HalfFloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector4H>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.HalfFloat, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.Float)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<float>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 1);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector2)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector2F>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 2);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector3)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector3F>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 3);
                }
                else if (attribute.Datatype == VertexAttributeType.FloatVector4)
                {
                    VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<Vector4F>)attribute).Values, usageHint);

                    meshBuffers.Attributes[shaderAttribute.Location] =
                        new VertexBufferAttribute(vertexBuffer, ComponentDatatype.Float, 4);
                }
                else if (attribute.Datatype == VertexAttributeType.UnsignedByte)
                {
                    if (attribute is VertexAttributeRGBA)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 4, true, 0, 0);
                    }

                    else if (attribute is VertexAttributeRGB)
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 3, true, 0, 0);
                    }
                    else
                    {
                        VertexBuffer vertexBuffer = CreateVertexBuffer(((VertexAttribute<byte>)attribute).Values, usageHint);

                        meshBuffers.Attributes[shaderAttribute.Location] =
                            new VertexBufferAttribute(vertexBuffer, ComponentDatatype.UnsignedByte, 1);
                    }
                }
                else
                {
                    Debug.Fail("attribute.Datatype");
                }
            }

            return meshBuffers;
        }

        private VertexBuffer CreateVertexBuffer<T>(IList<T> values, BufferHint usageHint) where T : struct
        {
            T[] valuesArray = new T[values.Count];
            values.CopyTo(valuesArray, 0);

            VertexBuffer vertexBuffer = CreateVertexBuffer(usageHint, ArraySizeInBytes.Size(valuesArray));
            vertexBuffer.CopyFromSystemMemory(valuesArray);
            return vertexBuffer;
        }

        public UniformBuffer CreateUniformBuffer(BufferHint usageHint, int sizeInBytes)
        {
            return new UniformBufferGL3x(usageHint, sizeInBytes);
        }

        public WritePixelBuffer CreateWritePixelBuffer(PixelBufferHint usageHint, int sizeInBytes)
        {
            return new WritePixelBufferGL3x(usageHint, sizeInBytes);
        }

        public Texture2D CreateTexture2D(Texture2DDescription description)
        {
            return new Texture2DGL3x(this, description, TextureTarget.Texture2D);
        }
        public Texture2D CreateTexture2D(string filename, TextureFormat format, bool generateMipmaps)
        {
            return CreateTexture2DFromBitmap(Bitmap.Decode(filename), format, generateMipmaps, TextureTarget.Texture2D);
        }

        public Texture2D CreateTexture2D(Bitmap bitmap, TextureFormat format, bool generateMipmaps)
        {
            return CreateTexture2DFromBitmap(bitmap, format, generateMipmaps, TextureTarget.Texture2D);
        }

        public Texture2D CreateTexture2DRectangle(Texture2DDescription description)
        {
            return new Texture2DGL3x(this, description, TextureTarget.TextureRectangle);
        }

        public Texture2D CreateTexture2DRectangle(Bitmap bitmap, TextureFormat format)
        {
            return CreateTexture2DFromBitmap(bitmap, format, false, TextureTarget.TextureRectangle);
        }

        private Texture2D CreateTexture2DFromBitmap(Bitmap bitmap, TextureFormat format, bool generateMipmaps, TextureTarget textureTarget)
        {
            using (WritePixelBuffer pixelBuffer = CreateWritePixelBuffer(PixelBufferHint.Stream,
                BitmapAlgorithms.SizeOfPixelsInBytes(bitmap)))
            {
                pixelBuffer.CopyFromBitmap(bitmap);

                Texture2DDescription description = new Texture2DDescription(bitmap.Width, bitmap.Height, format, generateMipmaps);
                Texture2D texture = new Texture2DGL3x(this, description, textureTarget);
                texture.CopyFromBuffer(pixelBuffer,
                    TextureUtility.ImagingPixelFormatToImageFormat(bitmap.ColorType),
                    TextureUtility.ImagingPixelFormatToDatatype(bitmap.ColorType));

                return texture;
            }
        }
        public SKFont CreateDefaultFont(float size)
        {
            return new SKFont(SKTypeface.Default, size);
        }
        public SKFont CreateFont(string familyName, float size)
        {
            return new SKFont(SKTypeface.FromFamilyName(familyName, SKFontStyle.Normal), size);
        }

        public SKRect MeasureText(string text, SKFont font, out float[] lineHeights)
        {

            SKRect? bounds = null;
            var heights = new List<float>();
            foreach (var textOnLine in text.Split('\n', '\r'))
            {
                var glyphs = new Span<ushort>(new ushort[textOnLine.Length]);
                font.GetGlyphs(textOnLine, glyphs);
                SKRect lineBounds;
                font.MeasureText(glyphs, out lineBounds);
                heights.Add(lineBounds.Height);
                if (bounds != null)
                {
                    bounds = new SKRect
                    {
                        Left = bounds.Value.Left,
                        Top = bounds.Value.Top,
                        Right = Math.Max(bounds.Value.Right, lineBounds.Right),
                        Bottom = bounds.Value.Bottom + lineBounds.Height
                    };
                }
                else
                {
                    bounds = lineBounds;
                }
            }

            lineHeights = heights.ToArray();

            return bounds.GetValueOrDefault();
        }

        public SKBitmap CreateBitmapFromText(string text, SKFont font)
        {
            float[] lineHeights;
            var bounds = MeasureText(text, font, out lineHeights);
            var x = -bounds.Left;
            var y = -bounds.Top;

            var bitmap = new SKBitmap(
                (int)Math.Ceiling(bounds.Width-bounds.Left),
                (int)Math.Ceiling(bounds.Height-bounds.Top),
                SKColorType.Rgba8888,
                SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            using (var paint = new SKPaint(font))
            {
                int i = 0;
                foreach (var textOnLine in text.Split('\n', '\r'))
                {
                    canvas.DrawText(textOnLine, x, y, paint);
                    y += lineHeights[i % lineHeights.Length];
                    i++;
                }

            }

            return bitmap;
        }

        public Bitmap CreateBitmapFromPoint(int radiusInPixels)
        {
            if (radiusInPixels < 1)
            {
                throw new ArgumentOutOfRangeException("radius");
            }

            int diameter = radiusInPixels * 2;
            var bitmap = new Bitmap(diameter, diameter, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var canvas = new SKCanvas(bitmap))
            using (var paint = new SKPaint())
            {
                paint.Color = SKColors.White;
                var xy = diameter / 2f;
                paint.StrokeWidth = 1;
                canvas.DrawCircle(xy, xy, radiusInPixels, paint);
            }

            return bitmap;
        }

        public TextureSampler CreateTexture2DSampler(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT)
        {
            return new TextureSamplerGL3x(
                m_Context,
                minificationFilter,
                magnificationFilter,
                wrapS,
                wrapT,
                1);
        }

        public TextureSampler CreateTexture2DSampler(
            TextureMinificationFilter minificationFilter,
            TextureMagnificationFilter magnificationFilter,
            TextureWrap wrapS,
            TextureWrap wrapT,
            float maximumAnistropy)
        {
            return new TextureSamplerGL3x(
                m_Context,
                minificationFilter,
                magnificationFilter,
                wrapS,
                wrapT,
                maximumAnistropy);
        }

        public TextureSamplers TextureSamplers => m_TextureSamplers;

        public Fence CreateFence()
        {
            return new FenceGL3x();
        }

        public void Finish()
        {
            GL.Finish();
        }

        public void Flush()
        {
            GL.Flush();
        }

        public Extensions Extensions => m_Extensions;

        /// <summary>
        /// The collection is not thread safe.
        /// </summary>
        public LinkAutomaticUniformCollection LinkAutomaticUniforms => m_LinkAutomaticUniforms;

        /// <summary>
        /// The collection is not thread safe.
        /// </summary>
        public DrawAutomaticUniformFactoryCollection DrawAutomaticUniformFactories => m_DrawAutomaticUniformFactories;

        public int MaximumNumberOfVertexAttributes => m_MaximumNumberOfVertexAttributes;

        public int NumberOfTextureUnits => m_NumberOfTextureUnits;

        public int MaximumNumberOfColorAttachments => m_MaximumNumberOfColorAttachments;

        private int m_MaximumNumberOfVertexAttributes;
        private int m_NumberOfTextureUnits;
        private int m_MaximumNumberOfColorAttachments;

        private Extensions m_Extensions;
        private LinkAutomaticUniformCollection m_LinkAutomaticUniforms;
        private DrawAutomaticUniformFactoryCollection m_DrawAutomaticUniformFactories;

        private TextureSamplers m_TextureSamplers;
    }
}
