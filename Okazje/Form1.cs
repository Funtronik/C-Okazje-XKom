using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Globalization;

namespace Okazje
{
    public partial class Form1 : Form
    {
        Okazje.Klasy.OperacjeBazyDanych Baza = new Klasy.OperacjeBazyDanych();
        Okazje.Klasy.SpecialMethods SpecialMethods = new Klasy.SpecialMethods();
        string command;
        int gv_errors_occured = 0;

        public Form1()
        {
            InitializeComponent();
            SpecialMethods.lo_ToolStripLabel = toolStripStatusLabel1;
            SpecialMethods.lo_ToolStripProgressBar = toolStripProgressBar1;
            SpecialMethods.lo_ToolStrip = statusStrip1;
            SpecialMethods.lo_ToolSttipLabelAdditional = toolStripStatusLabel2;
            SpecialMethods.lo_ToolStripLabelErrors = toolStripStatusLabel3;
        }
        private void clearVariables()
        {
            gv_errors_occured = 0;
            command = "";
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            getCategories("www.x-kom.pl");
        }
        public static string RemoveDiacritics(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Normalize(NormalizationForm.FormD);
            var chars = text.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            return new string(chars).Normalize(NormalizationForm.FormC);
        }
        private void insertCategories(DataTable categories, string iv_domain)
        {
            clearVariables();
            var lv_max_index_category = 0;

            command = "SELECT MAX(categoryId) FROM categories";
            var lt_result = Baza.Selection(command);
            if (lt_result.Rows.Count > 0)
            {
                if (int.TryParse(lt_result.Rows[0][0].ToString(), out var dummy))
                    int.TryParse(lt_result.Rows[0][0].ToString(), out lv_max_index_category);
                else
                    lv_max_index_category = 0;
            }

            // Prepare values in table
            var la_columns = new string[] { "categoryName", "categoryId", "categoryUrl", "categoryDomain" };
            var lt_values_to_insert = SpecialMethods.addColumns(la_columns);

            foreach (DataRow row in categories.Rows)
            {
                lt_values_to_insert.Rows.Add(row[0], lv_max_index_category++, row[1], iv_domain);
            }

            Baza.Insertion("CATEGORIES", la_columns, lt_values_to_insert, "");

            clearVariables();

        }
        private void getCategories(string iv_domain)
        {
            DataTable lt_categories = new DataTable();
            lt_categories.Columns.Add("categoryName");
            lt_categories.Columns.Add("Url");

            var url = "https://" + iv_domain + "/";
            var htmlCode = "";
            using (WebClient client = new WebClient())
            {
                var htmlData = client.DownloadData(url);
                var code = Encoding.UTF8.GetString(htmlData);
                htmlCode = code;
            }

            var lt_actual_categories = Baza.Selection("SELECT * FROM categories");
            var lv_pattern = @"<a href=""(.*?)a>";

            foreach (Match m in Regex.Matches(htmlCode, lv_pattern))
            {
                if (m.Value.Contains("/g-"))
                {
                    var ls_url = m.Value.ToString().Split('"');
                    var ls_values = new string[2];
                    ls_values[0] = ls_url[2].Substring(1, ls_url[2].Length - 5); // nazwa
                    ls_values[1] = ls_url[1]; // url
                    var ls_row = lt_actual_categories.Select("( categoryName = '" + ls_values[0].ToString() + "' " +
                        "AND categoryDomain = '" + iv_domain + "') " +
                        "OR categoryUrl LIKE '%" + ls_values[1].ToString() + "%'");
                    if (ls_row.Length > 0) continue;

                    lt_categories.Rows.Add(ls_values[0], iv_domain + ls_values[1]);
                }
            }
            //Delete duplicates 
            lt_categories = SpecialMethods.RemoveDuplicateRows(lt_categories, "categoryName");
            insertCategories(lt_categories, iv_domain);
        }
        private void getProductsFromCategoties(DataTable categories)
        {
            DataTable lt_product_urls_from_cat = new DataTable();
            string[] la_columns = new string[] { "productUrl", "categoryId" };

            var htmlCode = "";

            lt_product_urls_from_cat = SpecialMethods.addColumns(la_columns);

            //Get Max current ID
            var lt_max_product_id = Baza.Selection("SELECT MAX(productId) FROM product");
            var lv_actual_max_id = 0;

            if (int.TryParse(lt_max_product_id.Rows[0][0].ToString(), out var dummy))
                lv_actual_max_id = int.Parse(lt_max_product_id.Rows[0][0].ToString());

            //For ProgressBar
            var lv_current_category_index = 0;
            var lv_max_category_index = categories.Rows.Count;

            //Get Products
            foreach (DataRow row in categories.Rows)
            {
                SpecialMethods.progressBarUpdate(lv_current_category_index, lv_max_category_index, "Category");
                lv_current_category_index++;

                var categoryURL = row[3].ToString();
                var url = "https://" + categoryURL;
                for (int i = 1; i < 99; i++)
                {
                    var lv_url_additional_options = "?page=" + i + "&per_page=90";
                    using (WebClient client = new WebClient())
                    {
                        byte[] htmlData;
                        try
                        {
                            var temp = url + lv_url_additional_options;
                            htmlData = client.DownloadData(temp);
                        }catch (Exception e)
                        {
                            gv_errors_occured++;
                            continue;
                        }
                        htmlCode = Encoding.UTF8.GetString(htmlData);
                        htmlData = null;
                    }
                    Regex rx = new Regex(@"href=\""(.*?)\""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    MatchCollection matches = rx.Matches(htmlCode);
                    htmlCode = "";
                    foreach (Match m in matches)
                    {
                        var val = "";
                        if (m.Value.Contains("/p/"))
                        {
                            //Exclude
                            if (m.Value.Contains("#reviews")) continue;
                            if (m.Value.Contains("?page=")) continue;
                            if (m.Value.Contains("/c/")) continue;
                            if (m.Value.Contains("https://")) continue;

                            if (m.Value.Contains("www.x-kom.p"))
                            {
                                val = m.Value.Substring(6, m.Value.Length - 7);
                                lt_product_urls_from_cat.Rows.Add(val, row[2]);
                            }
                            else
                            {
                                val = "www.x-kom.pl/" + m.Value.Substring(7, m.Value.Length - 8);
                                lt_product_urls_from_cat.Rows.Add(val, row[2]);
                            }
                            SpecialMethods.additionalLabelStatusBar("Products :" + lt_product_urls_from_cat.Rows.Count.ToString());
                        }
                    }
                }
            }
            dataGridView1.DataSource = lt_product_urls_from_cat;
            lt_product_urls_from_cat = SpecialMethods.RemoveDuplicateRows(lt_product_urls_from_cat, "productUrl");

            //Insertion to DataBase
            string[] ll_columns_products = new string[] { "categoryId", "productId" };
            var lt_product_to_insert = SpecialMethods.addColumns(ll_columns_products);
            string[] ll_columns_links = new string[] { "productId", "productUrl", "productDomain", "linkId" };
            var lt_links_to_insert = SpecialMethods.addColumns(ll_columns_links);

            foreach (DataRow row in lt_product_urls_from_cat.Rows)
            {
                var lt_actual_products = Baza.Selection("SELECT COUNT(*)" +
                    " FROM productLinks" +
                    " WHERE productUrl = '" + row[0] + "'");
                if (lt_actual_products.Rows[0][0].ToString() != "0")
                {
                    continue;
                }

                var lv_id = lv_actual_max_id++;
                lt_product_to_insert.Rows.Add(row[1], lv_id);
                lt_links_to_insert.Rows.Add(lv_id, row[0], "www.x-kom.pl", "0");

            }

            Baza.Insertion("PRODUCT", ll_columns_products, lt_product_to_insert, "");
            Baza.Insertion("PRODUCTLINKS", ll_columns_links, lt_links_to_insert, "");

            SpecialMethods.progressBarDone();
            SpecialMethods.showErrors(gv_errors_occured);
            clearVariables();
        }
        private void Button2_Click(object sender, EventArgs e) // Refresh DataGrids
        {
            var lt_categories = Baza.Selection("SELECT * FROM categories");
            dataGridView1.DataSource = lt_categories;
            var lt_product = Baza.Selection("SELECT T0.productId, T2.categoryName, T1.productUrl, T1.productDomain, T1.linkId " +
                "FROM product AS T0 " +
                "INNER JOIN productLinks AS T1 on T1.productId = T0.productId " +
                "INNER JOIN categories AS T2 on T0.categoryId = T2.categoryId ");
            dataGridView2.DataSource = lt_product;
            //var lt_productDetail = Baza.Selection("SELECT * FROM productDetail");
            //dataGridView3.DataSource = lt_productDetail;
            //var lt_productPrices = Baza.Selection("SELECT * FROM productPrices");
            //dataGridView4.DataSource = lt_productPrices;
        }

        private void Button3_Click(object sender, EventArgs e) //Download Product from cat
        {
            var lt_categories = Baza.Selection("SELECT * FROM categories LIMIT 1");
            getProductsFromCategoties(lt_categories);
        }

        private void Button4_Click(object sender, EventArgs e)
        {
            Baza.WipeCategories();
        }

        private void Button5_Click(object sender, EventArgs e)
        {
            Baza.WipeProducts();
        }

        private void DataGridView2_RowHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            //Wyswietlanie row
        }

