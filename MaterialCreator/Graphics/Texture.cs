﻿using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

using SharpDX.DXGI;
using SharpDX.Direct3D11;
using Device = SharpDX.Direct3D11.Device;

namespace MaterialCreator.Graphics
{
    class Texture
    {
        TextureInfo m_Info;

        Texture2D m_Texture;
        ShaderResourceView m_TextureView;

        public Texture(String Name, UInt32 ID)
        {
            m_Info = new TextureInfo();

            m_Info.Name = Name;
            m_Info.TextureID = ID;
        }

        public UInt32 ID
        {
            get { return m_Info.TextureID; }
        }

        public String Name
        {
            get { return m_Info.Name; }
        }

        public TextureInfo Info
        {
            get { return m_Info; }
        }

        public ShaderResourceView View
        {
            get { return m_TextureView; }
        }

        public bool LoadFromFile(Device device, String FileName)
        {
            if (!File.Exists(FileName))
                return false;

            Bitmap bitmap = (Bitmap)Bitmap.FromFile(FileName);

            m_Info.Rect = new SharpDX.RectangleF(0, 0, bitmap.Width, bitmap.Height);

            if (bitmap.PixelFormat != PixelFormat.Format32bppArgb)
            {
                bitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);
            }

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            m_Texture = new SharpDX.Direct3D11.Texture2D(device, new SharpDX.Direct3D11.Texture2DDescription()
            {
                Width = bitmap.Width,
                Height = bitmap.Height,
                ArraySize = 1,
                BindFlags = SharpDX.Direct3D11.BindFlags.ShaderResource,
                Usage = SharpDX.Direct3D11.ResourceUsage.Immutable,
                CpuAccessFlags = SharpDX.Direct3D11.CpuAccessFlags.None,
                Format = SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                MipLevels = 1,
                OptionFlags = SharpDX.Direct3D11.ResourceOptionFlags.None,
                SampleDescription = new SharpDX.DXGI.SampleDescription(1, 0),
            }, new SharpDX.DataRectangle(data.Scan0, data.Stride));
            bitmap.UnlockBits(data);

            m_TextureView = new ShaderResourceView(device, m_Texture);

            return true;
        }


    }
}
