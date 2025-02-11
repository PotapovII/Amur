#region License
/*
Copyright (c) 2019 - 2024  Potapov Igor Ivanovich, Khabarovsk

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/
#endregion
//---------------------------------------------------------------------------
//                  - (C) Copyright 2025
//                        Потапов И.И.
//                          03.02.25
//---------------------------------------------------------------------------
namespace EddyViscosityLib
{
    using System;
    using CommonLib.EddyViscosity;
    /// <summary>
    /// простой менеджер моделей турбулентной вязкости
    /// выбо по перечислению
    /// </summary>
    public static class MuManager
    {
        public static IEddyViscosityTri Get(ETurbViscType type, BEddyViscosityParam p)
        {
            IEddyViscosityTri eddyViscosityTri = null;
            switch (type)
            {
                case ETurbViscType.Boussinesq1865:
                    eddyViscosityTri = new EddyViscosity_Boussinesq1865(type, p);
                    break;
                case ETurbViscType.Karaushev1977:
                    eddyViscosityTri = new EddyViscosity_Karaushev1977(type, p);
                    break;
                case ETurbViscType.Prandtl1934:
                    eddyViscosityTri = new EddyViscosity_Prandtl1934(type, p);
                    break;
                case ETurbViscType.Velikanov1948:
                    eddyViscosityTri = new EddyViscosity_Velikanov1948(type, p);
                    break;
                case ETurbViscType.Leo_C_van_Rijn1984:
                    eddyViscosityTri = new EddyViscosity_Leo_van_Rijn1984(type, p);
                    break;
                case ETurbViscType.Absi_2012:
                    eddyViscosityTri = new EddyViscosity_Absi_2012(type, p);
                    break;
                case ETurbViscType.Absi_2019:
                    eddyViscosityTri = new EddyViscosity_Absi_2019(type, p);
                    break;
                case ETurbViscType.VanDriest1956:
                    eddyViscosityTri = new EddyViscosity_VanDriest1956(type, p);
                    break;
                case ETurbViscType.GLS_1995:
                    eddyViscosityTri = new EddyViscosity_GLS_1995(type, p);
                    break;
                case ETurbViscType.Les_Smagorinsky_Lilly_1996:
                    eddyViscosityTri = new EddyViscosity_Smagorinsky_Lilly_1996(type, p);
                    break;
                case ETurbViscType.Derek_G_Goring_and_K_1997:
                    eddyViscosityTri = new EddyViscosity_Derek_G_Goring_and_K_1997(type, p);
                    break;
                case ETurbViscType.PotapobII_2024:
                    eddyViscosityTri = new EddyViscosity_PotapovII_2024(type, p);
                    break;
                case ETurbViscType.КЕModelS:
                    eddyViscosityTri = new EddyViscosityVKEModelTri(type, p);
                    break;
                case ETurbViscType.КЕModelN:
                    eddyViscosityTri = new EddyViscosityKEModelTri(type, p);
                    break;
                case ETurbViscType.SAModelS:
                    eddyViscosityTri = new EddyViscosity_sSA_lTri(type, p);
                    break;
                case ETurbViscType.SAModelN:
                    eddyViscosityTri = new EddyViscosity_nSA_lTri(type, p);
                    break;
                default:
                    throw new Exception("Выбранная модель турбулентности пока не реализована");
            }
            return eddyViscosityTri;
        }
    }

}
