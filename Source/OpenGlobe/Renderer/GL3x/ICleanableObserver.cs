#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer.GL3x
{
    internal interface ICleanableObserver
    {
        void NotifyDirty(ICleanable value);
    }
}
