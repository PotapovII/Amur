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
    using System;
    using System.IO;
    public enum WUAmode
    {
        product,
        geomean,
        minimum
    }
    public enum InterpCImode
    {
        /// <summary>
        /// непрерывный,
        /// </summary>
        Continuous,
        /// <summary>
        /// дискретный
        /// </summary>
        Discrete
    }
    /// <summary>
    /// Класс физики, который дает описание топографии русла реки. 
    /// Используемые узлы относятся к среде обитания.
    /// (Habitat)
    /// </summary>
    public class RVHabitatPhysics : RVShallowPhysics
    {
        protected RVListPoints dPref;
        protected RVListPoints vPref;
        protected RVListPoints sPref;
        /// <summary>
        /// как рассчитать csi
        /// </summary>
        protected WUAmode calcWUA;
        /// <summary>
        /// как интерполировать индекс канала
        /// </summary>
        protected InterpCImode interpCI;
        /// <summary>
        /// относительная плотность льда 
        /// </summary>
        public double RelDen_Ice=>relDen_Ice; 
        protected double relDen_Ice;

        public RVHabitatPhysics() : base()
        {
            dPref = null;
            vPref = null;
            sPref = null;
            calcWUA = WUAmode.product;
            interpCI = InterpCImode.Continuous;
            relDen_Ice = 0.92;
        }

        public void setCImode(InterpCImode newMode) 
        { 
            interpCI = newMode; 
        }
        public InterpCImode InterpCI => interpCI; 
        public override RVNode CreateNewNode(int n = 1, double x = 100.0,
            double y = 100.0, double z = 100.0)
        {
            return new RVNodeHabitat(this, n, x, y, z, 0.1);
        }
        public override RVNode ReadNode(StreamReader file)
        {
            return new RVNodeHabitat(this, file);
        }
        public override int CountVars => 3; 
        public override int CountParams => 7; 
        public override string NameType => "RVHabitatPhysics";
        #region
        public void calcSI(RVNode nP)
        {
            RVNodeHabitat hNP = (RVNodeHabitat)nP;
            double csi;
            double dsi = dPref.Interpolation(hNP.GetPapam(3));
            double vsi = vPref.Interpolation(hNP.GetPapam(7));
            double ssi = sPref.Interpolation(hNP.GetPapam(9));

            csi = dsi * vsi * ssi;
            if (calcWUA == WUAmode.geomean)
                csi = Math.Pow(csi, 0.33333333);
            if (calcWUA == WUAmode.minimum)
            {
                csi = dsi;
                csi = (vsi < csi) ? vsi : csi;
                csi = (ssi < csi) ? ssi : csi;
            }
            hNP.SetSI(dsi, vsi, ssi, csi);
            hNP.SetWUA(0.0);
        }
        public void CalcSurvival(RVNode nP)
        {
            RVNodeHabitat hNP = (RVNodeHabitat)nP;
            //double csi;
            double bedchange = hNP.GetPapam(23);
            double reddburialdepth = hNP.GetPapam(44);
            double fishegg = hNP.GetPapam(45);
            if (fishegg < 0.5)
            {
                hNP.Setfishegg(0.0);
                return;
            }
            bedchange = Math.Min(0, bedchange); // look for scour
            bedchange = Math.Abs(bedchange);
            if (bedchange > reddburialdepth) hNP.Setfishegg(0.0);
            else hNP.Setfishegg(1.0);
        }
        public void setSg_ice(double specific_grav_ice) { relDen_Ice = specific_grav_ice; }
        public void setWUAmode(WUAmode newMode) { calcWUA = newMode; }
        public WUAmode getWUAcalc() { return calcWUA; }
        #endregion
    }
}
