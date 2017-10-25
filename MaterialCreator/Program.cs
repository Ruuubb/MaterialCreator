using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;

using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Windows;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace MaterialCreator
{
    /// <summary>
    /// SharpDX MiniCube Direct3D 11 Sample
    /// </summary>
    internal static class Program
    {
        //      [STAThread]
        private static void Main()
        {
            var form = new RenderForm("SharpDX - MiniCube Direct3D11 Sample");
            form.ClientSize = new System.Drawing.Size(1800, 900);
            form.MaximizeBox = false;

            List<float> m = new List<float>();

            float[] ttt = { 2f, 3f };

            m.AddRange(ttt);

            var desc = new SwapChainDescription()
            {
                BufferCount = 1,
                ModeDescription =
                    new ModeDescription(form.ClientSize.Width, form.ClientSize.Height,
                                        new Rational(60, 1), Format.R8G8B8A8_UNorm),
                IsWindowed = true,
                OutputHandle = form.Handle,
                SampleDescription = new SampleDescription(1, 0),
                SwapEffect = SwapEffect.Discard,
                Usage = Usage.RenderTargetOutput
            };

            Device device;
            SwapChain swapChain;
            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, desc, out device, out swapChain);
            var context = device.ImmediateContext;

            var factory = swapChain.GetParent<Factory>();
            factory.MakeWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAll);
    
            var vertexShaderByteCode = ShaderBytecode.CompileFromFile(@"..\..\Shaders\MiniCube.fx", "VS", "vs_4_0");
            var vertexShader = new VertexShader(device, vertexShaderByteCode);

            var pixelShaderByteCode = ShaderBytecode.CompileFromFile(@"..\..\Shaders\MiniCube.fx", "PS", "ps_4_0");
            var pixelShader = new PixelShader(device, pixelShaderByteCode);

            var signature = ShaderSignature.GetInputSignature(vertexShaderByteCode);

            var layout = new InputLayout(device, signature, new[]
                    {
                        new InputElement("POSITION", 0, Format.R32G32B32A32_Float, 0, 0),
                        new InputElement("COLOR",    0, Format.R32G32B32A32_Float, 16, 0),
                        new InputElement("TEXCORD",  0, Format.R32G32_Float, 32, 0)
                        
                    });

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            var view = Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix proj = Matrix.Identity;

            bool userResized = true;
            Texture2D backBuffer = null;
            RenderTargetView renderView = null;


            form.UserResized += (sender, args) => userResized = true;

            bool Rotate = false;
            
            form.KeyUp += (sender, args) =>
            {
                if (args.KeyCode == Keys.F5)
                    Rotate = false;
                else if (args.KeyCode == Keys.F4)
                    Rotate = true;
                else if (args.KeyCode == Keys.Escape)
                    form.Close();
            };

            SamplerState Sampler;
            Sampler = new SamplerState(device, new SamplerStateDescription()
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

            Texture TestTex = new Texture();
            TestTex.LoadFromFile(device, @"..\..\Textures\brick.jpg", "Bricks");

            context.PixelShader.SetShaderResource(0, TestTex.TextureView);
            context.PixelShader.SetSampler(0, Sampler);

            Graphics.SpriteBatch SB = new Graphics.SpriteBatch();
            SB.Initialize(device);

            Sprite TestSprite = new Sprite(TestTex.Info);
            TestSprite.GetTransform.SetPosition(0, 0);

            RenderLoop.Run(form, () =>
            {
                if (userResized)
                {
                    Utilities.Dispose(ref backBuffer);
                    Utilities.Dispose(ref renderView);

                    swapChain.ResizeBuffers(desc.BufferCount, form.ClientSize.Width, form.ClientSize.Height, Format.Unknown, SwapChainFlags.None);

                    backBuffer = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);

                    renderView = new RenderTargetView(device, backBuffer);

                    context.Rasterizer.SetViewport(new Viewport(0, 0, form.ClientSize.Width, form.ClientSize.Height, 0.0f, 1.0f));
                    context.OutputMerger.SetTargets( renderView);

                    proj = Matrix.OrthoLH((float)form.ClientSize.Width, (float)form.ClientSize.Height , -1, 10);

                    userResized = false;
                }

                context.ClearRenderTargetView(renderView, Color.Black);

                if (Rotate)
                    TestSprite.GetTransform.Move(0.01f, 0);

                var worldViewProj =  view * proj * TestSprite.GetTransform.GetMatrix() ;
                worldViewProj.Transpose();

                SB.Begin(context);

                float[] data = new float[40];
                int index = 0;

                foreach(Vertex i in TestSprite.Vertices)
                {
                    Vector2 rp = Vector2.TransformCoordinate(i.Position - new Vector2(form.ClientSize.Width /2, form.ClientSize.Height/2), worldViewProj);
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

                SB.Draw(data, TestSprite.TextureID);

                SB.End();
                
                swapChain.Present(0, PresentFlags.None);
            });

            signature.Dispose();
            vertexShaderByteCode.Dispose();
            vertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            pixelShader.Dispose();
            layout.Dispose();
            renderView.Dispose();
            backBuffer.Dispose();
            context.ClearState();
            context.Flush();
            device.Dispose();
            context.Dispose();
            swapChain.Dispose();
            factory.Dispose();
        }
    }
}