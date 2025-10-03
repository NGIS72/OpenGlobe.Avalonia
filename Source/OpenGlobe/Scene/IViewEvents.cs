using Avalonia.Input;

namespace OpenGlobe.Scene
{
    public interface IViewEvents
    {
        void KeyDown(object sender, KeyEventArgs e);
        void KeyUp(object sender, KeyEventArgs e);

        /// <summary>
        /// Occurs when the mouse wheel is scrolled over the control.
        /// </summary>
        void PointerWheelChanged(object sender, PointerEventArgs e);
        /// <summary>
        /// Occurs when the pointer enters the control.
        /// </summary>
        void PointerEntered(object sender, PointerEventArgs e);
        /// <summary>
        /// Occurs when the pointer leaves the control.
        /// </summary>>
        void PointerExited(object sender, PointerEventArgs e);
        /// <summary>
        /// Occurs when the pointer moves over the control.
        /// </summary>
        void PointerMoved(object sender, PointerEventArgs e);
        /// <summary>
        /// Occurs when the pointer is pressed over the control.
        /// </summary>
        void PointerPressed(object sender, PointerEventArgs e);
        /// <summary>
        /// Occurs when the pointer is released over the control.
        /// </summary>
        void PointerReleased(object sender, PointerEventArgs e);

        void PointerCaptureLost(object sender, PointerCaptureLostEventArgs e);
    }
}
