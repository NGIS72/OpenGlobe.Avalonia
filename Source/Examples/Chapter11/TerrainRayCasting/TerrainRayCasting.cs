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
    sealed class TerrainRayCasting : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
        
            SceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            
            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(@"ps-e.lg.png");
            _tile = new RayCastedTerrainTile(context, terrainTile);
            _tile.HeightExaggeration = 30;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            SetCameraLookAtPoint(Ellipsoid.UnitSphere);
            var camera = (CameraLookAtPoint)Camera;
            camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            camera.MinimumRotateRate = 1.0;
            camera.MaximumRotateRate = 1.0;
            camera.RotateRateRangeAdjustment = 0.0;
            camera.RotateFactor = 0.0;
            SceneState.Camera.ZoomToTarget(tileRadius);
            
            ///////////////////////////////////////////////////////////////////

            _hudFont = context.Device.CreateDefaultFont(16);
            _hud = new HeadsUpDisplay(context);
            _hud.Color = Colors.Black;
            UpdateHUD();
        }

        private static string TerrainShadingAlgorithmToString(RayCastedTerrainShadingAlgorithm shading)
        {
            switch (shading)
            {
                case RayCastedTerrainShadingAlgorithm.ByHeight:
                    return "By Height";
                case RayCastedTerrainShadingAlgorithm.ByRaySteps:
                    return "Number of Ray Steps";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Shading Algorithm: " + TerrainShadingAlgorithmToString(_tile.ShadingAlgorithm) + " (left/right)\n";
            text += "Wireframe: " + (_tile.ShowWireframe ? "on" : "off") + " ('w')\n";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }


        public override void KeyDown(object sender, KeyEventArgs e)
        {
            base.KeyDown(sender, e);
        
            if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                _tile.ShadingAlgorithm += (e.Key == Key.Right) ? 1 : -1;
                if (_tile.ShadingAlgorithm < RayCastedTerrainShadingAlgorithm.ByHeight)
                {
                    _tile.ShadingAlgorithm = RayCastedTerrainShadingAlgorithm.ByRaySteps;
                }
                else if (_tile.ShadingAlgorithm > RayCastedTerrainShadingAlgorithm.ByRaySteps)
                {
                    _tile.ShadingAlgorithm = RayCastedTerrainShadingAlgorithm.ByHeight;
                }
            }
            if (e.Key == Key.W)
            {
                _tile.ShowWireframe = !_tile.ShowWireframe;
            }

            UpdateHUD();
        }

        public override void Render(Context context)
        {
            context.Clear(ClearState);

            _tile.Render(context, SceneState);
            _hud.Render(context, SceneState);
        }

        
        private RayCastedTerrainTile _tile;

        private SKFont _hudFont;
        private  HeadsUpDisplay _hud;
    }
}