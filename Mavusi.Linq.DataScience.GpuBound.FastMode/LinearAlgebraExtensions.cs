using ILGPU;
using ILGPU.Runtime;
using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience.GpuBound;

/// <summary>
/// GPU-accelerated linear algebra operations using ILGPU.
/// </summary>
public static class LinearAlgebraExtensions
{
    /// <summary>
    /// Creates a vector from a sequence of values.
    /// </summary>
    public static Vector ToVector(this IEnumerable<double> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        return new Vector(source);
    }
    private static (Context Context, Accelerator Accelerator) GpuContext => 
        GpuContextBase.GpuContext;

    /// <summary>
    /// Calculates the dot product of two vectors using GPU acceleration.
    /// </summary>
    public static float DotProductGpu(this Vector v1, Vector v2)
    {
        if (v1 == null) throw new ArgumentNullException(nameof(v1));
        if (v2 == null) throw new ArgumentNullException(nameof(v2));
        if (v1.Length != v2.Length) throw new ArgumentException("Vectors must have the same length");

        var (context, accelerator) = GpuContext;

        var array1 = v1.AsEnumerable().Select(d => (float)d).ToArray();
        var array2 = v2.AsEnumerable().Select(d => (float)d).ToArray();

        using var deviceV1 = accelerator.Allocate1D(array1);
        using var deviceV2 = accelerator.Allocate1D(array2);
        using var deviceResult = accelerator.Allocate1D<float>(1);

        deviceResult.MemSetToZero();

        var dotKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(DotProductKernel);

        dotKernel(v1.Length, deviceV1.View, deviceV2.View, deviceResult.View);
        accelerator.Synchronize();

        return deviceResult.GetAsArray1D()[0];
    }

    /// <summary>
    /// Calculates the dot product of two sequences using GPU acceleration.
    /// </summary>
    public static float DotProductGpu(this IEnumerable<double> source1, IEnumerable<double> source2)
    {
        if (source1 == null) throw new ArgumentNullException(nameof(source1));
        if (source2 == null) throw new ArgumentNullException(nameof(source2));

        return source1.ToVector().DotProductGpu(source2.ToVector());
    }

    /// <summary>
    /// Adds two vectors element-wise using GPU acceleration.
    /// </summary>
    public static Vector AddGpu(this Vector v1, Vector v2)
    {
        if (v1 == null) throw new ArgumentNullException(nameof(v1));
        if (v2 == null) throw new ArgumentNullException(nameof(v2));
        if (v1.Length != v2.Length) throw new ArgumentException("Vectors must have the same length");

        var (context, accelerator) = GpuContext;

        var array1 = v1.AsEnumerable().Select(d => (float)d).ToArray();
        var array2 = v2.AsEnumerable().Select(d => (float)d).ToArray();
        var result = new double[v1.Length];

        using var deviceV1 = accelerator.Allocate1D(array1);
        using var deviceV2 = accelerator.Allocate1D(array2);
        using var deviceResult = accelerator.Allocate1D<float>(v1.Length);

        var addKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(AddKernel);

        addKernel(v1.Length, deviceV1.View, deviceV2.View, deviceResult.View);
        accelerator.Synchronize();

        var floatResult = new float[v1.Length];
        deviceResult.CopyToCPU(floatResult);
        for (int i = 0; i < v1.Length; i++)
            result[i] = floatResult[i];
        return new Vector(result);
    }

    /// <summary>
    /// Subtracts two vectors element-wise using GPU acceleration.
    /// </summary>
    public static Vector SubtractGpu(this Vector v1, Vector v2)
    {
        if (v1 == null) throw new ArgumentNullException(nameof(v1));
        if (v2 == null) throw new ArgumentNullException(nameof(v2));
        if (v1.Length != v2.Length) throw new ArgumentException("Vectors must have the same length");

        var (context, accelerator) = GpuContext;

        var array1 = v1.AsEnumerable().Select(d => (float)d).ToArray();
        var array2 = v2.AsEnumerable().Select(d => (float)d).ToArray();
        var result = new double[v1.Length];

        using var deviceV1 = accelerator.Allocate1D(array1);
        using var deviceV2 = accelerator.Allocate1D(array2);
        using var deviceResult = accelerator.Allocate1D<float>(v1.Length);

        var subtractKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, ArrayView<float>>(SubtractKernel);

        subtractKernel(v1.Length, deviceV1.View, deviceV2.View, deviceResult.View);
        accelerator.Synchronize();

        var floatResult = new float[v1.Length];
        deviceResult.CopyToCPU(floatResult);
        for (int i = 0; i < v1.Length; i++)
            result[i] = floatResult[i];
        return new Vector(result);
    }

    /// <summary>
    /// Multiplies a vector by a scalar using GPU acceleration.
    /// </summary>
    public static Vector MultiplyGpu(this Vector vector, float scalar)
    {
        if (vector == null) throw new ArgumentNullException(nameof(vector));

        var (context, accelerator) = GpuContext;

        var array = vector.AsEnumerable().Select(d => (float)d).ToArray();
        var result = new double[vector.Length];

        using var deviceVector = accelerator.Allocate1D(array);
        using var deviceResult = accelerator.Allocate1D<float>(vector.Length);

        var scalarMultKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, float, ArrayView<float>>(ScalarMultiplyKernel);

        scalarMultKernel(vector.Length, deviceVector.View, scalar, deviceResult.View);
        accelerator.Synchronize();

        var floatResult = new float[vector.Length];
        deviceResult.CopyToCPU(floatResult);
        for (int i = 0; i < vector.Length; i++)
            result[i] = floatResult[i];
        return new Vector(result);
    }

