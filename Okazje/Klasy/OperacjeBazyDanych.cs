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

namespace Okazje.Klasy
{
    class OperacjeBazyDanych
    {
        //static SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Workspace\Okazje\Okazje\Baza\Baza.mdf;Integrated Security=True");

        static string connectionString = "SERVER=localhost" + ";" + "DATABASE=okazje;" + "UID=root;" + "PASSWORD=;;Convert Zero Datetime=True";
        static MySqlConnection con1 = new MySqlConnection(connectionString);
        static MySqlConnection con2 = new MySqlConnection(connectionString);
        static MySqlConnection con3 = new MySqlConnection(connectionString);
        static MySqlConnection con4 = new MySqlConnection(connectionString);

        static MySqlCommand cmd1 = new MySqlCommand();
        static MySqlCommand cmd2 = new MySqlCommand();
        static MySqlCommand cmd3 = new MySqlCommand();
        static MySqlCommand cmd4 = new MySqlCommand();

        private static void init()
        {

            switch ((Thread.CurrentThread.Name)) { 
                case "Thread0":
                    cmd1.Connection = con1;
                    cmd1.CommandType = System.Data.CommandType.Text;
                    con1.Open();
                    break;
                case "Thread1":
                    cmd2.Connection = con2;
                    cmd2.CommandType = System.Data.CommandType.Text;
                    con2.Open();
                    break;
                case "Thread2":
                    cmd3.Connection = con3;
                    cmd3.CommandType = System.Data.CommandType.Text;
                    con3.Open();
                    break;
                case "Thread3":
                    cmd4.Connection = con4;
                    cmd4.CommandType = System.Data.CommandType.Text;
                    con4.Open();
                    break;
                default:
                    cmd1.Connection = con1;
                    cmd1.CommandType = System.Data.CommandType.Text;
                    con1.Open();
                    break;
            }

            //if (con.State != ConnectionState.Open)
            //    try
            //    {
            //        con.Open();
            //    }
            //    catch (Exception e)
            //    {
            //        MessageBox.Show("Database connection is not established");
            //    }
        }
        public DataTable Selection(string iv_command)
{
    init();
    //if (con.State != ConnectionState.Open) return null;

    DataTable rt_values = new DataTable();
    MySqlCommand lv_database_comm;
    MySqlDataAdapter da1, da2, da3, da4;

            switch ((Thread.CurrentThread.Name))
            {
                case "Thread0":
                    lv_database_comm = new MySqlCommand(iv_command, con1);
                    da1 = new MySqlDataAdapter(lv_database_comm);
                    da1.Fill(rt_values);
                    con1.Close();
                    break;
                case "Thread1":
                    lv_database_comm = new MySqlCommand(iv_command, con2);
                    da2 = new MySqlDataAdapter(lv_database_comm);
                    da2.Fill(rt_values);
                    con2.Close();
                    break;
                case "Thread2":
                    lv_database_comm = new MySqlCommand(iv_command, con3);
                    da3 = new MySqlDataAdapter(lv_database_comm);
                    da3.Fill(rt_values);
                    con3.Close();
                    break;
                case "Thread3":
                    lv_database_comm = new MySqlCommand(iv_command, con4);
                    da4 = new MySqlDataAdapter(lv_database_comm);
                    da4.Fill(rt_values);
                    con4.Close();
                    break;
                default:
                    lv_database_comm = new MySqlCommand(iv_command, con1);
                    da1 = new MySqlDataAdapter(lv_database_comm);
                    da1.Fill(rt_values);
                    con1.Close();
                    break;
            }
    return rt_values;
}
public bool Insertion(string iv_table, string[] ia_columns, DataTable it_values, string iv_additionalOptions)
{
    //WipeCategories();
    init();
    var lv_success = false;
    var lv_columns = "";
    var lv_querry = "";

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
            lv_querry = "INSERT INTO " + iv_table + " (" + lv_columns + ") VALUES (" + lv_values + ") " + iv_additionalOptions.ToUpper();
                    switch ((Thread.CurrentThread.Name))
                    {
                        case "Thread0":
                            cmd1.CommandText = (lv_querry);
                            cmd1.ExecuteNonQuery();
                            con1.Close();
                            break;
                        case "Thread1":
                            cmd2.CommandText = (lv_querry);
                            cmd2.ExecuteNonQuery();
                            con2.Close();
                            break;
                        case "Thread2":
                            cmd3.CommandText = (lv_querry);
                            cmd3.ExecuteNonQuery();
                            con3.Close();
                            break;
                        case "Thread3":
                            cmd4.CommandText = (lv_querry);
                            cmd4.ExecuteNonQuery();
                            con4.Close();
                            break;
                        default:
                            cmd1.CommandText = (lv_querry);
                            cmd1.ExecuteNonQuery();
                            con1.Close();
                            break;
                    }
        }
        lv_success = true;
    }
    catch (Exception e)
    {
        con1.Close();
        con2.Close();
        con3.Close();
        con4.Close();
        return false;
    }
    // ((zmienna == '') ? tak : nie)
    return lv_success;
}
public void WipeCategories()
{
    init();
    cmd1.CommandText = ("DELETE FROM categories");
    cmd1.ExecuteNonQuery();
    con1.Close();
}
public void WipeProducts()
{
    init();

    cmd1.CommandText = ("DELETE FROM product");
    cmd1.ExecuteNonQuery();
    cmd1.CommandText = ("DELETE FROM productDetail");
    cmd1.ExecuteNonQuery();
    cmd1.CommandText = ("DELETE FROM productLinks");
    cmd1.ExecuteNonQuery();
    cmd1.CommandText = ("DELETE FROM productPrices");
    cmd1.ExecuteNonQuery();
    cmd1.CommandText = ("DELETE FROM productQuantity");
    cmd1.ExecuteNonQuery();

    con1.Close();
}
    }
}
