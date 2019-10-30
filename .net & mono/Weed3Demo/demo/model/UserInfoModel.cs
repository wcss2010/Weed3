using Noear.Weed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weed3Demo.demo.model
{
    [Serializable]
    public class UserInfoModel : IEntity
    {
        public long user_id;
        public int role;
        public String mobile;
        public String udid;
        public int city_id;
        public String name;
        public String icon;

        /// <summary>
        /// 主要用于将查询到的数据设置到对象中
        /// </summary>
        /// <param name="source"></param>
        public override void bind(GetHandlerEx source)
        {
            //1.source:数据源
            //
            user_id = source("user_id").value<long>(0);
            role = source("role").value<short>(0);
            mobile = source("mobile").value("");
            udid = source("udid").value("");
            city_id = source("city_id").value<int>(0);
            name = source("name").value("");
            icon = source("icon").value("");

        }

        /// <summary>
        /// 主要用于插入或更新时的赋值操作，比如obj.copyTo(context.table("user")).insert(); 这个操作的意思是将obj中所有的值设置到table("user")中执行插入操作
        /// </summary>
        /// <param name="dests"></param>
        /// <returns></returns>
        public override DbTableQuery copyTo(DbTableQuery dests)
        {
            dests.set("user_id", user_id);
            dests.set("role", role);
            dests.set("mobile", mobile);
            dests.set("udid", udid);
            dests.set("city_id", city_id);
            dests.set("name", name);
            dests.set("icon", icon);

            return dests;
        }
    }
}