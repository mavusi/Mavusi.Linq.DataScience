using Mavusi.Linq.DataScience.Models;

namespace Mavusi.Linq.DataScience;



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

    /// <summary>
    /// Creates a matrix from a sequence of sequences (rows).
    /// </summary>
    public static Matrix ToMatrix(this IEnumerable<IEnumerable<double>> source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));

        var rows = source.Select(row => row.ToList()).ToList();
        if (rows.Count == 0) throw new ArgumentException("Source must contain at least one row");

        var columns = rows[0].Count;
        if (columns == 0) throw new ArgumentException("Rows must contain at least one column");
        if (rows.Any(row => row.Count != columns))
            throw new ArgumentException("All rows must have the same number of columns");

        var matrix = new Matrix(rows.Count, columns);
        for (int i = 0; i < rows.Count; i++)
            for (int j = 0; j < columns; j++)
                matrix[i, j] = rows[i][j];

        return matrix;
    }

    /// <summary>
    /// Calculates the dot product of two vectors.
    /// </summary>
    public static double DotProduct(this Vector v1, Vector v2)
    {
        if (v1 == null) throw new ArgumentNullException(nameof(v1));
        if (v2 == null) throw new ArgumentNullException(nameof(v2));
        if (v1.Length != v2.Length) throw new ArgumentException("Vectors must have the same length");

        double result = 0;
        for (int i = 0; i < v1.Length; i++)
            result += v1[i] * v2[i];
        return result;
    }

    /// <summary>
    /// Calculates the dot product of two sequences.
    /// </summary>
    public static double DotProduct(this IEnumerable<double> source1, IEnumerable<double> source2)
    {
        if (source1 == null) throw new ArgumentNullException(nameof(source1));
        if (source2 == null) throw new ArgumentNullException(nameof(source2));

        return source1.ToVector().DotProduct(source2.ToVector());
    }

    /// <summary>
    /// Adds two vectors element-wise.
    /// </summary>
    public static Vector Add(this Vector v1, Vector v2)
    {
        if (v1 == null) throw new ArgumentNullException(nameof(v1));
        if (v2 == null) throw new ArgumentNullException(nameof(v2));
        if (v1.Length != v2.Length) throw new ArgumentException("Vectors must have the same length");

        var result = new double[v1.Length];
        for (int i = 0; i < v1.Length; i++)
            result[i] = v1[i] + v2[i];
        return new Vector(result);
    }

    /// <summary>
    /// Subtracts two vectors element-wise.
    /// </summary>
    public static Vector Subtract(this Vector v1, Vector v2)
    {
        if (v1 == null) throw new ArgumentNullException(nameof(v1));
        if (v2 == null) throw new ArgumentNullException(nameof(v2));
        if (v1.Length != v2.Length) throw new ArgumentException("Vectors must have the same length");

        var result = new double[v1.Length];
        for (int i = 0; i < v1.Length; i++)
            result[i] = v1[i] - v2[i];
        return new Vector(result);
    }

    /// <summary>
    /// Multiplies a vector by a scalar.
    /// </summary>
    public static Vector Multiply(this Vector vector, double scalar)
    {
        if (vector == null) throw new ArgumentNullException(nameof(vector));

        var result = new double[vector.Length];
        for (int i = 0; i < vector.Length; i++)
            result[i] = vector[i] * scalar;
        return new Vector(result);
    }

    /// <summary>
    /// Calculates the magnitude (Euclidean norm) of a vector.
    /// </summary>
    public static double Magnitude(this Vector vector)
    {
        if (vector == null) throw new ArgumentNullException(nameof(vector));

        double sumOfSquares = 0;
        for (int i = 0; i < vector.Length; i++)
            sumOfSquares += vector[i] * vector[i];
        return Math.Sqrt(sumOfSquares);
    }

    /// <summary>
    /// Normalizes a vector to unit length.
    /// </summary>
    public static Vector Normalize(this Vector vector)
    {
        if (vector == null) throw new ArgumentNullException(nameof(vector));

        var magnitude = vector.Magnitude();
        if (magnitude == 0) throw new InvalidOperationException("Cannot normalize a zero vector");

        return vector.Multiply(1.0 / magnitude);
    }

    /// <summary>
    /// Multiplies two matrices.
    /// </summary>
    public static Matrix Multiply(this Matrix m1, Matrix m2)
    {
        if (m1 == null) throw new ArgumentNullException(nameof(m1));
        if (m2 == null) throw new ArgumentNullException(nameof(m2));
        if (m1.Columns != m2.Rows)
            throw new ArgumentException("First matrix columns must equal second matrix rows");

        var result = new Matrix(m1.Rows, m2.Columns);
        for (int i = 0; i < m1.Rows; i++)
        {
            for (int j = 0; j < m2.Columns; j++)
            {
                double sum = 0;
                for (int k = 0; k < m1.Columns; k++)
                    sum += m1[i, k] * m2[k, j];
                result[i, j] = sum;
            }
        }
        return result;
    }

    /// <summary>
    /// Multiplies a matrix by a vector.
    /// </summary>
    public static Vector Multiply(this Matrix matrix, Vector vector)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));
        if (vector == null) throw new ArgumentNullException(nameof(vector));
        if (matrix.Columns != vector.Length)
            throw new ArgumentException("Matrix columns must equal vector length");

        var result = new double[matrix.Rows];
        for (int i = 0; i < matrix.Rows; i++)
        {
            double sum = 0;
            for (int j = 0; j < matrix.Columns; j++)
                sum += matrix[i, j] * vector[j];
            result[i] = sum;
        }
        return new Vector(result);
    }

    /// <summary>
    /// Transposes a matrix (swaps rows and columns).
    /// </summary>
    public static Matrix Transpose(this Matrix matrix)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));

        var result = new Matrix(matrix.Columns, matrix.Rows);
        for (int i = 0; i < matrix.Rows; i++)
            for (int j = 0; j < matrix.Columns; j++)
                result[j, i] = matrix[i, j];
        return result;
    }

    /// <summary>
    /// Creates an identity matrix of the specified size.
    /// </summary>
    public static Matrix CreateIdentityMatrix(int size)
    {
        if (size <= 0) throw new ArgumentException("Size must be greater than zero", nameof(size));

        var matrix = new Matrix(size, size);
        for (int i = 0; i < size; i++)
            matrix[i, i] = 1.0;
        return matrix;
    }

    /// <summary>
    /// Calculates the trace (sum of diagonal elements) of a square matrix.
    /// </summary>
    public static double Trace(this Matrix matrix)
    {
        if (matrix == null) throw new ArgumentNullException(nameof(matrix));
        if (matrix.Rows != matrix.Columns)
            throw new ArgumentException("Matrix must be square");

        double sum = 0;
        for (int i = 0; i < matrix.Rows; i++)
            sum += matrix[i, i];
        return sum;
    }
}
