using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Okazje.Klasy
{
    class OperacjeBazyDanych
    {
        static SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Workspace\Okazje\Okazje\Baza\Baza.mdf;Integrated Security=True");
        static SqlCommand cmd = new SqlCommand();
        private static void init()
        {
            cmd.Connection = con;
            cmd.CommandType = System.Data.CommandType.Text;
            con.Open();
        }
        public DataTable Selection(string iv_command)
        {
            init();

            DataTable lt_values = new DataTable();
            var lv_database_comm = new SqlCommand(iv_command, con);
            SqlDataAdapter da = new SqlDataAdapter(lv_database_comm);

            da.Fill(lt_values);

            con.Close();
            return lt_values;
        }
        public bool Insertion(string iv_table, string [] ia_columns, DataTable it_values, string iv_additionalOptions)
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
                        lv_values += ",'" + data[i].ToString()+"'";
                    }
                    lv_values = lv_values.Substring(1, lv_values.Length - 1);
                    lv_querry = "INSERT INTO " + iv_table + " (" + lv_columns + ") VALUES (" + lv_values + ") " + iv_additionalOptions.ToUpper();
                    cmd.CommandText = (lv_querry);
                    cmd.ExecuteNonQuery();
                }
                lv_success = true;
            }
            catch (Exception e) { con.Close(); return false; }
            // ((zmienna == '') ? tak : nie)
            con.Close();
            return lv_success;
        }
        public void WipeCategories()
        {
            init();
            cmd.CommandText = ("DELETE FROM categories");
            cmd.ExecuteNonQuery();
            con.Close();
        }
        public void WipeProducts()
        {
            init();

            cmd.CommandText = ("DELETE FROM product");
            cmd.ExecuteNonQuery();
            cmd.CommandText = ("DELETE FROM productDetail");
            cmd.ExecuteNonQuery();
            cmd.CommandText = ("DELETE FROM productLinks");
            cmd.ExecuteNonQuery();
            cmd.CommandText = ("DELETE FROM productPrices");
            cmd.ExecuteNonQuery();
            cmd.CommandText = ("DELETE FROM productQuantity");
            cmd.ExecuteNonQuery();

            con.Close();
        }
    }
}
