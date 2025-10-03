#region License
//
// (C) Copyright 2010 Patrick Cozzi, Deron Ohlarik, and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using Avalonia;
using Avalonia.Input;
using OpenGlobe.Renderer;
using OpenGlobe.Scene.Cameras;
using OpenTK.Mathematics;
using System.Diagnostics;
using Vector3D = OpenGlobe.Core.Vector3D;


namespace OpenGlobe.Scene
{
    public class CameraFly : IDisposable, ICameraView
    {
        public CameraFly(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException("camera");
            }

            _camera = camera;

            Enabled = true;
        }

        /// <summary>
        /// Disposes the camera.  After it is disposed, the camera should not be used.
        /// </summary>
        public void Dispose()
        {
            Enabled = false;
        }

        public Vector3D Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public Vector3D Look
        {
            get { return new Vector3D(_look.X, _look.Y, _look.Z); }
            set { _look = new Vector3d(value.X, value.Y, value.Z); }
        }

        public Vector3D Up
        {
            get { return new Vector3D(_up.X, _up.Y, _up.Z); }
            set { _up = new Vector3d(value.X, value.Y, value.Z); }
        }

        public double MovementRate
        {
            get { return _movementRate; }
            set { _movementRate = value; }
        }

        /// <summary>
        /// Simulates a press of a mouse button at a particular point in client window coordinates.
        /// </summary>
        /// <param name="button">The mouse button that is pressed.</param>
        /// <param name="point">The point at which the mouse button was pressed, in client window coordinates.</param>
        public void MouseDown(MouseButton button, Point point)
        {
            if (button == MouseButton.Left)
            {
                _leftButtonDown = true;
            }
            else if (button == MouseButton.Right)
            {
                _rightButtonDown = true;
            }

            _lastPoint = point;
        }

        /// <summary>
        /// Simulates a release of a mouse button at a particular point in client window coordinates.
        /// </summary>
        /// <param name="button">The mouse button that was released.</param>
        /// <param name="point">The point at which the mouse button was released, in client window coordinates.</param>
        public void MouseUp(MouseButton button, Point point)
        {
            if (button == MouseButton.Left)
            {
                _leftButtonDown = false;
            }
            else if (button == MouseButton.Right)
            {
                _rightButtonDown = false;
            }
        }

        /// <summary>
        /// Simulates a mouse move to a particular point in client window coordinates.
        /// </summary>
        /// <param name="point">The point to which the mouse moved, in client window coordinates.</param>
        public void MouseMove(Avalonia.Controls.Control view, Point point)
        {
            if (!_leftButtonDown && !_rightButtonDown)
            {
                return;
            }

            UpdateParametersFromCamera();

            Size movement = new Size(point.X - _lastPoint.X, point.Y - _lastPoint.Y);

            if (_leftButtonDown)
            {
                Rotate(view, movement);
            }

            UpdateCameraFromParameters();

            _lastPoint = point;
        }

        public void PreRender()
        {
            UpdateParametersFromCamera();
            HandleKey(Key.Up, MoveForward);
            HandleKey(Key.Down, MoveBackward);
            HandleKey(Key.Left, MoveLeft);
            HandleKey(Key.Right, MoveRight);
            HandleKey(Key.Delete, RollLeft);
            HandleKey(Key.PageDown, RollRight);
            UpdateCameraFromParameters();
        }

        private void HandleKey(Key key, Action<double> action)
        {
            long startTime;
            if (m_keyDownTime.TryGetValue(key, out startTime))
            {
                long now = Stopwatch.GetTimestamp();
                long moveTime = now - startTime;
                action((double)moveTime / Stopwatch.Frequency);
                m_keyDownTime[key] = now;
            }
        }

        private void MoveForward(double seconds)
        {
            double distance = MovementRate * seconds;
            _position += _camera.Forward * distance;
        }

        private void MoveBackward(double seconds)
        {
            double distance = MovementRate * seconds;
            _position -= _camera.Forward * distance;
        }

        private void MoveLeft(double seconds)
        {
            Vector3D right = _camera.Forward.Cross(_camera.Up);
            double distance = MovementRate * seconds;
            _position -= right * distance;
        }

        private void MoveRight(double seconds)
        {
            Vector3D right = _camera.Forward.Cross(_camera.Up);
            double distance = MovementRate * seconds;
            _position += right * distance;
        }

        private void RollLeft(double seconds)
        {
            Quaterniond rotation = Quaterniond.FromAxisAngle(_look, -seconds);

            _up = Vector3d.Transform(_up, rotation);
            _right = Vector3d.Cross(_look, _up);

            _up.Normalize();
            _right.Normalize();
        }

