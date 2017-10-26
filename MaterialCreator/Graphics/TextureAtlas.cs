using System;
using System.Collections.Generic;
using SharpDX;

namespace MaterialCreator.Graphics
{
    struct AtlasTexture
    {
        string Name;
        RectangleF TexCoords;
    }

    class TextureAtlas
    {
        UInt32 m_TextureID;

        List<AtlasTexture> m_Textures;

    }
}
