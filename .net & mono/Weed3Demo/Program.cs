using Noear.Weed;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Weed3Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DbConfig.pc_sqlite.IsSupportSelectIdentityAfterInsert = false;
            DbConfig.pc_sqlite.IsSupportGCAfterDispose = true;
            try
            {
                DbConfig.pc_sqlite.table("Version").delete();

                Version v1 = new Version();
                v1.VersionNum = "11";
                v1.Tag = "aa";
                v1.copyTo(DbConfig.pc_sqlite.table("Version")).insert();
                v1.VersionNum = "22";
                v1.Tag = "bb";
                v1.copyTo(DbConfig.pc_sqlite.table("Version")).insert();
                v1.VersionNum = "33";
                v1.Tag = "cc";
                v1.copyTo(DbConfig.pc_sqlite.table("Version")).insert();

                System.Console.WriteLine("+++++++++++++++++++++++++++++");
                System.Console.WriteLine("InsertAndSelectAll");
                List<Version> list = DbConfig.pc_sqlite.table("Version").select("*").getList<Version>(new Version());
                foreach (Version v in list)
                {
                    System.Console.WriteLine("**********************");
                    System.Console.WriteLine("VersionNum:" + v.VersionNum);
                    System.Console.WriteLine("Tag:" + v.Tag);
                    System.Console.WriteLine("**********************");
                }

                Thread.Sleep(15 * 1000);

                System.Console.WriteLine("+++++++++++++++++++++++++++++");
                System.Console.WriteLine("InsertAndSelectAndWhere");
                list = DbConfig.pc_sqlite.table("Version").where("VersionNum='11'").select("*").getList<Version>(new Version());
                foreach (Version v in list)
                {
                    System.Console.WriteLine("**********************");
                    System.Console.WriteLine("VersionNum:" + v.VersionNum);
                    System.Console.WriteLine("Tag:" + v.Tag);
                    System.Console.WriteLine("**********************");
                }

                DbConfig.pc_sqlite.table("Version").where("VersionNum='11'").delete();

                System.Console.WriteLine("+++++++++++++++++++++++++++++");
                System.Console.WriteLine("DeleteAndSelectAll");
                list = DbConfig.pc_sqlite.table("Version").select("*").getList<Version>(new Version());
                foreach (Version v in list)
                {
                    System.Console.WriteLine("**********************");
                    System.Console.WriteLine("VersionNum:" + v.VersionNum);
                    System.Console.WriteLine("Tag:" + v.Tag);
                    System.Console.WriteLine("**********************");
                }

                Thread.Sleep(15 * 1000);

                list[0].Tag = "UpdateResult" + DateTime.Now.ToString();
                list[0].copyTo(DbConfig.pc_sqlite.table("Version")).where("VersionNum='22'").update();

                System.Console.WriteLine("+++++++++++++++++++++++++++++");
                System.Console.WriteLine("UpdateAndSelectAll");
                list = DbConfig.pc_sqlite.table("Version").select("*").getList<Version>(new Version());
                foreach (Version v in list)
                {
                    System.Console.WriteLine("**********************");
                    System.Console.WriteLine("VersionNum:" + v.VersionNum);
                    System.Console.WriteLine("Tag:" + v.Tag);
                    System.Console.WriteLine("**********************");
                }

                Thread.Sleep(15 * 1000);
            }
            finally
            {
                DbConfig.pc_sqlite.Dispose();

                File.Move("c:\\static.db", "c:\\static_test.db");

                if (File.Exists("c:\\static_test.db"))
                {
                    System.Console.WriteLine("Dispose功能正常！！！");

                    File.Move("c:\\static_test.db", "c:\\static.db");

                    if (File.Exists("c:\\static.db"))
                    {
                        System.Console.WriteLine("完成了！！！");
                    }
                }

                Thread.Sleep(2000 * 1000);
            }
        }
    }

    /// <summary>
    /// 类Version。
    /// </summary>
    [Serializable]
    public partial class Version : IEntity
    {
        public Version() { }

        public override Noear.Weed.DbTableQuery copyTo(Noear.Weed.DbTableQuery query)
        {
            //设置值
            query.set("VersionNum", VersionNum);
            query.set("Tag", Tag);

            return query;
        }

        public string VersionNum { get; set; }
        public string Tag { get; set; }

        public override void bind(Noear.Weed.GetHandlerEx source)
        {
            VersionNum = source("VersionNum").value<string>("");
            Tag = source("Tag").value<string>("");
        }

        public override Noear.Weed.IBinder clone()
        {
            return new Version();
        }
    }
}