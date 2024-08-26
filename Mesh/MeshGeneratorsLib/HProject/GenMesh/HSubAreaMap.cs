namespace MeshGeneratorsLib
{
    using CommonLib;
    using System;
    /// <summary>
    /// Определяет контур области
    /// </summary>
    [Serializable]
    public class HSubAreaMap
    {
        public int ID = 0;
        /// <summary>
        ///  данные для генерации КЭ сетки
        /// </summary>
        public HMeshParams meshData;
        /// <summary>
        /// Данные для контура
        /// </summary>
        public HMapSegment[] Contur;
        /// <summary>
        /// Длина сегментов контура
        /// </summary>
        public double[] ConturLength;
        /// <summary>
        /// Количество узлов на сегменте контура
        /// </summary>
        public uint[] ContsKnots;
        /// <summary>
        /// средний диаметр КЭ
        /// </summary>
        public double diametrFE;
        /// <summary>
        /// границы области
        /// </summary>
        public double xmin, xmax, ymin, ymax;

        public HSubAreaMap(HMeshParams meshData, HMapSegment[] Contur)
        {
            this.meshData = meshData;
            this.Contur = Contur;
            ConturLength = new double[Contur.Length];
            ContsKnots = new uint[Contur.Length];
            GetMidleDiametrFE();
        }
        /// <summary>
        /// вычисление характеристик области
        /// </summary>
        void GetMidleDiametrFE()
        {
            xmin = Contur[0].Knots[0].x;
            xmax = Contur[0].Knots[0].x;
            ymin = Contur[0].Knots[0].y;
            ymax = Contur[0].Knots[0].y;
            for (int i = 0; i < Contur.Length; i++)
            {
                for (int j = 0; j < Contur[i].Knots.Length; j++)
                {
                    if (xmin > Contur[i].Knots[j].x)
                        xmin = Contur[i].Knots[j].x;
                    if (xmax < Contur[i].Knots[j].x)
                        xmax = Contur[i].Knots[j].x;
                    if (ymin > Contur[i].Knots[j].y)
                        ymin = Contur[i].Knots[j].y;
                    if (ymax < Contur[i].Knots[j].y)
                        ymax = Contur[i].Knots[j].y;
                }
            }
            double L = Math.Max(xmax - xmin, ymax - ymin);
            diametrFE = L * meshData.diametrFE.x / 100;
            for (int i = 0; i < Contur.Length; i++)
            {
                ConturLength[i] = Contur[i].Length();
                uint ke = (uint)(ConturLength[i] / (diametrFE * (int)meshData.meshRange));
                ke = ke == 0 ? 1 : ke;
                ContsKnots[i] = ke * (uint)meshData.meshRange + 1;
            }
            if (meshData.meshType == TypeMesh.Rectangle)
            {
                uint max = Math.Max(ContsKnots[0], ContsKnots[2]);
                ContsKnots[0] = max;
                ContsKnots[2] = max;

                max = Math.Max(ContsKnots[1], ContsKnots[3]);
                ContsKnots[1] = max;
                ContsKnots[3] = max;

            }
        }
    }
}
