#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using Avalonia.Controls;

namespace OpenGlobe.Renderer
{
    public static class PersistentView
    {
        public static void Execute(string filename, Control window, Camera camera)
        {
            if (File.Exists(filename))
            {
                camera.LoadView(filename);
            }

            window.KeyDown += (s,e)=>
            {
                if (e.Key==Avalonia.Input.Key.Space)
                {
                    camera.SaveView(filename);
                }
            };
        }
    }
}