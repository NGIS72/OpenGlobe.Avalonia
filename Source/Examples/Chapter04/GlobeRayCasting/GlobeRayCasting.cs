#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Drawing;

using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;

namespace OpenGlobe.Examples
{
    sealed class GlobeRayCasting : SceneBase, IDisposable
    {
        private Context m_Context;
        public override void Load(Context context)
        {
            base.Load(context);
            m_Context = context;
        
            Ellipsoid globeShape = Ellipsoid.ScaledWgs84;

            Camera = new CameraLookAtPoint(SceneState.Camera, globeShape);
            

            
            _texture = m_Context.Device.CreateTexture2D("NE2_50M_SR_W_4096.jpg", TextureFormat.RedGreenBlue8, false);

            _globe = new RayCastedGlobe(m_Context);
            _globe.Shape = globeShape;
            _globe.Texture = _texture;
            _globe.ShowWireframeBoundingBox = true;

            SceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);
        }

        public override void Render(Context context)
        {

            context.Clear(ClearState);
            _globe.Render(context, SceneState);
        }

        private void CenterCameraOnPoint()
        {
            var camera = Camera as CameraLookAtPoint;
            if (camera == null) return;
            camera.ViewPoint(_globe.Shape, new Geodetic3D(Trig.ToRadians(-75.697), Trig.ToRadians(40.039), 0.0));
            camera.Azimuth = 0.0;
            camera.Elevation = Math.PI / 4.0;
            camera.Range = _globe.Shape.MaximumRadius * 3.0;
        }

        private void CenterCameraOnGlobeCenter()
        {
            var camera = Camera as CameraLookAtPoint;
            if (camera == null) return;
            camera.CenterPoint = Vector3D.Zero;
            camera.FixedToLocalRotation = Matrix3D.Identity;
            camera.Azimuth = 0.0;
            camera.Elevation = 0.0;
            camera.Range = _globe.Shape.MaximumRadius * 3.0;
        }

        private RayCastedGlobe _globe;
        private Texture2D _texture;
    }
}