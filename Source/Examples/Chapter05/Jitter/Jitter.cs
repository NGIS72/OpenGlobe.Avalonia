#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using OpenGlobe.Core;
using OpenGlobe.Renderer;
using OpenGlobe.Scene;
using SkiaSharp;
using Avalonia.Media;
using Avalonia.Input;

namespace OpenGlobe.Examples
{
    public enum JitterAlgorithm
    {
        RelativeToWorld,
        RelativeToCenter,
        CPURelativeToEye,
        GPURelativeToEye,
        GPURelativeToEyeDSFUN90,
        GPURelativeToEyeLOD,
    }

    sealed class Jitter : SceneBase, IDisposable
    {
        private Context m_Context;

        public override void Load(Context context)
        {
            base.Load(context);
        
            m_Context = context;

            _hudFont = context.Device.CreateDefaultFont( 16);
            _hud = new HeadsUpDisplay(context);
            _hud.Color = Colors.Black;

            CreateCamera();
            CreateAlgorithm(context);
        }

        private double ToMeters(double value)
        {
            return _scaleWorldCoordinates ? (value * Ellipsoid.Wgs84.MaximumRadius) : value;
        }

        private double FromMeters(double value)
        {
            return _scaleWorldCoordinates ? (value / Ellipsoid.Wgs84.MaximumRadius) : value;
        }

        private static string JitterAlgorithmToString(JitterAlgorithm algorithm)
        {
            switch (algorithm)
            {
                case JitterAlgorithm.RelativeToWorld:
                    return "Relative to World [Jittery]";
                case JitterAlgorithm.RelativeToCenter:
                    return "Realtive to Center";
                case JitterAlgorithm.CPURelativeToEye:
                    return "CPU Relative to Eye";
                case JitterAlgorithm.GPURelativeToEye:
                    return "GPU Relative To Eye";
                case JitterAlgorithm.GPURelativeToEyeDSFUN90:
                    return "GPU Relative To Eye [DSFUN90]";
                case JitterAlgorithm.GPURelativeToEyeLOD:
                    return "GPU Relative To Eye [LOD]";
            }

            return string.Empty;
        }

        private void UpdateHUD(Context context)
        {
            var camera = (CameraLookAtPoint)Camera;
            string text;

            text = "Scale World Coordinates: " + _scaleWorldCoordinates + " ('s')\n";
            text += "Algorithm: " + JitterAlgorithmToString(_jitterAlgorithm) + " (left/right)\n";
            text += "Distance: " + string.Format(CultureInfo.CurrentCulture, "{0:N}", ToMeters(camera.Range));

            if (_hud.Texture != null)
            {
                _hud.Texture.Dispose();
                _hud.Texture = null;
            }
            _hud.Texture = context.Device.CreateTexture2D(
                context.Device.CreateBitmapFromText(text, _hudFont),
                TextureFormat.RedGreenBlueAlpha8, false);
        }

        private void CreateCamera()
        {
            _xTranslation = FromMeters(Ellipsoid.Wgs84.Radii.X);

            Camera camera = SceneState.Camera;
            camera.PerspectiveNearPlaneDistance = FromMeters(0.01);
            camera.PerspectiveFarPlaneDistance = FromMeters(5000000);
            camera.Target = Vector3D.UnitX * _xTranslation;
            camera.Eye = Vector3D.UnitX * _xTranslation * 1.1;

            
            var cameraView = new CameraLookAtPoint(camera, Ellipsoid.UnitSphere);
            cameraView.Range = (camera.Eye - camera.Target).Magnitude;
            cameraView.MinimumZoomRate = FromMeters(1);
            cameraView.MaximumZoomRate = FromMeters(Double.MaxValue);
            cameraView.ZoomFactor = 10;
            cameraView.ZoomRateRangeAdjustment = 0;
            cameraView.MinimumRotateRate = 1.0;
            cameraView.MaximumRotateRate = 1.0;
            cameraView.RotateRateRangeAdjustment = 0.0;
            cameraView.RotateFactor = 0.0;
            var old = Camera;
            Camera = cameraView;
            (old as IDisposable)?.Dispose();
        }

