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

        var rows = new List<List<double>>();
        var columns = -1;

        foreach (var row in source)
        {
            var rowValues = row.ToList();

            if (columns == -1)
            {
                columns = rowValues.Count;
                if (columns == 0) throw new ArgumentException("Rows must contain at least one column");
            }
            else if (rowValues.Count != columns)
            {
                throw new ArgumentException("All rows must have the same number of columns");
            }

            rows.Add(rowValues);
        }

        if (rows.Count == 0) throw new ArgumentException("Source must contain at least one row");

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

        var sum = 0.0;
        var compensation = 0.0;

        for (int i = 0; i < v1.Length; i++)
        {
            var product = v1[i] * v2[i];
            var corrected = product - compensation;
            var next = sum + corrected;
            compensation = (next - sum) - corrected;
            sum = next;
        }

        return sum;
    }

    /// <summary>
    /// Calculates the dot product of two sequences.
    /// </summary>
    public static double DotProduct(this IEnumerable<double> source1, IEnumerable<double> source2)
    {
        if (source1 == null) throw new ArgumentNullException(nameof(source1));
        if (source2 == null) throw new ArgumentNullException(nameof(source2));

        using var e1 = source1.GetEnumerator();
        using var e2 = source2.GetEnumerator();

        var has1 = e1.MoveNext();
        var has2 = e2.MoveNext();

        if (!has1 || !has2)
        {
            if (has1 != has2) throw new ArgumentException("Vectors must have the same length");
            throw new ArgumentException("Vector must contain at least one element");
        }

        var sum = 0.0;
        var compensation = 0.0;

        while (true)
        {
            var product = e1.Current * e2.Current;
            var corrected = product - compensation;
            var next = sum + corrected;
            compensation = (next - sum) - corrected;
            sum = next;

            has1 = e1.MoveNext();
            has2 = e2.MoveNext();

            if (has1 != has2) throw new ArgumentException("Vectors must have the same length");
            if (!has1) break;
        }

        return sum;
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

        var sumOfSquares = 0.0;
        var compensation = 0.0;

        for (int i = 0; i < vector.Length; i++)
        {
            var value = vector[i];
            var term = value * value;
            var corrected = term - compensation;
            var next = sumOfSquares + corrected;
            compensation = (next - sumOfSquares) - corrected;
            sumOfSquares = next;
        }

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

        var rows = m1.Rows;
        var inner = m1.Columns;
        var cols = m2.Columns;

        var m2T = m2.Transpose();
        var result = new Matrix(rows, cols);

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                var sum = 0.0;
                var compensation = 0.0;

                for (int k = 0; k < inner; k++)
                {
                    var term = m1[i, k] * m2T[j, k];
                    var corrected = term - compensation;
                    var next = sum + corrected;
                    compensation = (next - sum) - corrected;
                    sum = next;
                }

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

        var rows = matrix.Rows;
        var cols = matrix.Columns;
        var result = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            var sum = 0.0;
            var compensation = 0.0;

            for (int j = 0; j < cols; j++)
            {
                var term = matrix[i, j] * vector[j];
                var corrected = term - compensation;
                var next = sum + corrected;
                compensation = (next - sum) - corrected;
                sum = next;
            }

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
