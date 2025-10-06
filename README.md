OpenGlobe
=========
This is an updated version of the old [OpenGlobe](https://github.com/virtualglobebook/OpenGlobe) project.
Current stack: .NET 6, SkiaSharp, OpenTK and Avalonia UI.

There are problems with framebuffer and textures (only on linux) !!!


OpenGlobe is a 3D engine for virtual globes (think [Google Earth](http://earth.google.com) or [NASA World Wind](http://worldwind.arc.nasa.gov)) designed to illustrate the engine design and rendering techniques described in our book, [3D Engine Design for Virtual Globes](http://www.virtualglobebook.com).  It is written in C# and uses the OpenGL 3.3 core profile via [OpenTK](http://www.opentk.com).  It is not a complete virtual globe application, but is rather a core engine and a number of runnable examples.

OpenGlobe has the following features and capabilities:

- A well designed (and pragmatic) renderer abstraction making it easier and less error prone to interface with OpenGL.
- WGS84 (and other ellipsoid) globe rendering using tessellation or GPU ray casting.
- Techniques for avoiding depth buffer errors when rendered objects are found at widely varying distances from the camera.
- High-precision vertex rendering techniques to avoid jittering problems.
- Vector data rendering, including reading vector data from shapefiles.
- Multithreaded resource preparation.
- Terrain patch rendering using CPU triangulation, GPU displacement mapping, and GPU ray casting.
- Terrain shading using procedural techniques.
- Whole-world terrain and imagery rendering on an accurate WGS84 globe using geometry clipmapping.


