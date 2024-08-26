//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 30.12.2023 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.ConvertorOut
{
    using GeometryLib;
    /// <summary>
    /// о сегменте для создания mrf файла
    /// </summary>
    public class SegmentInfo
    {
        /// <summary>
        /// 1 - контур области, -1 контур острова
        /// </summary>
        public int ID;
        /// <summary>
        /// тип сегмента: берег 1 вход 2, выход 3 потока
        /// </summary>
        public int type;
        /// <summary>
        /// Вершина A сегмента
        /// </summary>
        public CloudKnot pA;
        /// <summary>
        /// Вершина A сегмента
        /// </summary>
        public CloudKnot pB;
        /// <summary>
        /// Нормальный к сегменту расход
        /// </summary>
        public double Qn = 0;
        /// <summary>
        /// Касательной к сегменту расход
        /// </summary>
        public double Qt = 0;
        /// <summary>
        /// Отметка свободной поверхности H + zeta
        /// </summary>
        public double Eta = 0;
        /// <summary>
        /// Номер вершины pA
        /// </summary>
        public int startID;
        /// <summary>
        /// Номер вершины pB
        /// </summary>
        public int endID;
        /// <summary>
        /// номер граничного элемента с вершиной с 1 - й вершиной pA
        /// </summary>
        public int BEstartID;
        /// <summary>
        /// номер граничного элемента с вершиной со 2 - й вершиной pB
        /// </summary>
        public int BEendID;
        /// <summary>
        /// Тип потока 
        /// </summary>
        public bool CriticalFlowRegime;
        /// <summary>
        //    switch (boundCondType)
        //    {
        //        case 0:            elem  segment
        //            bcc[0] = 0.0;  H      H
        //            bcc[1] = 1.0;  Qx     Qn
        //            bcc[2] = 1.0;  Qy     Qt
        //            break;
        //        case 1:
        //            bcc[0] = 0.0;
        //            bcc[1] = 1.0;
        //            bcc[2] = 1.0;
        //            break;
        //        case 2:
        //            bcc[0] = 1.0;
        //            bcc[1] = 1.0;
        //            bcc[2] = 1.0;
        //            break;
        //        case 3:
        //            bcc[0] = 1.0;
        //            bcc[1] = 0.0;
        //            bcc[2] = 0.0;
        //            break;
        //        case 4:
        //            bcc[0] = 0.0;
        //            bcc[1] = 0.0;
        //            bcc[2] = 0.0;
        //            break;
        //        case 5:
        //            bcc[0] = 0.0;
        //            bcc[1] = 0.0;
        //            bcc[2] = 0.0;
        //            break;
        //    }
        /// </summary>
        public int boundCondType()
        {
            //2 : вход потока
            if (type == 2)
            {
                // критический режим потока
                if( CriticalFlowRegime == true )
                    return 2;
                else // до критический режим потока
                    return 1;
            }
            // 3: выход потока
            else
            {
                // критический режим потока
                if (CriticalFlowRegime == true)
                    return 5;
                else // до критический режим потока
                    return 3;
            }
        }
        public SegmentInfo() { }
        public SegmentInfo(int iD, int type, CloudKnot pA, CloudKnot pB)
        {
            ID = iD;
            this.type = type;
            this.pA = pA;
            this.pB = pB;
            CriticalFlowRegime = false;
        }
        public SegmentInfo(int iD, int type, CloudKnot pA, CloudKnot pB, 
            CloudKnot pAp, CloudKnot pBm, double qn, double qt, double eta) 
            : this(iD, type, pA, pB)
        {
            Qn = qn;
            Qt = qt;
            Eta = eta;
            CriticalFlowRegime = false;
        }


        public void Set(bool CriticalFlowRegime, double qn, double qt, double eta)
        {
            this.CriticalFlowRegime = CriticalFlowRegime;
            this.Qn = qn;
            this.Qt = qt;
            this.Eta = eta;
        }
    }
}
