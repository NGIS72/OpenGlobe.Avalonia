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
using System.IO;
using SkiaSharp;
using Avalonia.Input;
using Avalonia.Media;

namespace OpenGlobe.Examples
{
    sealed class VectorData : SceneBase, IDisposable
    {
        public override void Load(Context context)
        {
            base.Load(context);
            
            Ellipsoid globeShape = Ellipsoid.ScaledWgs84;

            SetCameraLookAtPoint(globeShape);

            _framebuffer = context.CreateFramebuffer();

            _clearBlack = new ClearState();
            _clearBlack.Color = Colors.Black;

            _clearWhite = new ClearState();
            _clearWhite.Color = Colors.White;

            _quad = new DayNightViewportQuad(context);

            _globe = new DayNightGlobe(context);
            _globe.Shape = globeShape;
            _globe.UseAverageDepth = true;
            _globe.DayTexture = context.Device.CreateTexture2D("NE2_50M_SR_W_4096.jpg", TextureFormat.RedGreenBlueAlpha8, false);
            _globe.NightTexture = context.Device.CreateTexture2D("land_ocean_ice_lights_2048.jpg", TextureFormat.RedGreenBlueAlpha8, false);

            _countries = new ShapefileRenderer("110m_admin_0_countries.shp", context, globeShape,
                new ShapefileAppearance()
                {
                    PolylineWidth = 1.0,
                    PolylineOutlineWidth = 1.0
                });
            _states = new ShapefileRenderer("110m_admin_1_states_provinces_lines_shp.shp", context, globeShape,
                new ShapefileAppearance()
                {
                    PolylineWidth = 1.0,
                    PolylineOutlineWidth = 1.0
                });
            _rivers = new ShapefileRenderer("50m-rivers-lake-centerlines.shp", context, globeShape,
                new ShapefileAppearance()
                {
                    PolylineColor = Colors.LightBlue,
                    PolylineOutlineColor = Colors.LightBlue,
                    PolylineWidth = 1.0,
                    PolylineOutlineWidth = 0.0
                });
            
            _populatedPlaces = new ShapefileRenderer("110m_populated_places_simple.shp", context, globeShape,
                new ShapefileAppearance() { Bitmap = SKBitmap.Decode("032.png") });
            _airports = new ShapefileRenderer("airprtx020.shp", context, globeShape, 
                new ShapefileAppearance() { Bitmap = SKBitmap.Decode("car-red.png") });
            _amtrakStations = new ShapefileRenderer("amtrakx020.shp", context, globeShape, 
                new ShapefileAppearance() { Bitmap = SKBitmap.Decode("paper-plane--arrow.png") });

            _hudFont = context.Device.CreateDefaultFont(16);
            _hud = new HeadsUpDisplay(context);
            _hud.Color = Colors.Blue;

            //_showVectorData = true;

            SceneState.DiffuseIntensity = 0.5f;
            SceneState.SpecularIntensity = 0.1f;
            SceneState.AmbientIntensity = 0.4f;
            SceneState.Camera.ZoomToTarget(globeShape.MaximumRadius);

            UpdateHUD();
        }

        private static string DayNightOutputToString(DayNightOutput dayNightOutput)
        {
            switch (dayNightOutput)
            {
                case DayNightOutput.Composite:
                    return "Composited Buffers";
                case DayNightOutput.DayBuffer:
                    return "Day Buffer";
                case DayNightOutput.NightBuffer:
                    return "Night Buffer";
                case DayNightOutput.BlendBuffer:
                    return "Blend Buffer";
            }

            return string.Empty;
        }

        private void UpdateHUD()
        {
            string text = "Output: " + DayNightOutputToString(_quad.DayNightOutput) + " ('o' + left/right)\n";
            text += "Vector Data: " + (_showVectorData ? "on" : "off") + " ('v')\n";
            text += "Wireframe: " + (_wireframe ? "on" : "off") + " ('w')\n";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = Context.Device.CreateTexture2D(
                Context.Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }
        public override void KeyDown(object sender, KeyEventArgs e)
        {
            base.KeyDown(sender, e);
        
            if (e.Key == Key.O)
            {
                _oKeyDown = true;
            }
            else if (_oKeyDown && ((e.Key == Key.Left) || (e.Key == Key.Right)))
            {
                _quad.DayNightOutput += (e.Key == Key.Right) ? 1 : -1;

                if (_quad.DayNightOutput < DayNightOutput.Composite)
                {
                    _quad.DayNightOutput = DayNightOutput.BlendBuffer;
                }
                else if (_quad.DayNightOutput > DayNightOutput.BlendBuffer)
                {
                    _quad.DayNightOutput = DayNightOutput.Composite;
                }
            }
            else if (e.Key == Key.V)
            {
                _showVectorData = !_showVectorData;
            }
            else if (e.Key == Key.W)
            {
                _wireframe = !_wireframe;

                _countries.Wireframe = _wireframe;
                _states.Wireframe = _wireframe;
                _rivers.Wireframe = _wireframe;
                _populatedPlaces.Wireframe = _wireframe;
                _airports.Wireframe = _wireframe;
                _amtrakStations.Wireframe = _wireframe;
            }

            UpdateHUD();
        }

        public override void KeyUp(object sender, KeyEventArgs e)
        {
            base.KeyUp(sender, e);
        
            if (e.Key == Key.O)
            {
                _oKeyDown = false;
            }
        }

        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
            UpdateFramebufferAttachments();
        }

