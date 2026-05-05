using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mavusi.Linq.DataScience.Models
{
    /// <summary>
    /// Represents a vector for linear algebra operations.
    /// </summary>
    public class Vector
    {
        private readonly double[] _values;

        public int Length => _values.Length;

        public double this[int index]
        {
            get => _values[index];
            set => _values[index] = value;
        }

        public Vector(params double[] values)
        {
            if (values == null || values.Length == 0)
                throw new ArgumentException("Vector must contain at least one element");
            _values = (double[])values.Clone();
        }

        public Vector(IEnumerable<double> values) : this(values.ToArray()) { }

        public IEnumerable<double> AsEnumerable() => _values.AsEnumerable();

        public override string ToString() => $"[{string.Join(", ", _values)}]";
    }

    /// <summary>
    /// Represents a matrix for linear algebra operations.
    /// </summary>
    public class Matrix
    {
        private readonly double[,] _values;

        public int Rows { get; }
        public int Columns { get; }

        public double this[int row, int col]
        {
            get => _values[row, col];
            set => _values[row, col] = value;
        }

        public Matrix(int rows, int columns)
        {
            if (rows <= 0) throw new ArgumentException("Rows must be greater than zero", nameof(rows));
            if (columns <= 0) throw new ArgumentException("Columns must be greater than zero", nameof(columns));

            Rows = rows;
            Columns = columns;
            _values = new double[rows, columns];
        }

        public Matrix(double[,] values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            Rows = values.GetLength(0);
            Columns = values.GetLength(1);
            _values = (double[,])values.Clone();
        }

        public Vector GetRow(int row)
        {
            if (row < 0 || row >= Rows) throw new ArgumentOutOfRangeException(nameof(row));

            var values = new double[Columns];
            for (int i = 0; i < Columns; i++)
                values[i] = _values[row, i];
            return new Vector(values);
        }

        public Vector GetColumn(int col)
        {
            if (col < 0 || col >= Columns) throw new ArgumentOutOfRangeException(nameof(col));

            var values = new double[Rows];
            for (int i = 0; i < Rows; i++)
                values[i] = _values[i, col];
            return new Vector(values);
        }

        public override string ToString()
        {
            var rows = new List<string>();
            for (int i = 0; i < Rows; i++)
            {
                var cols = new List<double>();
                for (int j = 0; j < Columns; j++)
                    cols.Add(_values[i, j]);
                rows.Add($"[{string.Join(", ", cols)}]");
            }
            return string.Join(Environment.NewLine, rows);
        }
    }
}
