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
using Avalonia.Input;
using Avalonia.Media;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using SkiaSharp;

namespace OpenGlobe.Examples
{
    sealed class TerrainShading : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
        
            SceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            SceneState.DiffuseIntensity = 0.9f;
            SceneState.SpecularIntensity = 0.05f;
            SceneState.AmbientIntensity = 0.05f;
            

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap("ps-e.lg.png");
            _tile = new VertexDisplacementMapTerrainTile(context, terrainTile);
            _tile.HeightExaggeration = 30;
            _tile.ColorMapTexture = Device.CreateTexture2D("ps_texture_1k.png", TextureFormat.RedGreenBlue8, false);
            _tile.ColorRampHeightTexture = Device.CreateTexture2D("ColorRamp.jpg", TextureFormat.RedGreenBlue8, false);
            _tile.ColorRampSlopeTexture = Device.CreateTexture2D("ColorRampSlope.jpg", TextureFormat.RedGreenBlue8, false);
            _tile.BlendRampTexture = Device.CreateTexture2D("BlendRamp.jpg", TextureFormat.Red8, false);
            _tile.GrassTexture = Device.CreateTexture2D("Grass.jpg", TextureFormat.RedGreenBlue8, false);
            _tile.StoneTexture = Device.CreateTexture2D("Stone.jpg", TextureFormat.RedGreenBlue8, false);
            _tile.BlendMaskTexture = Device.CreateTexture2D("BlendMask.jpg", TextureFormat.Red8, false);

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            var camera = new CameraLookAtPoint(SceneState.Camera, Ellipsoid.UnitSphere);
            camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            camera.MinimumRotateRate = 1.0;
            camera.MaximumRotateRate = 1.0;
            camera.RotateRateRangeAdjustment = 0.0;
            camera.RotateFactor = 0.0;
            Camera = camera;
            SceneState.Camera.ZoomToTarget(tileRadius);

            ///////////////////////////////////////////////////////////////////

            _hudFont = Device.CreateDefaultFont(16);
            _hud = new HeadsUpDisplay(context);
            _hud.Color = Colors.Black;
            UpdateHUD();
        }

        private static string TerrainNormalsAlgorithmToString(TerrainNormalsAlgorithm normals)
        {
            switch(normals)
            {
                case TerrainNormalsAlgorithm.None:
                    return "n/a";
                case TerrainNormalsAlgorithm.ForwardDifference:
                    return "Forward Samples";
                case TerrainNormalsAlgorithm.CentralDifference:
                    return "Central Samples";
                case TerrainNormalsAlgorithm.SobelFilter:
                    return "Sobel Filter";
            }

            return string.Empty;
        }

        private static string TerrainShadingAlgorithmToString(TerrainShadingAlgorithm shading)
        {
            switch (shading)
            {
                case TerrainShadingAlgorithm.ColorMap:
                    return "Color Map";
                case TerrainShadingAlgorithm.Solid:
                    return "Solid";
                case TerrainShadingAlgorithm.ByHeight:
                    return "By Height";
                case TerrainShadingAlgorithm.HeightContour:
                    return "Height Contour";
                case TerrainShadingAlgorithm.ColorRampByHeight:
                    return "Color Ramp By Height";
                case TerrainShadingAlgorithm.BlendRampByHeight:
                    return "Blend Ramp By Height";
                case TerrainShadingAlgorithm.BySlope:
                    return "By Slope";
                case TerrainShadingAlgorithm.SlopeContour:
                    return "Slope Contour";
                case TerrainShadingAlgorithm.ColorRampBySlope:
                    return "Color Ramp By Slope";
                case TerrainShadingAlgorithm.BlendRampBySlope:
                    return "Blend Ramp By Slope";
                case TerrainShadingAlgorithm.BlendMask:
                    return "Blend Mask";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text;

            text = "Height Exaggeration: " + _tile.HeightExaggeration + " (up/down)\n";
            text += "Shading Algorithm: " + TerrainShadingAlgorithmToString(_tile.ShadingAlgorithm) + " ('s' + left/right)\n";
            text += "Normals Algorithm: " + TerrainNormalsAlgorithmToString(_tile.NormalsAlgorithm) + " ('a' + left/right)\n";
            text += "Terrain: " + (_tile.ShowTerrain ? "on" : "off") + " ('t')\n";
            text += "Silhouette: " + (_tile.ShowSilhouette ? "on" : "off") + " ('l')\n";
            text += "Wireframe: " + (_tile.ShowWireframe ? "on" : "off") + " ('w')\n";
            text += "Normals: " + (_tile.ShowNormals ? "on" : "off") + " ('n')\n";

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
       
            if (e.Key == Key.S)
            {
                _sKeyDown = true;
            } 
            else if (e.Key == Key.A)
            {
                _aKeyDown = true;
            }
            else if ((e.Key == Key.Up) || (e.Key == Key.Down))
            {
                _tile.HeightExaggeration = Math.Max(1, _tile.HeightExaggeration + ((e.Key == Key.Up) ? 1 : -1));
            }
            else if (_sKeyDown && ((e.Key == Key.Left) || (e.Key == Key.Right)))
            {
                _tile.ShadingAlgorithm += (e.Key == Key.Right) ? 1 : -1;
                if (_tile.ShadingAlgorithm < TerrainShadingAlgorithm.ColorMap)
                {
                    _tile.ShadingAlgorithm = TerrainShadingAlgorithm.BlendMask;
                }
                else if (_tile.ShadingAlgorithm > TerrainShadingAlgorithm.BlendMask)
                {
                    _tile.ShadingAlgorithm = TerrainShadingAlgorithm.ColorMap;
                }
            }
            else if (_aKeyDown && ((e.Key == Key.Left) || (e.Key == Key.Right)))
            {
                _tile.NormalsAlgorithm += (e.Key == Key.Right) ? 1 : -1;
                if (_tile.NormalsAlgorithm < TerrainNormalsAlgorithm.None)
                {
                    _tile.NormalsAlgorithm = TerrainNormalsAlgorithm.SobelFilter;
                }
                else if (_tile.NormalsAlgorithm > TerrainNormalsAlgorithm.SobelFilter)
                {
                    _tile.NormalsAlgorithm = TerrainNormalsAlgorithm.None;
                }
            }
            else if (e.Key == Key.T)
            {
                _tile.ShowTerrain = !_tile.ShowTerrain;
            }
            else if (e.Key == Key.L)
            {
                _tile.ShowSilhouette = !_tile.ShowSilhouette;
            }
            else if (e.Key == Key.W)
            {
                _tile.ShowWireframe = !_tile.ShowWireframe;
            }
            else if (e.Key == Key.N)
            {
                _tile.ShowNormals = !_tile.ShowNormals;
            }

            UpdateHUD();
        }
        public override void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                _sKeyDown = false;
            }
            else if (e.Key == Key.A)
            {
                _aKeyDown = false;
            }
        }

        public override void Render(Context context)
        {
            context.Clear(ClearState);

            _tile.Render(context, SceneState);
            _hud.Render(context, SceneState);
        }

        private VertexDisplacementMapTerrainTile _tile;

        private SKFont _hudFont;
        private HeadsUpDisplay _hud;

        private bool _sKeyDown;
        private bool _aKeyDown;
    }
}