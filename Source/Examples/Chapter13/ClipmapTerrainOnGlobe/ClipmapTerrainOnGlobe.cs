#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using Avalonia.Input;
using Avalonia.Media;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using SkiaSharp;

namespace OpenGlobe.Examples
{
    sealed class ClipmapTerrainOnGlobe : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
        
            _ellipsoid = Ellipsoid.Wgs84;

            var imagery = new EsriRestImagery(context);
            //var terrain = new EsriRestImagery(context, EsriRestImagery.TERRAIN_BASE);
            var terrain = new WorldWindTerrainSource(context);
            
            _clipmap = new GlobeClipmapTerrain(context, terrain, imagery, _ellipsoid, 511);
            _clipmap.HeightExaggeration = 1.0f;

            
            SceneState.DiffuseIntensity = 0.90f;
            SceneState.SpecularIntensity = 0.05f;
            SceneState.AmbientIntensity = 0.05f;
            SceneState.Camera.FieldOfViewY = Math.PI / 3.0;

            ClearState.Color = Colors.White;

            SceneState.Camera.PerspectiveNearPlaneDistance = 0.000001 * _ellipsoid.MaximumRadius;
            SceneState.Camera.PerspectiveFarPlaneDistance = 10.0 * _ellipsoid.MaximumRadius;
            SceneState.SunPosition = new Vector3D(200000, 300000, 200000) * _ellipsoid.MaximumRadius;

             _lookCamera = new CameraLookAtPoint(SceneState.Camera, _ellipsoid);
             _lookCamera.Range = 1.5 * _ellipsoid.MaximumRadius;

             _globe = new RayCastedGlobe(context);
             _globe.Shape = _ellipsoid;
             
             _globe.Texture = Device.CreateTexture2D("NE2_50M_SR_W_4096.jpg", TextureFormat.RedGreenBlue8, false);

             _clearDepth = new ClearState();
             _clearDepth.Buffers = ClearBuffers.DepthBuffer | ClearBuffers.StencilBuffer;


