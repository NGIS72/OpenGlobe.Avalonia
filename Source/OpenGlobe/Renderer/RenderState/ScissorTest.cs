#region License
//
// (C) Copyright 2009 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class ScissorTest
    {
        public ScissorTest()
        {
            Enabled = false;
            Rectangle = new Avalonia.PixelRect(0, 0, 0, 0);
        }

        public bool Enabled { get; set; }
        public Avalonia.PixelRect Rectangle { get; set; }
    }
}
