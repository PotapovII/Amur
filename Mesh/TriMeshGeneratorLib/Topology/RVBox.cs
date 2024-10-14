//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    /// <summary>
    /// Ограничивающий ректангл
    /// </summary>
    public struct RVBox
    {
        /// <summary>
        /// минимум х
        /// </summary>
        public double x1;
        /// <summary>
        /// минимум у
        /// </summary>
        public double y1;
        /// <summary>
        /// максимум х
        /// </summary>
        public double x2;
        /// <summary>
        /// максимум у
        /// </summary>
        public double y2;
        
        public RVBox(double x1, double y1, double x2, double y2)
        {
            this.x1 = x1; this.y1 = y1; 
            this.x2 = x2; this.y2 = y2;
        }
        /// <summary>
        /// ищем размеры области RVBox limits 
        /// </summary>
        /// <param name="nodesList"></param>
        public static RVBox CreateRVBoxLimits(RVList nodesList, double margin = 0.01)
        {
            RVBox limits = new RVBox();
            // первый узел
            RVNode node = (RVNode)nodesList.FirstItem();
            if (node != null)
            {
                limits.x1 = node.X;
                limits.y1 = node.Y;
                limits.x2 = node.X;
                limits.y2 = node.Y;
                node = (RVNode)nodesList.NextItem();
            }
            // ищем минимах бокс
            while (node != null)
            {
                if (node.X < limits.x1)
                    limits.x1 = node.X;
                if (node.X > limits.x2)
                    limits.x2 = node.X;
                if (node.Y < limits.y1)
                    limits.y1 = node.Y;
                if (node.Y > limits.y2)
                    limits.y2 = node.Y;
                node = (RVNode)nodesList.NextItem();
            }
            // ищем интервал области по х
            double dx = limits.x2 - limits.x1;
            // и по у
            double dy = limits.y2 - limits.y1;
            // расширяем коробку
            limits.x1 -= margin * dx;
            limits.x2 += margin * dx;
            limits.y1 -= margin * dy;
            limits.y2 += margin * dy;
            return limits;
        }
        public static RVBox CreateRVBoxLimits(RVMeshIrregular mesh, double margin = 0.01)
        {
            return CreateRVBoxLimits(mesh.nodesList, margin);
        }
    }
}
