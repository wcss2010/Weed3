using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;

namespace Noear.Weed
{
    /**
     * Created by noear on 14-6-12.
     * 数据库上下文
     */
    public class DbContext
    {
        private string _fieldFormat;
        private String _url;
        private String _schemaName;
        private DbProviderFactory _provider;
        private bool _isSupportSelectIdentityAfterInsert = true;
        /// <summary>
        /// 是否支持Insert后使用SELECT @@IDENTITY
        /// </summary>
        public bool IsSupportSelectIdentityAfterInsert
        {
            get { return _isSupportSelectIdentityAfterInsert; }
            set { _isSupportSelectIdentityAfterInsert = value; }
        }

        private bool _isSupportGCAfterDispose = false;
        /// <summary>
        /// 是否支持在Dispose后运行GC(主要针对sqlite数据库Dispose后仍然无法释放文件的BUG)
        /// </summary>
        public bool IsSupportGCAfterDispose
        {
            get { return _isSupportGCAfterDispose; }
            set { _isSupportGCAfterDispose = value; }
        }

        private List<DbConnection> _connectionList = new List<DbConnection>();

        static DbProviderFactory provider(string providerString)
        {
            if (providerString == null)
                return null;

            if (providerString.IndexOf(",") > 0)
                return (DbProviderFactory)Activator.CreateInstance(Type.GetType(providerString, true, true));
            else
                return DbProviderFactories.GetFactory(providerString);
        }

        public Command lastCommand { get; internal set; }
        public bool allowMultiQueries;

        public DbContext(String schemaName, string name)
            : this(schemaName, name, "")
        {

        }

        //fieldFormat："`%`"
        public DbContext(String schemaName, string name, string fieldFormat)
        {
            var set = ConfigurationManager.ConnectionStrings[name];
            var p = provider(set.ProviderName);
            doInit(schemaName, set.ConnectionString, p);

            _fieldFormat = fieldFormat;
        }

        public DbContext(String schemaName, string connectionString, DbProviderFactory provider)
            : this(schemaName, connectionString, "", provider)
        {

        }

        public DbContext(String schemaName, string connectionString, String fieldFormat, DbProviderFactory provider)
        {
            doInit(schemaName, connectionString, provider);
            _fieldFormat = fieldFormat;
        }

        protected virtual void doInit(String schemaName, string connectionString, DbProviderFactory provider)
        {
            _provider = provider;
            _schemaName = schemaName;
            _url = connectionString;
        }

        public String field(String key)
        {
            if (string.IsNullOrEmpty(_fieldFormat))
                return key;
            else
                return _fieldFormat.Replace("%", key);
        }

        /*是否配置了schema*/
        public bool hasSchema() { return _schemaName != null; }

        /*获取schema*/
        public String getSchema()
        {
            return _schemaName;
        }

        /*获取连接*/
        public DbConnection getConnection()
        {
            //创建连接
            DbConnection conn = _provider.CreateConnection();
            conn.ConnectionString = _url;

            //保存这个连接
            _connectionList.Add(conn);

            //返回这个连接
            return conn;
        }

        public DbQuery sql(String code, params object[] args)
        {
            return new DbQuery(this).sql(new SQLBuilder().append(code, args));
        }

        public DbQuery sql(SQLBuilder sqlBuilder)
        {
            return new DbQuery(this).sql(sqlBuilder);
        }

        /*获取process执行对象*/
        public DbStoredProcedure call(String process)
        {
            return new DbStoredProcedure(this).call(process);
        }

        /*获取一个表对象［用于操作插入也更新］*/
        public DbTableQuery table(String table)
        {
            return new DbTableQuery(this).table(table);
        }

        public DbTran tran(Action<DbTran> handler)
        {
            return new DbTran(this).execute(handler);
        }

        public DbTran tran()
        {
            return new DbTran(this);
        }

        /* 结束所有连接 */
        public void Dispose()
        {
            //尝试释放所有连接
            if (_connectionList != null)
            {
                foreach (DbConnection conn in _connectionList)
                {
                    try
                    {
                        conn.Close();
                    }
                    catch (Exception ex) { }

                    try
                    {
                        conn.Dispose();
                    }
                    catch (Exception ex) { }
                }
            }

            //尝试GC
            if (IsSupportGCAfterDispose)
            {
                //**链接close（）和dispose（）之后任然不能释放与db文件的连接,原因是sqlite在执行SQLiteConnectionHandle.Dispose()操作时候，其实并没有真正的释放连接，只有显式调用 CLR垃圾回收之后才能真正释放连接
                try
                {
                    GC.Collect();
                }
                catch (Exception ex) { }

                try
                {
                    GC.WaitForPendingFinalizers();
                }
                catch (Exception ex) { }
            }
        }
    }
}