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
using System.Globalization;
using Avalonia.Input;
using Avalonia.Media;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using SkiaSharp;

namespace OpenGlobe.Examples
{
    sealed class DepthBufferPrecision : SceneBase, IDisposable
    {
        private Context m_Context;
        public override void Load(Context context)
        {
            base.Load(context);
            m_Context = context;
        
            _globeShape = Ellipsoid.Wgs84;
            _nearDistance = 1;
            _cubeRootFarDistance = 300;

            SceneState.DiffuseIntensity = 0.45f;
            SceneState.SpecularIntensity = 0.05f;
            SceneState.AmbientIntensity = 0.5f;

            SetCameraLookAtPoint(_globeShape);

            SceneState.Camera.ZoomToTarget(_globeShape.MaximumRadius);

            ///////////////////////////////////////////////////////////////////

            _globe = new TessellatedGlobe(context);
            _globe.Shape = _globeShape;
            _globe.NumberOfSlicePartitions = 64;
            _globe.NumberOfStackPartitions = 32;
            _globe.Texture = context.Device.CreateTexture2D("world_topo_bathy_200411_3x5400x2700.jpg", TextureFormat.RedGreenBlue8, false);
            _globe.Textured = true;

            _plane = new Plane(context);
            _plane.XAxis = 0.6 * _globeShape.MaximumRadius * Vector3D.UnitX;
            _plane.YAxis = 0.6 * _globeShape.MinimumRadius * Vector3D.UnitZ;
            _plane.OutlineWidth = 3;
            _cubeRootPlaneHeight = 100.0;
            UpdatePlaneOrigin();

            _viewportQuad = new ViewportQuad(context, null);

            _framebuffer = context.CreateFramebuffer();
            _depthFormatIndex = 1;
            _depthTestLess = true;
            _logarithmicDepthConstant = 1;
            UpdatePlanesAndDepthTests();

            ///////////////////////////////////////////////////////////////////

            _hudFont = context.Device.CreateDefaultFont(16);
            _hud = new HeadsUpDisplay(context);
            _hud.Color = Colors.Blue;
            UpdateHUD();
        }
        public override void Resize(int width, int height)
        {
            base.Resize(width, height);
        
            UpdateFramebufferAttachments();
        }

