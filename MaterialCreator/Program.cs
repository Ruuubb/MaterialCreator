using System;
using System.Diagnostics;
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
                        new InputElement("TEXCORD", 0, Format.R32G32_Float, 16, 0)
                        //new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 16, 0)
                    });

            var vertices = Buffer.Create(device, BindFlags.VertexBuffer, new[]
                                  {/*
                                      new Vector4(0f  , 0f    ,  0f, 1.0f),     new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                                      new Vector4(0f  , 100f    ,  0f, 1.0f),   new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(100f  ,100f  ,  0f, 1.0f),    new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(100f  ,100f    ,  0f, 1.0f),  new Vector4(0.0f, 0.0f, 1.0f, 1.0f),
                                      new Vector4(100f  , 0    ,  0f, 1.0f),    new Vector4(0.0f, 1.0f, 0.0f, 1.0f),
                                      new Vector4(0  ,0  ,  0f, 1.0f),          new Vector4(1.0f, 0.0f, 0.0f, 1.0f) */
                                      0  ,     0,  0,  1,       0,  0,
                                      0  ,   100,  0,  1,       0,  1,
                                      100,   100,  0,  1,       1,  1,
                                      100,   100,  0,  1,       1,  1,
                                      100,     0,  0,  1,       1,  0,
                                      0  ,     0,  0,  1,       0,  0

                            });

            var contantBuffer = new Buffer(device, Utilities.SizeOf<Matrix>(), ResourceUsage.Default, BindFlags.ConstantBuffer, CpuAccessFlags.None, ResourceOptionFlags.None, 0);

            context.InputAssembler.InputLayout = layout;
            context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertices, sizeof(float) * 6/*Utilities.SizeOf<Vector4>() * 2*/, 0));
            context.VertexShader.SetConstantBuffer(0, contantBuffer);
            context.VertexShader.Set(vertexShader);
            context.PixelShader.Set(pixelShader);

            var view = Matrix.LookAtLH(new Vector3(0, 0, -1), new Vector3(0, 0, 0), Vector3.UnitY);
            Matrix proj = Matrix.Identity;

            bool userResized = true;
            Texture2D backBuffer = null;
            RenderTargetView renderView = null;

            Transform tt = new Transform();

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
            
            tt.SetPosition(000, 0);
            //tt.SetScale(50, 50);
            tt.SetOrigin(50,50);

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

                tt.Move(0.00f, 0.01f);
                if(Rotate)
                    tt.Rotate(0.1f);
                var worldViewProj = tt.GetMatrix()* view * proj  ;
                worldViewProj.Transpose();

                context.UpdateSubresource(ref worldViewProj, contantBuffer);

                context.Draw(6, 0);

                swapChain.Present(0, PresentFlags.None);
            });

            signature.Dispose();
            vertexShaderByteCode.Dispose();
            vertexShader.Dispose();
            pixelShaderByteCode.Dispose();
            pixelShader.Dispose();
            vertices.Dispose();
            layout.Dispose();
            contantBuffer.Dispose();
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