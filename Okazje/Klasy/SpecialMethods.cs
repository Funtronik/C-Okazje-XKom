using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Okazje.Klasy
{
    class SpecialMethods
    {
        public ToolStripLabel lo_ToolStripLabel;
        public ToolStripProgressBar lo_ToolStripProgressBar;
        public ToolStrip lo_ToolStrip;
        public ToolStripLabel lo_ToolSttipLabelAdditional;
        public ToolStripLabel lo_ToolStripLabelErrors;

        public DataTable RemoveDuplicateRows(DataTable table, string DistinctColumn)
        {
            try
            {
                ArrayList UniqueRecords = new ArrayList();
                ArrayList DuplicateRecords = new ArrayList();

                // Check if records is already added to UniqueRecords otherwise,
                // Add the records to DuplicateRecords
                foreach (DataRow dRow in table.Rows)
                {
                    if (UniqueRecords.Contains(dRow[DistinctColumn]))
                        DuplicateRecords.Add(dRow);
                    else
                        UniqueRecords.Add(dRow[DistinctColumn]);
                }

                // Remove duplicate rows from DataTable added to DuplicateRecords
                foreach (DataRow dRow in DuplicateRecords)
                {
                    table.Rows.Remove(dRow);
                }

                // Return the clean DataTable which contains unique records.
                return table;
            }
            catch (Exception ex) { return null; }
        }
        public DataTable addColumns(string[] columns)
        {
            try
            {
                var lt_table = new DataTable();
                foreach (var cell in columns)
                {
                    lt_table.Columns.Add(cell);
                }
                return lt_table;
            }
            catch (Exception ex) { return null; }
        }
        public string getProductParameter(string iv_parameter, string iv_html_code) // "product:"
        {
            try
            { 
                //  @"product:(.*?)\/>"
                Regex rx = new Regex(iv_parameter + @"(.*?)\/>", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(iv_html_code);
                foreach (Match m in matches)
                {
                    return m.Value.ToString();
                }
                return "";
            }
            catch (Exception ex) { return null; }
        }
        public string getProductParameterV2(string iv_parameter, string iv_html_code) // "data-product:"
        {
            try
            {
                //  @"data-product-id=""(.*?)\""";
                Regex rx = new Regex(iv_parameter + @"=""(.*?)\""", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                MatchCollection matches = rx.Matches(iv_html_code);
                foreach (Match m in matches)
                {
                    return m.Value.ToString();
                }
                return "";
            }
            catch (Exception ex) { return null; }
        }
        public void progressBarUpdate(int iv_current_index,int iv_max_index, string iv_item_name)
        {
            lo_ToolStripProgressBar.Visible = true;
            lo_ToolStripLabel.Visible = true;

            float lv_current_percentage = ((iv_current_index+1) * 100) / iv_max_index;
            lo_ToolStripProgressBar.Value = int.Parse(lv_current_percentage.ToString());

            lo_ToolStripLabel.Text = "(" + iv_current_index + " of " + iv_max_index + ") " + iv_item_name + "'s";

            lo_ToolStrip.Update();
        }
        public void progressBarDone()
        {
            lo_ToolStripProgressBar.Value = 0;
            lo_ToolStripProgressBar.Visible = false;
            lo_ToolSttipLabelAdditional.Visible = false;

            lo_ToolStripLabel.Text = "Done";
            lo_ToolSttipLabelAdditional.Text = "";
            lo_ToolStrip.Update();
        }
        public void additionalLabelStatusBar(string iv_value)
        {
            lo_ToolSttipLabelAdditional.Visible = true;
            lo_ToolSttipLabelAdditional.Text = iv_value;
            lo_ToolStrip.Update();
        }

        public void showErrors(int iv_value)
        {
            if (iv_value == 0) return;

            lo_ToolStripLabelErrors.Visible = true;
            lo_ToolStripLabelErrors.Text = iv_value.ToString();
            var lo_image = Image.FromFile(@"C:\Workspace\Okazje\Okazje\Images\error_icon.jpg");
            lo_ToolStripLabelErrors.Image = lo_image;
        }
    }
}
