#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using Avalonia.Media;
using System;
using System.Collections.Generic;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using Avalonia.Input;

namespace OpenGlobe.Examples
{
    sealed class Curves : SceneBase, IDisposable
    {
        private Context m_Context;
        public override void Load(Context context)
        {
            base.Load(context);
            m_Context = context;
        
            _semiMinorAxis = Ellipsoid.ScaledWgs84.Radii.Z;
            SetShape();

            Camera = new CameraLookAtPoint(SceneState.Camera, _globeShape);

            _texture = m_Context.Device.CreateTexture2D(new Texture2DDescription(1, 1, TextureFormat.RedGreenBlue8));
            WritePixelBuffer pixelBuffer = m_Context.Device.CreateWritePixelBuffer(PixelBufferHint.Stream, 3);
            pixelBuffer.CopyFromSystemMemory(new byte[] { 0, 255, 127 });
            _texture.CopyFromBuffer(pixelBuffer, ImageFormat.RedGreenBlue, ImageDatatype.UnsignedByte, 1);

            _instructions = new HeadsUpDisplay(m_Context);
            _instructions.Color = Colors.Black;
            
            _sampledPoints = new BillboardCollection(m_Context);
            _sampledPoints.Texture = m_Context.Device.CreateTexture2D(m_Context.Device.CreateBitmapFromPoint(8), TextureFormat.RedGreenBlueAlpha8, false);
            _sampledPoints.DepthTestEnabled = false;
            
            _ellipsoid = new RayCastedGlobe(m_Context);
            _ellipsoid.Texture = _texture;

            _polyline = new Polyline();
            _polyline.Width = 3;
            _polyline.DepthTestEnabled = false;

            _plane = new Plane(m_Context);
            _plane.Origin = Vector3D.Zero;
            _plane.OutlineWidth = 3;

            CreateScene();
            
            ///////////////////////////////////////////////////////////////////

            SceneState.Camera.Eye = Vector3D.UnitY;
            SceneState.Camera.ZoomToTarget(2 * _globeShape.MaximumRadius);
        }

        private void CreateScene()
        {
            string text = "Granularity: " + _granularityInDegrees + " (left/right)\n";
            text += "Points: " + (_sampledPoints.Show ? "on" : "off") + " ('1')\n";
            text += "Polyline: " + (_polyline.Show ? "on" : "off") + " ('2')\n";
            text += "Plane: " + (_plane.Show ? "on" : "off") + " ('3')\n";
            text += "Semi-minor axis (up/down)\n";

            _instructions.Texture = m_Context.Device.CreateTexture2D(
                m_Context.Device.CreateBitmapFromText(text, m_Context.Device.CreateDefaultFont(14)),
                TextureFormat.RedGreenBlueAlpha8, false);

            ///////////////////////////////////////////////////////////////////

            IList<Vector3D> positions = _globeShape.ComputeCurve(
                _p, _q, Trig.ToRadians(_granularityInDegrees));

            _sampledPoints.Clear();
            _sampledPoints.Add(new Billboard() { Position = positions[0], Color = Colors.Orange });
            _sampledPoints.Add(new Billboard() { Position = positions[positions.Count - 1], Color = Colors.Orange });

            for (int i = 1; i < positions.Count - 1; ++i)
            {
                _sampledPoints.Add(new Billboard() 
                { 
                    Position = positions[i], 
                    Color = Colors.Yellow 
                });
            }

            ///////////////////////////////////////////////////////////////////

            _ellipsoid.Shape = _globeShape;

            ///////////////////////////////////////////////////////////////////
            
            VertexAttributeFloatVector3 positionAttribute = new VertexAttributeFloatVector3("position", positions.Count);
            VertexAttributeRGBA colorAttribute = new VertexAttributeRGBA("color", positions.Count);

            for (int i = 0; i < positions.Count; ++i)
            {
                positionAttribute.Values.Add(positions[i].ToVector3F());
                colorAttribute.AddColor(Colors.Red);
            }

            Mesh mesh = new Mesh();
            mesh.PrimitiveType = PrimitiveType.LineStrip;
            mesh.Attributes.Add(positionAttribute);
            mesh.Attributes.Add(colorAttribute);

            _polyline.Set(m_Context, mesh);

            ///////////////////////////////////////////////////////////////////

            double scale = 1.25 * _globeShape.Radii.MaximumComponent;
            _plane.XAxis = scale * _p.Normalize();
            _plane.YAxis = scale * _p.Cross(_q).Cross(_p).Normalize();
        }

        public override void Render(Context context)
        {
            context.Clear(ClearState);

            _ellipsoid.Render(context, SceneState);
            _polyline.Render(context, SceneState);
            _sampledPoints.Render(context, SceneState);
            _plane.Render(context, SceneState);
            _instructions.Render(context, SceneState);
        }

        public override void KeyDown(object sender, KeyEventArgs e)
        {
            base.KeyDown(sender, e);
      
            if ((e.Key == Key.Left) || (e.Key == Key.Right) ||
                (e.Key == Key.Up) || (e.Key == Key.Down))
            {
                if (e.Key == Key.Left)
                {
                    _granularityInDegrees = Math.Max(_granularityInDegrees - 1.0, 1.0);
                }
                else if (e.Key == Key.Right)
                {
                    _granularityInDegrees = Math.Min(_granularityInDegrees + 1.0, 30.0);
                }
                else if (e.Key == Key.Up)
                {
                    _semiMinorAxis = Math.Min(_semiMinorAxis + _semiMinorAxisDelta, 2.0);
                }
                else if (e.Key == Key.Down)
                {
                    _semiMinorAxis = Math.Max(_semiMinorAxis - _semiMinorAxisDelta, 0.1);
                }
                SetShape();
            }
            else if (e.Key == Key.NumPad1)
            {
                _sampledPoints.Show = !_sampledPoints.Show;
            }
            else if (e.Key == Key.NumPad2)
            {
                _polyline.Show = !_polyline.Show;
            }
            else if (e.Key == Key.NumPad3)
            {
                _plane.Show = !_plane.Show;
            }

            CreateScene();
        }

        private void SetShape()
        {
            _globeShape = new Ellipsoid(
                Ellipsoid.ScaledWgs84.Radii.X,
                Ellipsoid.ScaledWgs84.Radii.Y,
                _semiMinorAxis);
            _p = _globeShape.ToVector3D(new Geodetic2D(Trig.ToRadians(40), Trig.ToRadians(40)));
            _q = _globeShape.ToVector3D(new Geodetic2D(Trig.ToRadians(120), Trig.ToRadians(-30)));
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        
            _texture.Dispose();
            _instructions.Dispose();
            _ellipsoid.Dispose();
            _sampledPoints.Dispose();
            _sampledPoints.Texture.Dispose();
            _polyline.Dispose();
            _plane.Dispose();
        }

        #endregion

       
        private Texture2D _texture;
        private HeadsUpDisplay _instructions;
        private RayCastedGlobe _ellipsoid;
        private BillboardCollection _sampledPoints;
        private Polyline _polyline;
        private Plane _plane;

        private Ellipsoid _globeShape;
        private Vector3D _p;
        private Vector3D _q;

        private double _semiMinorAxis;
        private const double _semiMinorAxisDelta = 0.025;
        private double _granularityInDegrees = 5.0;
    }
}