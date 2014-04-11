using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Model
{
    public class ReqModel
    {
        public string seller { get; set; }
        public string service { set; get; }
        public int? timestamp { get; set; }
        public string user { set; get; }
        public string pass { get; set; }
        public string sign { set; get; }
        public string callback { set; get; }
        public int?    filter_type{ set; get; }
        public string keyword{get;set;}
        public int? page_no{ set; get; }
        public int? page_size{ set; get; }
        public int? type_id{ set; get; }
        public int? sort_type{ set; get; }
        public int? in_stock { set; get; }
        public int? goods_id { set; get; }
        public string goods_ids { set; get; }
        public int? img_type { set; get; }
        public int? member_id { set; get; }
        public string stime { set; get; }
        public string etime { set; get; }
        public string order_note { get; set; }
        public string goods_data { set; get; }
        public int? order_id { set; get; }
        public string delivery_date { get; set; }
        public int? cat_goods_count { get; set; }
    }

    public class GoodData
    {
        public int? goods_id { set; get; }

        /// <summary>
        /// 件數
        /// </summary>
        public int? jcount { set; get; }

        /// <summary>
        /// 散數
        /// </summary>
        public int? scount { set; get; }

        public string note { set; get; }


    }
}