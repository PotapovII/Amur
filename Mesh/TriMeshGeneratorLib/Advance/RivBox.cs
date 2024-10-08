//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib.Advance
{
    using System.Collections.Generic;
    /// <summary>
    /// Ограничитель
    /// </summary>
    public struct RivBox
    {
        public double x1;
        public double y1;
        public double x2;
        public double y2;
        
        public RivBox(double x1, double y1, double x2, double y2)
        {
            this.x1 = x1; this.y1 = y1; 
            this.x2 = x2; this.y2 = y2;
        }
        /// <summary>
        /// ищем размеры области RivBox limits 
        /// </summary>
        /// <param name="nodesList"></param>
        public static RivBox CreateRVBoxLimits(List<RivNode> nodesList, double margin = 0.01)
        {
            RivBox limits = new RivBox();
            // первый узел
            //RivNode node = nodesList[0];// (RivNode)nodesList.FirstItem();
            foreach (RivNode node in nodesList)
                if (node != null)
                {
                    limits.x1 = node.X;
                    limits.y1 = node.Y;
                    limits.x2 = node.X;
                    limits.y2 = node.Y;
                    break;
                }
            // ищем минимах бокс
            foreach (RivNode node in nodesList)
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
        public static RivBox CreateRVBoxLimits(RivMeshIrregular mesh, double margin = 0.01)
        {
            return CreateRVBoxLimits(mesh.nodesList, margin);
        }
    }
}
