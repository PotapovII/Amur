using ConnectLib;
using RiverDB.Report;

namespace RiverDB.ConvertorOut
{
    public class RiverFlowManager
    {

        public int GetID(int Place, int idx = 0)
        {
            if (Place == 1)
                return idx;
            if (Place == 3)
                return 3;
            return -1;
        }

        /// <summary>
        /// Расчет расходов по кривым связи между отметкой гидропоста в см и расходом реки
        /// </summary>
        /// <param name="H"></param>
        /// <param name="ID"></param>
        /// <returns></returns>
        public double riverFlow(double H, int ID)
        {
            IRiverFlow rf = new RiverFlowZerro();
            switch (ID)
            {
                case 0:
                    rf = new RiverFlowAmur();
                    break;
                case 1:
                    rf = new RiverFlowPemza();
                    break;
                case 2:
                    rf = new RiverFlowMad();
                    break;
                case 3:
                    rf = new RiverFlowKomsomolskNaAmure();
                    break;
            }
            return rf.GetRiverFlow(H);
        }
        public void ChangGP_WL(int placeID, int ID, string Data,  
                    ref double BaltikLvl, ref double RiverLvl, ref double Q)
        {
            BaltikLvl = ConnectDB.WaterLevelGP(placeID);
            double H = ConnectDB.WaterLevelData(Data, placeID);
            Q = riverFlow(H, ID);
            RiverLvl = (BaltikLvl + H / 100);
        }
    }
}