        public override void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N)
            {
                _nKeyDown = false;
            }
            else if (e.Key == Key.F)
            {
                _fKeyDown = false;
            }
            else if (e.Key == Key.C)
            {
                _cKeyDown = false;
            }
        }
        public override void KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.N)
            {
                _nKeyDown = true;
            }
            else if (e.Key == Key.F)
            {
                _fKeyDown = true;
            }
            else if (e.Key == Key.C)
            {
                _cKeyDown = true;
            }
            else if (_nKeyDown && ((e.Key == Key.Up) || (e.Key == Key.Down)))
            {
                _nearDistance += (e.Key == Key.Up) ? 1 : -1;

                UpdatePlanesAndDepthTests();
            }
            else if (_fKeyDown && ((e.Key == Key.Up) || (e.Key == Key.Down)))
            {
                _cubeRootFarDistance += (e.Key == Key.Up) ? 1 : -1;

                UpdatePlanesAndDepthTests();
            }
            else if ((e.Key == Key.Add) || (e.Key == Key.Subtract))
            {
                _cubeRootPlaneHeight += (e.Key == Key.Add) ? 1 : -1;

                UpdatePlaneOrigin();
            }
            else if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                _depthFormatIndex += (e.Key == Key.Right) ? 1 : -1;
                if (_depthFormatIndex < 0)
                {
                    _depthFormatIndex = 2;
                }
                else if (_depthFormatIndex > 2)
                {
                    _depthFormatIndex = 0;
                }

                UpdateFramebufferAttachments();
            }
            else if (e.Key == Key.D)
            {
                _depthTestLess = !_depthTestLess;

                UpdatePlanesAndDepthTests();
            }
            else if (e.Key == Key.L)
            {
                _logarithmicDepthBuffer = !_logarithmicDepthBuffer;

                UpdateLogarithmicDepthBuffer();
            }
            else if (_cKeyDown && ((e.Key == Key.Up) || (e.Key == Key.Down)))
            {
                _logarithmicDepthConstant += (e.Key == Key.Up) ? 0.1 : -0.1;

                UpdateLogarithmicDepthConstant();
            }

            UpdateHUD();
        }

        private void UpdatePlaneOrigin()
        {
            _plane.Origin = -(_globeShape.MaximumRadius * Vector3D.UnitY +
                (_cubeRootPlaneHeight * _cubeRootPlaneHeight * _cubeRootPlaneHeight * Vector3D.UnitY));
        }

        private void UpdateFramebufferAttachments()
        {
            DisposeFramebufferAttachments();
            _colorTexture = m_Context.Device.CreateTexture2D(new Texture2DDescription(Width, Height, TextureFormat.RedGreenBlue8, false));
            _depthTexture = m_Context.Device.CreateTexture2D(new Texture2DDescription(Width, Height, _depthFormats[_depthFormatIndex], false));
            _framebuffer.ColorAttachments[0] = _colorTexture;
            _framebuffer.DepthAttachment = _depthTexture;
            _viewportQuad.Texture = _colorTexture;
        }

        private void UpdatePlanesAndDepthTests()
        {
            double farDistance = _cubeRootFarDistance * _cubeRootFarDistance * _cubeRootFarDistance;

            SceneState.Camera.PerspectiveNearPlaneDistance = _depthTestLess ? _nearDistance : farDistance;
            SceneState.Camera.PerspectiveFarPlaneDistance = _depthTestLess ? farDistance : _nearDistance;

            _globe.DepthTestFunction = _depthTestLess ? DepthTestFunction.Less : DepthTestFunction.Greater;
            _plane.DepthTestFunction = _depthTestLess ? DepthTestFunction.Less : DepthTestFunction.Greater;
        }

        private void UpdateLogarithmicDepthBuffer()
        {
            _globe.LogarithmicDepth = _logarithmicDepthBuffer;
            _plane.LogarithmicDepth = _logarithmicDepthBuffer;
        }

        private void UpdateLogarithmicDepthConstant()
        {
            _globe.LogarithmicDepthConstant = (float)_logarithmicDepthConstant;
            _plane.LogarithmicDepthConstant = (float)_logarithmicDepthConstant;
        }

        private void UpdateViewerHeight()
        {
            double height = SceneState.Camera.Height(_globeShape);
            if (_viewerHeight != height)
            {
                _viewerHeight = height;
                UpdateHUD();
            }
        }

        private void UpdateHUD()
        {
            string text;

            text = "Near Plane: " + string.Format(CultureInfo.CurrentCulture, "{0:N}" + " ('n' + up/down)", _nearDistance) + "\n";
            text += "Far Plane: " + string.Format(CultureInfo.CurrentCulture, "{0:N}" + " ('f' + up/down)", _cubeRootFarDistance * _cubeRootFarDistance * _cubeRootFarDistance) + "\n";
            text += "Viewer Height: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _viewerHeight) + "\n";
            text += "Plane Height: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", _cubeRootPlaneHeight * _cubeRootPlaneHeight * _cubeRootPlaneHeight) + " ('-'/'+')\n";
            text += "Depth Test: " + (_depthTestLess ? "less" : "greater") + " ('d')\n";
            text += "Depth Format: " + _depthFormatsStrings[_depthFormatIndex] + " (left/right)\n";
            text += "Logarithmic Depth Buffer: " + (_logarithmicDepthBuffer ? "on" : "off") + " ('l')\n";
            text += "Logarithmic Depth Constant: " + _logarithmicDepthConstant + " ('c' + up/down)";

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = m_Context.Device.CreateTexture2D(
                m_Context.Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        public override void Render(Context context)
        {            
            UpdateViewerHeight();

            //
            // Render to frame buffer
            //
            context.Framebuffer = _framebuffer;

            ClearState.Depth = _depthTestLess ? 1 : 0;
            context.Clear(ClearState);

            _globe.Render(context, SceneState);
            _plane.Render(context, SceneState);

            //
            // Render viewport quad to show contents of frame buffer's color buffer
            //
            context.Framebuffer = null;
            _viewportQuad.Render(context, SceneState);
            _hud.Render(context, SceneState);
        }

        private void DisposeFramebufferAttachments()
        {
            if (_colorTexture != null)
            {
                _colorTexture.Dispose();
                _colorTexture = null;
            }

            if (_depthTexture != null)
            {
                _depthTexture.Dispose();
                _depthTexture = null;
            }
        }

        private Ellipsoid _globeShape;
        private double _nearDistance;
        private double _cubeRootFarDistance;

        private TessellatedGlobe _globe;
        private Plane _plane;
        private double _cubeRootPlaneHeight;
        private double _viewerHeight;
        private ViewportQuad _viewportQuad;

        private Texture2D _colorTexture;
        private Texture2D _depthTexture;
        private Framebuffer _framebuffer;
        private int _depthFormatIndex;
        private bool _depthTestLess;
        private bool _logarithmicDepthBuffer;
        private double _logarithmicDepthConstant;

        private SKFont _hudFont;
        private HeadsUpDisplay _hud;

        private bool _nKeyDown;
        private bool _fKeyDown;
        private bool _cKeyDown;

        private readonly TextureFormat[] _depthFormats = new TextureFormat[]
        {
            TextureFormat.Depth16,
            TextureFormat.Depth24,
            TextureFormat.Depth32f
        };
        private readonly string[] _depthFormatsStrings = new string[]
        {
            "16-bit fixed point",
            "24-bit fixed point",
            "32-bit floating point",
        };
    }
}