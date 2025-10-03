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
using System.Collections.Generic;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using Avalonia.Input;
using Avalonia.Media;

// deron junk todo
//
// clipping to wall shader
// wall normal angle is not a good way to determine which shader to use
//

namespace OpenGlobe.Research
{
    sealed class LinesOnTerrain : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
        
            SceneState.Camera.PerspectiveFarPlaneDistance = 4096;
            SceneState.Camera.PerspectiveNearPlaneDistance = 10;
            
            _instructions = new HeadsUpDisplay(context);
            _instructions.Texture = Device.CreateTexture2D(
                Device.CreateBitmapFromText(
                    "u - Use silhouette\ns - Show silhouette\n",
                    Device.CreateDefaultFont(24)),
                TextureFormat.RedGreenBlueAlpha8, false);
            _instructions.Color = Colors.LightBlue;

            ///////////////////////////////////////////////////////////////////

            TerrainTile terrainTile = TerrainTile.FromBitmap(@"ps-e.lg.png");
            _tile = new TriangleMeshTerrainTile(context, terrainTile);
            _tile.HeightExaggeration = 30.0f;

            ///////////////////////////////////////////////////////////////////

            double tileRadius = Math.Max(terrainTile.Resolution.X, terrainTile.Resolution.Y) * 0.5;
            var camera = new CameraLookAtPoint(SceneState.Camera, Ellipsoid.UnitSphere);
            camera.CenterPoint = new Vector3D(terrainTile.Resolution.X * 0.5, terrainTile.Resolution.Y * 0.5, 0.0);
            Camera = camera;
            
            SceneState.Camera.Eye = new Vector3D(_xPos, 256, 0);

            SceneState.Camera.ZoomToTarget(tileRadius);
            //
            // Positions
            //
            IList<Vector3D> positions = new List<Vector3D>();
            double temp = 1.2 * _tile.HeightExaggeration;
            positions.Add(new Vector3D(0.0, 0.0, -temp));
            positions.Add(new Vector3D(0.0, 0.0, temp));
            positions.Add(new Vector3D(100.0, 100.0, -temp));
            positions.Add(new Vector3D(100.0, 100.0, temp));
            positions.Add(new Vector3D(200.0, 100.0, -temp));
            positions.Add(new Vector3D(200.0, 100.0, temp));
            positions.Add(new Vector3D(256.0, 256.0, -temp));
            positions.Add(new Vector3D(256.0, 256.0, temp));
            positions.Add(new Vector3D(512.0, 512.0, -temp));
            positions.Add(new Vector3D(512.0, 512.0, temp));

            //
            // junk 
            _polylineOnTerrain = new PolylineOnTerrain(context);
            _polylineOnTerrain.Set(context, positions);


            // junk
            string fs =
                @"#version 330

                uniform sampler2D og_texture0;
                in vec2 fsTextureCoordinates;
                out vec4 fragmentColor;

                void main()
                {
                    if (texture(og_texture0, fsTextureCoordinates).r == 0.0)
                    {
                        fragmentColor = vec4(0.0, 0.0, 0.0, 1.0);
                    }
                    else
                    {
                        discard;
                    }
                }";

            _viewportQuad = new ViewportQuad(context, fs);
        }

        public override void Render(Context context)
        {
            //
            // Terrain and silhouette textures
            //
            _tile.RenderDepthAndSilhouetteTextures(context, SceneState, _silhouette);

            //
            // Terrain to framebuffer
            //
            context.Framebuffer = null;
             context.Clear(ClearState);
            _tile.Render(context, SceneState);

            //
            // Overlay the silhouette texture over the framebuffer
            //
            if (_showSilhouette)
            {
                _viewportQuad.Texture = _tile.SilhouetteTexture;
                _viewportQuad.Render(context, SceneState);
            }

            //
            // Render the line on terrain
            //
            _polylineOnTerrain.Render(context, SceneState, _tile.SilhouetteTexture, _tile.DepthTexture);

            //
            // Render the instructions
            //
            _instructions.Render(context, SceneState);
        }
        public override void KeyDown(object sender, KeyEventArgs e)
        {
            base.KeyDown(sender, e);
        
            if (e.Key == Key.U)
            {
                _silhouette = !_silhouette;
            }
            else if (e.Key == Key.S)
            {
                _showSilhouette = !_showSilhouette;
            }
        }

        #region IDisposable Members

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        
            _instructions.Dispose();
            _tile.Dispose();
            _polylineOnTerrain.Dispose();
            _viewportQuad.Dispose();
        }

        #endregion

        private HeadsUpDisplay _instructions;
        private TriangleMeshTerrainTile _tile;
        private PolylineOnTerrain _polylineOnTerrain;
        private ViewportQuad _viewportQuad;
        
        private bool _silhouette = true;
        private bool _showSilhouette = false;

        private double _xPos = 448; // junk deron todo use this still?
    }
}