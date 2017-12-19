using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLP.HAN;
namespace ZOOTest.NLP.HAN
{
    public class RoughSplitterTest
    {
        public static string[] TestCases = new[]
        {
            "安徽易元堂置业有限公司",                      // 公司名
            "0558-5586988",                             // 电话
            "13800000000",                              // 手机
            "(010)55556666",                            // 电话
            "15155905199@163.com",                      // 邮箱地址
            "huangfd@zhongdinggroup.com",               // 邮箱地址
            "082MQ348X",                                // 机构代码
            "M756B517X",                                // 机构代码
            "阿里巴巴（中国）有限公司",                 // 公司名
            "阿里巴巴（中国）alibaba-group有限公司",    // 公司名
            "阿里巴巴（group）alibaba有限公司",       // 公司名
            "阿里巴巴（group）有限公司",              // 公司名
            "2301001002875(1-1)",                   // 注册号
            "9111010563365416XH",                   // 统一信用代码
            "91233000126965107C",                   // 统一信用代码
            "91370200724016913U",                   // 统一信用代码
            "9113112280985641XY",                   // 统一信用代码
            "2003-08-27 00:00:00.000",              // 时间
            "2012-07-25",                           // 日期
            "18:48:03.000",                          // 时间
            "310117",                               // 地区码
            "https://shop126202828.taobao.com",
            "http://3539.cn",
            "https://shop143824246.taobao.com/?spm=a313o.7775905.1998679131.d0011.l2d1qz",
            "www.feiaoparking.com",
            "https://shop36192312.taobao.com/?spm=a230r.7195193.1997079397.2.XG2bRb",
            "http://jingpinyq.tmall.com/shop/view_shop.htm?spm=a220m.1000862.1000730.2.f53rwD&user_number_id=1859386725&rn=43218c4cf22a7b585f138223442ca7e2",
            "五、八版中缝",                               // 公告 页版
            "Lin Ling\\林玲",                             // 公告 相关方
            "罗子清（430202197410260039）",               // 公告 相关方
            "郭丽萍（身份证号为612722197312100064）",     // 公告 相关方
            "687045917|550426866",                      // 判决 公司机构代码
        };

        public static void Test()
        {
            for (int i = 0; i < TestCases.Length; i++)
            {
                var chunks = RoughSplitter.RoughSplit(TestCases[i]);
                foreach (var chunk in chunks)
                    Console.Write(chunk + " | ");
                Console.WriteLine();
            }
        }
    }
}
