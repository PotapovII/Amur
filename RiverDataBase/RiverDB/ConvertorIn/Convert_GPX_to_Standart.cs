//---------------------------------------------------------------------------
//                   ПРОЕКТ  "Русловые процессы"
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//              кодировка : 13.08.2021 Потапов И.И.
//---------------------------------------------------------------------------
namespace RiverDB.Convertors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Data.OleDb;
    using System.Data;
    using ConnectLib;

    /// <summary>
    /// Чтение усеченного формата GPX и конвертация в формат StandartGPX
    /// </summary>
    public class Convert_GPX_to_Standart
    {
        public static List<IStandartSQLCommand> LoadGPX(string name)
        {
            List<IStandartSQLCommand> list = new List<IStandartSQLCommand>();
            double lat;
            double lon;
            DateTime dataTime;
            double H;
            double T;
            int count = 0;
            // 0 <trkpt lat="48.4630609956" lon="135.0764992833">
            // 1   <time>2018-08-04T06:38:04Z</time>
            // 2   <extensions>
            // 3       <gpxtpx:TrackPointExtension>
            // 4           <gpxtpx:wtemp>21.51</gpxtpx:wtemp>
            // 5           <gpxtpx:depth>1.66</gpxtpx:depth>
            // 6       </gpxtpx:TrackPointExtension>
            // 7   </extensions>
            // 8 </trkpt>

            // 0       < trkpt lat = "48.4508649912" lon = "135.0944731012" >
            // 1       < time > 2020 - 08 - 16T01: 36:31Z </ time >
            // 2               < extensions >
            // 3                   < gpxtpx:TrackPointExtension >
            // 4                        < gpxtpx:wtemp > 22.79 </ gpxtpx:wtemp >
            // 5                        < gpxtpx:depth > 0.35 </ gpxtpx:depth >
            // 6                   </ gpxtpx:TrackPointExtension >
            // 7               </ extensions >
            // 8       </ trkpt >

            bool old = false;
            if (old)
            {
                using (StreamReader file = new StreamReader(name))
                {
                    int k = 0;
                    for (string line = file.ReadLine(); line != null; line = file.ReadLine())
                    {

                        line = line.Trim();
                        string[] sline = line.Split('"');
                        lat = double.Parse(sline[1]);
                        lon = double.Parse(sline[3]);

                        line = file.ReadLine(); // 
                        line = line.Trim();
                        sline = line.Split('>', '<');
                        dataTime = DateTime.Parse(sline[2]);

                        line = file.ReadLine(); // extensions
                        line = file.ReadLine(); // TrackPointExtension
                        line = file.ReadLine(); // TrackPointExtension
                        line = line.Trim();
                        sline = line.Split('>', '<');
                        T = double.Parse(sline[2]);

                        line = file.ReadLine(); // TrackPointExtension
                        line = line.Trim();
                        sline = line.Split('>', '<');
                        H = double.Parse(sline[2]);

                        line = file.ReadLine(); // TrackPointExtension
                        line = file.ReadLine(); // TrackPointExtension
                        line = file.ReadLine(); // TrackPointExtension

                        StandartGPX s = new StandartGPX(lat, lon, dataTime, H, T);
                        list.Add(s); count++;
                        Console.WriteLine("k = " + k.ToString() + " " + s.ToString());
                        k++;
                    }
                }
            }
            else
            {
                using (StreamReader file = new StreamReader(name))
                {
                    int k = 0;
                    int N = 9;
                    string[] lines = new string[N];
                    for (string line = file.ReadLine(); line != null; line = file.ReadLine())
                    {
                        line = line.Trim();
                        lines[0] = line;
                        string[] sline = line.Split('"');
                        if ("<trkpt lat=" == sline[0])
                        {
                            bool complete = false;
                            for (int i = 1; i < N; i++)
                            {
                                line = file.ReadLine();
                                if (line == null)
                                    break;
                                lines[i] = line;
                                sline = line.Split('"');
                                string ss = sline[0].Trim();
                                if ("</trkpt>" == ss && i == 8)
                                {
                                    complete = true;
                                    break;
                                }
                            }
                            if (complete == true)
                            {
                                // 0 строка
                                line = lines[0];
                                sline = line.Split('"');
                                lat = double.Parse(sline[1]);
                                lon = double.Parse(sline[3]);
                                // 1 строка
                                line = lines[1].Trim();
                                sline = line.Split('>', '<');
                                dataTime = DateTime.Parse(sline[2]);
                                // 4 строка
                                line = lines[4].Trim();
                                sline = line.Split('>', '<');
                                T = double.Parse(sline[2]);
                                // 5 строка
                                line = lines[5].Trim();
                                sline = line.Split('>', '<');
                                H = double.Parse(sline[2]);

                                StandartGPX s = new StandartGPX(lat, lon, dataTime, H, T);
                                list.Add(s); count++;
                                Console.WriteLine("k = " + k.ToString() + " " + s.ToString());
                                k++;
                            }
                        }
                    }
                }
            }
            return list;
        }

        public static List<IStandartSQLCommand> LoadExcel(string name)
        {
            DataRow[] rows = null;
            List<IStandartSQLCommand> list = new List<IStandartSQLCommand>();
            string Con = String.Format("Provider=Microsoft.Jet.OLEDB.4.0;Extended Properties=\"Excel 8.0;HDR=No\"; Data Source={0}", name);
            OleDbConnection connect = new OleDbConnection(Con);
            string strCmd = "SELECT * FROM [Лист1$A1:AN50000]";
            OleDbCommand command = new OleDbCommand(strCmd, connect);
            DataSet mapDataSet = new DataSet();
            try
            {
                connect.Open();
                command.ExecuteNonQuery();
                OleDbDataAdapter dataAdapter = new OleDbDataAdapter(command);
                dataAdapter.Fill(mapDataSet);
                DataTable mapTable = mapDataSet.Tables[0];
                rows = mapTable.Select();
                int Count = 0;
                foreach (var r in rows)
                {
                    string s0 = r[0].ToString();
                    string s1 = r[1].ToString().TrimEnd(' ', 'm', 'м'); // depth
                    if (s1 == "")
                        s1 = "0.1";
                    //string ss1 = r[1].ToString();
                    string s2 = r[2].ToString(); // ...
                    //string ss3 = r[3].ToString(); 
                    string s3 = r[3].ToString().TrimEnd(' ', 'm', 'м');  // step
                    string s4 = r[4].ToString(); // time
                    string s5 = r[5].ToString().TrimEnd(' ', 'k', 'm', 'м', '/', 'h', 'к', 'м', 'ч');  // speed
                    //string ss5 = r[5].ToString(); 
                    string s6 = r[6].ToString().TrimEnd('°', ' ', 't', 'r', 'u', 'e', 'и', 'с', 'т', 'н', 'а'); // course
                    //string ss6 = r[6].ToString();
                    string s7 = r[7].ToString(); // dataTime
                    string s8 = r[8].ToString();
                    string s81 = r[8].ToString().Substring(1, 9);   //  e  lat
                    string s82 = r[8].ToString().Substring(12, 10);  // n  lon
                    string s9 = r[9].ToString().TrimEnd('°', 'C', 'С', ' '); // T
                    //string ss9 = r[9].ToString();
                    Count++;
                    string ss = Count.ToString() + " " + s0 + " " + s1 + " " + s2 + " " + s3 +
                        " " + s4 + " " + s5 + " " + s6 + " " + s7 + " " + s8 + " " + s9;
                    Console.WriteLine(ss);

                    ExtStandart point = new ExtStandart();
                    point.depth = double.Parse(s1);
                    point.step = double.Parse(s3);
                    point.time = DateTime.Parse(s4);
                    point.speed = double.Parse(s5);
                    point.course = double.Parse(s6);
                    point.dataTime = DateTime.Parse(s7);
                    point.lat = double.Parse(s81);
                    point.lon = double.Parse(s82);
                    point.T = double.Parse(s9);
                    list.Add(point);
                }
                connect.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return list;
        }
        /// <summary>
        /// Загрузка данных старых форматов ( 2005-2008 г)
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static List<IStandartSQLCommand> LoadDat(string name)
        {
            List<IStandartSQLCommand> list = new List<IStandartSQLCommand>();
            int count = 0;
            // дата создания файла
            //DateTime creationFile = File.GetCreationTime(name);
            using (StreamReader file = new StreamReader(name))
            {
                int IdxFormat = -1;
                for (string line = file.ReadLine(); line != null; line = file.ReadLine())
                {
                    line = line.Trim();
                    string[] slines = line.Split(',');
                    string[] lines = new string[slines.Length];
                    int k = 0;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (slines[i] != "")//null)
                            lines[k++] = slines[i];
                    }
                    if (lines.Length < 8)
                        continue;
                    try
                    {
                        double H = 10;
                        // долгота Y
                        double Lon = double.Parse(lines[0]);
                        // широта X
                        double Lat = double.Parse(lines[1]);
                        // срезанная глубина H
                        double cutH = double.Parse(lines[2]);
                        DateTime dTime;
                        // вычисление времени
                        string ss = lines[3].Trim('\"', ' ', '\'');
                        TimeSpan Time = TimeSpan.Parse(ss);
                        string s1, s2, s3, sss;
                        string line4;
                        if (IdxFormat == -1)
                        {
                            line4 = lines[4];
                            if (line4.Length > 4) // формат 2005
                            {
                                IdxFormat = 4;
                                // рабочая глубина H
                                H = double.Parse(lines[6]);

                            }
                            else
                            {
                                IdxFormat = 5;
                                line4 = lines[5];
                                // рабочая глубина H
                                H = double.Parse(lines[7]);
                            }
                        }
                        else
                        {
                            line4 = lines[IdxFormat];
                            // рабочая глубина H
                            H = double.Parse(lines[IdxFormat + 2]);
                        }
                        if (line4.Length >= 6)
                        {
                            s1 = line4.Substring(0, 2);
                            s2 = line4.Substring(2, 2);
                            s3 = line4.Substring(4, 2);
                        }
                        else
                        {
                            s1 = line4.Substring(0, 1);
                            s2 = line4.Substring(1, 2);
                            s3 = line4.Substring(3, 2);
                        }
                        sss = s1 + "." + s2 + ".20" + s3;
                        DateTime creation = DateTime.Parse(sss);
                        dTime = creation.Date + Time;

                        // широта X
                        double Latitude = Convertor_SK42_to_WGS84.SK42XTOB(Lat, Lon, H);
                        // долгота Y
                        double Longitude = Convertor_SK42_to_WGS84.SK42YTOL(Lat, Lon, H);

                        ExtStandartOld point = new ExtStandartOld();
                        point.cutDepth = cutH;
                        point.depth = H;
                        point.step = 0;
                        point.time = new DateTime();
                        point.speed = 0;
                        point.course = 0;
                        point.dataTime = dTime;// DateTime.Parse(SdataTime);
                        point.lat = Latitude;
                        point.lon = Longitude;
                        point.T = 1;
                        list.Add(point);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(count.ToString() + " " + ex.Message);
                    }
                    count++;
                }
            }
            return list;
        }
        /// <summary>
        /// Загрузка урезов форматов ( 2005-2008 г)
        /// </summary>
        public static List<IStandartSQLCommand> LoadDatUres(string name)
        {
            List<IStandartSQLCommand> list = new List<IStandartSQLCommand>();
            int count = 0;
            // дата создания файла
            //DateTime creationFile = File.GetCreationTime(name);
            using (StreamReader file = new StreamReader(name))
            {
                int IdxFormat = -1;
                for (string line = file.ReadLine(); line != null; line = file.ReadLine())
                {
                    line = line.Trim();
                    string[] slines = line.Split(',');
                    string[] lines = new string[slines.Length];
                    int k = 0;
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (slines[i] != "") //null)
                            lines[k++] = slines[i];
                    }
                    if (lines.Length < 8)
                        continue;
                    try
                    {
                        double H = 10;
                        // долгота Y
                        double Lon = double.Parse(lines[0]);
                        // широта X
                        double Lat = double.Parse(lines[1]);
                        // срезанная глубина H
                        double cutH = double.Parse(lines[2]);
                        DateTime dTime;
                        // вычисление времени
                        string ss = lines[3].Trim('\"', ' ', '\'');
                        TimeSpan Time = TimeSpan.Parse(ss);
                        string s1, s2, s3, sss;
                        string line4;
                        if (IdxFormat == -1)
                        {
                            line4 = lines[4];
                            if (line4.Length > 4) // формат 2005
                            {
                                IdxFormat = 4;
                                // рабочая глубина H
                                H = double.Parse(lines[6]);

                            }
                            else
                            {
                                IdxFormat = 5;
                                line4 = lines[5];
                                // рабочая глубина H
                                H = double.Parse(lines[7]);
                            }
                        }
                        else
                        {
                            line4 = lines[IdxFormat];
                            // рабочая глубина H
                            H = double.Parse(lines[IdxFormat + 2]);
                        }
                        if (line4.Length >= 6)
                        {
                            s1 = line4.Substring(0, 2);
                            s2 = line4.Substring(2, 2);
                            s3 = line4.Substring(4, 2);
                        }
                        else
                        {
                            s1 = line4.Substring(0, 1);
                            s2 = line4.Substring(1, 2);
                            s3 = line4.Substring(3, 2);
                        }
                        sss = s1 + "." + s2 + ".20" + s3;
                        DateTime creation = DateTime.Parse(sss);
                        dTime = creation.Date + Time;

                        // широта X
                        double Latitude = Convertor_SK42_to_WGS84.SK42XTOB(Lat, Lon, H);
                        // долгота Y
                        double Longitude = Convertor_SK42_to_WGS84.SK42YTOL(Lat, Lon, H);

                        ExtStandartOld point = new ExtStandartOld();
                        point.cutDepth = cutH;
                        point.depth = H;
                        point.step = 0;
                        point.time = new DateTime();
                        point.speed = 0;
                        point.course = 0;
                        point.dataTime = dTime;// DateTime.Parse(SdataTime);
                        point.lat = Latitude;
                        point.lon = Longitude;
                        point.T = 1;
                        list.Add(point);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(count.ToString() + " " + ex.Message);
                    }
                    count++;
                }
            }
            return list;
        }

    }

}