            _hudFont = Device.CreateDefaultFont(16);
            _hud = new HeadsUpDisplay(context);
            _hud.Color = Colors.Blue;
            UpdateHUD();
        }

        public override void KeyDown(object sender, KeyEventArgs e)
        {
            base.KeyDown(sender, e);
        
            if (e.Key == Key.U)
            {
                SceneState.SunPosition = SceneState.Camera.Eye;
            }
            else if (e.Key == Key.W)
            {
                _clipmap.Wireframe = !_clipmap.Wireframe;
                UpdateHUD();
            }
            else if (e.Key == Key.B)
            {
                if (!_clipmap.BlendRegionsEnabled)
                {
                    _clipmap.BlendRegionsEnabled = true;
                    _clipmap.ShowBlendRegions = false;
                }
                else if (_clipmap.ShowBlendRegions)
                {
                    _clipmap.BlendRegionsEnabled = false;
                }
                else
                {
                    _clipmap.ShowBlendRegions = true;
                }
                UpdateHUD();
            }
            else if (e.Key == Key.L)
            {
                _clipmap.LodUpdateEnabled = !_clipmap.LodUpdateEnabled;
                UpdateHUD();
            }
            else if (e.Key == Key.C)
            {
                _clipmap.ColorClipmapLevels = !_clipmap.ColorClipmapLevels;
                if (_clipmap.ColorClipmapLevels)
                {
                    _clipmap.ShowImagery = false;
                    _clipmap.Lighting = true;
                }
                UpdateHUD();
            }
            else if (e.Key == Key.I)
            {
                _clipmap.ShowImagery = !_clipmap.ShowImagery;
                _clipmap.Lighting = !_clipmap.ShowImagery;
                if (_clipmap.ShowImagery)
                {
                    _clipmap.ColorClipmapLevels = false;
                }
                UpdateHUD();
            }
            else if (e.Key == Key.S)
            {
                _clipmap.Lighting = !_clipmap.Lighting;
                UpdateHUD();
            }
            else if (e.Key == Key.Z)
            {
                if (_lookCamera != null)
                {
                    double longitude = -119.5326056;
                    double latitude = 37.74451389;
                    Geodetic3D halfDome = new Geodetic3D(Trig.ToRadians(longitude), Trig.ToRadians(latitude), 2700.0);
                    _lookCamera.ViewPoint(_ellipsoid, halfDome);
                    _lookCamera.Azimuth = 0.0;
                    _lookCamera.Elevation = Trig.ToRadians(30.0);
                    _lookCamera.Range = 10000.0;
                }
            }
            else if (e.Key == Key.F)
            {
                if (_lookCamera != null)
                {
                    _lookCamera.Dispose();
                    _lookCamera = null;
                    _flyCamera = new CameraFly(SceneState.Camera);
                    _flyCamera.MovementRate = 1200.0;
                }
                else if (_flyCamera != null)
                {
                    _flyCamera.Dispose();
                    _flyCamera = null;
                    SceneState.Camera.Target = new Vector3D(0.0, 0.0, 0.0);
                    _lookCamera = new CameraLookAtPoint(SceneState.Camera, _ellipsoid);
                    _lookCamera.UpdateParametersFromCamera();
                }
                UpdateHUD();
            }
            else if (_flyCamera != null && (e.Key == Key.Add || e.Key==Key.OemPlus))
            {
                _flyCamera.MovementRate *= 2.0;
                UpdateHUD();
            }
            else if (_flyCamera != null && (e.Key == Key.Subtract || e.Key == Key.OemMinus))
            {
                _flyCamera.MovementRate *= 0.5;
                UpdateHUD();
            }
            else if (e.Key == Key.E)
            {
                if (_clipmap.Ellipsoid.MaximumRadius == _clipmap.Ellipsoid.MinimumRadius)
                {
                    _clipmap.Ellipsoid = Ellipsoid.Wgs84;
                    _globe.Shape = Ellipsoid.Wgs84;
                }
                else
                {
                    double radius = Ellipsoid.Wgs84.MaximumRadius;
                    _clipmap.Ellipsoid = new Ellipsoid(radius, radius, radius);
                    _globe.Shape = _clipmap.Ellipsoid;
                }
            }
        }
        public override void Render(Context context)
        {
            context.Clear(ClearState);

            _globe.Render(context, SceneState);

            context.Clear(_clearDepth);

            _clipmap.Render(context, SceneState);

            if (_hud != null)
            {
                _hud.Render(context, SceneState);
            }
        }
        public override void PreRender(Context context)
        {
            _clipmap.PreRender(context, SceneState);
        }

        private void UpdateHUD()
        {
            if (_hud == null)
                return;

            string text;

            text = "Blending: " + GetBlendingString() + " (B)\n";
            text += "Imagery: " + (_clipmap.ShowImagery ? "Enabled" : "Disabled") + " (I)\n";
            text += "Lighting: " + (_clipmap.Lighting ? "Enabled" : "Disabled") + " (S)\n";
            text += "Wireframe: " + (_clipmap.Wireframe ? "Enabled" : "Disabled") + " (W)\n";
            text += "LOD Update: " + (_clipmap.LodUpdateEnabled ? "Enabled" : "Disabled") + " (L)\n";
            text += "Color Clipmap Levels: " + (_clipmap.ColorClipmapLevels ? "Enabled" : "Disabled") + " (C)\n";
            text += "Camera: " + (_lookCamera != null ? "Look At" : "Fly") + " (F)\n";

            if (_flyCamera != null)
            {
                text += "Speed: " + _flyCamera.MovementRate + "m/s (+/-)\n";
            }

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private string GetBlendingString()
        {
            if (!_clipmap.BlendRegionsEnabled)
                return "Disabled";
            else if (_clipmap.ShowBlendRegions)
                return "Enabled and Shown";
            else
                return "Enabled";
        }

        private CameraLookAtPoint _lookCamera;
        private CameraFly _flyCamera;
        
        private GlobeClipmapTerrain _clipmap;
        private HeadsUpDisplay _hud;
        private SKFont _hudFont;
        private RayCastedGlobe _globe;
        private ClearState _clearDepth;
        private Ellipsoid _ellipsoid;
    }
}