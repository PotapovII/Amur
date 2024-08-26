//---------------------------------------------------------------------------
//                    ПРОЕКТ  "РУСЛОВЫЕ ПРОЦЕССЫ"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//                 кодировка : 11.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace TriMeshGeneratorLib
{
    /// <summary>
    /// статус списка
    /// </summary>
    public enum ListStatus
    {
        /// <summary>
        /// массив списка сформирован 
        /// </summary>
        ListStatusValid,
        /// <summary>
        /// массив списка не сформирован или нарушен
        /// </summary>
        ListStatusNoValid
    };
    /// <summary>
    /// Список точек 
    /// </summary>
    public class RVListPoints
    {
        /// <summary>
        /// ссылка на первый элемент списка
        /// </summary>
        protected RVPoint theFirstInterpPt;
        /// <summary>
        /// ссылка на текущий элемент списка
        /// </summary>
        protected RVPoint theCurrentInterpPt;
        /// <summary>
        /// массив элементов в списке
        /// </summary>
        protected RVPoint[] InterpPtIndex;
        /// <summary>
        /// статус списка
        /// </summary>
        protected ListStatus indexState;
        /// <summary>
        /// соличество элементов в списке
        /// </summary>
        protected int count = 0;
        public RVListPoints()
        {
            count = 0;
            theFirstInterpPt = null;
            theCurrentInterpPt = null;
            InterpPtIndex = null;
            indexState = ListStatus.ListStatusNoValid;
        }
        /// <summary>
        /// не удаляет InterpPts
        /// does not delete InterpPts
        /// </summary>
        public void Clear()
        {
            firstInterpPt();
            while (count > 0)
                deleteCurrentInterpPt();
            InterpPtIndex = null;
            indexState = ListStatus.ListStatusNoValid;
        }
        public int Count => count;
        
        /// <summary>
        /// возвратный указатель в RVPoint
        /// return pointer to ith RVPoint
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public RVPoint GetIndexItem(int n)                        //	
        {
            if (indexState == ListStatus.ListStatusNoValid)
            {
                BuildIndex();
            }
            if ((n <= count) && (n > 0))
                return (InterpPtIndex[n - 1]);
            else
                return null;
        }
        public int BuildIndex()
        {
            int index = 0;
            InterpPtIndex = null;
            if (count > 0)
            {
                InterpPtIndex = new RVPoint[count];
                InterpPtIndex[index] = firstInterpPt();
                theFirstInterpPt.Index = 0;
                for (index = 1; index < count; index++)
                {
                    InterpPtIndex[index] = nextInterpPt();
                    theCurrentInterpPt.Index =index;
                }
            }
            indexState = ListStatus.ListStatusValid;
            return count;
        }
        /// <summary>
        /// получить первый RVPoint в списке
        /// get first RVPoint in list
        /// </summary>
        /// <returns></returns>
        public RVPoint firstInterpPt()
        {
            if (count > 0)
            {
                theCurrentInterpPt = theFirstInterpPt;
                return (theFirstInterpPt);
            }
            else
                return null;
        }
        //	get current RVPoint in list
        public RVPoint currentInterpPt()
        {
            if (count > 0)
            {
                return (theCurrentInterpPt);
            }
            else
                return null;
        }
        //	set current RVPoint in list
        public RVPoint setCurrentInterpPt(RVPoint InterpPtP)
        {
            if (InterpPtP != null)
                theCurrentInterpPt = InterpPtP;
            else
                theCurrentInterpPt = theFirstInterpPt;

            return theCurrentInterpPt;
        }
        //	get next RVPoint in list
        public RVPoint nextInterpPt()
        {
            if (count > 0)
            {
                if (theCurrentInterpPt.Next != null)
                {
                    theCurrentInterpPt = (RVPoint)theCurrentInterpPt.Next;
                    return (theCurrentInterpPt);
                }
                else
                    return null;
            }
            else
                return null;
        }
        //	get RVPoint with n = ID
        RVPoint Get(int ID)
        {
            if (count > 0)
            {
                theCurrentInterpPt = theFirstInterpPt;
                while (theCurrentInterpPt.ID != ID)
                {
                    if (theCurrentInterpPt.Next != null)
                    {
                        theCurrentInterpPt = (RVPoint)theCurrentInterpPt.Next;
                    }
                    else
                        return (null);
                }
                return (theCurrentInterpPt);
            }
            else
                return (null);
        }
        //	add new RVPoint to end of list
        public RVPoint appendInterpPt(RVPoint theNewInterpPt)
        {
            if (count > 0)
            {
                while (theCurrentInterpPt.Next != null)
                {
                    theCurrentInterpPt = (RVPoint)theCurrentInterpPt.Next;
                }
            }
            return (insertInterpPt(theNewInterpPt));
        }
        //	add new RVPoint after current RVPoint
        public RVPoint insertInterpPt(RVPoint theNewInterpPt)
        {
            if (count > 0)
            {
                theNewInterpPt.Next = theCurrentInterpPt.Next;
                theCurrentInterpPt.Next = theNewInterpPt;
                theCurrentInterpPt = theNewInterpPt;
            }
            else
            {
                theNewInterpPt.Next = null;
                theFirstInterpPt = theNewInterpPt;
                theCurrentInterpPt = theNewInterpPt;
            }
            count += 1;
            indexState = ListStatus.ListStatusNoValid;

            return (theNewInterpPt);
        }
        //	delete (really delete) all InterpPts in list
        public void clearList()
        {
            RVPoint nextInterpPtP;

            theCurrentInterpPt = theFirstInterpPt;

            while (count > 0)
            {
                nextInterpPtP = (RVPoint)theCurrentInterpPt.Next;
                theCurrentInterpPt = null;
                theCurrentInterpPt = nextInterpPtP;
                count -= 1;
            }
            theFirstInterpPt = null;
            InterpPtIndex = null;
            indexState = ListStatus.ListStatusNoValid;
        }
        //	remove current RVPoint, next becomes current
        public RVPoint deleteCurrentInterpPt()
        {
            RVPoint prevInterpPt;

            prevInterpPt = theFirstInterpPt;

            if (theFirstInterpPt != null)
            {
                if (theCurrentInterpPt != theFirstInterpPt)
                {
                    while (prevInterpPt.Next != theCurrentInterpPt)
                    {
                        prevInterpPt = (RVPoint)prevInterpPt.Next;
                        if (prevInterpPt == null)
                        {
                            return theCurrentInterpPt;
                        }
                    }
                    prevInterpPt.Next = theCurrentInterpPt.Next;
                    if (prevInterpPt.Next != null)
                        theCurrentInterpPt = (RVPoint)prevInterpPt.Next;
                    else
                        theCurrentInterpPt = prevInterpPt;
                }
                else
                {
                    theFirstInterpPt = (RVPoint)theCurrentInterpPt.Next;
                    theCurrentInterpPt = theFirstInterpPt;
                }
                count -= 1;
            }
            indexState = ListStatus.ListStatusNoValid;
            return (theCurrentInterpPt);
        }
        /// <summary>
        /// delete RVPoint pointed at, return currentInterpPt
        /// </summary>
        /// <param name="iP"></param>
        /// <returns></returns>
        public RVPoint deleteInterpPt(RVPoint iP)
        {
            RVPoint prevInterpPt = theFirstInterpPt;
            if (theFirstInterpPt == null)
                return null;
            if (iP == theFirstInterpPt)
            {
                theFirstInterpPt = (RVPoint)iP.Next;
                count -= 1;
                indexState = ListStatus.ListStatusNoValid;
                if (iP == theCurrentInterpPt)
                {
                    theCurrentInterpPt = theFirstInterpPt;
                }
                return theCurrentInterpPt;
            }
            while (prevInterpPt != null)
            {
                if (iP == prevInterpPt.Next)
                {
                    prevInterpPt.Next = iP.Next;
                    count -= 1;
                    indexState = ListStatus.ListStatusNoValid;
                    if (iP == theCurrentInterpPt)
                    {
                        theCurrentInterpPt = prevInterpPt;
                    }
                    return theCurrentInterpPt;
                }
                prevInterpPt = (RVPoint)prevInterpPt.Next;
            }
            return theCurrentInterpPt;
        }
        /// <summary>
        /// вернуть интерполированное значение
        /// </summary>
        /// <param name="xr"></param>
        /// <returns></returns>
        public double Interpolation(double xr)
        {
            RVPoint pt1 = null;
            RVPoint pt2;
            if ((pt2 = firstInterpPt()) == null)
                return 0.0;
            if (xr <= pt2.X)
                return pt2.Y;
            while (pt2 != null)
            {
                pt1 = pt2;
                if ((pt2 = nextInterpPt()) == null)
                    break;
                if (xr <= pt2.X)
                {
                    double r = (xr - pt1.X) / (pt2.X - pt1.X);
                    return (1 - r) * pt1.Y + r * pt2.Y;
                }
            }
            return pt1.Y;
        }
    }
}
