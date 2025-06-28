namespace AlgebraLib.Utils
{
    using MemLogLib;
    public class AlgebraUtils
    {
        public static void MultiplyMatrices(double[,] A, double[,] B,double [,] C)
        {
            
            int m = A.GetLength(0);
            int n = B.GetLength(1);
            int p = A.GetLength(1);
            MEM.Alloc2D(m, n, ref C);
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < p; k++)
                        C[i, j] += A[i, k] * B[k, j];
            
        }
        public static void MultiplyMatrices(double[][] A, double[][] B, double[][] C)
        {
            int m = A.Length;
            int n = B[0].Length;
            int p = A[0].Length;
            MEM.Alloc2D(m, n, ref C);
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                    for (int k = 0; k < p; k++)
                        C[i][j] += A[i][k] * B[k][j];
        }

    }
}
