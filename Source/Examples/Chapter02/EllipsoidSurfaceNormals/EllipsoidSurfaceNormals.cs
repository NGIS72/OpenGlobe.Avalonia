#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Collections.Generic;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using Avalonia.Input;
using Avalonia.Media;
using System;
using SkiaSharp;

namespace OpenGlobe.Examples
{
    sealed class EllipsoidSurfaceNormals : SceneBase, IDisposable
    {
        private Context m_Context;
        public override void Load(Context context)
        {
            base.Load(context);
            m_Context = context;
       
            _globeShape = new Ellipsoid(1, 1, _semiMinorAxis);

       
            Camera = new CameraLookAtPoint(SceneState.Camera, _globeShape);

            _instructions = new HeadsUpDisplay(context);
            _instructions.Texture = context.Device.CreateTexture2D(
                context.Device.CreateBitmapFromText("Up - Increase semi-minor axis\nDown - Decrease semi-minor axis",
                    context.Device.CreateDefaultFont(14)),
                TextureFormat.RedGreenBlueAlpha8, false);
            _instructions.Color = Colors.Black;

            CreateScene();
            
            ///////////////////////////////////////////////////////////////////

            SceneState.Camera.Eye = Vector3D.UnitY;
            SceneState.Camera.ZoomToTarget(2 * _globeShape.MaximumRadius);
        }

        private void CreateScene()
        {
            DisposeScene();

            _ellipsoid = new TessellatedGlobe(m_Context);
            _ellipsoid.Shape = _globeShape;
            _ellipsoid.NumberOfSlicePartitions = 64;
            _ellipsoid.NumberOfStackPartitions = 32;

            ///////////////////////////////////////////////////////////////////

            Mesh mesh = GeographicGridEllipsoidTessellator.Compute(_globeShape,
                64, 32, GeographicGridEllipsoidVertexAttributes.Position);

            _wireframe = new Wireframe(m_Context, mesh);
            _wireframe.Width = 2;

            ///////////////////////////////////////////////////////////////////

            _axes = new Axes();
            _axes.Length = 1.5;
            _axes.Width = 3;

            ///////////////////////////////////////////////////////////////////

            Vector3D p = _globeShape.ToVector3D(new Geodetic3D(0, Trig.ToRadians(45), 0));
            Vector3D deticNormal = _globeShape.GeodeticSurfaceNormal(p);
            Vector3D centricNormal = Ellipsoid.CentricSurfaceNormal(p);

            double normalLength = _globeShape.MaximumRadius;
            Vector3D pDetic = p + (normalLength * deticNormal);
            Vector3D pCentric = p + (normalLength * centricNormal);

            VertexAttributeFloatVector3 positionAttribute = new VertexAttributeFloatVector3("position", 4);
            positionAttribute.Values.Add(p.ToVector3F());
            positionAttribute.Values.Add(pDetic.ToVector3F());
            positionAttribute.Values.Add(p.ToVector3F());
            positionAttribute.Values.Add(pCentric.ToVector3F());

            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", 4);
            colorAttribute.AddColor(Colors.DarkGreen);
            colorAttribute.AddColor(Colors.DarkGreen);
            colorAttribute.AddColor(Colors.DarkCyan);
            colorAttribute.AddColor(Colors.DarkCyan);

            Mesh polyline = new Mesh();
            polyline.PrimitiveType = PrimitiveType.Lines;
            polyline.Attributes.Add(positionAttribute);
            polyline.Attributes.Add(colorAttribute);

            _normals = new Polyline();
            _normals.Set(m_Context, polyline);
            _normals.Width = 3;

            ///////////////////////////////////////////////////////////////////
            var font = m_Context.Device.CreateDefaultFont(24);
            var labelBitmaps = new List<SKBitmap>(2);
            labelBitmaps.Add(m_Context.Device.CreateBitmapFromText("Geodetic", font));
            labelBitmaps.Add(m_Context.Device.CreateBitmapFromText("Geocentric", font));
            font.Dispose();

            TextureAtlas atlas = new TextureAtlas(labelBitmaps);

            _labels = new BillboardCollection(m_Context, 2);
            _labels.Texture = m_Context.Device.CreateTexture2D(atlas.Bitmap, TextureFormat.RedGreenBlueAlpha8, false);
            _labels.Add(new Billboard()
            {
                Position = pDetic,
                TextureCoordinates = atlas.TextureCoordinates[0],
                Color = Colors.DarkGreen,
                HorizontalOrigin = HorizontalOrigin.Right,
                VerticalOrigin = VerticalOrigin.Bottom
            });
            _labels.Add(new Billboard()
            {
                Position = pCentric,
                TextureCoordinates = atlas.TextureCoordinates[1],
                Color = Colors.DarkCyan,
                HorizontalOrigin = HorizontalOrigin.Right,
                VerticalOrigin = VerticalOrigin.Bottom
            });

            atlas.Dispose();

            ///////////////////////////////////////////////////////////////////
            Vector3D east = Vector3D.UnitZ.Cross(deticNormal);
            Vector3D north = deticNormal.Cross(east);

            _tangentPlane = new Plane(m_Context);
            _tangentPlane.Origin = p;
            _tangentPlane.XAxis = east;
            _tangentPlane.YAxis = north;
            _tangentPlane.OutlineWidth = 3;
        }

        public override void Render(Context context)
        {
            context.Clear(ClearState);

            _ellipsoid.Render(context, SceneState);
            _wireframe.Render(context, SceneState);
            _axes.Render(context, SceneState);
            _normals.Render(context, SceneState);
            _labels.Render(context, SceneState);
            _tangentPlane.Render(context, SceneState);
            _instructions.Render(context, SceneState);
        }
        public override void KeyDown(object sender, KeyEventArgs e)
        {
            base.KeyDown(sender, e);
       
            if ((e.Key == Key.Up) || (e.Key == Key.Down))
            {
                if (e.Key == Key.Up)
                {
                    _semiMinorAxis = Math.Min(_semiMinorAxis + _semiMinorAxisDelta, 1.0);
                }
                else
                {
                    _semiMinorAxis = Math.Max(_semiMinorAxis - _semiMinorAxisDelta, 0.1);
                }
                _globeShape = new Ellipsoid(1, 1, _semiMinorAxis);

                CreateScene();
            }
        }

        private void DisposeScene()
        {
            if (_ellipsoid != null)
            {
                _ellipsoid.Dispose();
            }

            if (_wireframe != null)
            {
                _wireframe.Dispose();
            }

            if (_axes != null)
            {
                _axes.Dispose();
            }

            if (_labels != null)
            {
                _labels.Texture.Dispose();
                _labels.Dispose();
            }

            if (_normals != null)
            {
                _normals.Dispose();
            }

            if (_tangentPlane != null)
            {
                _tangentPlane.Dispose();
            }
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
       
            _instructions.Dispose();
            DisposeScene();
        }

        #endregion

        private HeadsUpDisplay _instructions;
        
        private TessellatedGlobe _ellipsoid;
        private Wireframe _wireframe;
        private Axes _axes;
        private BillboardCollection _labels;
        private Polyline _normals;
        private Plane _tangentPlane;

        private Ellipsoid _globeShape;
        private double _semiMinorAxis = 0.7;
        private const double _semiMinorAxisDelta = 0.025;
    }
}