        private void CreateAlgorithm(Context context)
        {
            double triangleLength = FromMeters(200000);
            double triangleDelta = FromMeters(0.5);

            Vector3D[] positions = new Vector3D[]
            {
                new Vector3D(_xTranslation, triangleDelta + 0, 0),                  // Red triangle
                new Vector3D(_xTranslation, triangleDelta + triangleLength, 0),
                new Vector3D(_xTranslation, triangleDelta + 0, triangleLength),
                new Vector3D(_xTranslation, -triangleDelta - 0, 0),                 // Green triangle
                new Vector3D(_xTranslation, -triangleDelta - 0, triangleLength),
                new Vector3D(_xTranslation, -triangleDelta - triangleLength, 0),
                new Vector3D(_xTranslation, 0, 0),                                  // Blue point
            };

            byte[] colors = new byte[]
            {
                255, 0, 0,
                255, 0, 0,
                255, 0, 0,
                0, 255, 0,
                0, 255, 0,
                0, 255, 0,
                0, 0, 255
            };

            if (_algorithm != null)
            {
                ((IDisposable)_algorithm).Dispose();
                _algorithm = null;
            }

            switch (_jitterAlgorithm)
            {
                case JitterAlgorithm.RelativeToWorld:
                    _algorithm = new RelativeToWorld(context, positions, colors);
                    break;
                case JitterAlgorithm.RelativeToCenter:
                    _algorithm = new RelativeToCenter(context, positions, colors);
                    break;
                case JitterAlgorithm.CPURelativeToEye:
                    _algorithm = new CPURelativeToEye(context, positions, colors);
                    break;
                case JitterAlgorithm.GPURelativeToEye:
                    _algorithm = new GPURelativeToEye(context, positions, colors);
                    break;
                case JitterAlgorithm.GPURelativeToEyeDSFUN90:
                    _algorithm = new GPURelativeToEyeDSFUN90(context, positions, colors);
                    break;
                case JitterAlgorithm.GPURelativeToEyeLOD:
                    _algorithm = new SceneGPURelativeToEyeLOD(context, positions, colors);
                    break;
            }
        }
        public override void KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.S)
            {
                _scaleWorldCoordinates = !_scaleWorldCoordinates;

                CreateCamera();
                CreateAlgorithm(m_Context);
            }
            else if ((e.Key == Key.Left) || (e.Key == Key.Right))
            {
                _jitterAlgorithm += (e.Key == Key.Right) ? 1 : -1;

                if (_jitterAlgorithm < JitterAlgorithm.RelativeToWorld)
                {
                    _jitterAlgorithm = JitterAlgorithm.GPURelativeToEyeLOD;
                }
                else if (_jitterAlgorithm > JitterAlgorithm.GPURelativeToEyeLOD)
                {
                    _jitterAlgorithm = JitterAlgorithm.RelativeToWorld;
                }

                CreateAlgorithm(m_Context);
            }
            else if ((e.Key == Key.Down) || (e.Key == Key.Up))
            {
                var camera = Camera as CameraLookAtPoint;
                if (camera != null)
                    camera.Range += (e.Key == Key.Down) ? FromMeters(0.01) : FromMeters(-0.01);
            }
        }

        public override void Render(Context context)
        {
            UpdateHUD(context);

            context.Clear(ClearState);

            _algorithm.Render(context, SceneState);
            _hud.Render(context, SceneState);
        }

        private SKFont _hudFont;
        private HeadsUpDisplay _hud;

        private double _xTranslation;
        
        private bool _scaleWorldCoordinates;
        private IRenderable _algorithm;
        private JitterAlgorithm _jitterAlgorithm;
    }
}