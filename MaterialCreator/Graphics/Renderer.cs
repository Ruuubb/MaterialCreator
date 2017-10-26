using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace MaterialCreator.Graphics
{
    class Renderer
    {
        TexturePool m_TexturePool;
        SpriteBatch m_SpriteBatch;

        //D3D11 stuff
        Device m_Device;
        SwapChain m_SwapChain;
        DeviceContext m_Context;
        SamplerState m_SamplerState;
        Texture2D m_BackBuffer;
        RenderTargetView m_RenderView;

        //2D static stuff
        VertexShader m_2DVertexShader;
        PixelShader m_2DPixelShader;
        InputLayout m_2DInputLayout;

        //3D dynamic stuff
        //

        Matrix m_ViewMatrix;
        Matrix m_ProjMatrix;

        public Renderer()
        {
            m_TexturePool = new TexturePool();
            m_SpriteBatch = new SpriteBatch();
        }

        public Device RawDevice
        {
            get { return m_Device; }
        }

        public TexturePool Textures
        {
            get { return m_TexturePool; }
        }

        public bool Initialize(RenderForm Form)
        {
            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription = new ModeDescription(Form.ClientSize.Width, Form.ClientSize.Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = Form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out m_Device, out m_SwapChain);
            m_Context = m_Device.ImmediateContext;

            m_SwapChain.ResizeBuffers(desc.BufferCount, Form.ClientSize.Width, Form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            m_BackBuffer = Texture2D.FromSwapChain<Texture2D>(m_SwapChain, 0);

            m_RenderView = new RenderTargetView(m_Device, m_BackBuffer);

            // Create 2D vars

            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"..\..\Shaders\MiniCube.fx", "VS", "vs_4_0");
            m_2DVertexShader = new VertexShader(m_Device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile(@"..\..\Shaders\MiniCube.fx", "PS", "ps_4_0");
            m_2DPixelShader = new PixelShader(m_Device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            m_2DInputLayout = new InputLayout(m_Device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR",    0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCORD",  0, Format.R32G32_Float, 32, 0)

                    });

            m_SamplerState = new SamplerState(m_Device, new SamplerStateDescription()
            {
                Filter = Filter.MinMagMipLinear,
                AddressU = TextureAddressMode.Clamp,
                AddressV = TextureAddressMode.Clamp,
                AddressW = TextureAddressMode.Clamp,
                BorderColor = Color.Black,
                ComparisonFunction = Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = -float.MaxValue,
                MaximumLod = float.MaxValue
            });


            // Set 2d context
            m_Context.Rasterizer.SetViewport(new Viewport(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f));
            m_Context.OutputMerger.SetTargets(m_RenderView);
            m_Context.InputAssembler.InputLayout = m_2DInputLayout;
            m_Context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            m_Context.VertexShader.Set(m_2DVertexShader);
            m_Context.PixelShader.Set(m_2DPixelShader);
            m_Context.PixelShader.SetSampler(0, m_SamplerState);


            m_SpriteBatch.Initialize(m_Device);

            m_ViewMatrix = Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), Vector3.UnitY);
            m_ProjMatrix = Matrix.OrthoLH((float)Form.ClientSize.Width, (float)Form.ClientSize.Height, -1, 10);

            return true;
        }

        public void OnResize(RenderForm Form)
        {
            Utilities.Dispose(ref m_BackBuffer);
            Utilities.Dispose(ref m_RenderView);

            m_SwapChain.ResizeBuffers(1, Form.ClientSize.Width, Form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

            m_BackBuffer = Texture2D.FromSwapChain<Texture2D>(m_SwapChain, 0);

            m_RenderView = new RenderTargetView(m_Device, m_BackBuffer);

            m_Context.Rasterizer.SetViewport(new Viewport(0, 0, Form.ClientSize.Width, Form.ClientSize.Height, 0.0f, 1.0f));
            m_Context.OutputMerger.SetTargets(m_RenderView);

            m_ProjMatrix = Matrix.OrthoLH((float)Form.ClientSize.Width, (float)Form.ClientSize.Height, -1, 10);
        }

        public void OnDraw(Sprite sp)
        {
            m_Context.ClearRenderTargetView(m_RenderView, Color.Black);

            m_Context.VertexShader.SetShaderResource(0, m_TexturePool.GetView(sp.TextureID));

            m_SpriteBatch.Begin(m_Context);

            var worldViewProj = m_ViewMatrix * m_ProjMatrix * sp.GetTransform.GetMatrix();
            worldViewProj.Transpose();

            float[] data = new float[40];
            int index = 0;

            foreach (Vertex i in sp.Vertices)
            {
                Vector2 rp = Vector2.TransformCoordinate(i.Position, worldViewProj);
                //rp -= new Vector2(1f, 1f);
                rp.ToArray().CopyTo(data, index);
                index += 2;
                data[index++] = 0f;
                data[index++] = 1f;

                i.Color.ToArray().CopyTo(data, index);
                index += 3;
                data[index++] = 1f;

                i.TexCoords.ToArray().CopyTo(data, index);
                index += 2;
            }

            m_SpriteBatch.Draw(data, sp.TextureID);


            m_SpriteBatch.End();


            m_SwapChain.Present(0, PresentFlags.None);
        }



    }
}
