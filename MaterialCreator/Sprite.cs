using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using SharpDX;

namespace MaterialCreator
{
    class Sprite
    {
        UInt32 m_TextureID;
        RectangleF m_TextCords;

        Transform m_Transform;

        Vertex[] m_Vertices;

        public Transform GetTransform
        {
            get { return m_Transform; }
        }     

        public UInt32 TextureID
        {
            get { return m_TextureID; }
        }

        public Vertex[] Vertices
        {
            get { return m_Vertices; }
        }

        public Sprite()
        {
            m_TextureID = 0;
            m_TextCords = new RectangleF(0, 0, 0, 0);
            m_Transform = new Transform();
            m_Vertices = new Vertex[4] { new Vertex(), new Vertex(), new Vertex(), new Vertex() };
;        }

        public Sprite(Graphics.TextureInfo Info)
        {
            m_TextCords = Info.Rect;
            m_TextureID = Info.TextureID;
            m_Transform = new Transform();
            m_Vertices = new Vertex[4] { new Vertex(), new Vertex(), new Vertex(), new Vertex() };

            UpdatePosition();
            UpdateTexCoords();
        }

        private void UpdateTexCoords()
        {
            m_Vertices[0].TexCoords = new Vector2(0, 1);
            m_Vertices[1].TexCoords = new Vector2(0, 0);
            m_Vertices[2].TexCoords = new Vector2(1, 0);
            m_Vertices[3].TexCoords = new Vector2(1, 1);
        }

        private void UpdatePosition()
        {
            m_Vertices[0].Position = new Vector2(0, 0);
            m_Vertices[1].Position = new Vector2(0, m_TextCords.Height);
            m_Vertices[2].Position = new Vector2(m_TextCords.Width, m_TextCords.Height);
            m_Vertices[3].Position = new Vector2(m_TextCords.Width, 0);
        }
    }
}