        private void Button6_Click(object sender, EventArgs e) // get product details button
        {
            var lt_productLinks = Baza.Selection("SELECT T0.productId, T1.productUrl FROM product AS T0 " +
                "INNER JOIN productLinks AS T1 ON T0.productId = T1.productId LIMIT 40");

            if (lt_productLinks == null) return;

            string[] la_search_parameters_product = new string[] {
                "product:price",
                "product:sale_price",
                "product:original_price",
                "product:condition",
                "og:image"
            };
            string[] la_seach_parameters_data_product = new string[] {
                "data-product-name",
                "data-product-price",
                "data-product-brand",
                "data-product-category"
            };

            string[] la_datatable_columns = new string[]
            {
                //standard
                "productId",
                "productUrl",
                //product:
                "product:price",
                "product:sale_price",
                "product:original_price",
                "product:condition",
                "og:image",
                //data-product
                "data-product-name",
                "data-product-price",
                "data-product-brand",
                "data-product-category",
            };

            var lv_items_to_process = lt_productLinks.Rows.Count;
            var lv_index = 0;

            var lt_product_line_to_insert = SpecialMethods.addColumns(la_datatable_columns);

            foreach (DataRow row in lt_productLinks.Rows)
            {
                SpecialMethods.progressBarUpdate(lv_index, lv_items_to_process, "Link");
                lt_product_line_to_insert.Rows.Add();

                var lv_html = "";
                using (WebClient client = new WebClient())
                {
                    byte[] htmlData;
                    try
                    {
                        htmlData = client.DownloadData(row["productUrl"].ToString());
                        lv_html = Encoding.UTF8.GetString(htmlData);
                    }catch (Exception ex)
                    {
                        gv_errors_occured++;
                        continue;
                    }
                }

                //product:
                foreach (var parameter in la_search_parameters_product)
                {
                    var lv_fetched_val = SpecialMethods.getProductParameter(parameter, lv_html);
                    if (lv_fetched_val == "") continue;

                    Regex regex = new Regex(@"content=""(.*?)\""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var lt_matches = regex.Matches(lv_fetched_val);
                    lt_product_line_to_insert.Rows[lv_index][parameter] = lt_matches[0].Groups[1];
                }

                //data-product:
                foreach (var parameter in la_seach_parameters_data_product)
                {
                    var lv_fetched_val = SpecialMethods.getProductParameterV2(parameter, lv_html);
                    if (lv_fetched_val == "") continue;

                    Regex regex = new Regex(@"=\""(.*?)\""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                    var lt_matches = regex.Matches(lv_fetched_val);
                    lt_product_line_to_insert.Rows[lv_index][parameter] = lt_matches[0].Groups[1];
                }

                //Additional parameters of current line from ProductList
                lt_product_line_to_insert.Rows[lv_index]["productId"] = row["productId"];
                lt_product_line_to_insert.Rows[lv_index]["productUrl"] = row["productUrl"];

                lv_index++;
            }

            // Insert Details if not exist in table
            lv_index = 0;
            lv_items_to_process = lt_product_line_to_insert.Rows.Count;

            string[] la_product_details_columns = new string[]
            {
                "productId",
                "productFullName",
                "productParameters",
                "productModel",
                "productManufacturer",
                "productImageUrl"
            };
            var lt_product_details = SpecialMethods.addColumns(la_product_details_columns);

            foreach (DataRow line in lt_product_line_to_insert.Rows)
            {
                SpecialMethods.progressBarUpdate(lv_index, lv_items_to_process, "Product Detail");
                lv_index++;

                var ls_current_prod_details = Baza.Selection(@"SELECT * FROM productdetail where productId = '"+ line[0] + "'");
                if (ls_current_prod_details.Rows.Count > 0) continue;

                lt_product_details.Rows.Add(line["productId"],
                    line["data-product-name"],
                    line["data-product-category"].ToString().Replace("&quot;", "cale"),
                    line["data-product-name"].ToString().Split(' ')[1],
                    line["data-product-brand"],
                    line["og:image"]);
            }

            if (lt_productLinks.Rows.Count > 0)
            Baza.Insertion("productdetail", la_product_details_columns, lt_product_details, "");

            // End of everything
            dataGridView2.DataSource = lt_product_details;
            SpecialMethods.progressBarDone();
            SpecialMethods.showErrors(gv_errors_occured);
            clearVariables();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel2.Visible = false;
        }
    }
}
