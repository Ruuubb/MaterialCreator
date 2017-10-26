using System;
using System.Collections.Generic;
using SharpDX;
using SharpDX.Direct3D11;

namespace MaterialCreator.Graphics
{
    class TexturePool
    {
        UInt32 m_NextTextureID;

        List<Texture> m_Textures;
        List<TextureAtlas> m_TextureAtlasses;

        public TexturePool()
        {
            m_NextTextureID = 1;

            m_Textures = new List<Texture>();
            m_TextureAtlasses = new List<TextureAtlas>();
        }

        public bool LoadTexture(Device Dev, string FileName, string TextureName)
        {
            Texture NewText = new Texture(TextureName, m_NextTextureID);
            m_NextTextureID++;

            m_Textures.Add(NewText);

            return NewText.LoadFromFile(Dev, FileName);
        }

        public bool LoadTextureAtlass(Device Dev, string FileName, List<AtlasTexture> m_Textures)
        {
            return false;
        }

        public bool GetTexture(string Name, out TextureInfo Info)
        {
            foreach(Texture i in m_Textures)
            {
                if(i.Name == Name)
                {
                    Info = i.Info;
                    return true;
                }
            }

            foreach(TextureAtlas i in m_TextureAtlasses)
            {

            }

            Info = new TextureInfo();
            return false;
        }

        public ShaderResourceView GetView(UInt32 ID)
        {
            foreach (Texture i in m_Textures)
            {
                if (i.ID == ID)
                    return i.View;
            }

            return null;
        }

    }
}
