using CommonLib.Geometry;
using GeometryLib.Aalgorithms;
using MemLogLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestSHillObj
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {

            //int[] m = { 0, 7, 1, 4, 2, 3, 4, 5, 6,-2, 7, 8, 9 };
            //LOG.Print("m", m);
            //Duplicates.RemoveDuplicatesShort(ref m);
            //LOG.Print("m1", m);
            //HPoint[] hPoint = new HPoint[]
            //{
            //    new HPoint(0,0),
            //    new HPoint(0,1),
            //    new HPoint(1,1),
            //    new HPoint(1,1),
            //    new HPoint(1,0),
            //    new HPoint(0,1)
            //};
            //for (int i = 0; i < hPoint.Length; i++)
            //    Console.Write(hPoint[i].ToString());
            //Console.WriteLine();
            //Duplicates.RemoveDuplicates(ref hPoint);
            //for (int i = 0; i < hPoint.Length; i++)
            //    Console.Write(hPoint[i].ToString());

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
