using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLHelper;

namespace DAL
{
    public  class Handler
    {
        public Handler()
        { 
        }

        public  ReturnModel ValidateSller(string seller)
        {
            try
            {
                if (Common.Tool.filterSql(seller) == 1)
                {
                    return new ReturnModel() { Succeed=false,Message="商家编号有误！"} ;
                    //return new RetObj("商家编号有误！");
                }
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" select * from ( SELECT  [编号]
                              ,p.[PW],p.[DBNAME] ,p.[UID],p.[SERVER]
                              FROM [dbo].[会员表] h
                              join [dbo].[配置表] p on p.hyid=h.id
                              where h.编号='" + seller + "') as a");
                SqlHelper.ConfigConnecStr(true);
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        SqlHelper.DBConnectionString =string.Format( @"server={3};Database={0};
                        Persist Security Info=True;User ID={1};Password={2}", dt.Rows[0]["DBNAME"].ToString().Trim()
                         , dt.Rows[0]["UID"].ToString().Trim(), dt.Rows[0]["PW"].ToString().Trim(),
                         dt.Rows[0]["SERVER"].ToString().Trim());

                        return new ReturnModel() { Succeed = true };
                    }
                    return new ReturnModel() { Succeed = false,Message="未能找到指定商家信息！" };
                }
                else
                {
                    return new ReturnModel() { Succeed = false,Message=retmodel.Message };
                }

            }
            catch (Exception ex)
            {
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
        }

        public  ReturnModel ValidateMemberID(int member_id)
        {
            try
            {
                //if (Common.Tool.filterSql(member_id) == 1)
                //{
                //    return new ReturnModel(){ Succeed=false,Message= "会员编号有误！"};
                //}
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@"SELECT id,[编号],[名称],[密码]
                FROM [dbo].[客户档案]
                with(nolock) where [id]='" + member_id + "'");
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "客户档案");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        return new ReturnModel() { Succeed = true};
                    }
                    return new ReturnModel() { Succeed = false, Message = "会员id不存在！" };
                }
                else
                {
                    return new ReturnModel() { Succeed = false, Message = retmodel.Message };
                }

            }
            catch (Exception ex)
            {
                return new ReturnModel() { Succeed = false, Message = ex.Message };
            }
        }

        /// <summary>
        /// 1会员登录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj member_login(ReqModel data)
        {
            try
            {
                if (Common.Tool.filterSql(data.user) == 1)
                {
                    return new RetObj("会员卡有误！");
                }
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@"select * from (SELECT a.id,[编号],a.[名称],[密码],cfg_allow_oos = isnull(b.数值,0) 
                FROM [dbo].[客户档案] a,[Mall_参数表] b
                with(nolock) WHERE b.名称='禁止库存不足下单' and [编号]='" + data.user + "')as c");
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "c");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        string pwd = dt.Rows[0]["密码"].ToString();
                        int id = Convert.ToInt32(dt.Rows[0]["id"].ToString());
                        string userpwd = Common.Tool.GetMD5_32(id + data.pass).ToUpper();
                        RetObj ret = new RetObj(1, "登录成功！");
                        if (userpwd.Equals(pwd))//Common.Tool.GetMD5_32(id +pwd).ToUpper()
                        {
                            ret.RetDics["m_id"] = id;
                            ret.RetDics["m_name"] = dt.Rows[0]["名称"].ToString().Trim();
                            ret.RetDics["m_num"] = dt.Rows[0]["编号"].ToString().Trim();
                            ret.RetDics["cfg_allow_oos"] = Convert.ToInt32(dt.Rows[0]["cfg_allow_oos"]);
                        }
                        else
                        {
                            ret.RetDics["state"] = false;
                            ret.RetDics["info"] = "密码有误！";
                         }
                        return ret;
                    }
                    return new RetObj( "会员不存在！");
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        /// 2管理员登录
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj admin_login(ReqModel data)
        {
            try
            {
                if (Common.Tool.filterSql(data.user) == 1)
                {
                //    dynamic model2 = new JObject();
                //    model2.state = false;
                //    model2.info = "帐号有误！";
                //    return model2;
                    return new RetObj("帐号有误！");
                }
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" select id,[姓名],[密码] from dbo.操作员档案 with(nolock)
                where [编号]='" + data.user + "'");
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "操作员档案");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        string pwd = dt.Rows[0]["密码"].ToString();
                        int id = Convert.ToInt32(dt.Rows[0]["id"].ToString());
                        string userpwd = Common.Tool.GetMD5_32(id + data.pass).ToUpper();
                        RetObj model = new RetObj(1,"登录成功！");
                        //dynamic model = new JObject(); Common.Tool.GetMD5_32(id + pwd).ToUpper()
                        if (userpwd.Equals(pwd))
                        {
                            model.RetDics.Add("a_id",dt.Rows[0]["id"].ToString().Trim());
                            model.RetDics.Add("a_name", dt.Rows[0]["姓名"].ToString().Trim());
                        }
                        else
                        {
                            model.RetDics["state"] = false;
                            model.RetDics["info"] = "密码有误！";
                        }
                        return model;
                    }
                 
                    return new RetObj("帐号不存在！");
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        /// 3获取商品分类信息
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_goods_category(ReqModel data)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" SELECT c_id = [id]
                                  ,c_title = [名称]
                                  ,c_level = [级别]
                                  ,c_pid = (select id from [商品类型] where 编号=a.[父编号])
                              FROM [dbo].[商品类型] a with(nolock) ");
                #region 
         
                #endregion 
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "商品类型");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        RetObj model = new RetObj(1, "获取成功！");

                        //StringBuilder sb = new StringBuilder();
                        //sb.Append("[");
                        List<Dictionary<string, object>> categorys = new List<Dictionary<string, object>>();
                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("c_id", row["c_id"].ToString().Trim());
                            dic.Add("c_title", row["c_title"].ToString().Trim());
                            dic.Add("c_pid", row["c_pid"].ToString().Trim());
                            dic.Add("c_level", row["c_level"].ToString().Trim());
                            categorys.Add(dic);
                        }
                    
                       // string tmp = sb.ToString().Substring(0, sb.ToString().Length - 1);
                        model.RetDics.Add("categorys" , categorys);
                          
                        return model;
                    }

                    return new RetObj("商品分类无数据！");
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        /// 4获取商品列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_goods_list(ReqModel data)
        {
            try
            {
                if (data.page_size == null)
                {
                    return new RetObj("缺少 page_size 参数 ！");
                }

           
                if (data.page_size == 0)
                {
                    return new RetObj("page_size 不能为0 ！");
                }


                //如果是有畅销类型 或有过滤内容传来就不过滤分类了

                StringBuilder searchSql = new StringBuilder();
                StringBuilder strCount = new StringBuilder();
            
                    searchSql.AppendFormat(@"declare @login int,@mid int set @mid = {0}
                                            select @login = (case when @mid<=0 then 0 else 1 end) ",
                                           data.member_id??0);

                    strCount.AppendFormat(@"declare @mid int set @mid = {0} ",
                                           data.member_id??0);
                
                data.filter_type = data.filter_type ?? 0;
                if (string.IsNullOrEmpty(data.keyword) && data.filter_type == 0)
                {
                    searchSql.AppendFormat(@"declare @分类编号 nvarchar(10)
                                            select top 1 @分类编号=isnull(编号,'') from 商品类型 where id ={0} "
                                                           , data.type_id);
                    strCount.AppendFormat(@"declare @分类编号 nvarchar(10)
                                            select top 1 @分类编号=isnull(编号,'') from 商品类型 where id ={0} ",
                                    data.type_id);

                }
             
                searchSql.AppendFormat(@"  SELECT TOP {0} * from ( select
                                        g_id = p.id
                                        ,g_img_ts = cast(p.updateno as bigint)  
                                        ,p.编号 ", data.page_size);
                if (data.member_id > 0)
                {
                    searchSql.Append(@"  , g_quantity = (case when ISNULL((SELECT  数值
                                            FROM    [dbo].[Mall_参数表]
                                             WHERE 名称='禁止库存不足下单'),0)=1 THEN kc.数量 ELSE 0 END )
                                          ,g_price =  dbo.getsj(p.id,@mid,0,@login) 
                                          ,g_bulk_price =  dbo.getsj(p.id,@mid,1,@login )");
                }
                else
                {
                    searchSql.Append(@"  ,g_quantity =0  ,g_price = 0 ,g_bulk_price =0 ");
                }                                             
                searchSql.Append(@" ,g_num = 条码
                                    ,g_name = p.名称 
                                    ,g_spec = 包装规格                     
                                    ,p.零售价
                                    ,销量 = (select top 1 件数  
                                        from Mall_销量排行  where spid = p.id)  
                                    ,行号 = row_number()OVER (ORDER BY p.ID DESC)
                                    ,p.ID FROM 商品档案 p  with(nolock) ");

                strCount.Append(@"  select count(1) from dbo.商品档案 p with(nolock) ");
                if (data.member_id > 0)
                {
                    searchSql.AppendFormat(@"  left join (select spid, x.khid,数量
                                          from 库存表 y inner join mall_会员仓库 x 
                                         on y.仓库id = x.仓库id where x.khid ={0} 
                                         ) as kc on p.id =kc.spid  ",data.member_id);
                    strCount.AppendFormat(@"     left join (select spid, x.khid,数量
                                          from 库存表 y inner join mall_会员仓库 x 
                                         on y.仓库id = x.仓库id where x.khid ={0} 
                                         ) as k on p.id =k.spid ",data.member_id);
//                    searchSql.Append(@"  left join 库存表 k on p.id =k.spid    
//                                        join dbo.Mall_会员仓库 c on c.仓库id = k.仓库id ");
//                    strCount.Append(@"     left join 库存表 k on p.id =k.spid    
//                                join dbo.Mall_会员仓库 c on c.仓库id = k.仓库id  ");
                }
               
              
                if (data.filter_type > 0)
                {
                    searchSql.AppendFormat(@" left join dbo.Mall_畅销关联表 u on u.spid = p.id   
                                        left join dbo.Mall_畅销类型 h on h.id = u.类型ID  ");
                    strCount.Append(@" left join dbo.Mall_畅销关联表 u on u.spid = p.id   
                                left join dbo.Mall_畅销类型 h on h.id = u.类型ID  ");
                }

                string ban = @" and exists(select 0 from 商品类型 lx with (nolock)
                             where p.编号 like ltrim(rtrim(p.编号))+'%' 
                              and not exists(select 0 from 操作员权限 with (nolock)
                             where 标志=1 and 类别ID=lx.ID and 用户ID in (0,@mid)))
                            and not exists(select 0 from 操作员权限 with (nolock)
                            where 标志=1 and 档案ID=p.ID and 用户ID in (0,@mid)) ";

                searchSql.Append( @"  where p.停用 = 0 "+ ban );
                strCount.Append(@"  where p.停用 = 0 " + ban );

                //order by p.id 
                if (string.IsNullOrEmpty(data.keyword) && data.filter_type == 0)
                {
                    searchSql.AppendFormat(@" and p.编号 like rtrim(@分类编号)+'%' collate Chinese_PRC_CI_AI ");
                    strCount.AppendFormat(@" and p.编号 like rtrim(@分类编号)+'%' collate Chinese_PRC_CI_AI ");
                }

                //keyword 
                if (!string.IsNullOrEmpty(data.keyword))
                {
                    List<string> list = data.keyword.Split(' ', '+').ToList();
                    searchSql.Append(" and (");
                    strCount.Append(" and (");
                    int index = 0;
                    foreach (var item in list)
                    {
                        if (index == 0)
                        {
                            searchSql.Append(@" (p.名称+p.拼音+ p.条码 + p.辅助条码 + p.型号规格 + p.产地) like '%" + item + "%' collate Chinese_PRC_CI_AI ");
                            strCount.Append(@" (p.名称+p.拼音+ p.条码 + p.辅助条码 + p.型号规格 + p.产地) like '%" + item + "%' collate Chinese_PRC_CI_AI ");
                            break;
                        }
                        searchSql.Append(@" and  (p.名称+p.拼音+ p.条码 + p.辅助条码 + p.型号规格 + p.产地) like '%" + item + "%' collate Chinese_PRC_CI_AI ");
                        strCount.Append(@" and  (p.名称+p.拼音+ p.条码 + p.辅助条码 + p.型号规格 + p.产地) like '%" + item + "%' collate Chinese_PRC_CI_AI ");

                   
                        index++;
                    }
                     searchSql.Append(" ) ");
                     strCount.Append(" ) ");
                }
              
                //仅显示有库存数量
                if (data.in_stock == 1)
                {
                    searchSql.Append(@" and  k.数量 > 0 ");
                    strCount.Append(@" and  k.数量 > 0 ");
                }
              
                if (data.filter_type > 0)
                {
                    searchSql.AppendFormat(" and h.id ={0} ", data.filter_type);
                    strCount.AppendFormat(" and h.id ={0} ", data.filter_type);
                }

                searchSql.AppendFormat(" )as x where 行号 between {0}  and {1} ",
                    data.page_size * (data.page_no - 1)+1, data.page_size * data.page_no);

                //0-销量是倒序 
                //1 销量是升序
                //2 价格是升序
                //3是价格倒序
                if (data.sort_type == null)
                {
                    searchSql.AppendFormat("order by 编号 ");
                }

                //按销量降序
                if (data.sort_type == 0)
                {
                    searchSql.Append(@" order by 销量 desc ");
                }
                //按销量升序
                if (data.sort_type == 1)
                {
                    searchSql.Append(@" order by 销量 asc ");
                }
                //按价格升序
                if (data.sort_type == 2)
                {
                  searchSql.Append(@" order by  零售价 asc ");
                }
                //按价格降序
                if (data.sort_type == 3)
                {
                    searchSql.Append(@" order by 零售价 desc ");
                }
           
                ReturnModel retmodel = SqlHelper.GetDataTable(searchSql.ToString(), "a");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        ReturnModel rm = SqlHelper.ExecuteScalarByString(strCount.ToString(),null);
                        if (rm.Succeed == false) { return new RetObj(rm.Message); }

                        RetObj model = new RetObj(1, "获取成功！");
                        //符合条件的总数
                        model.RetDics.Add("r_count",Convert.ToInt32(rm.SearchResult));
                      
                        List<Dictionary<string, object>> goods = new List<Dictionary<string, object>>();
                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("g_id", row["g_id"]);
                            dic.Add("g_img_ts", row["g_img_ts"]);
                            dic.Add("g_quantity", row["g_quantity"]);
                            dic.Add("g_num", row["g_num"].ToString().Trim());
                            dic.Add("g_name", row["g_name"].ToString().Trim());
                            dic.Add("g_spec", row["g_spec"]);
                            dic.Add("g_price", row["g_price"]);
                            dic.Add("g_bulk_price", row["g_bulk_price"]);
                            goods.Add(dic);
                        }
                    
                        model.RetDics.Add("goods",goods);
                   
                        return model;
                    }
                    RetObj rmt = new RetObj("结果无数据！");
                    rmt.RetDics.Add("r_count",0);
                    return rmt;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        /// 5 商品详细信息（get_goods_info）
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_goods_info(ReqModel data)
        {
            try
            {
                //if (Common.Tool.filterSql(data.user) == 1)
                //{
                //    return new RetObj("帐号有误！");
                //}
                List<int> ids = JsonConvert.DeserializeObject<List<int>>(data.goods_ids);
                string list = "( ";
                foreach (var item in ids)
                {
                    list += item + ",";
                }
                list = list.Substring(0, list.Length - 1);
                list += ")";

                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" declare @login int select @login = (case when {0}<=0 then 0 else 1 end)
                                 select * from ( select top 100 percent g_id = p.id ,g_img_ts = cast(updateno as bigint) ,",
                                  data.member_id == null ? 0 : data.member_id);
                if(data.member_id>0)
                {
                  strSql.Append(@"  g_quantity = (case when ISNULL((SELECT  数值  FROM    [dbo].[Mall_参数表]
                                      WHERE 名称='禁止库存不足下单'),0)=1 THEN k.数量 ELSE 0 END ) ,");
                }
                else
                {
                    strSql.Append(" g_quantity = 0, ");
                }

                strSql.AppendFormat( @" g_num = 条码,
                                        g_name = p.名称,
                                        g_spec = 包装规格 ,
                                        g_price =  dbo.getsj(p.id,{0},0,@login),
                                        g_bulk_price =  dbo.getsj(p.id,{0},1,@login)  
                                        from  dbo.商品档案 p with(nolock) ",
                                        data.member_id==null?0:data.member_id);

                if (data.member_id > 0)
                {
                    strSql.Append(@"  left join 库存表 k on p.id =k.spid    
                                      join dbo.Mall_会员仓库 c on c.仓库id = k.仓库id ");
                }
                if (data.member_id > 0)
                {
                    strSql.AppendFormat(" where p.停用=0 and c.khid={0} and p.id in {1} ) as a  ",data.member_id, list);

                }
                else
                {
                    strSql.AppendFormat(" where p.停用=0 and p.id in {0} ) as a  ", list);
                }

                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        RetObj model = new RetObj(1, "获取成功！");
                      
                        StringBuilder sb = new StringBuilder();
                          List<Dictionary<string, object>> goods = new List<Dictionary<string, object>>();
                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, object> dic = new Dictionary<string,object>();
                            dic.Add("g_id", row["g_id"]);
                            dic.Add("g_img_ts", row["g_img_ts"]);
                            dic.Add("g_quantity", row["g_quantity"]);
                            dic.Add("g_num", row["g_num"].ToString().Trim());
                            dic.Add("g_name", row["g_name"].ToString().Trim());
                            dic.Add("g_spec", row["g_spec"]);
                            dic.Add("g_price", row["g_price"]);
                            dic.Add("g_bulk_price", row["g_bulk_price"]);
                            dic.Add("g_info:", row["g_name"].ToString().Trim());
                            goods.Add(dic);

                        }

                        model.RetDics.Add("goods", goods);
                        return model;
                    }
                    RetObj rmt = new RetObj("结果无数据！");
                    rmt.RetDics.Add("r_count", 0);
                    return rmt;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        /// 6 商品图片更新接口
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_goods_image(ReqModel data)
        {
            try
            {
                //if (Common.Tool.filterSql(data.user) == 1)
                //{
                //    return new RetObj("帐号有误！");
                //}

                List<int> ids = JsonConvert.DeserializeObject<List<int>>(data.goods_ids);
                string list = "( ";
                foreach (var item in ids)
                {
                    list += item+",";
                }
                list = list.Substring(0,list.Length -1);
                list += ")";

                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" select   商品档案.id as g_id,cast(updateno as bigint) as  g_img_ts,
                                 图片 as g_data from 商品档案 with(nolock) where 商品档案.id  in " + list +" order by id asc");
               
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");
                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                  
                    if (dt.Rows.Count > 0)
                    {
                        RetObj model = new RetObj(1, "获取成功！");
                        List<Dictionary<string, object>> images = new List<Dictionary<string, object>>();
                        //StringBuilder sb = new StringBuilder();
                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, object> dics = new Dictionary<string, object>();
                            if (string.IsNullOrEmpty(row["g_data"].ToString()))
                            {
                                dics.Add("g_data", "");
                            }
                            else
                            {
                                byte[] imgdata = (byte[])row["g_data"];
                                dics.Add("g_data", Convert.ToBase64String(imgdata));
                                //获取缩略图
                                //if (data.img_type == 0)
                                //{
                                //    dics.Add("g_data", Common.Tool.GetMicroImage(imgdata, 64, 64));
                                //}
                                //else
                                //{
                                //}
                            }
                            dics.Add("g_img_ts", row["g_img_ts"]);
                            dics.Add("g_id", row["g_id"].ToString().Trim());
                            images.Add(dics);
                        }
                        model.RetDics.Add("images", images);
                        return model;
                    }
                    RetObj rmt = new RetObj("结果无数据！");
             
                    return rmt;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        ///  7 会员卡余额查询
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_card_info(ReqModel data)
        {
            try
            {
                if (data.member_id==null)
                {
                    return new RetObj("会员id不能为空！");
                }
                ReturnModel rm = ValidateMemberID((int)data.member_id);
                if (!rm.Succeed)
                {
                    return new RetObj(rm.Message);
                }
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" select isnull(a.金额,0) as balance from ( 
                                select sum(case when 标志='|应收账款|'
                                 then isnull(ye.期末余额,0)*-1 else 
                                 isnull(ye.期末余额,0) end ) as 金额
                                from (select 子表id,科目id ,期末余额 from 
                                 会计科目余额 with (nolock) where 子表id ="+ data.member_id + @") as ye 
                                inner join 会计科目 as km with (nolock) on ye.科目id =km.id
                                where  标志='|应收账款|' or 标志='|预收账款|'
                                ) as a  ");

           
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");
           
                if (retmodel.Succeed)
                {
                
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        RetObj model = new RetObj(1, "获取成功！");
                        model.RetDics.Add("m_balance", Convert.ToInt32(dt.Rows[0]["balance"]));
                        return model;
                    }
                    RetObj ret = new RetObj(0, "获取失败！");
                    return ret;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        ///  8 会员卡日志
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_card_log(ReqModel data)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" ");

                ReturnModel retmodel = SqlHelper.GetDataTableByProc("getMemberLog", "x",
                    new List<SqlParameter>() {
                        new SqlParameter("@sdate ", data.stime),
                        new SqlParameter("@edate ",data.etime),
                        new SqlParameter("@pagesize ",data.page_size),
                        new SqlParameter("@pageindex ", data.page_no),
                        new SqlParameter("@nKHID ", data.member_id??0)
                    },
                    new List<SqlParameter>() { new SqlParameter("@r_count ", SqlDbType.Int) });

                if (retmodel.Succeed)
                {
                    RetObj model = new RetObj(1,"获取成功！");
                    Dictionary<string ,object > outValue = retmodel.ReturnValue as Dictionary<string,object>;
                    if (outValue != null)
                    {
                        model.RetDics.Add("r_count", outValue["r_count"]);
                    }
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        List<Dictionary<string, object>> logs = new List<Dictionary<string, object>>();
                        foreach (DataRow item in dt.Rows)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("l_type", item["类型"].ToString().Trim());
                            dic.Add("l_time", item["日期"].ToString().Trim());
                            dic.Add("l_order_num", item["单号"].ToString().Trim());
                            dic.Add("l_order_price", item["出货金额"].ToString().Trim());
                            dic.Add("l_money_paid", item["付款金额"].ToString().Trim());
                            dic.Add("l_balance", item["余额"].ToString().Trim());
                            logs.Add(dic);
                        }
                        //logs
                        model.RetDics.Add("logs",logs);
                        return model;
                    }
                    RetObj ret = new RetObj(0, "获取失败！");
                    return ret;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        ///  9 下单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj place_an_order(ReqModel data)
        {
            try
            {
                if (data.goods_data == null)
                {
                    return new RetObj("商品明细不能为空！");
                }
                if (data.member_id==null)
                {
                    return new RetObj("会员id不能为空！");
                }
                ReturnModel rm = ValidateMemberID((int)data.member_id);
                if(!rm.Succeed)
                {
                    return new RetObj(rm.Message);
                }

                #region 验证商品id


                if (string.IsNullOrEmpty(data.goods_data))
                {
                    return new RetObj("商品明细不能为空！");
                }

                string proids = "(";

                List<GoodData> goods = JsonConvert.DeserializeObject<List<GoodData>>(data.goods_data);
                if (goods == null)
                {
                    return new RetObj("商品明细不能为空！");
                }
                if (goods.Count == 0)
                {
                    return new RetObj("商品明细不能为空！");
                }
                foreach (var item in goods)
                {
                    proids += item.goods_id + ",";
                }
                proids = proids.Substring(0,proids.Length-1)+")";



                string valProid = @" select count(1) from 商品档案 with(nolock) 
                                     where id in " + proids;
                ReturnModel ret = SqlHelper.ExecuteScalarByString(valProid, null);
                if (ret.Succeed)
                {
                    int count = Convert.ToInt32(ret.SearchResult);
                    if (goods.Count != count)
                    {
                        return new RetObj("部分商品id不存在，保存失败！");
                    }
                }
                else
                {
                    return new RetObj(ret.Message);
                }

                #endregion 

                #region 验证数量

                var valcount = from a in goods
                               where a.jcount < 0 || a.scount<0
                               select a;
                if (valcount.Count() > 0)
                {
                    return new RetObj("商品id为："+valcount.First().goods_id+" 的件数或散数不能为负数！");
                }
               
                #endregion 

                #region 将商品名称及其数量插入临时表
                

                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" declare @errid int  begin try if exists (select * from tempdb.dbo.sysobjects where id = object_id(N'tempdb..#tmpf')
                                        and type='U') begin  delete from #tmpf  end else  begin  create table #tmpf ( pid int, jcount numeric(18,2), scount numeric(18,2),
                                        jprice numeric(18,5), sprice numeric(18,5),stand numeric(18,2),note nvarchar(500)) end
                                        insert into #tmpf(pid,jcount,scount,jprice,sprice,stand,note)");
                foreach (var item in goods)
                {
                    strSql.AppendFormat(" select {0},{1},{2},dbo.getsj({0},{3},0,1),dbo.getsj({0},{3},1,1),isnull((select isnull(包装规格,0) from 商品档案 where id={0}),0),'{4}' union"
                                        , item.goods_id, item.jcount, item.scount, data.member_id??0, item.note);
                }

                string tmp = strSql.ToString().Substring(0, strSql.ToString().Length - 5);
                strSql.Clear();
                strSql.Append(tmp);
                #endregion

                #region  检测是否允许缺货下单

                strSql.AppendFormat(@" DECLARE @count int
                                SELECT @count=ISNULL(COUNT(1),0)
                                FROM    ( SELECT    id ,名称
                                FROM      商品档案 a WITH ( NOLOCK )
                                INNER JOIN #tmpf t ON a.id = t.pid
                                WHERE     ( t.jcount * t.stand + t.scount ) > ISNULL(( SELECT
                                数量 FROM 库存表 k
                                join dbo.Mall_会员仓库 c ON c.仓库id = k.仓库id
                                WHERE k.spid = t.pid AND c.khid = {0}), 0)) AS x
                                IF ((ISNULL((SELECT  数值  FROM    [dbo].[Mall_参数表]
                                WHERE 名称='禁止库存不足下单'),0)=1) AND (@count>0))
                                BEGIN   SET @errid = -3  RAISERROR  ('禁止库存不足下单！', 16, 1 )  END  ",data.member_id??0);

                #endregion 

                #region  生存订单

                //将数据插入订单主表  
                strSql.AppendFormat(@"  declare @c单号 nvarchar(100) declare @date nvarchar(100) set  @date = cast(getdate()as nvarchar(100))
                                exec P最大单号 '网络订单',@date, @c单号 out if @c单号 is null begin  set @errid=-1  RAISERROR  ('单号生存有误！', 16, 1 )  end
                                if (select count(1) from 客户档案 where id= {0})=0   begin   set @errid=-2 RAISERROR  ('会员id不存在！', 16, 1 ) end 
                                INSERT INTO [dbo].[Mall_订单主表]([单号],[日期],[KHID],[总数量],[总金额] ,[备注],[仓库ID],[送货时间])select  @c单号,getdate(),
                                {0},isnull((select  sum(jcount*stand+scount) from #tmpf),0),isnull((select  sum(jcount*isnull(jprice,0)+
                                scount*isnull(sprice,0)) from #tmpf),0),'{1}',{0},{2}  INSERT INTO dbo.Mall_订单流程明细 ( 父id, 订单id, 日期, 经手人id )
                                 VALUES  ( (SELECT TOP 1 id FROM dbo.Mall_订单流程 WHERE 编号='10'),(select top 1 id from [Mall_订单主表] where 单号= @c单号), GETDATE(),0 ) ",
                               data.member_id ?? 0, data.order_note??"''", data.delivery_date == null ? "null" : "'" + data.delivery_date + "'");
              
                //插入明细
                strSql.Append(@" declare @oid int  select @oid=max(id) from [dbo].[Mall_订单主表]
                                INSERT INTO [dbo].[Mall_订单明细表]([父id],[spid],[数量],[散价],[件价] ,[折扣] ,[金额],[转换率],[备注])  ");
               
                strSql.AppendFormat(@" select @oid,pid,(jcount*stand + scount),jprice,sprice,0, isnull((jcount*isnull(jprice,0) + scount*isnull(sprice,0)),0),stand,note from #tmpf with(nolock) ");
                strSql.Append(@" if  @@error>0 begin declare @err int set @err='ErrCode : '+ @@error set @errid=@@error RAISERROR(@err,  16,  1 )  end   if exists (select * from tempdb.dbo.sysobjects                                      
                                 where id = object_id(N'tempdb..#tmpf') and type='U') begin  drop table #tmpf   end   ");

                //插入成功后返回库存不足的商品信息
                strSql.AppendFormat(@"  end try  begin catch  select * from  (select @errid as col) as result    print ERROR_MESSAGE()  end catch ");

                #endregion 

                #region  使用事务提交数据 

                SqlConnection conn = new SqlConnection(SqlHelper.DBConnectionString);
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
                SqlTransaction trans = conn.BeginTransaction();
                SqlCommand cmd = new SqlCommand(strSql.ToString(), conn, trans);
                cmd.CommandType = CommandType.Text;

                SqlDataAdapter da = new SqlDataAdapter(cmd);
                DataTable resultdt = new DataTable("result");
                da.Fill(resultdt);
          
              
               // ReturnModel retmodel = SqlHelper.ExecCmdByTranc(trans,strSql.ToString(), "x");

                //if (retmodel.Succeed)
                //{
                //    DataTable dt = retmodel.SearchResult as DataTable;
                try
                {
                    if (resultdt.Rows.Count > 0)
                    {
                       
                        int value = Convert.ToInt32(resultdt.Rows[0]["col"]);
                        if (value == -1)
                        {
                            return new RetObj("单号生成有误！");

                        }
                        if (value == -2)
                        {
                            return new RetObj("客户不存在！");
                        }
                        if (value == -3)
                        {
                            StringBuilder getresult = new StringBuilder();
                            getresult.AppendFormat( @" SELECT  *  FROM ( SELECT    g_id=id ,g_name=名称,g_quantity=kc.数量                                        
                                        FROM 商品档案 a WITH ( NOLOCK )      
                                        INNER JOIN (SELECT  数量,spid FROM 库存表 k                                        
                                        INNER JOIN dbo.Mall_会员仓库 c ON c.仓库id = k.仓库id                                        
                                        WHERE  c.khid = {0}) AS kc ON kc.spid = a.id                                        
                                        WHERE kc.数量< ISNULL(( SELECT ( t.jcount * t.stand + t.scount) FROM #tmpf as t                                        
                                        WHERE  t.pid = a.id ) ,0) ) AS pro   if exists (select * from tempdb.dbo.sysobjects
                                        where id = object_id(N'tempdb..#tmpf') and type='U') begin  drop table #tmpf   end
                                       ", data.member_id ?? 0);
                            cmd.CommandText = getresult.ToString();
                            //SqlDataAdapter da = new SqlDataAdapter(cmd);
                            DataTable prodt = new DataTable("pro");
                            da.Fill(prodt);
                            List<Dictionary<string, object>> oos = new List<Dictionary<string, object>>();
                            foreach (DataRow row in prodt.Rows)
                            {
                                Dictionary<string, object> dic = new Dictionary<string, object>();
                                dic.Add("g_id", row["g_id"].ToString().Trim());
                                dic.Add("g_name", row["g_name"].ToString().Trim());
                                dic.Add("g_quantity", Convert.ToInt32(row["g_quantity"]));
                                oos.Add(dic);
                            }

                            RetObj model = new RetObj(0, "库存不足！");
                            model.RetDics.Add("oos_goods", oos);
                            trans.Rollback();
                            return model;
                        }
                  
                        return new RetObj("订单保存失败，错误码: " + value + " ！");
                    }
                    else
                    {
                        //trans.Rollback();
                        //return new RetObj("订单保存失败！");
                        RetObj model = new RetObj(1, "订单保存成功！");
                       
                        trans.Commit();
                        return model;
                    }
                }
                catch (Exception ex2)
                {
                    trans.Rollback();
                    return new RetObj(ex2.Message);
                }
                finally
                {
                    conn.Close();
                }

                #endregion 

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }

        }

        /// <summary>
        ///  10 订单列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_order_list(ReqModel data)
        {
            try
            {
                if (data.page_size == null)
                {
                    return new RetObj(" page_size参数不能为空！");
                }
              
                if (string.IsNullOrEmpty(data.stime))
                {
                    return new RetObj(" stime参数不能为空！");
                }
                if (string.IsNullOrEmpty(data.etime))
                {
                    return new RetObj(" etime参数不能为空！");
                }
                
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" DECLARE	@stime NVARCHAR(30),@etime NVARCHAR(30)
                                        SET @stime='{2}' SET @etime='{1}'
                                        IF @stime=@etime
                                        BEGIN 
                                           SET @etime=convert(nvarchar(30),DATEADD(DAY,1,@etime),120)
                                        END  select top {0} * from( select 
                                        行号 = row_number()OVER (ORDER BY a.ID DESC),
                                        o_id = a.id,
                                        o_num = 单号 ,
                                        o_status = b.名称,
                                        o_note = 备注 ,
                                        o_total = 总金额 ,
                                        o_time = a.日期,
                                        o_audit_time = c.日期  from  [Mall_订单主表] a
                                        inner join dbo.Mall_订单流程明细 c on a.id = c.订单Id 
                                        inner join [dbo].[Mall_订单流程] b on  c.父id = b.id 
                                        where a.日期<=@etime and a.日期>=@stime
                                        )as x where  行号 between {3}  and {4} ",
                               data.page_size , data.etime, data.stime, data.page_size*(data.page_no-1)+1, data.page_size*data.page_no);

                StringBuilder sqlCount = new StringBuilder();
                sqlCount.AppendFormat(@" select c from( select c=count(1) from  [Mall_订单主表] a
                                        inner join dbo.Mall_订单流程明细 c on a.id = c.订单Id 
                                        inner join [dbo].[Mall_订单流程] b on  c.父id = b.id 
                                        where a.日期<='{0}' and a.日期>='{1}') as x ", data.etime, data.stime);
                //r_count
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "x");

                if (retmodel.Succeed)
                {
                    RetObj model = new RetObj(1, "获取成功！");
                    ReturnModel retCount = SqlHelper.GetDataTable(sqlCount.ToString(), "x");
                    if (retCount.Succeed)
                    {
                        DataTable dt2 = retCount.SearchResult as DataTable;
                        if (dt2.Rows.Count > 0)
                        {
                            model.RetDics.Add("r_count", dt2.Rows[0]["c"].ToString().Trim());
                        }
                    }
                    else
                    {
                        return new RetObj(retCount.Message);
                    }
                       
                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        List<Dictionary<string, object>> orders = new List<Dictionary<string, object>>();
                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("o_id", row["o_id"].ToString().Trim());
                            dic.Add("o_num", row["o_num"].ToString().Trim());
                            dic.Add("o_status", row["o_status"].ToString().Trim());
                            dic.Add("o_note", row["o_note"].ToString().Trim());
                            dic.Add("o_time", row["o_time"].ToString().Trim());
                            dic.Add("o_audit_time", row["o_audit_time"].ToString().Trim());
                            dic.Add("o_total", row["o_total"].ToString().Trim());
                            orders.Add(dic);
                        }
                        model.RetDics.Add("orders",orders);
                          
                        return model;
                    }
                    RetObj ret = new RetObj(0, "获取失败！");
                    return ret;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        ///  11 订单明细
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_order_info(ReqModel data)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" select * from ( select a.id as o_id,单号 as o_num ,
                                (select top 1 父id from dbo.Mall_订单流程明细  
                                where 订单Id =a.id  order by id desc  ) as o_status,
                                a.备注 o_note,总金额 o_total,a.日期 as o_time,
                                (select 日期 from dbo.Mall_订单流程明细 m 
                                inner join [dbo].[Mall_订单流程] x on  m.父id = x.id 
                                where m.订单Id =a.id  and x.名称 ='审核订单' ) 
                                o_audit_time, (select 日期 from dbo.Mall_订单流程明细 m 
                                inner join [dbo].[Mall_订单流程] x on  m.父id = x.id 
                                where m.订单Id =a.id  and x.名称 ='已收货' ) 
                                o_receipt_time, 送货时间 o_shipping_time,
                                d.id as g_id,s.名称 as g_name,isnull(floor(d.数量/d.转换率),0) as g_amount,
                                d.数量%d.转换率 as g_bulk_amount from  [Mall_订单主表] a 
                                inner join dbo.Mall_订单明细表 d on d.父id = a.id      
                                inner join 商品档案 s on s.id = d.spid  where a.id={0} )as n ",data.order_id);


                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "n");

                if (retmodel.Succeed)
                {

                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        RetObj model = new RetObj(1, "获取成功！");

                        model.RetDics["o_id"] = dt.Rows[0]["o_id"].ToString().Trim();
                        model.RetDics["o_num"] = dt.Rows[0]["o_num"].ToString().Trim() ;
                        model.RetDics["o_status"]= dt.Rows[0]["o_status"].ToString().Trim();
                        model.RetDics["o_note"]= dt.Rows[0]["o_note"].ToString().Trim() ;
                        model.RetDics["o_time"]=dt.Rows[0]["o_time"].ToString().Trim() ;
                        model.RetDics["o_audit_time"]= dt.Rows[0]["o_audit_time"].ToString().Trim();
                        model.RetDics["o_shipping_time"]= dt.Rows[0]["o_shipping_time"].ToString().Trim();
                        model.RetDics["o_receipt_time"] = dt.Rows[0]["o_receipt_time"].ToString().Trim();
                        model.RetDics["o_total"]= dt.Rows[0]["o_total"].ToString().Trim();

                        List<Dictionary<string, object>> o_goods = new List<Dictionary<string, object>>();
                        foreach (DataRow row in dt.Rows)
                        {
                            Dictionary<string, object> dic = new Dictionary<string, object>();
                            dic.Add("g_id", row["g_id"].ToString().Trim());
                            dic.Add("g_name", row["g_name"].ToString().Trim());
                            dic.Add("g_amount", row["g_amount"].ToString().Trim());
                            dic.Add("g_bulk_amount", row["g_bulk_amount"].ToString().Trim());
                            o_goods.Add(dic);
                        }
                        model.RetDics.Add("o_goods", o_goods);
                        return model;
                    }
                    RetObj ret = new RetObj(0, "获取失败！");
                    return ret;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        ///  12 订单日志
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj get_order_log(ReqModel data)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@"  ");


                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");

                if (retmodel.Succeed)
                {

                    DataTable dt = retmodel.SearchResult as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        RetObj model = new RetObj(1, "获取成功！");
                        model.RetDics.Add("m_balance", Convert.ToInt32(dt.Rows[0]["balance"]));
                        return model;
                    }
                    RetObj ret = new RetObj(0, "获取失败！");
                    return ret;
                }
                else
                {
                    return new RetObj(retmodel.Message);
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        ///  13 取消订单
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj cancel_order(ReqModel data)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat(@" select c=count(1)  from  dbo.Mall_订单主表 a where a.id ={0}",data.order_id);


                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");

                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;

                    strSql.Clear();
                    strSql.AppendFormat(@" insert into  dbo.Mall_订单流程明细(父id,订单id,日期,
                                       经手人)select (select id from Mall_订单流程 where 编号='15' and 有效=1),
                                       {0},getdate(),(select khid from dbo.Mall_订单主表 where id= {0}) where exists(
                                         select id from Mall_订单流程 where  编号='15' and 有效=1  )  ", data.order_id);

                    ReturnModel ret = SqlHelper.ExecuteNonQuery(strSql.ToString(),null);
                    if (ret.Succeed)
                    {
                        int count = Convert.ToInt32(ret.ReturnValue);
                        if (count == 0)
                        {
                            return new RetObj("订单流程编号有误！");
                        }
                        return new RetObj("订单取消成功！");
                    }
                    //if (dt.Rows.Count > 0)
                    //{
                    //    RetObj model = new RetObj(1, "获取成功！");
                    //    model.RetDics.Add("m_balance", Convert.ToInt32(dt.Rows[0]["balance"]));
                    //    return model;
                    //}
                    
                    return  new RetObj(ret.Message);
                }
                else
                {
                    return new RetObj("订单不存在！");
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

        /// <summary>
        /// 14 商品推荐列表
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public  RetObj goods_recommend_list(ReqModel data)
        {
            try
            {
                StringBuilder strSql = new StringBuilder();
                strSql.Append(@" select * from ( SELECT 
                                g_cat_id =c.id ,
                                g_cat_name = c.名称,
                                g_id = p.id,
                                g_img_ts= p.updateno,
                                g_name = p.名称
                                FROM dbo.商品档案 p
                                LEFT JOIN dbo.商品类型 c ON p.类型编号 = c.编号
                                join dbo.Mall_首页项目明细 a ON a.spid	=p.id
                                JOIN dbo.Mall_首页项目 b ON a.父id = b.id
                                WHERE b.名称='推荐' ) as a ");
                ReturnModel retmodel = SqlHelper.GetDataTable(strSql.ToString(), "a");

                if (retmodel.Succeed)
                {
                    DataTable dt = retmodel.SearchResult as DataTable;
                    RetObj model = new RetObj(1,"获取成功！");
                    List<Dictionary<string, object>> goods = new List<Dictionary<string, object>>();
                    List<Dictionary<string, object>> list = new List<Dictionary<string, object>>();

                    foreach (DataRow item in dt.Rows)
                    {
                        Dictionary<string, object> cat = new Dictionary<string, object>();
                        cat.Add("g_cat_id", item["g_cat_id"].ToString().Trim());
                        cat.Add("g_cat_name", item["g_cat_name"].ToString().Trim());
                        Dictionary<string, object> god = new Dictionary<string, object>();
                        god.Add("g_id", item["g_id"].ToString().Trim());
                        god.Add("g_img_ts", item["g_img_ts"].ToString());
                        god.Add("g_name", item["g_name"].ToString());

                        goods.Add(god);
                        list.Add(cat);
                    }
                    model.RetDics.Add("list", list);
                    model.RetDics.Add("goods",goods);
                    return model;
                }
                else
                {
                    return new RetObj("获取失败！");
                }

            }
            catch (Exception ex)
            {
                return new RetObj(ex.Message);
            }
        }

    }
}
