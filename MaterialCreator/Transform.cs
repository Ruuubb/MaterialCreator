using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace MaterialCreator
{
    class Transform
    {
        Matrix m_Matrix;

        Vector2 m_Origin;
        Vector2 m_Position;
        Vector2 m_Scale;

        float m_Rotation;

        bool m_NeedUpdate;

        public Transform()
        {
            m_Matrix = Matrix.Identity;

            m_Origin = new Vector2(0, 0);
            m_Position = new Vector2(0, 0);
            m_Scale = new Vector2(1, 1);

            m_Rotation = 0;

            m_NeedUpdate = true;
        }

        public Matrix GetMatrix()
        {
            if(m_NeedUpdate)
            {
                float Angle = m_Rotation * (float)Math.PI / 180f;

                float Cosine = (float)Math.Cos(Angle);
                float Sine = (float)Math.Sin(Angle);

                float sxc = m_Scale.X * Cosine;
                float syc = m_Scale.Y * Cosine;
                float sxs = m_Scale.X * Sine;
                float sys = m_Scale.Y * Sine;

                float tx = -m_Origin.X * sxc - m_Origin.Y * sys + m_Position.X;
                float ty =  m_Origin.X * sxs - m_Origin.Y * syc + m_Position.Y;

                //THIS ONE IS FOR THE SHADER in dx11
                /*  m_Matrix = new Matrix(sxc, -sxs, 0.0f, 0    ,
                                       sys , syc , 0.0f, 0    ,
                                       0.0f, 0.0f, 1.0f, 0.0f ,
                                       tx  , ty  , 0.0f, 1.0f  );*/


                m_Matrix = new Matrix(sxc   , sys   , 0.0f  , tx    ,
                                      -sxs  , syc   , 0.0f  , ty    ,
                                      0     , 0     , 1.0f  , 0.0f  ,
                                      0     , 0     , 0     , 1.0f  );

                m_NeedUpdate = false;
            }

            return m_Matrix;
        }

        /* Position */
        public void SetPosition(float x, float y)
        {
            m_NeedUpdate = true;

            m_Position.X = x;
            m_Position.Y = y;
        }

        public void SetPosition(Vector2 v)
        {
            SetPosition(v.X, v.Y);
        }

        public void Move(float x, float y)
        {
            SetPosition(m_Position.X + x, m_Position.Y + y);
        }

        public void Move(Vector2 v)
        {
            SetPosition(m_Position.X + v.X, m_Position.Y + v.Y);
        }

        /* Rotation */
        public void SetRotation(float angle)
        {
            m_Rotation = angle % 360;

            if (m_Rotation < 0)
                m_Rotation += 360.0f;

            m_NeedUpdate = true;
        }

        public void Rotate(float angle)
        {
            SetRotation(m_Rotation + angle);
        }


        /* Scale */
        public void SetScale(float x, float y)
        {
            m_NeedUpdate = true;

            m_Scale.X = x;
            m_Scale.Y = y;
        }

        public void SetScale(Vector2 v)
        {
            SetScale(v.X, v.Y);
        }

        public void Scale(float x, float y)
        {
            SetScale(m_Scale.X + x, m_Scale.Y + y);
        }

        public void Scale(Vector2 v)
        {
            SetPosition(m_Scale.X + v.X, m_Scale.Y + v.Y);
        }

        /* Origin */
        public void SetOrigin(float x, float y)
        {
            m_NeedUpdate = true;

            m_Origin.X = x;
            m_Origin.Y = y;
        }

        public void SetOrigin(Vector2 v)
        {
            SetOrigin(v.X, v.Y);
        }
    }
}
