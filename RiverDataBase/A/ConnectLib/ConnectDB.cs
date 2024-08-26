
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
                    return "";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    // откатываем транзакцию
                    transaction.Rollback();
                    return "Ошибка: " + ex.Message;
                }
                connection.Close();
            }
        }
        public static bool DoListCommand(List<IStandartSQLCommand> list, ref int Count, ref string Message)
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
                            command.CommandText = point.GetCommand();
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
    }
}

