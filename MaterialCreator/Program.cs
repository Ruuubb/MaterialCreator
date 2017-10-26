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
            var form = new RenderForm("Material editor");
            form.ClientSize = new System.Drawing.Size(1800, 900);
            form.MaximizeBox = false;

            Graphics.Renderer Ren = new Graphics.Renderer();
            Ren.Initialize(form);

            form.UserResized += (sender, args) =>
            {
                Ren.OnResize(form);
            };

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

            if(!Ren.Textures.LoadTexture(Ren.RawDevice, @"..\..\Textures\brick.jpg", "Brick"))
                Console.WriteLine("ERROR in loading brick.jpg");

            Graphics.TextureInfo Info;
            if (!Ren.Textures.GetTexture("Brick", out Info))
                Console.WriteLine("ERROR in getting brick");

            Sprite TestSprite = new Sprite(Info);

            RenderLoop.Run(form, () =>
            {
                Ren.OnDraw(TestSprite);
 
                if (Rotate)
                    TestSprite.GetTransform.Rotate(0.01f);               
            });
        }
    }
}