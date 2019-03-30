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
        static SqlConnection con = new SqlConnection(@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\" 
            + System.Environment.UserName 
            + @"\source\repos\Okazje\Okazje\Baza\Baza.mdf;Integrated Security=True");
        static SqlCommand cmd = new SqlCommand();
        private static void init() {
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
        public bool Insertion(string polecenie, List<string> kolumny)
        {
            init();
            var lv_success = false;

            //basdasdasdasd


            // ((zmienna == '') ? tak : nie)

            con.Close();
            return lv_success;
        }
    }
}
