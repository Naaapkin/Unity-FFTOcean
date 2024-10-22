using UnityEngine;
using UnityEngine.Rendering;

public class FastFourierTransform
{
    private readonly int KERNEL_SCALE;
    private readonly int KERNEL_SHIFT;
    private readonly int KERNEL_STEP;
    private readonly int KERNEL_BIT_REVERSE;
    
    private ComputeShader fftShader;
    private ComputeBuffer bitReverseIndices;
    private RenderTexture buffer;
    
    private int size;

    public FastFourierTransform(ComputeShader fftShader, int size)
    {
        this.fftShader = fftShader;

        KERNEL_SCALE = fftShader.FindKernel("Scale");
        KERNEL_SHIFT = fftShader.FindKernel("Shift");
        KERNEL_STEP = fftShader.FindKernel("Step");
        KERNEL_BIT_REVERSE = fftShader.FindKernel("BitReverse");
        
        Allocate(size);
    }
    
    public void FFT2D(RenderTexture input)
    {
        int fftGroupSize = Mathf.Max(1, size >> 4);
        int groupSize = Mathf.Max(1, size >> 3);
        fftShader.SetTexture(KERNEL_BIT_REVERSE, P_BUFFER0, input);
        fftShader.SetTexture(KERNEL_BIT_REVERSE, P_BUFFER1, buffer);
        fftShader.Dispatch(KERNEL_BIT_REVERSE, groupSize, groupSize, 1);

        int logSize = (int)Mathf.Log(size, 2);
        bool pingPong = true;
        
        fftShader.SetTexture(KERNEL_STEP, P_BUFFER0, input);
        fftShader.SetTexture(KERNEL_STEP, P_BUFFER1, buffer);
        fftShader.SetInt(P_OPERATION, 1);
        for (int i = 0; i < logSize; i++)
        {
            pingPong = !pingPong;
            fftShader.SetInt(P_DEPTH, i);
            fftShader.SetBool(P_PING_PONG, pingPong);
            fftShader.Dispatch(KERNEL_STEP, fftGroupSize, fftGroupSize, 1);
        }

        if (pingPong) Graphics.Blit(buffer, input);

        fftShader.SetInt(P_SIZE, size);
        fftShader.SetTexture(KERNEL_SHIFT, P_BUFFER0, input);
        fftShader.Dispatch(KERNEL_SHIFT, groupSize, groupSize, 1);
    }

    public void FFT2D(RenderTexture input, CommandBuffer cmd)
    {
        int fftGroupSize = Mathf.Max(1, size >> 4);
        int groupSize = Mathf.Max(1, size >> 3);
        cmd.SetComputeTextureParam(fftShader, KERNEL_BIT_REVERSE, P_BUFFER0, input);
        cmd.SetComputeTextureParam(fftShader, KERNEL_BIT_REVERSE, P_BUFFER1, buffer);
        cmd.DispatchCompute(fftShader, KERNEL_BIT_REVERSE, groupSize, groupSize, 1);

        int logSize = (int)Mathf.Log(size, 2);
        bool pingPong = true;
        
        cmd.SetComputeTextureParam(fftShader, KERNEL_STEP, P_BUFFER0, input);
        cmd.SetComputeTextureParam(fftShader, KERNEL_STEP, P_BUFFER1, buffer);
        cmd.SetComputeIntParam(fftShader, P_OPERATION, 1);
        for (int i = 0; i < logSize; i++)
        {
            pingPong = !pingPong;
            cmd.SetComputeIntParam(fftShader, P_DEPTH, i);
            cmd.SetComputeIntParam(fftShader, P_PING_PONG, pingPong ? 1 : 0);
            cmd.DispatchCompute(fftShader, KERNEL_STEP, fftGroupSize, fftGroupSize, 1);
        }

        if (pingPong)
        {
            cmd.Blit(buffer, input);
        }

        cmd.SetComputeIntParam(fftShader, P_SIZE, size);
        cmd.SetComputeTextureParam(fftShader, KERNEL_SHIFT, P_BUFFER0, input);
        cmd.DispatchCompute(fftShader, KERNEL_SHIFT, groupSize, groupSize, 1);
    }
    
