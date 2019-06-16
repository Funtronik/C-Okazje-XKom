using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using System.Net;
using System.Text.RegularExpressions;
using System.Data.SqlClient;
using System.Globalization;
using System.Net.Mail;

namespace Okazje
{
    public partial class Form1 : Form
    {
        Okazje.Klasy.OperacjeBazyDanych Baza = new Klasy.OperacjeBazyDanych();
        Okazje.Klasy.SpecialMethods SpecialMethods = new Klasy.SpecialMethods();
        Okazje.Klasy.EmailNotification EmailNotification = new Klasy.EmailNotification();

        string command;
        int gv_errors_occured = 0;
        int gv_num_of_threads = 100;
        List<string> gl_errors_email = new List<string>();

        public Form1()
        {
            InitializeComponent();
            SpecialMethods.lo_ToolStripLabel = toolStripStatusLabel1;
            SpecialMethods.lo_ToolStripProgressBar = toolStripProgressBar1;
            SpecialMethods.lo_ToolStrip = statusStrip1;
            SpecialMethods.lo_ToolSttipLabelAdditional = toolStripStatusLabel2;
            SpecialMethods.lo_ToolStripLabelErrors = toolStripStatusLabel3;
            gv_num_of_threads = int.Parse(TB_Number_Of_Threads.Text.ToString());
            Baza.initializeClass(gv_num_of_threads);
            EmailNotification.initializeClass();
            B_SMS_Click(null,null);
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
            var lt_max_product_id = Baza.Selection("SELECT MAX(CAST(productId AS SIGNED)) FROM product");
            var lv_actual_max_id = 0;

            if (int.TryParse(lt_max_product_id.Rows[0][0].ToString(), out var dummy))
                lv_actual_max_id = int.Parse(lt_max_product_id.Rows[0][0].ToString());

            //For ProgressBar
            var lv_current_category_index = 0;
            var lv_max_category_index = categories.Rows.Count;

            //For safety
            var lv_is_unnecessary = false;

            //Get Products
            foreach (DataRow row in categories.Rows)
            {
                SpecialMethods.progressBarUpdate(lv_current_category_index, lv_max_category_index, "Category");
                lv_current_category_index++;

                var categoryURL = row[3].ToString();
                var url = "https://" + categoryURL;
                var lv_rows_added = 0;
                for (int i = 1; i < 99; i++)
                {
                    if (lv_is_unnecessary)
                    {
                        lv_is_unnecessary = false;
                        break;
                    }
                    //for safety delete last 30 product from "proposal" section 
                    //at the end of page if no real product were selected from page.

                    if (lv_rows_added == 30 || lv_rows_added == 38 || lv_rows_added == 36)
                    {
                        var lv_row_index = lt_product_urls_from_cat.Rows.Count;
                        for (int a = 1; a <= lv_rows_added; a++)
                        {
                            var lv_to_delete = lv_row_index - a;
                            lt_product_urls_from_cat.Rows.RemoveAt(lv_to_delete);
                        }
                        lv_rows_added = 0;
                        lv_is_unnecessary = true;
                        continue;
                    }
                    lv_rows_added = 0;
                    var lv_url_additional_options = "?page=" + i + "&per_page=90";
                    using (WebClient client = new WebClient())
                    {
                        byte[] htmlData;
                        try
                        {
                            var temp = url + lv_url_additional_options;
                            htmlData = client.DownloadData(temp);
                        }
                        catch (Exception e)
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
                            if (m.Value.Contains("#Opinie")) continue;

                            if (m.Value.Contains("www.x-kom.p"))
                            {
                                val = m.Value.Substring(6, m.Value.Length - 7);
                                lt_product_urls_from_cat.Rows.Add(val, row[2]);
                                lv_rows_added++;
                            }
                            else
                            {
                                val = "www.x-kom.pl/" + m.Value.Substring(7, m.Value.Length - 8);
                                lt_product_urls_from_cat.Rows.Add(val, row[2]);
                                lv_rows_added++;
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
                    continue;

                var lv_id = lv_actual_max_id++;

                lt_product_to_insert.Rows.Add(row[1], lv_id);
                lt_links_to_insert.Rows.Add(lv_id, row[0], "www.x-kom.pl", "0");

            }
            dataGridView1.DataSource = lt_product_to_insert;
            dataGridView2.DataSource = lt_links_to_insert;
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
                "INNER JOIN productlinks AS T1 on T1.productId = T0.productId " +
                "INNER JOIN categories AS T2 on T0.categoryId = T2.categoryId LIMIT 100");
            dataGridView2.DataSource = lt_product;
        }

        private void Button3_Click(object sender, EventArgs e) //Download Product from cat
        {
            var lt_categories = Baza.Selection("SELECT * FROM categories");
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
            gv_num_of_threads = int.Parse(TB_Number_Of_Threads.Text.ToString());
            Baza.initializeClass(gv_num_of_threads);
            gl_errors_email.Clear();
            var lt_links_to_process = Baza.Selection("SELECT COUNT(*) FROM productLinks");
            var lv_current = 0;
            var lv_how_much = int.Parse(lt_links_to_process.Rows[0][0].ToString());
            var lv_details_inserted = 0;
            var lv_prices_inserted = 0;

            var la_threads = new Thread[gv_num_of_threads];

            do
            {
                for (int i = 0; i < gv_num_of_threads; i++)
                {
                    la_threads[i] = new Thread(() => getProductDetails(lv_current));
                    la_threads[i].Name = ("Thread" + i.ToString());
                    la_threads[i].Start();
                    SpecialMethods.progressBarUpdate(lv_current, lv_how_much, "Item");
                    lv_current++;
                }
                for (int i = 0; i < gv_num_of_threads; i++)
                {
                    la_threads[i].Join();
                }

                // End of everything
                clearVariables();

            } while (lv_current < lv_how_much);
            if (gl_errors_email.Count > 0)
            {
                EmailNotification.sendErrorOccuredDownloadDetails(gl_errors_email);
                gl_errors_email.Clear();
            }
            SpecialMethods.progressBarDone();
            SpecialMethods.showErrors(gv_errors_occured);
            MessageBox.Show("Product details inserted " + lv_details_inserted + ". Prices inserted " + lv_prices_inserted + ".");
        }

        private void getProductDetails(int iv_current)
        {
            var ls_product_max_date = Baza.Selection("SELECT MAX(productPriceDate) FROM productprices WHERE productId = '" + iv_current + "'");
            if (ls_product_max_date.Rows[0][0].ToString() == DateTime.UtcNow.ToString("dd.MM.yyyy 00:00:00")) return;

            var lt_productLinks = Baza.Selection("SELECT T0.productId, T1.productUrl FROM product AS T0 " +
                     "INNER JOIN productLinks AS T1 ON T0.productId = T1.productId WHERE T0.productId = '" + iv_current + "'");

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

            var lv_index = 0;

            var lt_product_line_to_insert = SpecialMethods.addColumns(la_datatable_columns);

            foreach (DataRow row in lt_productLinks.Rows)
            {
                var lv_html = "";
                using (WebClient client = new WebClient())
                {
                    byte[] htmlData = null;
                    for (int i = 0; i < 3; i++) // try 3 times
                    {
                        try
                        {
                            var temp = "https://" + row["productUrl"].ToString();
                            htmlData = client.DownloadData(temp);
                            break;
                        }
                        catch (Exception ex)
                        {
                            Thread.Sleep(500);
                        }
                    }
                    if (htmlData == null)// error occured. No data was downloaded
                    {
                        gl_errors_email.Add("https://" + row["productUrl"].ToString());
                        return;
                    }
                    else
                    {
                        lv_html = Encoding.UTF8.GetString(htmlData);
                    }
                    htmlData = null;
                }
                lt_product_line_to_insert.Rows.Add();
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

            string[] la_product_details_columns = new string[]
            {
                "productId",
                "productFullName",
                "productParameters",
                "productModel",
                "productManufacturer",
                "productImageUrl"
            };
            string[] la_product_prices_columns = new string[]
            {
                "productId",
                "productPriceNow",
                "productDomain",
                "productPriceDate",
                "productPricePrevious",
                "productDiscounted",
                "productDiscountRate",
                "productOutlet"
                //"linkId"
            };

            var lt_product_details = SpecialMethods.addColumns(la_product_details_columns);

            //Product details
            foreach (DataRow line in lt_product_line_to_insert.Rows)
            {
                var ls_current_prod_details = Baza.Selection(@"SELECT * FROM productdetail where productId = '" + line[0] + "'");
                if (ls_current_prod_details.Rows.Count > 0) continue;

                lt_product_details.Rows.Add(line["productId"],
                    line["data-product-name"],
                    line["data-product-category"].ToString().Replace("&quot;", "cale"),
                    line["data-product-name"].ToString().Split(' ')[1],
                    line["data-product-brand"],
                    line["og:image"]);
            }

            if (lt_product_details.Rows.Count > 0)
            {
                Baza.Insertion("productdetail", la_product_details_columns, lt_product_details, "");
                //lv_details_inserted++;
            }

            // Prices
            var lt_product_prices = SpecialMethods.addColumns(la_product_prices_columns);

            foreach (DataRow line in lt_product_line_to_insert.Rows)
            {
                float lv_discount = 0;

                if (line["product:original_price"].ToString() != "")
                {
                    lv_discount = ((float.Parse(line["product:price"].ToString().Replace('.', ',')) * 100) / float.Parse(line["product:original_price"].ToString().Replace('.', ',')));
                    lv_discount = 100 - lv_discount;
                }

                float lv_original_price = 0;
                if (line["product:original_price"].ToString() != "")
                    lv_original_price = float.Parse(line["product:original_price"].ToString().Replace('.', ','));

                var ls_product_prev_price = Baza.Selection("SELECT productPricePrevious FROM productprices WHERE productId = '" + line[0] + "'");

                if (ls_product_prev_price.Rows.Count > 0)
                { // exist
                    lt_product_prices.Rows.Add(line["productId"],
                        float.Parse(line["product:price"].ToString().Replace(".", ",")),
                        "www.x-kom.pl",//product domain,
                        DateTime.UtcNow.ToString("yyyy-MM-dd H:mm:ss"),
                        float.Parse(ls_product_prev_price.Rows[0]["productPricePrevious"].ToString().Replace(".", ",")),
                        lv_discount != 0 ? 1 : 0,
                        lv_discount.ToString().Replace(".", ","),
                        line["product:condition"]);
                }
                else
                { // dont exist
                    lt_product_prices.Rows.Add(line["productId"],
                        float.Parse(line["product:price"].ToString().Replace(".", ",")),
                        "www.x-kom.pl",//product domain,
                        DateTime.UtcNow.ToString("yyyy-MM-dd H:mm:ss"),
                        lv_original_price,
                        lv_discount != 0 ? 1 : 0,
                        lv_discount.ToString().Replace(".", ","),
                        line["product:condition"]);
                }
            }
            //    "productId",
            //    "productPriceNow",
            //    "productDomain",
            //    "productPriceDate",
            //    "productPricePrevious",
            //    "productDiscounted",
            //    "productDiscountRate",
            //    "productOutlet"
            if (lt_product_prices.Rows.Count > 0)
            {
                Baza.Insertion("productprices", la_product_prices_columns, lt_product_prices, "");
                //lv_prices_inserted++;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            toolStripProgressBar1.Visible = false;
            toolStripStatusLabel1.Visible = false;
            toolStripStatusLabel2.Visible = false;
        }

        private void B_SMS_Click(object sender, EventArgs e)
        {
            try
            {
                Okazje.Klasy.Scheduler.IntervalInDays(9, 1, 1, () => { EmailNotification.specialOfferMailNotification("alto"); });
                Okazje.Klasy.Scheduler.IntervalInDays(10, 1, 1, () => { EmailNotification.specialOfferMailNotification("xkom"); });
                Okazje.Klasy.Scheduler.IntervalInDays(21, 1, 1, () => { EmailNotification.specialOfferMailNotification("alto"); });
                Okazje.Klasy.Scheduler.IntervalInDays(22, 1, 1, () => { EmailNotification.specialOfferMailNotification("xkom"); });
                PB_SMS.Image = Okazje.Properties.Resources.on;
                B_SMS.Text = "Enabled";
                PB_SMS.Refresh();
            }
            catch (Exception ex)
            {
                PB_SMS.Image = Okazje.Properties.Resources.off;
                B_SMS.Text = "Disabled";
                PB_SMS.Refresh();
            }
        }
    }
}