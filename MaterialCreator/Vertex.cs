using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MaterialCreator
{
    class Vertex
    {
        Vector2 m_Position;
        Vector3 m_Color;
        Vector2 m_TexCoords;

        public Vector2 Position
        {
            get { return m_Position; }
            set { m_Position = value; }
        }

        public Vector3 Color
        {
            get { return m_Color; }
            set { m_Color = value; }
        }

        public Vector2 TexCoords
        {
            get { return m_TexCoords; }
            set { m_TexCoords = value; }
        }

        public Vertex()
        {
            m_Position = new Vector2(0, 0); 
            m_Color = new Vector3(255f, 255f, 255f);
            m_TexCoords = new Vector2(0, 0);
        }

        public Vertex(Vector2 p)
        {
            m_Position = p;
            m_Color = new Vector3(255f, 255f, 255f);
            m_TexCoords = new Vector2(0, 0);
        }

        public Vertex(Vector2 p, Vector3 c)
        {
            m_Position = p;
            m_Color = c;
            m_TexCoords = new Vector2(0, 0);
        }

        public Vertex(Vector2 p, Vector2 tc)
        {
            m_Position = p;
            m_Color = new Vector3(255f, 255f, 255f);
            m_TexCoords = tc;
        }

        public Vertex(Vector2 p, Vector3 c, Vector2 tc)
        {
            m_Position = p;
            m_Color = c;
            m_TexCoords = tc;
        }
    }
}