    /// <summary>
    /// Calculates the magnitude (Euclidean norm) of a vector using GPU acceleration.
    /// </summary>
    public static float MagnitudeGpu(this Vector vector)
    {
        if (vector == null) throw new ArgumentNullException(nameof(vector));

        var (context, accelerator) = GpuContext;

        var array = vector.AsEnumerable().Select(d => (float)d).ToArray();

        using var deviceVector = accelerator.Allocate1D(array);
        using var deviceResult = accelerator.Allocate1D<float>(1);

        deviceResult.MemSetToZero();

        var magnitudeKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>>(MagnitudeKernel);

        magnitudeKernel(vector.Length, deviceVector.View, deviceResult.View);
        accelerator.Synchronize();

        var sumOfSquares = deviceResult.GetAsArray1D()[0];
        return (float)Math.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Normalizes a vector to unit length using GPU acceleration.
    /// </summary>
    public static Vector NormalizeGpu(this Vector vector)
    {
        if (vector == null) throw new ArgumentNullException(nameof(vector));

        var magnitude = vector.MagnitudeGpu();
        if (magnitude == 0) throw new InvalidOperationException("Cannot normalize a zero vector");

        return vector.MultiplyGpu(1.0f / magnitude);
    }

    /// <summary>
    /// Multiplies two matrices using GPU acceleration.
    /// </summary>
    public static Matrix MultiplyGpu(this Matrix m1, Matrix m2)
    {
        if (m1 == null) throw new ArgumentNullException(nameof(m1));
        if (m2 == null) throw new ArgumentNullException(nameof(m2));
        if (m1.Columns != m2.Rows)
            throw new ArgumentException("First matrix columns must equal second matrix rows");

        var (context, accelerator) = GpuContext;

        // Flatten matrices to 1D arrays for GPU processing and convert to float
        var m1Flat = new float[m1.Rows * m1.Columns];
        var m2Flat = new float[m2.Rows * m2.Columns];

        for (int i = 0; i < m1.Rows; i++)
            for (int j = 0; j < m1.Columns; j++)
                m1Flat[i * m1.Columns + j] = (float)m1[i, j];

        for (int i = 0; i < m2.Rows; i++)
            for (int j = 0; j < m2.Columns; j++)
                m2Flat[i * m2.Columns + j] = (float)m2[i, j];

        var resultFlat = new float[m1.Rows * m2.Columns];

        using var deviceM1 = accelerator.Allocate1D(m1Flat);
        using var deviceM2 = accelerator.Allocate1D(m2Flat);
        using var deviceResult = accelerator.Allocate1D<float>(m1.Rows * m2.Columns);

        deviceResult.MemSetToZero();

        var matrixMultKernel = accelerator.LoadAutoGroupedStreamKernel<
            Index1D, ArrayView<float>, ArrayView<float>, int, int, int, ArrayView<float>>(
            MatrixMultiplyKernel);

        matrixMultKernel(m1.Rows * m2.Columns, deviceM1.View, deviceM2.View, 
            m1.Rows, m1.Columns, m2.Columns, deviceResult.View);
        accelerator.Synchronize();

        deviceResult.CopyToCPU(resultFlat);

        // Convert flat array back to matrix
        var result = new Matrix(m1.Rows, m2.Columns);
        for (int i = 0; i < m1.Rows; i++)
            for (int j = 0; j < m2.Columns; j++)
                result[i, j] = resultFlat[i * m2.Columns + j];

        return result;
    }

    // GPU Kernels

    private static void DotProductKernel(
        Index1D index,
        ArrayView<float> v1,
        ArrayView<float> v2,
        ArrayView<float> result)
    {
        var sum = 0.0f;
        for (int i = index; i < v1.Length; i += Grid.DimX * Group.DimX)
        {
            sum += v1[i] * v2[i];
        }
        Atomic.Add(ref result[0], sum);
    }

    private static void AddKernel(
        Index1D index,
        ArrayView<float> v1,
        ArrayView<float> v2,
        ArrayView<float> result)
    {
        if (index < v1.Length)
        {
            result[index] = v1[index] + v2[index];
        }
    }

    private static void SubtractKernel(
        Index1D index,
        ArrayView<float> v1,
        ArrayView<float> v2,
        ArrayView<float> result)
    {
        if (index < v1.Length)
        {
            result[index] = v1[index] - v2[index];
        }
    }

    private static void ScalarMultiplyKernel(
        Index1D index,
        ArrayView<float> vector,
        float scalar,
        ArrayView<float> result)
    {
        if (index < vector.Length)
        {
            result[index] = vector[index] * scalar;
        }
    }

    private static void MagnitudeKernel(
        Index1D index,
        ArrayView<float> vector,
        ArrayView<float> result)
    {
        var sumSquares = 0.0f;
        for (int i = index; i < vector.Length; i += Grid.DimX * Group.DimX)
        {
            sumSquares += vector[i] * vector[i];
        }
        Atomic.Add(ref result[0], sumSquares);
    }

    private static void MatrixMultiplyKernel(
        Index1D index,
        ArrayView<float> m1,
        ArrayView<float> m2,
        int m1Rows,
        int m1Cols,
        int m2Cols,
        ArrayView<float> result)
    {
        if (index < m1Rows * m2Cols)
        {
            int row = (int)index / m2Cols;
            int col = (int)index % m2Cols;

            float sum = 0;
            for (int k = 0; k < m1Cols; k++)
            {
                sum += m1[row * m1Cols + k] * m2[k * m2Cols + col];
            }
            result[index] = sum;
        }
    }
}
