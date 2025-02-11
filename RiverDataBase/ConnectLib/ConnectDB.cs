//---------------------------------------------------------------------------
//                         проектировщик:
//                           Потапов И.И.
//---------------------------------------------------------------------------
//        кодировка : 01.10.2020 Потапов И.И. & Потапов Д.И.
//---------------------------------------------------------------------------
namespace ConnectLib
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    public class ConnectDB
    {
        /// <summary>
        /// Получение диапазона
        /// </summary>
        /// <param name="LatMin"></param>
        /// <param name="LatMax"></param>
        /// <param name="LonMin"></param>
        /// <param name="LonMax"></param>
        public static void GetMinMaxCoords(ref double LatMin, ref double LatMax, ref double LonMin, ref double LonMax)
        {
            string Select = "SELECT MAX(knot_latitude) as LatMax, MAX(knot_longitude) as LonMax," +
                            "MIN(knot_latitude) as LatMin, MIN(knot_longitude) as LonMin FROM knot";
            //OleDbConnection  
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            connection.Open();
            var cmd = new SqlCommand(Select, connection);
            cmd.ExecuteNonQuery();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
            DataSet mapDataSet = new DataSet();
            dataAdapter.Fill(mapDataSet, "knot");
            DataTable pointsTable = mapDataSet.Tables["knot"];
            connection.Close();
            DataRow dr1 = pointsTable.Rows[0];
            try
            {
                LatMin = double.Parse(dr1["LatMin"].ToString());
                LatMax = double.Parse(dr1["LatMax"].ToString());
                LonMin = double.Parse(dr1["LonMin"].ToString());
                LonMax = double.Parse(dr1["LonMax"].ToString());
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        /// <summary>
        /// Получить имя справочника по ID 
        /// </summary>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ID записи</param>
        /// <returns></returns>
        public static string GetName(string TName, int id)
        {
            DataRow row = GetDataRowBook(TName, id);
            return row[TName + "_name"].ToString();
        }
        /// <summary>
        /// Получить имя справочника по ID 
        /// </summary>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ID записи</param>
        /// <returns></returns>
        public static string GetName(string TName, string id)
        {
            return GetName(TName, int.Parse(id));
        }
        /// <summary>
        /// Получить строку в таблице по ID 
        /// </summary>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ID записи</param>
        /// <returns></returns>
        public static DataRow GetDataRowBook(string TName, int id)
        {
            string strAccessSelect = "SELECT " + TName + "_id , " + TName + "_name  FROM " 
                                    + TName + " WHERE " + TName + "_id = @ID";
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            connection.Open();
            var cmd = new SqlCommand(strAccessSelect, connection);
            cmd.Parameters.Add("@ID", id.ToString());
            cmd.ExecuteNonQuery();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
            DataSet mapDataSet = new DataSet();
            dataAdapter.Fill(mapDataSet, TName);
            DataTable mapTable = mapDataSet.Tables[TName];
            connection.Close();
            DataRow[] rows = mapTable.Select(TName + "_id = " + id.ToString());
            return rows[0];
        }
        /// <summary>
        /// Получить строку в таблице по ID 
        /// </summary>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ID записи</param>
        /// <returns></returns>
        public static DataRow GetDataRow(string TName, int id)
        {
            string strAccessSelect = "SELECT * FROM " + TName + " WHERE " + TName + "_id = @ID";
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            connection.Open();
            var cmd = new SqlCommand(strAccessSelect, connection);
            cmd.Parameters.Add("@ID", id.ToString());
            cmd.ExecuteNonQuery();
            SqlDataAdapter dataAdapter = new SqlDataAdapter(cmd);
            DataSet mapDataSet = new DataSet();
            dataAdapter.Fill(mapDataSet, TName);
            DataTable mapTable = mapDataSet.Tables[TName];
            connection.Close();
            DataRow[] rows = mapTable.Select(TName + "_id = " + id.ToString());
            return rows[0];

        }
        /// <summary>
        /// Получить таблицу 
        /// </summary>
        /// <param name="strCommand">строка запроса</param>
        /// <param name="TName">название таблицы</param>
        /// <returns></returns>
        static public DataTable GetDataTable(string strCommand, string TName)
        {
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            SqlCommand com = new SqlCommand(strCommand, connection);
            SqlDataAdapter dataAdapter = new SqlDataAdapter(com);
            connection.Open();
            DataSet mapDataSet = new DataSet();
            dataAdapter.Fill(mapDataSet, TName);
            DataTable mapTable = mapDataSet.Tables[TName];
            connection.Close();
            return mapTable;
        }
        /// <summary>
        /// Получить таблицу
        /// </summary>
        /// <param name="strCommand">строка запроса</param>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ключ фильтрации</param>
        /// <returns></returns>
        public static DataTable SelectByKey(string strCommand, string TName, int id)
        {
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            SqlCommand com = new SqlCommand(strCommand, connection);
            com.Parameters.Add("@id", id.ToString());
            SqlDataAdapter dataAdapter = new SqlDataAdapter(com);
            connection.Open();
            DataSet mapDataSet = new DataSet();
            dataAdapter.Fill(mapDataSet, TName);
            DataTable mapTable = mapDataSet.Tables[TName];
            connection.Close();
            return mapTable;
        }
        /// <summary>
        /// Получить таблицу
        /// </summary>
        /// <param name="strCommand">строка запроса</param>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ключ фильтрации</param>
        /// <returns></returns>
        public static DataTable SelectTableBystrKey(string strCommand, string TName, string pole)
        {
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);

            SqlCommand com = new SqlCommand(strCommand, connection);
            com.Parameters.Add("@POLE", "%" + pole + "%");
            SqlDataAdapter dataAdapter = new SqlDataAdapter(com);
            connection.Open();
            DataSet mapDataSet = new DataSet();
            dataAdapter.Fill(mapDataSet, TName);
            DataTable mapTable = mapDataSet.Tables[TName];
            connection.Close();
            return mapTable;
        }
        /// <summary>
        /// Удаление строки
        /// </summary>
        /// <param name="TName">название таблицы</param>
        /// <param name="id">ключ фильтрации</param>
        public static void DeleteRow(string TName, int id)
        {
            string strCom = "DELETE FROM " + TName + " WHERE " + TName + "_ID = @ID";
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            connection.Open();
            var cmd = new SqlCommand(strCom, connection);
            cmd.Parameters.Add("@ID", id.ToString());
            cmd.ExecuteNonQuery();
            connection.Close();
        }
        /// <summary>
        /// Выполнение SQL команды
        /// </summary>
        /// <param name="strCom"></param>
        public static void SQLCommandDo(string strCom)
        {
            SqlConnection connection = new SqlConnection(ConnectPath.connectString);
            connection.Open();
            SqlCommand cmd = new SqlCommand(strCom, connection);
            cmd.ExecuteNonQuery();
            connection.Close();
        }

        /// <summary>
        /// Тестирование строки соединения
        /// </summary>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static bool ConnectStringTest(string connectString, ref string Message)
        {
            try
            {
                SqlConnection connection = new SqlConnection(connectString);
                connection.Open();
                connection.Close();
                Message = "Ok";
                return true;
            }
            catch(Exception ex)
            {
                Message = ex.Message;
                return false;
            }
        }
        /// <summary>
        /// выполняеем сборку мусора - не синхронизированные данных
        /// </summary>
        /// <param name="Count">количество удаляемых данных</param>
        /// <param name="filter">фильтр по долготе</param>
        /// <returns></returns>
        public static string DoSqlTransactionCommand(string strClearErr)
        {
            // соединение с БД
            using (SqlConnection connection = new SqlConnection(ConnectPath.connectString))
            {
                // открыть соединение с БД
                connection.Open();
                // создать транзакцию
                SqlTransaction transaction = connection.BeginTransaction();
                // создать команду
                SqlCommand command = connection.CreateCommand();
                // соединить команду с транзакцией
                command.Transaction = transaction;
                try
                {
                    // присвоить текст комманды SQL
                    command.CommandText = strClearErr;
                    // выполнить команду SQL на обновление
                    command.ExecuteNonQuery();
                    // подтверждаем транзакцию
                    transaction.Commit();
                    Console.WriteLine("Удаление не синхронизированных данных добавленых в базу данных");
                    connection.Close();
                    return "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // откатываем транзакцию
                    transaction.Rollback();
                    connection.Close();
                    return "Ошибка: " + ex.Message;
                }
            }
        }
        /// <summary>
        /// Выполнение групповых операций
        /// </summary>
        /// <param name="list"></param>
        /// <param name="tcom"></param>
        /// <param name="Count"></param>
        /// <param name="Message"></param>
        /// <returns></returns>
        public static bool DoListCommand(List<IStandartSQLCommand> list, TypeCommand tcom, ref int Count, ref string Message)
        {
            bool result = true;
            Count = 0;
            // соединение с БД
            using (SqlConnection connection = new SqlConnection(ConnectPath.connectString))
            {
                // открыть соединение с БД
                connection.Open();
                // создать транзакцию
                SqlTransaction transaction = connection.BeginTransaction();
                // создать команду
                SqlCommand command = connection.CreateCommand();
                // соединить команду с транзакцией
                command.Transaction = transaction;
                try
                {
                    // выполняем команды добавления данных в БД
                    foreach (IStandartSQLCommand point in list)
                    {
                        try
                        {
                            string ss = Count.ToString() + " " + point.ToString();
                            Console.WriteLine(ss);
                            // присвоить текст комманды SQL по вставке узла
                            command.CommandText = point.GetCommand(tcom);
                            // выполнить команду SQL
                            command.ExecuteNonQuery();
                            Count++;
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }
                    // подтверждаем транзакцию
                    transaction.Commit();
                    Console.WriteLine("Данные добавлены в базу данных");
                    list.Clear();
                    Message = "В базу данных добавлено " + list.Count.ToString() + " записей из xls";
                }
                catch (Exception ex)
                {
                    // откатываем транзакцию
                    transaction.Rollback();
                    Message = "Ошибка :" + ex.Message;
                    result = false;
                }
                connection.Close();
                return result;
            }
        }
        /// <summary>
        /// Получение отметки нуля водомерного поста 
        /// </summary>
        /// <param name="placeID">1 == Хабаровск</param>
        /// <returns></returns>
        public static double WaterLevelGP(int placeID = 1)
        {
            string TName = "Place";
            
            try
            {
              string strAccessSelect = "select * from dbo.place";
                DataTable place = ConnectDB.GetDataTable(strAccessSelect, TName);
                if (place != null)
                {
                    foreach (DataRow dr in place.Rows)
                    {
                        int id = (int)dr["place_id"];
                        if (placeID == id)
                        {
                            // Ноль графика - отметка репера по Балтийской системе
                            double nullheight = (double)dr["place_nullheight"];
                            return nullheight;
                        }
                    }
                }
            }
            catch (Exception e)
            { 
                Console.WriteLine(e.Message);
            }
            return 0;
        }
        /// <summary>
        /// Информация об водомерном посте 
        /// </summary>
        /// <param name="placeID"></param>
        /// <param name="Name"></param>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Zerro">отметка нуля водомерного поста</param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static double PlaceInfo(int placeID, ref string Name, ref string X, 
            ref string Y,ref double Zerro, ref double height)
        {
            string TName = "Place";
            try
            {
                string strAccessSelect = "select * from dbo.place where place_id = '" + placeID.ToString() + "'";
                DataTable place = ConnectDB.GetDataTable(strAccessSelect, TName);
                if (place != null)
                {
                    foreach (DataRow dr in place.Rows)
                    {
                        int id = (int)dr["place_id"];
                        if (placeID == id)
                        {
                            // Ноль графика - отметка репера по Балтийской системе
                            Name = (string)dr["place_name"];
                            X = ((double)dr["place_x"]).ToString();
                            Y = ((double)dr["place_y"]).ToString();
                            Zerro = (double)dr["place_nullheight"];
                            height = (double)dr["place_height"];
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }
        /// <summary>
        /// Получение отметки нуля водомерного поста 
        /// </summary>
        /// <param name="placeID">1 == Хабаровск</param>
        /// <returns></returns>
        public static double WaterLevelData(string Data, int placeID = 1)
        {
            string TName = "Experiment";
            try
            {
                string sql = "select * from dbo.Experiment where [place_id] = '"
                 + placeID.ToString() + "' AND CAST([experiment_datetime] AS DATE) = '" +
                            Data + "'";
                DataTable place = ConnectDB.GetDataTable(sql, TName);
                DataRow dr = place.Rows[0];
                double WL = (double)dr["experiment_waterlevel"];
                return WL;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return 0;
        }

    }
}

