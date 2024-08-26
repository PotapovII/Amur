//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    using System;
    using System.IO;
    /// <summary>
    /// Класс для расчета рыбных запасов
    /// </summary>
    public class RVNodeHabitat : RVNodeShallowWater
    {
        /// <summary>
        /// Value of shear stress in uniform straight flume, 
        /// to normalize tau in bends, for display only
        /// Значение напряжения сдвига в однородном прямом лотке 
        /// для нормализации тау на изгибах, только для отображения
        /// </summary>
        public static double TAUZERO = 1.000;

        protected double fishegg;
        /// <summary>
        /// Velocity magnitude
        /// Величина скорости
        /// </summary>
        protected double V;
        /// <summary>
        /// Froude Number
        /// </summary>
        protected double Fr;
        /// <summary>
        /// индекс канала
        /// channel index
        /// </summary>
        protected double ci;
        /// <summary>
        /// индекс пригодности по глубине
        /// depth suitability index
        /// </summary>
        protected double dsi;
        /// <summary>
        /// индекс пригодности субстрата
        /// velocity suitability index
        /// </summary>
        protected double vsi;
        /// <summary>
        /// комбинированный индекс пригодности
        /// substrate suitability index
        /// </summary>
        protected double ssi;
        /// <summary>
        /// combined suitability index
        /// комбинированный индекс пригодности
        /// </summary>
        protected double csi;         //	
        /// <summary>
        /// weighted useable Area in vicinity
        /// взвешенная полезная площадь в непосредственной близости
        /// </summary>
        protected double WUA;
        /// <summary>
        /// функция тока
        /// stream function
        /// </summary>
        public double Psi { get => psi; set => psi = value; }
        protected double psi;
        /// <summary>
        /// толщина льда
        /// ice thickness
        /// </summary>
        protected double tice;
        /// <summary>
        /// шероховатость льда
        /// ice roughness
        /// </summary>
        protected double kice;
        /// <summary>
        /// vorticity / завихренность
        /// </summary>
        public double Vorticity { get => vorticity; set => vorticity = value; }
        protected double vorticity;
        /// <summary>
        /// Curvature of streamlines
        /// Кривизна линий тока
        /// </summary>
        protected double curvature;
        /// <summary>
        /// physics object which governs behaviour
        /// физический объект, который управляет поведением
        /// </summary>
        protected RVHabitatPhysics theHabitat;
        public RVNodeHabitat(RVHabitatPhysics hab, int nm = 1,
                double xc = 1000.0, double yc = 1000.0, double zc = 100.0,
                double rf = 0.05, double chi = 10.0, double depth = 1.0,
                double xDis = 0.0, double yDis = 0.0) : base(nm, xc, yc, zc, rf, depth, xDis, yDis)
        {
            theHabitat = hab;
            ks = rf;
            d = depth;
            qx = xDis;
            qy = yDis;
            ci = chi;
            dsi = 0.0;
            vsi = 0.0;
            ssi = 0.0;
            csi = 0.0;
            WUA = 0.0;
            psi = 0.0;
            tice = 0.0;
            kice = 0.0;
            vorticity = 0.0;
            SetVandF();
            curvature = 0.0;
            fishegg = 0.0;
        }
        public RVNodeHabitat(RVHabitatPhysics hab, StreamReader file) : base(file)
        {
            theHabitat = hab;
            V = 0.0;
            Fr = 0.0;
            ci = ks;
            dsi = 0.0;
            vsi = 0.0;
            ssi = 0.0;
            csi = 0.0;
            WUA = 0.0;
            psi = 0.0;
            tice = 0.0;
            kice = 0.0;
            vorticity = 0.0;
            curvature = 0.0;
            fishegg = 0.0;
        }
        public void SetVandF()
        {
            if ((d - theHabitat.RelDen_Ice * tice) >= theHabitat.MinDepth)
            {
                V = Math.Sqrt(qx * qx + qy * qy) / (d - theHabitat.RelDen_Ice * tice);
                Fr = V / Math.Sqrt(G * (d - theHabitat.RelDen_Ice * tice));
            }
            else
            {
                V = 0.0;
                Fr = 0.0;
            }
        }
        public void Setfishegg(double fisheggIndex) { fishegg = fisheggIndex; }
        public void SetSI(double d, double v, double s, double c)
        {
            dsi = d;
            vsi = v;
            ssi = s;
            csi = c;
        }
        public void SetWUA(double wua)
        {
            WUA = wua;
        }
        /// <summary>
        /// Получить имя параметра
        /// </summary>
        /// <param name="i"></param>
        /// <param name="parName"></param>
        public override void GetParName(int i, ref string parName)
        {
            switch (i)
            {
                case 0:
                    parName = "Bed Elevation";
                    break;
                case 1:
                    parName = "Bed Elevation";
                    break;
                case 2:
                    parName = "Bed Roughness";
                    break;
                case 3:
                    parName = "Depth";
                    break;
                case 4:
                    parName = "qx";
                    break;
                case 5:
                    parName = "qy";
                    break;
                case 6:
                    parName = "Water Surface Elev";
                    break;
                case 7:
                    parName = "Velocity";
                    break;
                case 8:
                    parName = "Froude #";
                    break;
                case 9:
                    parName = "Channel Index";
                    break;
                case 10:
                    parName = "Depth Suitability";
                    break;
                case 11:
                    parName = "Velocity Suitability";
                    break;
                case 12:
                    parName = "Substrate Suitability";
                    break;
                case 13:
                    parName = "Combined Suitability";
                    break;
                case 14:
                    parName = "Weighted Useable Area";
                    break;
                case 15:
                    parName = "Cumulative Discharge";
                    break;
                case 16:
                    parName = "Shear Stress"; //Velocity Magnitude"); //PP 8/10/04
                    break;
                case 17:
                    parName = "Ice Thickness";
                    break;
                case 18:
                    parName = "Ice Roughness";
                    break;
                case 19:
                    parName = "Bed Shear Deviation angle (degrees)";
                    break;
            }
        }
        public override int CountPapams() { return 3; }
        public override int CountVariables() { return 3; }
        public override double GetPapam(int i)
        {
            switch (i)
            {
                case 0:
                    return z;
                case 1:
                    return z;
                case 2:
                    return ks;
                case 3:
                    return (d - (theHabitat.RelDen_Ice * tice));
                case 4:
                    return qx;
                case 5:
                    return qy;
                case 6:
                    return z + d;     // water surface elevation
                case 7:
                    if ((d - theHabitat.RelDen_Ice * tice) > theHabitat.MinDepth)
                        return V;  // Velocity
                    else
                        return 0.0;
                case 8:
                    if (d > theHabitat.MinDepth)   // changed by S. Kwan June 2010
                        return Fr;  //  Froude Number
                    else
                        return 0.0;
                case 9:
                    return ci;
                case 10:
                    return dsi;
                case 11:
                    return vsi;
                case 12:
                    return ssi;
                case 13:
                    return csi;
                case 14:
                    return WUA;
                case 15:
                    return psi;
                case 16:
                    if ((d - theHabitat.RelDen_Ice * tice) > theHabitat.MinDepth)
                    {
                        if (((d - theHabitat.RelDen_Ice * tice) / ks) > (2.718 / 12))
                            return Math.Pow(V / (2.5 * Math.Log(12.0 * (d - theHabitat.RelDen_Ice * tice) / ks)), 2) * 1000 / TAUZERO; //PP 8/10/04
                        else
                            return Math.Pow(V / ((30.0 * (d - theHabitat.RelDen_Ice * tice)) / (2.718 * ks)), 2) * 1000 / TAUZERO;//PP 8/10/04
                    }
                    else
                        return 0.0;
                case 17:
                    return tice;
                case 18:
                    return kice;
                case 19:
                    return curvature; /*return vorticity;*/ //vorticity was variable 19 before
                case 45:
                    return fishegg;
            }
            return 0.0;
        }
        public override double GetVariable(int i)
        {
            switch (i)
            {
                case 1:
                    return d;
                case 2:
                    return qx;
                case 3:
                    return qy;
            }
            return 0.0;
        }
        public override void Interpolation(int n, RVNode[] nPtrs, double[] wts)
        {
            x = 0.0;
            y = 0.0;
            z = 0.0;
            ks = 0.0;
            ci = 0.0;
            d = 0.0;
            qx = 0.0;
            qy = 0.0;
            V = 0.0;
            Fr = 0.0;
            ci = 0.0;
            dsi = 0.0;
            vsi = 0.0;
            ssi = 0.0;
            csi = 0.0;
            psi = 0.0;
            tice = 0.0;
            kice = 0.0;
            vorticity = 0.0;
            curvature = 0.0;
            //// added by PP August 1, 2004
            fishegg = 0.0;
            for (int i = 0; i < n; i++)
            {
                if (nPtrs[i] != null)
                {
                    x += wts[i] * nPtrs[i].Xo;
                    y += wts[i] * nPtrs[i].Yo;
                    z += wts[i] * nPtrs[i].Z;
                    ks += wts[i] * nPtrs[i].GetPapam(2);
                    ci += wts[i] * nPtrs[i].GetPapam(9);
                    d += wts[i] * nPtrs[i].GetPapam(3);
                    qx += wts[i] * nPtrs[i].GetPapam(4);
                    qy += wts[i] * nPtrs[i].GetPapam(5);
                    V += wts[i] * nPtrs[i].GetPapam(7);
                    Fr += wts[i] * nPtrs[i].GetPapam(8);
                    dsi += wts[i] * nPtrs[i].GetPapam(10);
                    vsi += wts[i] * nPtrs[i].GetPapam(11);
                    ssi += wts[i] * nPtrs[i].GetPapam(12);
                    csi += wts[i] * nPtrs[i].GetPapam(13);
                    psi += wts[i] * nPtrs[i].GetPapam(15);
                    tice += wts[i] * nPtrs[i].GetPapam(17);
                    kice += wts[i] * nPtrs[i].GetPapam(18);
                    curvature += wts[i] * nPtrs[i].GetPapam(19);
                    fishegg += wts[i] * nPtrs[i].GetPapam(45);
                }
            }
            d = d + theHabitat.RelDen_Ice * tice;
            if (theHabitat.InterpCI == InterpCImode.Discrete)
            {
                ssi = nPtrs[0].GetPapam(12);
                ci = nPtrs[0].GetPapam(9);
                double maxwt = wts[0];
                for (int ii = 1; ii < n; ii++)
                {
                    if (wts[ii] > maxwt)
                    {
                        ssi = nPtrs[ii].GetPapam(12);
                        ci = nPtrs[ii].GetPapam(9);
                        maxwt = wts[ii];
                    }
                }
            }
            SaveLocation();

        }
    }
}
