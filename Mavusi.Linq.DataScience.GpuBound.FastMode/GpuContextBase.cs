using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using ILGPU.Runtime.CPU;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// Base class for managing GPU context and accelerator initialization for ILGPU operations.
/// </summary>
internal static class GpuContextBase
{
    private static readonly Lazy<(Context, Accelerator)> _gpuContext = new(() => InitializeGpu());

    private static readonly Lazy<(Context, Accelerator)> _fastGpuContext = new(() => InitializeFastGpu());

    private static (Context, Accelerator) InitializeGpu()
    {
        var context = Context.Create(x=> x.Math(MathMode.Fast32BitOnly));
        //var context = Context.CreateDefault();

        // Prefer CUDA, then OpenCL, then CPU (which always supports doubles)
        var device = context.Devices.FirstOrDefault(d => 
                         d.AcceleratorType == AcceleratorType.Cuda)

                     ?? context.Devices.FirstOrDefault(d => 
                         d.AcceleratorType == AcceleratorType.OpenCL)

                     ?? context.Devices.FirstOrDefault(d => 
                         d.AcceleratorType == AcceleratorType.CPU);
        if (device == null)
        {
            throw new NotSupportedException("No compatible accelerator found.");
        }

        //check if device supports double precision

        var accelerator = device.CreateAccelerator(context);
        
        
        return (context, accelerator);
    }

    private static (Context, Accelerator) InitializeFastGpu()
    {
        var context = Context.Create(x => x.Math(MathMode.Fast));
        //var context = Context.CreateDefault();

        // Prefer CUDA, then OpenCL, then CPU (which always supports doubles)
        var device = context.Devices.FirstOrDefault(d =>
                         d.AcceleratorType == AcceleratorType.Cuda)

                     ?? context.Devices.FirstOrDefault(d =>
                         d.AcceleratorType == AcceleratorType.OpenCL)

                     ?? context.Devices.FirstOrDefault(d =>
                         d.AcceleratorType == AcceleratorType.CPU);
        if (device == null)
        {
            throw new NotSupportedException("No compatible accelerator found.");
        }

        //check if device supports double precision

        var accelerator = device.CreateAccelerator(context);


        return (context, accelerator);
    }

    /// <summary>
    /// Gets the GPU context and accelerator. Initializes on first access.
    /// </summary>
    internal static (Context Context, Accelerator Accelerator) GpuContext => _gpuContext.Value;
    internal static (Context Context, Accelerator Accelerator) FastGpuContext => _fastGpuContext.Value;

    /// <summary>
    /// Disposes the GPU context and releases resources.
    /// Call this when you're done using GPU-accelerated operations.
    /// </summary>
    public static void DisposeGpuContext()
    {
        if (_gpuContext.IsValueCreated)
        {
            var (context, accelerator) = _gpuContext.Value;
            accelerator.Dispose();
            context.Dispose();
        }
    }
}
