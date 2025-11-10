using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverDB.Report
{

    public class PDay : IComparable<PDay>
    {
        public double waterlevel;
        public DateTime dateTime;
        public int day;
        public int ID;
        public PDay(double waterlevel, DateTime dateTime, int day, int ID)
        {
            this.waterlevel = waterlevel;
            this.dateTime = dateTime;
            this.day = day;
            this.ID = ID;
        }
        public PDay(PDay pd)
        {
            this.waterlevel = pd.waterlevel;
            this.dateTime = pd.dateTime;
            this.day = pd.day;
            this.ID = pd.ID;
        }
        public virtual int CompareTo(PDay other)
        {
            return day.CompareTo(other.day);
        }
    }

}
