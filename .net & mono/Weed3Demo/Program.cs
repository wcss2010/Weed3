using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Weed3Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DbConfig.pc_sqlite.IsSupportInsertAfterSelectIdentity = false;
            DbConfig.pc_sqlite.table("Robot_Actions").set("Id", 25).set("Code", "A").set("Name", "B").insert();
        }
    }
}