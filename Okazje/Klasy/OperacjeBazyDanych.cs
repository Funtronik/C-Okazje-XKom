using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Windows.Forms;
using System.Threading;
using System.Net.NetworkInformation;

namespace Okazje.Klasy
{
    class OperacjeBazyDanych
    {
        static string connectionString = "SERVER=&&&&" + ";" + "DATABASE=okazje;" + "UID=root;" + "PASSWORD=;;Convert Zero Datetime=True;Connection Timeout=90";
        static int gv_thread_id = 0;
        static int gv_num_of_threads = 100;
        static MySqlConnection[] la_MySqlConnections = new MySqlConnection[gv_num_of_threads];
        static MySqlCommand[] la_MySqlCommands = new MySqlCommand[gv_num_of_threads];
        static MySqlDataAdapter[] la_MySqlDataAdapters = new MySqlDataAdapter[gv_num_of_threads];
        static Dictionary<string, int> ld_thread_ids = new Dictionary<string, int>();
        private static void init()
        {
            getThreadId();
            do
            {
                if (la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]].State != ConnectionState.Open)
                    la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]].Open();
                else
                    Thread.Sleep(500);
            } while (la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]].State != ConnectionState.Open);
        }
        public DataTable Selection(string iv_command)
        {
            init();
            //if (con.State != ConnectionState.Open) return null;

            DataTable rt_values = new DataTable();
            MySqlCommand lv_database_comm;

            lv_database_comm = new MySqlCommand(iv_command, la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]]);
            lv_database_comm.CommandTimeout = 90;
            la_MySqlDataAdapters[ld_thread_ids[Thread.CurrentThread.Name]] = new MySqlDataAdapter(lv_database_comm);
            la_MySqlDataAdapters[ld_thread_ids[Thread.CurrentThread.Name]].Fill(rt_values);
            la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]].Close();
            return rt_values;
        }
        public bool Insertion(string iv_table, string[] ia_columns, DataTable it_values, string iv_additionalOptions)
        {
            //WipeCategories();
            init();
            var lv_success = false;
            var lv_columns = "";

            //preparing columns
            foreach (var row in ia_columns)
            {
                lv_columns += "," + row;
            }
            lv_columns = lv_columns.Substring(1, lv_columns.Length - 1);
            var lv_columns_count = it_values.Columns.Count;

            try
            {
                foreach (DataRow data in it_values.Rows)
                {
                    var lv_values = "";
                    for (var i = 0; i < lv_columns_count; i++)
                    {
                        lv_values += ",'" + data[i].ToString() + "'";
                    }
                    lv_values = lv_values.Substring(1, lv_values.Length - 1);
                    var lv_querry = "INSERT INTO " + iv_table + " (" + lv_columns + ") VALUES (" + lv_values + ") " + iv_additionalOptions.ToUpper();
                    la_MySqlCommands[ld_thread_ids[Thread.CurrentThread.Name]].CommandText = (lv_querry);
                    la_MySqlCommands[ld_thread_ids[Thread.CurrentThread.Name]].CommandTimeout = 90;
                    la_MySqlCommands[ld_thread_ids[Thread.CurrentThread.Name]].ExecuteNonQuery();
                    la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]].Close();
                }
                lv_success = true;
            }
            catch (Exception e)
            {
                la_MySqlConnections[ld_thread_ids[Thread.CurrentThread.Name]].Close();
                return false;
            }
            return lv_success;
        }
        public bool Deletion(string iv_command)
        {
            init();
            la_MySqlCommands[gv_thread_id].CommandText = (iv_command);
            la_MySqlCommands[gv_thread_id].ExecuteNonQuery();
            return true;
        }
        public void WipeCategories()
        {
            //init();
            //la_MySqlCommands[gv_thread_id].CommandText = ("DELETE FROM categories");
            //la_MySqlCommands[gv_thread_id].ExecuteNonQuery();
            //la_MySqlConnections[gv_thread_id].Close();
        }
        public void WipeProducts()
        {
            //init();
            //la_MySqlCommands[gv_thread_id].CommandText = ("DELETE FROM product");
            //la_MySqlCommands[gv_thread_id].ExecuteNonQuery();
            //la_MySqlCommands[gv_thread_id].CommandText = ("DELETE FROM productDetail");
            //la_MySqlCommands[gv_thread_id].ExecuteNonQuery();
            //la_MySqlCommands[gv_thread_id].CommandText = ("DELETE FROM productLinks");
            //la_MySqlCommands[gv_thread_id].ExecuteNonQuery();
            //la_MySqlCommands[gv_thread_id].CommandText = ("DELETE FROM productPrices");
            //la_MySqlCommands[gv_thread_id].ExecuteNonQuery();
            //la_MySqlCommands[gv_thread_id].CommandText = ("DELETE FROM productQuantity");
            //la_MySqlCommands[gv_thread_id].ExecuteNonQuery();

            //la_MySqlConnections[gv_thread_id].Close();
        }
        public void initializeClass(int iv_num_of_threads)
        {

            connectionString = connectionString.Replace("&&&&", getMySQLServerAdress());

            ld_thread_ids.Clear();
            gv_num_of_threads = iv_num_of_threads;
            for (int i = 0; i < gv_num_of_threads; i++)
            {
                //Connection
                la_MySqlConnections[i] = new MySqlConnection(connectionString);
                //Command
                la_MySqlCommands[i] = new MySqlCommand();
                la_MySqlCommands[i].CommandType = System.Data.CommandType.Text;
                la_MySqlCommands[i].Connection = la_MySqlConnections[i];
                //Adapters
                ld_thread_ids.Add("Thread" + i.ToString(), i);
            }
        }
        private static void getThreadId()
        {
            if (Thread.CurrentThread.Name == null)
                Thread.CurrentThread.Name = "Thread0";
        }
        private string getMySQLServerAdress()
        {
            Ping pingSender = new Ping();
            string data = "TEST";
            byte[] buffer = Encoding.ASCII.GetBytes(data);
            int timeout = 5000;
            PingOptions options = new PingOptions(64, true);
            PingReply reply = pingSender.Send("192.168.137.1", timeout, buffer, options);

            return (reply.Status == IPStatus.Success) ? "192.168.137.1" : "localhost";
        }
    }
}
