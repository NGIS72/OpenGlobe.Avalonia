#region License
//
// (C) Copyright 2010 Patrick Cozzi and Kevin Ring
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

using System.Reflection;

namespace OpenGlobe.Scene
{
    internal class EmbeddedResources
    {
        public static string GetText(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return GetText(assembly, resourceName) ?? GetText(typeof(EmbeddedResources).Assembly, resourceName);
        }

        public static string GetText(Assembly assembly, string resourceName)
        {
            var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null) return null;

            using (stream)
            using (var streamReader = new StreamReader(stream))
                return streamReader.ReadToEnd();
        }

    }
}
