//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 10.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
//             обновление и чистка : 01.01.2024 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    /// <summary>
    /// Информация о параметрах задачи и алгоритма которая 
    /// не нужна алгоритму генерации сетки и переименована в задаче
    /// мелкой воды, руки не дошли до синхронизации (:
    /// </summary>
    public class RVTransient
    {
        public int nsteps;
        public double dtfac;
        public double time;
        public double dtime;
        public double theta;
        public double UW;
        public double DIF;
        public double minH;
        public double gwH;
        public double GWD;
        public double T;
        public double S;
        public double latitude;
        public int diffusivewave;
        public int plotcode;
        public int transbcs;
        public int maxitnum;
        public int smallH;
        public int JE;
        public double uwJ;
        public double epsilon1;
        public double epsilon3;
    }
}
