﻿namespace RiverDB
{
    using System;
    using System.Windows.Forms;
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MFRiverDB());
        }
    }
}
