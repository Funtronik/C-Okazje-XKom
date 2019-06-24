using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Okazje.Klasy
{
    class EmailNotification
    {
        private static Okazje.Klasy.SpecialMethods SpecialMethods = new SpecialMethods();
        private static Okazje.Klasy.OperacjeBazyDanych Baza = new OperacjeBazyDanych();

        private static string[] gt_emails = new string[]{
                        "paulina.szmatula243@gmail.com"};

        private static string[] ga_special_offer_columns = new string[]{
                    "product_name",
                    "product_id",
                    "old_price",
                    "new_price",
                    "discount",
                    "sold_info",
                    "sold_already",
                    "sold_total"};

        private static Dictionary<string, string> gd_domain_link = new Dictionary<string, string>();
        public void initializeClass()
        {
            gd_domain_link.Add("xkom", "https://www.x-kom.pl/goracy-strzal");
            gd_domain_link.Add("alto", "https://www.al.to/goracy_strzal");
        }
        public void specialOfferMailNotification(string iv_domain)
        {
            switch (iv_domain)
            {
                case "xkom":
                    getSpecialOfferXkom(gd_domain_link[iv_domain]);
                    break;
                case "alto":
                    getSpecialOfferAlTo(gd_domain_link[iv_domain]);
                    break;
                default:
                    break;
            }
        }
        private static bool getSpecialOfferXkom(string iv_link)
        {
            try
            {
                var lt_special_offer = SpecialMethods.addColumns(ga_special_offer_columns);
                lt_special_offer.Rows.Add();

                var lv_html = "";
                using (WebClient client1 = new WebClient())
                {
                    byte[] htmlData;
                    htmlData = client1.DownloadData(iv_link);
                    lv_html = Encoding.UTF8.GetString(htmlData);
                    htmlData = null;
                }
                var index = 0;
                foreach (var parameter in Okazje.Klasy.SearchParametersInSourceView.ga_search_parameters_special_offer_xkom)
                {
                    var lv_fetched_val = SpecialMethods.getDetailForNotification(parameter, lv_html);
                    lt_special_offer.Rows[0][index] = lv_fetched_val;
                    index++;
                }

                var lv_sold = "";
                if (lt_special_offer.Rows[0]["sold_already"].ToString() == "" && lt_special_offer.Rows[0]["sold_total"].ToString() == "")
                    lv_sold = lt_special_offer.Rows[0]["sold_info"].ToString();
                else
                    lv_sold = lt_special_offer.Rows[0]["sold_total"] + @"<strong>/</strong>" + lt_special_offer.Rows[0]["sold_already"].ToString();

                var lv_mail_body = Okazje.Properties.Resources.MailBody;
                lv_mail_body = lv_mail_body.Replace(@"$PRODUCT_NAME$", lt_special_offer.Rows[0]["product_name"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$PRODUCT_PRICE$", lt_special_offer.Rows[0]["new_price"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$product_id$", lt_special_offer.Rows[0]["product_id"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$prev_price$", lt_special_offer.Rows[0]["old_price"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$discount$", lt_special_offer.Rows[0]["discount"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$sold$", lv_sold);

                setEmail(lt_special_offer.Rows[0][0].ToString(), lv_mail_body, gt_emails);

                var la_insetion_notification = new string[]
                {
                    "date",
                    "hasbeensend",
                    "domain"
                };
                var lt_insertion_notification = SpecialMethods.addColumns(la_insetion_notification);
                lt_insertion_notification.Rows.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), "X", "x-kom");

                do
                {
                    var lv_inserted = Baza.Insertion("notifications", la_insetion_notification, lt_insertion_notification, "");
                    if (!lv_inserted) System.Threading.Thread.Sleep(1000);
                    else break;
                } while (true); // do until u insert this value

                return true;
            }
            catch (Exception ex)
            {
                var lv_mail_body = Okazje.Properties.Resources.MailErrorBody;
                lv_mail_body = lv_mail_body.Replace(@"$ERROR_NAME$", ex.ToString());
                lv_mail_body = lv_mail_body.Replace(@"$VARIABLES$", "");
                setEmail("Error in Notification Method", lv_mail_body, gt_emails);
                return false;
            }
        }
        private static bool getSpecialOfferAlTo(string iv_link)
        {
            try
            {
                var lt_special_offer = SpecialMethods.addColumns(ga_special_offer_columns);
                lt_special_offer.Rows.Add();

                var lv_html = "";
                using (WebClient client1 = new WebClient())
                {
                    byte[] htmlData;
                    htmlData = client1.DownloadData(iv_link);
                    lv_html = Encoding.UTF8.GetString(htmlData);
                    htmlData = null;
                }
                var index = 0;
                lv_html = SpecialMethods.getDetailForNotification(Okazje.Klasy.SearchParametersInSourceView.gv_alto_initial_hot_shot_section, lv_html);
                lv_html = lv_html.Replace('\"', '"');
                foreach (var parameter in Okazje.Klasy.SearchParametersInSourceView.ga_search_parameters_special_offer_alto)
                {
                    var lv_fetched_val = SpecialMethods.getDetailForNotification(parameter.Replace('%','\\'), lv_html);
                    lt_special_offer.Rows[0][index] = lv_fetched_val;
                    index++;
                }

                var lv_sold = "";
                if (lt_special_offer.Rows[0]["sold_already"].ToString() == "" && lt_special_offer.Rows[0]["sold_total"].ToString() == "")
                    lv_sold = lt_special_offer.Rows[0]["sold_info"].ToString();
                else
                    lv_sold = lt_special_offer.Rows[0]["sold_total"] + @"<strong>/</strong>" + lt_special_offer.Rows[0]["sold_already"].ToString();

                var lv_mail_body = Okazje.Properties.Resources.MailBody;
                lv_mail_body = lv_mail_body.Replace(@"$PRODUCT_NAME$", lt_special_offer.Rows[0]["product_name"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$PRODUCT_PRICE$", lt_special_offer.Rows[0]["new_price"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$product_id$", lt_special_offer.Rows[0]["product_id"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$prev_price$", lt_special_offer.Rows[0]["old_price"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$discount$", lt_special_offer.Rows[0]["discount"].ToString());
                lv_mail_body = lv_mail_body.Replace(@"$sold$", lv_sold);


                setEmail(lt_special_offer.Rows[0][0].ToString(), lv_mail_body, gt_emails);

                var la_insetion_notification = new string[]
                {
                    "date",
                    "hasbeensend",
                    "domain"
                };
                var lt_insertion_notification = SpecialMethods.addColumns(la_insetion_notification);
                lt_insertion_notification.Rows.Add(DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString(), "X", "x-kom");

                do
                {
                    var lv_inserted = Baza.Insertion("notifications", la_insetion_notification, lt_insertion_notification, "");
                    if (!lv_inserted) System.Threading.Thread.Sleep(1000);
                    else break;
                } while (true); // do until u insert this value

                return true;
            }
            catch (Exception ex)
            {
                var lv_mail_body = Okazje.Properties.Resources.MailErrorBody;
                lv_mail_body = lv_mail_body.Replace(@"$ERROR_NAME$", ex.ToString());
                lv_mail_body = lv_mail_body.Replace(@"$VARIABLES$", "");
                setEmail("Error in Notification Method", lv_mail_body, gt_emails);
                return false;
            }
        }
        private static void setEmail(string iv_title, string iv_body, string[] iv_emails)
        {
            SmtpClient client = new SmtpClient();
            client.Port = 587;
            client.Host = "smtp.gmail.com";
            client.EnableSsl = true;
            client.Timeout = 10000;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.UseDefaultCredentials = false;
            client.Credentials = new System.Net.NetworkCredential("fioriokazje@gmail.com", "oiasdjoiasjdoijad");

            MailMessage mm = new MailMessage("donotreply@domain.com", "funtronik@gmail.com", iv_title, iv_body);

            for (int i = 0; i < iv_emails.Length; i++)
            {
                mm.To.Add(iv_emails[i]);
            }

            mm.BodyEncoding = UTF8Encoding.UTF8;
            mm.IsBodyHtml = true;
            mm.DeliveryNotificationOptions = DeliveryNotificationOptions.OnFailure;

            client.Send(mm);
        }
        public void sendErrorOccuredDownloadDetails(List<string> iv_error_list)
        {
            var lv_mail_body = Okazje.Properties.Resources.MailLinksListBody;
            lv_mail_body = lv_mail_body.Replace(@"$LINKS_COUNT$", iv_error_list.Count.ToString());
            var lv_mail_table_body = "";
            foreach (var item in iv_error_list)
            {
                var lv_single_item_body = Okazje.Properties.Resources.MailLinksListSingleItemBody;
                lv_single_item_body.Replace(@"$PRODUCT_LINK$", item);
                lv_mail_table_body += lv_single_item_body;
            }
            lv_mail_body = lv_mail_body.Replace(@"$SINGLE_ITEM$", lv_mail_table_body);
            setEmail("Errors in Details Downloader Method", lv_mail_body, gt_emails);
        }
    }
}