    public void IFFT2D(RenderTexture input, bool noScale = false)
    {
        int fftGroupSize = Mathf.Max(1, size >> 4);
        int groupSize = Mathf.Max(1, size >> 3);
        fftShader.SetTexture(KERNEL_BIT_REVERSE, P_BUFFER0, input);
        fftShader.SetTexture(KERNEL_BIT_REVERSE, P_BUFFER1, buffer);
        fftShader.Dispatch(KERNEL_BIT_REVERSE, groupSize, groupSize, 1);

        int logSize = (int)Mathf.Log(size, 2);
        bool pingPong = true;
        
        fftShader.SetTexture(KERNEL_STEP, P_BUFFER0, input);
        fftShader.SetTexture(KERNEL_STEP, P_BUFFER1, buffer);
        fftShader.SetInt(P_OPERATION, -1);
        for (int i = 0; i < logSize; i++)
        {
            pingPong = !pingPong;
            fftShader.SetInt(P_DEPTH, i);
            fftShader.SetBool(P_PING_PONG, pingPong);
            fftShader.Dispatch(KERNEL_STEP, fftGroupSize, fftGroupSize, 1);
        }

        if (pingPong) Graphics.Blit(buffer, input);

        fftShader.SetInt(P_SIZE, size);
        fftShader.SetTexture(KERNEL_SHIFT, P_BUFFER0, input);
        fftShader.Dispatch(KERNEL_SHIFT, groupSize, groupSize, 1);
        
        if (noScale) return;
        fftShader.SetTexture(KERNEL_SCALE, P_BUFFER0, input);
        fftShader.Dispatch(KERNEL_SCALE, groupSize, groupSize, 1);
    }

    public void IFFT2D(RenderTexture input, CommandBuffer cmd, bool noScale = false)
    {
        int fftGroupSize = Mathf.Max(1, size >> 4);
        int groupSize = Mathf.Max(1, size >> 3);
        cmd.SetComputeTextureParam(fftShader, KERNEL_BIT_REVERSE, P_BUFFER0, input);
        cmd.SetComputeTextureParam(fftShader, KERNEL_BIT_REVERSE, P_BUFFER1, buffer);
        cmd.DispatchCompute(fftShader, KERNEL_BIT_REVERSE, groupSize, groupSize, 1);

        int logSize = (int)Mathf.Log(size, 2);
        bool pingPong = true;
        
        cmd.SetComputeTextureParam(fftShader, KERNEL_STEP, P_BUFFER0, input);
        cmd.SetComputeTextureParam(fftShader, KERNEL_STEP, P_BUFFER1, buffer);
        cmd.SetComputeIntParam(fftShader, P_OPERATION, -1);
        for (int i = 0; i < logSize; i++)
        {
            pingPong = !pingPong;
            cmd.SetComputeIntParam(fftShader, P_DEPTH, i);
            cmd.SetComputeIntParam(fftShader, P_PING_PONG, pingPong ? 1 : 0);
            cmd.DispatchCompute(fftShader, KERNEL_STEP, fftGroupSize, fftGroupSize, 1);
        }

        if (pingPong)
        {
            cmd.Blit(buffer, input);
        }

        cmd.SetComputeIntParam(fftShader, P_SIZE, size);
        cmd.SetComputeTextureParam(fftShader, KERNEL_SHIFT, P_BUFFER0, input);
        cmd.DispatchCompute(fftShader, KERNEL_SHIFT, groupSize, groupSize, 1);
        
        if (noScale) return;
        cmd.SetComputeTextureParam(fftShader, KERNEL_SCALE, P_BUFFER0, input);
        cmd.DispatchCompute(fftShader, KERNEL_SCALE, groupSize, groupSize, 1);
    }
    
    public void Allocate(int size)
    {
        if (this.size != 0) return;
        this.size = size;
        buffer = new RenderTexture(size, size, 0,
            RenderTextureFormat.RGFloat, RenderTextureReadWrite.Linear)
        {
            useMipMap = false,
            autoGenerateMips = false,
            wrapMode = TextureWrapMode.Clamp,
            enableRandomWrite = true
        };
        buffer.Create();
        
        uint[] biteReverseIndices = new uint[size];
        for (uint i = 0; i < size; i++)
        {
            biteReverseIndices[i] = biteReverseIndices[i >> 1] >> 1 | (i & 1) * ((uint)size >> 1);
        }
        bitReverseIndices = new ComputeBuffer(size, sizeof(uint));
        bitReverseIndices.SetData(biteReverseIndices);
        fftShader.SetBuffer(KERNEL_BIT_REVERSE, P_BIT_REV_INDICES, bitReverseIndices);
    }
    
    public void Dispose()
    {
        size = 0;
        bitReverseIndices?.Release();
        buffer?.Release();
    }

    private static readonly int P_BUFFER0 = Shader.PropertyToID("Buffer0");
    private static readonly int P_BUFFER1 = Shader.PropertyToID("Buffer1");
    private static readonly int P_SIZE = Shader.PropertyToID("Size");
    private static readonly int P_DEPTH = Shader.PropertyToID("Depth");
    private static readonly int P_OPERATION = Shader.PropertyToID("Operation");
    private static readonly int P_PING_PONG = Shader.PropertyToID("PingPong");
    private static readonly int P_BIT_REV_INDICES = Shader.PropertyToID("BitRevIndices");
}
