#region License
//
// (C) Copyright 2010 Patrick Cozzi and Deron Ohlarik
//
// Distributed under the MIT License.
// See License.txt or http://www.opensource.org/licenses/mit-license.php.
//
#endregion

namespace OpenGlobe.Renderer
{
    public class ShaderCache
    {
        private Context m_Context;
        public ShaderCache(Context context) 
        {
            m_Context = context;
        }
        public ShaderProgram FindOrAdd(
            string key,
            string vertexShaderSource,
            string fragmentShaderSource)
        {
            ShaderProgram shaderProgram;

            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram;
                if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
                {
                    ++cachedShaderProgram.ReferenceCount;
                    shaderProgram = cachedShaderProgram.ShaderProgram;
                }
                else
                {
                    shaderProgram = m_Context.Device.CreateShaderProgram(vertexShaderSource, fragmentShaderSource);
                    _shaderPrograms.Add(key, new CachedShaderProgram(shaderProgram));
                }
            }

            return shaderProgram;
        }

        public ShaderProgram FindOrAdd(
            string key,
            string vertexShaderSource,
            string geometryShaderSource,
            string fragmentShaderSource)
        {
            ShaderProgram shaderProgram;

            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram;
                if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
                {
                    ++cachedShaderProgram.ReferenceCount;
                    shaderProgram = cachedShaderProgram.ShaderProgram;
                }
                else
                {
                    shaderProgram = m_Context.Device.CreateShaderProgram(vertexShaderSource,
                        geometryShaderSource, fragmentShaderSource);
                    _shaderPrograms.Add(key, new CachedShaderProgram(shaderProgram));
                }
            }

            return shaderProgram;
        }

        public ShaderProgram Find(string key)
        {
            ShaderProgram shaderProgram = null;

            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram;
                if (_shaderPrograms.TryGetValue(key, out cachedShaderProgram))
                {
                    ++cachedShaderProgram.ReferenceCount;
                    shaderProgram = cachedShaderProgram.ShaderProgram;
                }
            }

            return shaderProgram;
        }

        public void Release(string key)
        {
            lock (_shaderPrograms)
            {
                CachedShaderProgram cachedShaderProgram = _shaderPrograms[key];
                if (--cachedShaderProgram.ReferenceCount == 0)
                {
                    _shaderPrograms.Remove(key);
                    cachedShaderProgram.ShaderProgram.Dispose();
                }
            }
        }

        private Dictionary<string, CachedShaderProgram> _shaderPrograms = new Dictionary<string, CachedShaderProgram>();
    }
}
