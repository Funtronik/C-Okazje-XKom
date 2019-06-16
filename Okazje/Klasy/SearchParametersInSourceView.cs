using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Okazje.Klasy
{
    public static class SearchParametersInSourceView
    {
        // & is "
        // % is \\
        public static string gv_alto_initial_hot_shot_section = "window.__INITIAL_STATE__(.*?)USE_SELECTOR_PLZ";
        public static string gt_alto_search_parameters;
        public static string[] ga_search_parameters_special_offer_xkom = new string[] {
                    @"&product-name&>(.*?)<\/",
                    @"data-product-id=&(.*?) ",
                    @"&old-price&>(.*?)<\/",
                    @"&new-price&>(.*?)<\/",
                    @"&discount-value&>(.*?)<\/",
                    @"&sold-info&(.*?)<",
                    @"<div class=&pull-left&>pozostało <span class=&gs-quantity&<strong>(.*?)<\/strong><\/span>",
                    @"<div class=&pull-right&>sprzedano <span class=&gs-quantity&<strong>(.*?)<\/strong><\/span>"};
        public static string[] ga_search_parameters_special_offer_alto = new string[] {
                    @"&promotionName%&:%&(.*?)%&",
                    @"&data-product-id=&(.*?)&",
                    @"&oldPrice%&:(.*?),%",
                    @"&price%&:(.*?),%",
                    @"&discount-value&>(.*?)<\/",
                    @"&sold-info&(.*?)<",
                    @"&promotionTotalCount%&:(.*?),",
                    @"&saleCount%&:(.*?),"};

    }
}
