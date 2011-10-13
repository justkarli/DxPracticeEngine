using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CoreEngine.VertexFormats;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D10;
using SlimDX.DXGI;
using Buffer = SlimDX.Direct3D10.Buffer;
using Device = SlimDX.Direct3D10.Device;
using System.Drawing;

namespace DxApplication
{
    public class SimpleBoxRenderEngine : IRenderEngine
    {
        public void LoadResources(Device device)
        {
            ColoredVertex[] coloredVertices = new ColoredVertex[]
            {
                new ColoredVertex(new Vector3(-1, -1, -1), Color.Red.ToArgb()), 
                new ColoredVertex(new Vector3(1, -1, -1), Color.LightBlue.ToArgb()), 
                new ColoredVertex(new Vector3(1, -1, 1), Color.LightCyan.ToArgb()), 
                new ColoredVertex(new Vector3(-1, -1, 1), Color.CadetBlue.ToArgb()), 
                new ColoredVertex(new Vector3(-1, 1, -1), Color.Red.ToArgb()), 
                new ColoredVertex(new Vector3(1, 1, -1), Color.OrangeRed.ToArgb()), 
                new ColoredVertex(new Vector3(1, 1, 1), Color.Goldenrod.ToArgb()), 
                new ColoredVertex(new Vector3(-1, 1, 1), Color.Yellow.ToArgb())
            };

            short[] indices = new short[]
            {
                4, 1, 0, 4, 5, 1, 
                5, 2, 1, 5, 6, 2,
                6, 3, 2, 6, 7, 3,
                7, 0, 3, 7, 4, 0,
                7, 5, 4, 7, 6, 5, 
                2, 3, 0, 2, 0, 1
            };

            DataStream outStream = new DataStream(8 * Marshal.SizeOf(typeof(ColoredVertex)), true, true);
            DataStream outStreamIndex = new DataStream(36 * Marshal.SizeOf(typeof(short)), true, true);

            outStream.WriteRange(coloredVertices);
            outStreamIndex.WriteRange(indices);

            outStream.Position = 0;
            outStreamIndex.Position = 0;

            BufferDescription buffer_description = new BufferDescription();
            buffer_description.BindFlags = BindFlags.VertexBuffer;
            buffer_description.CpuAccessFlags = CpuAccessFlags.None;
            buffer_description.OptionFlags = ResourceOptionFlags.None;
            buffer_description.SizeInBytes = 8*Marshal.SizeOf(typeof (short));
            buffer_description.Usage = ResourceUsage.Default;

            BufferDescription buffer_description_index = new BufferDescription();
            buffer_description_index.BindFlags = BindFlags.IndexBuffer;
            buffer_description_index.CpuAccessFlags = CpuAccessFlags.None;
            buffer_description_index.OptionFlags = ResourceOptionFlags.None;
            buffer_description_index.SizeInBytes = 36*Marshal.SizeOf(typeof (short));
            buffer_description_index.Usage = ResourceUsage.Default;

            _vertexBuffer = new Buffer(device, outStream, buffer_description);
            _indexBuffer = new Buffer(device, outStreamIndex, buffer_description_index);

            outStream.Close();
            outStreamIndex.Close();

            _effect = Effect.FromFile(device, @"C:\Users\karli\Documents\Visual Studio 2010\Projects\DxApplications\DxApplication\Shader\SimpleRendering.fx",
                "fx_4_0", ShaderFlags.None, EffectFlags.None);
            _effectTechnique = _effect.GetTechniqueByIndex(0);
            _effectPass = _effectTechnique.GetPassByIndex(0);
            _transformVariable = _effect.GetVariableByName("WorldViewProj").AsMatrix();

            _vertexLayout = new InputLayout(device, _inputElements, _effectPass.Description.Signature);
        }

        public void Render(Device device, Matrix world, Matrix viewProj)
        {
            device.InputAssembler.SetInputLayout(_vertexLayout);
            device.InputAssembler.SetPrimitiveTopology(PrimitiveTopology.TriangleList);
            device.InputAssembler.SetIndexBuffer(_indexBuffer, Format.R16_UInt, 0);
            device.InputAssembler.SetVertexBuffers(0, 
                new VertexBufferBinding(_vertexBuffer, Marshal.SizeOf(typeof(ColoredVertex)), 0));
            _transformVariable.SetMatrix(world * viewProj);
            _effectPass.Apply();

            device.DrawIndexed(36, 0, 0);
        }

        public void OnDeviceCreated(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnDeviceDestroyed(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnDeviceLost(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnDeviceReset(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public void OnMainLoop(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private Buffer _vertexBuffer;
        private Buffer _indexBuffer;
        private readonly InputElement[] _inputElements = new InputElement[]
        {
            new InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0),
            new InputElement("COLOR", 0, Format.R8G8_B8G8_UNorm, 12, 0)
        };

        private InputLayout _vertexLayout;
        private Effect _effect;
        private EffectTechnique _effectTechnique;
        private EffectPass _effectPass;
        private EffectMatrixVariable _transformVariable;
    }
}