        private void UpdateFramebufferAttachments()
        {
            DisposeFramebufferAttachments();
            _dayTexture = Context.Device.CreateTexture2D(new Texture2DDescription(Width, Height, TextureFormat.RedGreenBlueAlpha8, false));
            _nightTexture = Context.Device.CreateTexture2D(new Texture2DDescription(Width, Height, TextureFormat.RedGreenBlueAlpha8, false));
            _blendTexture = Context.Device.CreateTexture2D(new Texture2DDescription(Width, Height, TextureFormat.Red32f, false));
            _depthTexture = Context.Device.CreateTexture2D(new Texture2DDescription(Width, Height, TextureFormat.Depth32f, false));
            
            _quad.DayTexture = _dayTexture;
            _quad.NightTexture = _nightTexture;
            _quad.BlendTexture = _blendTexture;
        }

        public override void Render(Context context)
        {
            //
            // Render to frame buffer
            //
            context.Framebuffer = _framebuffer;

            SetFramebufferAttachments(_dayTexture, _nightTexture, null);
            context.Clear(_clearBlack);

            SetFramebufferAttachments(null, null, _blendTexture);
            context.Clear(_clearWhite);

            //
            // Render globe to day, night, and blend buffers
            //
            SetFramebufferAttachments(_dayTexture, _nightTexture, _blendTexture);
            _globe.Render(context, SceneState);

            if (_showVectorData)
            {
                SetFramebufferAttachments(_dayTexture, null, null);

                //
                // Render vector data, layered bottom to top, to the day buffer only
                //
                _countries.Render(context, SceneState);
                _rivers.Render(context, SceneState);
                _states.Render(context, SceneState);
                _populatedPlaces.Render(context, SceneState);
                _airports.Render(context, SceneState);
                _amtrakStations.Render(context, SceneState);
            }

            //
            // Render viewport quad to composite buffers
            //
            context.Framebuffer = null;
            _quad.Render(context, SceneState);
            _hud.Render(context, SceneState);
        }

        private void SetFramebufferAttachments(Texture2D day, Texture2D night, Texture2D blend)
        {
            _framebuffer.ColorAttachments[_globe.FragmentOutputs("dayColor")] = day;
            _framebuffer.ColorAttachments[_globe.FragmentOutputs("nightColor")] = night;
            _framebuffer.ColorAttachments[_globe.FragmentOutputs("blendAlpha")] = blend;
            _framebuffer.DepthAttachment = _depthTexture;
        }

        private void DisposeFramebufferAttachments()
        {
            if (_dayTexture != null)
            {
                _dayTexture.Dispose();
                _dayTexture = null;
            }

            if (_nightTexture != null)
            {
                _nightTexture.Dispose();
                _nightTexture = null;
            }

            if (_blendTexture != null)
            {
                _blendTexture.Dispose();
                _blendTexture = null;
            }

            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
                _depthTexture = null;
            }
        }

        private ClearState _clearBlack;
        private ClearState _clearWhite;

        private Texture2D _dayTexture;
        private Texture2D _nightTexture;
        private Texture2D _blendTexture;
        private Texture2D _depthTexture;
        private Framebuffer _framebuffer;
        private DayNightViewportQuad _quad;

        private DayNightGlobe _globe;
        private ShapefileRenderer _countries;
        private ShapefileRenderer _states;
        private ShapefileRenderer _rivers;
        private ShapefileRenderer _populatedPlaces;
        private ShapefileRenderer _airports;
        private ShapefileRenderer _amtrakStations;
        
        private SKFont _hudFont;
        private HeadsUpDisplay _hud;

        private bool _showVectorData;
        private bool _wireframe;

        private bool _oKeyDown;
    }
}