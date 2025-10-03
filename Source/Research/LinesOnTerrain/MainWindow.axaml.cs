using Avalonia.Controls;
using OpenGlobe.Examples;
using OpenGlobe.Research;

namespace OpenGlobe.Examples
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            GlobeView.Scene = new LinesOnTerrain();
        }
    }
}