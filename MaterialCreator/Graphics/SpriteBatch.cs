using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

namespace MaterialCreator.Graphics
{
    class SpriteBatch
    {
        DeviceContext m_Context;

        Buffer m_Vbo;
        Buffer m_Ibo;

        VertexBufferBinding m_BufferBinding;

        int m_MaxSpritesPerDraw;

        int m_VboSize;

        const int m_SpriteSizeInBytes = 10; // Vec4 pos - Vec4 col - Vec 2 texcoords == 10.
        const int m_IndicesPerQuad = 6;

        int m_QueueIndex;
        float[] m_DataQueue;

        int m_IndicesIndex;
        UInt16[] m_IndicesQueue;

        UInt32 m_CurrentTexID;

        public SpriteBatch(int MaxSpritesPerDraw = 1000)
        {
            if (MaxSpritesPerDraw > 2000)
                MaxSpritesPerDraw = 2000;

            m_MaxSpritesPerDraw = MaxSpritesPerDraw;
            m_VboSize = MaxSpritesPerDraw * sizeof(float) * m_SpriteSizeInBytes;

            m_QueueIndex = 0;
            m_DataQueue = new float[m_VboSize];

            m_IndicesIndex = 0;
            m_IndicesQueue = new UInt16[MaxSpritesPerDraw * m_IndicesPerQuad];

            m_CurrentTexID = 0;         
        }

        public bool Initialize(Device dev)
        {
            if (m_VboSize < 1)
                return false;

            var VboBufDesc = new BufferDescription(m_VboSize, ResourceUsage.Dynamic, BindFlags.VertexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);

            m_Vbo = new Buffer(dev, VboBufDesc);
            m_BufferBinding = new VertexBufferBinding(m_Vbo, sizeof(float) * m_SpriteSizeInBytes, 0);



            var IboBufDesc = new BufferDescription(m_MaxSpritesPerDraw * m_IndicesPerQuad, ResourceUsage.Dynamic, BindFlags.IndexBuffer, CpuAccessFlags.Write, ResourceOptionFlags.None, 0);
            m_Ibo = new Buffer(dev, IboBufDesc);

            return true;
        }

        public void Begin(DeviceContext Context)
        {
            m_Context = Context;

            m_Context.InputAssembler.SetVertexBuffers(0, m_BufferBinding);
            m_Context.InputAssembler.SetIndexBuffer(m_Ibo, SharpDX.DXGI.Format.R16_UInt, 0);
        }

        public void End()
        {
            if(m_QueueIndex > 0)
            {
                UploadToGpu();
                Draw();
                m_QueueIndex = 0;
                m_IndicesIndex = 0;
            }

            m_QueueIndex = 0;
        }

        public void Draw(float[] Vertices, UInt32 TextureID)
        {
            if(m_CurrentTexID != 0 && m_CurrentTexID != TextureID) //If true, upload current sprites to gpu -> draw -> restart
            {
                UploadToGpu();
                Draw();
                m_QueueIndex = 0;
                m_IndicesIndex = 0;
            }

            m_IndicesQueue[m_IndicesIndex++] = (UInt16)(m_QueueIndex + 0);
            m_IndicesQueue[m_IndicesIndex++] = (UInt16)(m_QueueIndex + 1);
            m_IndicesQueue[m_IndicesIndex++] = (UInt16)(m_QueueIndex + 2);
            m_IndicesQueue[m_IndicesIndex++] = (UInt16)(m_QueueIndex + 3);
            m_IndicesQueue[m_IndicesIndex++] = (UInt16)(m_QueueIndex + 0);
            m_IndicesQueue[m_IndicesIndex++] = (UInt16)(m_QueueIndex + 2);

            Vertices.CopyTo(m_DataQueue, m_QueueIndex);
            m_QueueIndex += Vertices.Length;
        }

        private void UploadToGpu()
        {
            {
                DataStream Stream;

                var dataBox = m_Context.MapSubresource(m_Vbo, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out Stream);

                Stream.WriteRange<float>(m_DataQueue, 0, m_QueueIndex );

                m_Context.UnmapSubresource(m_Vbo, 0);
            }
            {
                DataStream Stream;

                var dataBox = m_Context.MapSubresource(m_Ibo, MapMode.WriteDiscard, SharpDX.Direct3D11.MapFlags.None, out Stream);

                Stream.WriteRange<UInt16>(m_IndicesQueue, 0, m_IndicesIndex);

                m_Context.UnmapSubresource(m_Ibo, 0);
            }        
        }

        private void Draw()
        {
            m_Context.DrawIndexed(m_IndicesIndex, 0, 0);
        }
    }
}
