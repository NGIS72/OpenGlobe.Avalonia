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
    public enum SourceBlendingFactor
    {
        Zero,
        One,
        SourceAlpha,
        OneMinusSourceAlpha,
        DestinationAlpha,
        OneMinusDestinationAlpha,
        DestinationColor,
        OneMinusDestinationColor,
        SourceAlphaSaturate,
        ConstantColor,
        OneMinusConstantColor,
        ConstantAlpha,
        OneMinusConstantAlpha
    }

    public enum DestinationBlendingFactor
    {
        Zero,
        One,
        SourceColor,
        OneMinusSourceColor,
        SourceAlpha,
        OneMinusSourceAlpha,
        DestinationAlpha,
        OneMinusDestinationAlpha,
        DestinationColor,
        OneMinusDestinationColor,
        ConstantColor,
        OneMinusConstantColor,
        ConstantAlpha,
        OneMinusConstantAlpha
    }

    public enum BlendEquation
    {
        Add,
        Minimum,
        Maximum,
        Subtract,
        ReverseSubtract
    }

    public class Blending
    {
        public Blending()
        {
            Enabled = false;
            SourceRGBFactor = SourceBlendingFactor.One;
            SourceAlphaFactor = SourceBlendingFactor.One;
            DestinationRGBFactor = DestinationBlendingFactor.Zero;
            DestinationAlphaFactor = DestinationBlendingFactor.Zero;
            RGBEquation = BlendEquation.Add;
            AlphaEquation = BlendEquation.Add;
            Color = Avalonia.Media.Color.FromArgb(0, 0, 0, 0);
        }

        public bool Enabled { get; set; }
        public SourceBlendingFactor SourceRGBFactor { get; set; }
        public SourceBlendingFactor SourceAlphaFactor { get; set; }
        public DestinationBlendingFactor DestinationRGBFactor { get; set; }
        public DestinationBlendingFactor DestinationAlphaFactor { get; set; }
        public BlendEquation RGBEquation { get; set; }
        public BlendEquation AlphaEquation { get; set; }
        public Avalonia.Media.Color Color { get; set; }
    }
}
