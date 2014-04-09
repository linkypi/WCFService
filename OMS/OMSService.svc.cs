using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using DAL;
using Model;
using OMS.Tool;

namespace OMS
{
    // 注意: 使用“重构”菜单上的“重命名”命令，可以同时更改代码、svc 和配置文件中的类名“Service1”。
    // 注意: 为了启动 WCF 测试客户端以测试此服务，请在解决方案资源管理器中选择 Service1.svc 或 Service1.svc.cs，然后开始调试。
   [AspNetCompatibilityRequirements(RequirementsMode=AspNetCompatibilityRequirementsMode.Allowed)]
   [ServiceKnownType(typeof(Person))]
   [ServiceKnownType(typeof(object))]
    [ServiceContract]
    public class OMSService 
    {

       [OperationContract]
       [WebInvoke(Method = "GET", ResponseFormat = WebMessageFormat.Json)]
       public Person GetData()
       {
           return new Person() {Name="Jack" ,Age=24};
       }

       [OperationContract]
       [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json,
           UriTemplate = "/jmp",
           BodyStyle = WebMessageBodyStyle.WrappedRequest)]
       public string jmp(ReqModel data)
       {
           ReturnModel model = Validate(data);
           if (model.Succeed == false)
           {
               return  "";
           }
           Handler hand = new Handler();
           string asd = hand.admin_login(data).ToString().Replace("\r\n", "");
            return asd;
       }

       [OperationContract]
       [WebInvoke(Method = "POST", ResponseFormat = WebMessageFormat.Json, RequestFormat = WebMessageFormat.Json,
           UriTemplate = "/exec",BodyStyle = WebMessageBodyStyle.WrappedRequest)]
       public Stream exec(ReqModel data)
       {
           DateTime date = DateTime.Now;
           ReturnModel model = Validate(data);
           if (model.Succeed == false)
           {
               return (Stream)model.SearchResult;
           }
           Handler hand = new Handler();
           switch (data.service)
           {
               case "member_login":
                   return Common.ReturnValue(hand.member_login(data),date);

               case "admin_login":
                   return Common.ReturnValue(hand.admin_login(data),date);
                 
               case "get_goods_category":
                   return Common.ReturnValue(hand.get_goods_category(data),date);
                   
               case "get_goods_list":
                   return Common.ReturnValue(hand.get_goods_list(data),date);

               case "get_goods_info":
                   return Common.ReturnValue(hand.get_goods_info(data),date);

               case "get_goods_image":
                   return Common.ReturnValue(hand.get_goods_image(data),date);
                  
               case "get_card_info":
                   return Common.ReturnValue(hand.get_card_info(data),date);
                 
               case "get_card_log":
                   return Common.ReturnValue(hand.get_card_log(data),date);
               case "place_an_order":
                   return Common.ReturnValue(hand.place_an_order(data),date);
                   
               case "get_order_list":
                   return Common.ReturnValue(hand.get_order_list(data),date);
                   
               case "get_order_info":
                   return Common.ReturnValue(hand.get_order_info(data),date);
               
               case "get_order_log":
                   return Common.ReturnValue(hand.get_order_log(data), date);
               
               case "cancel_order":
                   return Common.ReturnValue(hand.cancel_order(data), date);

               case   "goods_recommend_list":
                   return Common.ReturnValue(hand.goods_recommend_list(data));
               
           }

           RetObj hret = new RetObj("服务名称不存在！");
           return Common.ReturnValue(hret);
       }

       private static ReturnModel Validate(ReqModel data)
       {
           if (data == null)
           {
               RetObj ret = new RetObj( "数据有误！");
               return new ReturnModel() { Succeed = false, SearchResult = Common.ReturnValue(ret) };
           }
           if (string.IsNullOrEmpty(data.service))
           {
               RetObj ret = new RetObj("接口服务标识有误！");
               return new ReturnModel() { Succeed = false, SearchResult = Common.ReturnValue(ret) };
           }

           
           ReturnModel model = new Handler().ValidateSller(data.seller);
           if (model.Succeed==false)
           {
               RetObj ret = new RetObj(model.Message );
               return new ReturnModel() { Succeed = false, SearchResult = Common.ReturnValue(ret) };
           }

           if (!Common.Validate(data))
           {
               RetObj ret = new RetObj("验证失败！");
               return new ReturnModel() { Succeed = false, SearchResult = Common.ReturnValue(ret) };
           }
           return new ReturnModel() { Succeed=true};
       }


    }
}
