namespace OpenGlobe.Scene.Cameras
{
    public interface ICameraView:IViewEvents
    {
        void PreRender();
        bool Enabled { get; set; }
        
    }
}
