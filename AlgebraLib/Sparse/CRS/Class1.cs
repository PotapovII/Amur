using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlgebraLib.Sparse.CRS
{
    using System;

    public class _ILU1Preconditioner
    {
        private _SparseMatrixCSR A;
        private _SparseMatrixCSC L; // Lower triangular factor from ILU(1)
        private _SparseMatrixCSC U; // Upper triangular factor from ILU(1)


        public _ILU1Preconditioner(_SparseMatrixCSR matrix)
        {
            if (matrix == null) throw new ArgumentNullException(nameof(matrix));

            A = new _SparseMatrixCSR(matrix); // Create a copy to avoid modifying the original matrix
            int n = A.N;

            L = ComputeILU1(A);
            U = GetUpperTriangular(L);
        }

        private _SparseMatrixCSC ComputeILU1(_SparseMatrixCSR crs)
        {
            int n = crs.N;
            int[] colPointers = new int[n + 1];
            int[] rowIndices = new int[crs.Values.Length];
            double[] values = new double[crs.Values.Length];

            //Initialize L to identity matrix (CSC format)
            colPointers[0] = 0;
            for (int i = 1; i <= n; ++i)
            {
                colPointers[i] = colPointers[i - 1] + 1; //Each column has one non-zero element on diagonal
            }

            //Perform ILU(1) factorization.  This is a simplified version. More robust implementations would handle pivoting and fill-in.
            for (int k = 0; k < n; ++k)
            {
                double diagValue = crs.GetElement(k, k);
                //Check for zero diagonal element - This simplistic implementation doesn't handle it. In reality, pivoting is needed.
                if (Math.Abs(diagValue) < 1e-9)  //Small value to prevent division by near zero. Replace with an actual pivot selection strategy in a robust implementation!
                {
                    throw new ArithmeticException("Zero diagonal element encountered during ILU(1).  Pivoting is required.");
                }
                //L update
                for (int j = 0; j < n; ++j)
                {
                    if (j != k) // Skip the diagonal element.
                    {
                        double aij = crs.GetElement(k, j);
                        double factor = aij / diagValue;
                        if (Math.Abs(factor) > 1e-9) //Prevent small values to avoid unnecessary entries
                        {
                            int pos = colPointers[j];

                            rowIndices[pos] = k;
                            values[pos] = factor;
                            colPointers[j]++;
                        }

                    }
                }
                //U update (not really needed in this simplified version, but keeping for consistency and potential future extension)
                for (int i = 0; i < n; ++i)
                {
                    if (i != k)
                    {
                        double aij = crs.GetElement(i, k);
                        double factor = aij / diagValue;

                        if (Math.Abs(factor) > 1e-9) //Prevent small values to avoid unnecessary entries
                        {
                            int pos = colPointers[k];

                            rowIndices[pos] = i;
                            values[pos] = factor;
                            colPointers[k]++;
                        }
                    }
                }
            }
            return new _SparseMatrixCSC(colPointers, rowIndices, values);
        }


        private _SparseMatrixCSC GetUpperTriangular(_SparseMatrixCSC L)
        {
            int n = L.N;
            int[] colPointers = new int[n + 1];
            int[] rowIndices = new int[L.Values.Length];
            double[] values = new double[L.Values.Length];

            for (int j = 0; j < n; ++j)
            {
                colPointers[j] = 0;
            }
            colPointers[n] = 0;
            // Copy diagonal elements to U. Non-diagonal elements are zeroed out, since we only want upper triangular part.
            for (int i = 0; i < n; ++i)
            {
                double val = L.Values[L.ColumnPointers[i]]; // Assuming first element is the diagonal for CSC
                if (Math.Abs(val) > 1e-9)
                {
                    int pos = colPointers[i];
                    rowIndices[pos] = i;
                    values[pos] = val;
                    colPointers[i]++;
                }
            }
            return new _SparseMatrixCSC(colPointers, rowIndices, values);
        }

        public double[] Multiply(double[] vector)
        {
            if (vector == null) throw new ArgumentNullException(nameof(vector));
            double[] result = new double[A.N];
            double[] temp1 = new double[A.N]; //Temporary for L*x
            double[] temp2 = new double[A.N]; //Temporary for U*y

            L.MultiplyRowVector(vector, ref temp1);  // L * x
            U.MultiplyRowVector(temp1, ref temp2); // U * (L * x)


            return temp2;
        }



    }

}
