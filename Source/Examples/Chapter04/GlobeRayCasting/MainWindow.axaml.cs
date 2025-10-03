using Avalonia.Controls;
using OpenGlobe.Examples;

namespace OpenGlobe.Examples
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GlobeView.Scene = new GlobeRayCasting();
        }
    }
}