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
        public DataTable Selection(string polecenie, List<string> kolumny)
        {
            init();

            //Prepare table
            DataTable lt_wynik = new DataTable();
            foreach (String row in kolumny)
                lt_wynik.Columns.Add(row);

            cmd.CommandText = (polecenie);

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    foreach (var data in reader)
                    {
                        var dupa = reader;

                    }
                }
            }

            con.Close();
            return lt_wynik;
        }
        public bool Insertion(string table, List<string> columns, DataTable values, string additionalOptions)
        {
            init();
            var lv_success = false;
            var lv_columns = "";
            var lv_querry = "";

            //preparing columns
            foreach (var row in columns)
            {
                lv_columns += "," + row;
            }
            lv_columns = lv_columns.Substring(1, lv_columns.Length - 1);

            //command = "INSERT INTO KATEGORIE " +
            //            "(nazwa, numerKategorii, urlKategorii) " +
            //            "VALUES ('')";
            var lv_columns_count = values.Columns.Count;
            try
            {
                foreach (DataRow data in values.Rows)
                {
                    var lv_values = "";
                    for (var i = 0; i < lv_columns_count; i++)
                    {
                        lv_values += ",'" + data[i].ToString()+"'";
                    }
                    lv_values = lv_values.Substring(1, lv_values.Length - 1);
                    lv_querry = "INSERT INTO " + table + " (" + lv_columns + ") VALUES (" + lv_values + ") " + additionalOptions.ToUpper();
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
        public void WipeKategories()
        {
            init();
            cmd.CommandText = ("DELETE FROM KATEGORIE");
            cmd.ExecuteNonQuery();
            con.Close();
        }
    }
}
