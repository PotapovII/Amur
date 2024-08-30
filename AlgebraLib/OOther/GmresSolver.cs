using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlgebraLib.CG
{
    internal struct Rotation
    {
        public static Rotation CreateRotationToMakeYZero(double x, double y)
        {
            if (Math.Abs(x) < double.Epsilon)
                return new Rotation(0.0, 1.0);

            var r = Math.Sqrt(x * x + y * y);
            var cos = Math.Abs(x) / r;
            var sin = cos * y / x;
            return new Rotation(cos, sin);
        }

        public Rotation(double cos, double sin)
        {
            Cos = cos;
            Sin = sin;
        }

        public double Cos { get; }
        public double Sin { get; }
    }
    public class GmresResult
    {
        public GmresResult(double[] X, bool isConverged, int outerIterations, int innerIterations, double[] errors)
        {
            this.X = X;
            this.IsConverged = isConverged;
            this.OuterIterations = outerIterations;
            this.InnerIterations = innerIterations;
            this.Errors = errors;
        }
        public double[] X;
        public bool IsConverged;
        public int OuterIterations;
        public int InnerIterations;
        public double[] Errors;
    }
    public static class GmresSolver
    {
        public const double DefaultEpsilon = 1e-6;

        public static GmresResult Solve(double[][] A, double[] b, int? maxInnerIterations = null, int? maxOuterIterations = null, double epsilon = DefaultEpsilon, double[] x0 = null, int? degreeOfParallelism = null)
        {
            Validate(A, b, ref maxInnerIterations, ref maxOuterIterations, ref degreeOfParallelism);
            //x0 = new double[b.Length];

            // Control.MaxDegreeOfParallelism = degreeOfParallelism.Value;
            //x0 = new double[b.Length];
            double[] x = new double[b.Length];
            var errors = new List<double>();
            for (var i = 0; i < maxOuterIterations; i++)
            {
                var iterationResult = MakeInnerIterations(A, b, x, maxInnerIterations.Value, epsilon, degreeOfParallelism.Value);
                x = iterationResult.x;
                errors.AddRange(iterationResult.Errors);

                if (iterationResult.IsConverged)
                    return new GmresResult(x, true, i, iterationResult.InnerIterations, errors.ToArray());
            }
            return new GmresResult(x, false, maxOuterIterations.Value, maxInnerIterations.Value, errors.ToArray());
        }

        private static void Validate(double[][] A, double[] b, ref int? maxInnerIterations, ref int? maxOuterIterations, ref int? degreeOfParallelism)
        {
            if (A.Length != A[0].Length)
                throw new ArgumentException("Coeffitient matrix must be square");
            if (A.Length != b.Length)
                throw new ArgumentException("Coeffitient matrix and vector must have same dimentions");

            maxInnerIterations = maxInnerIterations ?? Math.Min(b.Length, 10);
            if (maxInnerIterations < 1)
                maxInnerIterations = 1;
            if (maxInnerIterations > A.Length)
                maxInnerIterations = A.Length;

            maxOuterIterations = maxOuterIterations ?? Math.Min(b.Length / maxInnerIterations.Value, 10);
            if (maxInnerIterations < 1)
                maxOuterIterations = 1;

            //degreeOfParallelism = degreeOfParallelism ?? Control.MaxDegreeOfParallelism;
        }

        private static (double[] x, bool IsConverged, int InnerIterations, List<double> Errors)
            MakeInnerIterations(double[][] A, double[] b, double[] x0, int maxInnerIterations, double epsilon, int degreeOfParallelism)
        {
            var n = A.Length;
            var m = maxInnerIterations;
            double[] r0 = new double[b.Length];
            for (int i = 0; i < b.Length; i++)
                for (int j = 0; j < b.Length; j++)
                    r0[i] = b[i] - A[i][j] * x0[j];
            //var r0 = b - A * x0;
            // (sum(abs(this[i])^p))^(1/p)
            //var bNorm = b.Norm(2);
            //var error = r0.Norm(2) / bNorm;
            var bNorm = Norm(b);
            var error = Norm(r0) / bNorm;
            var errors = new List<double> { error };

            Rotation[] rotations = new Rotation[m];

            if (error <= epsilon)
                return (x0, true, 0, errors);

            var r0Norm = Norm(r0);
            //var Q = new DenseMatrix(n, m + 1);
            double[][] Q = new double[n][];
            for (int j = 0; j < n; j++)
                Q[j] = new double[m + 1];
            //Q.SetColumn(0, r0 / r0Norm);
            for (int i = 0; i < b.Length; i++)
                Q[0][i] = r0[i] / r0Norm;

            //var beta = new DenseVector(n + 1) { [0] = r0Norm }; // Use n+1 instead of n - to hold beta[k+1]
            double[] beta = new double[n + 1];
            beta[0] = r0Norm;

            //var H = new DenseMatrix(m + 1, m);
            double[][] H = new double[m + 1][];
            for (int j = 0; j < m; j++)
                H[j] = new double[m];

            for (var k = 0; k < m; k++)
            {
                var (hColumn, q) = MakeArnoldiIteration(A, Q, k + 1, degreeOfParallelism);

                //Q.SetColumn(k + 1, 0, q.Count, q);
                for (int i = 0; i < q.Length; i++)
                    Q[k + 1][i] = q[i];

                var (rotatedHColumn, rotation) = ApplyGivenRotationsToHColumn(hColumn, rotations, k);
                //H.SetColumn(k, 0, rotatedHColumn.Count, rotatedHColumn);
                for (int i = 0; i < rotatedHColumn.Length; i++)
                    H[k][i] = rotatedHColumn[i];

                rotations[k] = rotation;

                beta[k + 1] = -rotation.Sin * beta[k];
                beta[k] = rotation.Cos * beta[k];
                error = Math.Abs(beta[k + 1]) / bNorm;
                errors.Add(error);

                if (error <= epsilon)
                {
                    return (GetSolution(H, Q, x0, beta, k + 1), true, k + 1, errors);
                }
            }

            return (GetSolution(H, Q, x0, beta, m), false, m, errors);
        }

        public static double Norm(double[] a)
        {
            double norm = 0;
            for (int i = 0; i < a.Length; i++)
                norm += a[i];
            return Math.Sqrt(norm);
        }

        private static double[] GetSolution(double[][] H, double[][] Q, double[] x0, double[] beta, int k)
        {
            double[] x = null;
            //var y = H.SubMatrix(0, k, 0, k).Solve(beta.SubVector(0, k));

            //var x = x0 + Q.SubMatrix(0, Q.RowCount, 0, k) * y;

            return x;
        }

        private static (double[] h, double[] q) MakeArnoldiIteration(double[][] A, double[][] Q, int k, int degreeOfParallelism)
        {
            double[] h = new double[k + 1];
            //var q = A * Q.Column(k - 1);
            double[] q = new double[Q.Length];
            for (int i = 0; i < Q.Length; i++)
                for (int j = 0; j < Q.Length; j++)
                    q[i] = A[i][j] * Q[j][k - 1];

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = degreeOfParallelism };
            Parallel.For(0, k, parallelOptions, i =>
            {
                // var qi = Q.Column(i);
                // double[] qi = new double[Q.Length];
                for (int j = 0; j < Q.Length; j++)
                    h[i] += q[j] * Q[j][i];
                //h[i] = q * qi;
                //q.Subtract(h[i] * qi, q);
                for (int j = 0; j < Q.Length; j++)
                    q[j] = q[j] - h[i] * Q[j][i];
            });

            // var qNorm = q.Norm(2);
            var qNorm = Norm(q);
            h[k] = qNorm;
            //q = q / qNorm;
            for (int i = 0; i < Q.Length; i++)
                q[i] /= qNorm;
            return (h, q);
        }

        private static (double[] h, Rotation rotation) ApplyGivenRotationsToHColumn(double[] h, IReadOnlyList<Rotation> rotations, int k)
        {
            for (var i = 0; i < k; i++)
            {
                var temp = rotations[i].Cos * h[i] + rotations[i].Sin * h[i + 1];
                h[i + 1] = -rotations[i].Sin * h[i] + rotations[i].Cos * h[i + 1];
                h[i] = temp;
            }

            var newRotation = Rotation.CreateRotationToMakeYZero(h[k], h[k + 1]);

            h[k] = newRotation.Cos * h[k] + newRotation.Sin * h[k + 1];
            h[k + 1] = 0.0;

            return (h, newRotation);
        }
    }
}