        private void RollRight(double seconds)
        {
            Quaterniond rotation = Quaterniond.FromAxisAngle(_look, seconds);

            _up = Vector3d.Transform(_up, rotation);
            _right = Vector3d.Cross(_look, _up);

            _up.Normalize();
            _right.Normalize();
        }

        private void Rotate(Avalonia.Controls.Control window, Size movement)
        {
            double horizontalWindowRatio = (double)movement.Width / (double)window.Bounds.Width;
            double verticalWindowRatio = (double)movement.Height / (double)window.Bounds.Height;

            // Horizontal movement is rotation around the Up-axis
            // Vertical movement is rotation around the Right-axis
            Quaterniond horizontalRotation = Quaterniond.FromAxisAngle(_up, -horizontalWindowRatio);
            Quaterniond verticalRotation = Quaterniond.FromAxisAngle(_right, verticalWindowRatio);

            _look = Vector3d.Transform(_look, horizontalRotation);
            _look = Vector3d.Transform(_look, verticalRotation);
            _up = Vector3d.Transform(_up, horizontalRotation);
            _up = Vector3d.Transform(_up, verticalRotation);
            _right = Vector3d.Cross(_look, _up);

            _look.Normalize();
            _up.Normalize();
            _right.Normalize();
        }

        /// <summary>
        /// Updates <see cref="Azimuth"/>, <see cref="Elevation"/>, and <see cref="Range"/>
        /// properties based on the current position of the renderer <see cref="Camera"/>.
        /// </summary>
        public void UpdateParametersFromCamera()
        {
            _position = _camera.Eye;
            _look = new Vector3d(_camera.Forward.X, _camera.Forward.Y, _camera.Forward.Z);
            _up = new Vector3d(_camera.Up.X, _camera.Up.Y, _camera.Up.Z);
            _right = new Vector3d(_camera.Right.X, _camera.Right.Y, _camera.Right.Z);
        }

        private void UpdateCameraFromParameters()
        {
            _camera.Eye = _position;
            _camera.Target = _position + Look;
            _camera.Up = Up;
        }

        /// <summary>
        /// Gets a value indicating if mouse and keyboard input is enabled or disabled.  If the value of this property
        /// is <see langword="true" />, the camera will respond to mouse and keyboard events.  If it is <see langword="false" />,
        /// mouse and keyboard events will be ignored.
        /// </summary>
        public bool Enabled
        {
            get { return m_Enabled; }
            set
            {
                if (value != m_Enabled)
                    m_Enabled = value;
            }
        }

        #region ICameraView

        public void KeyDown(object sender, Avalonia.Input.KeyEventArgs args)
        {
            if (!m_Enabled) return;
            if(!m_keyDownTime.ContainsKey(args.Key)) 
                m_keyDownTime.Add(args.Key, Stopwatch.GetTimestamp());
        }

        public void KeyUp(object sender, Avalonia.Input.KeyEventArgs args)
        {
            if (!m_Enabled) return;
            if (m_keyDownTime.ContainsKey(args.Key))
                m_keyDownTime.Remove(args.Key);
        }

        public void PointerWheelChanged(object sender, PointerEventArgs e)
        {

        }

        public void PointerEntered(object sender, PointerEventArgs e)
        {
            
        }

        public void PointerExited(object sender, PointerEventArgs e)
        {
            
        }

        public void PointerMoved(object sender, PointerEventArgs e)
        {
            if (!m_Enabled) return;
            var pos = e.GetPosition(sender as Avalonia.Visual);
            MouseMove((Avalonia.Controls.Control)sender, pos);
        }

        public void PointerPressed(object sender, PointerEventArgs e)
        {
            if (!m_Enabled) return;
            var button = e.Properties.PointerUpdateKind.GetMouseButton();
            var pos = e.GetPosition(sender as Avalonia.Visual);
            MouseDown(button, pos);
        }

        public void PointerReleased(object sender, PointerEventArgs e)
        {
            if (!m_Enabled) return;
            var button = e.Properties.PointerUpdateKind.GetMouseButton();
            var pos = e.GetPosition(sender as Avalonia.Visual);
            MouseUp(button, pos);
        }

        public void PointerCaptureLost(object sender, PointerCaptureLostEventArgs e)
        {

        }

        #endregion

        private Camera _camera;

        private bool m_Enabled;
        private bool _leftButtonDown;
        private bool _rightButtonDown;
        private Dictionary<Key, long> m_keyDownTime = new Dictionary<Key, long>();
        private Point _lastPoint;

        private Vector3D _position;
        private Vector3d _look;
        private Vector3d _up;
        private Vector3d _right;
        private double _movementRate = 300.0;
    }
}
