# Weed for .net/mono/java/php
超强跨平台轻量级ORM（无反射；缓存控制；分布式事务；万能绑定）<br/>

支持什么数据库？？？<br/>
与具体数据库无关（或许支持所有数据库）<br/>

*****************************************************
这是一个Weed3的个人修改版，只针对".net & mono"，同时我弄了一个代码生成器基于我修改的".net & mono",主要是修复了针对sqlite数据库和oracle的支持问题，在DbContext中增加了二个属性IsSupportSelectIdentityAfterInsert(因为原来代码中insert之后会执行select @@Identity,但是这个sqlite不支持)和IsSupportGcAfterDispose(因为原来代码中没有Dispose这个函数，也没有执行GC所以会导致sqlite库没办法释放)，还有一个Dispose函数,增加了一个DataFlagConfig用于对代码中的一些占位符进行替换(用Oracle时发现某些符号有错误所以加的这个),增加了一个IEntity用于实现实体类的功能(主要是赋值部分)

代码生成器的地址在:https://github.com/wcss2010/SuperCodeFactory.git

个人使用的库，原本没打算放出，不喜误喷！！！！

*****************************************************

理念：<br/>
高性能、跨平台、轻量、有个性<br/>

特点：<br/>
1.零反射零注解<br/>
2.漂亮的缓存控制（缓存服务由外部提供）<br/>
3.纯代码无任何配置<br/>
4.分布式事务集成<br/>
5.万能的数据绑定<br/>

占位符说明：<br/>
 $.       //数据库名占位数<br/>
 $fun     //SQL函数占位符<br/>
 ?        //参数占位符<br/>
 ?...     //数组型参数占位符<br/>

网站:<br/>
 http://www.noear.org<br/>

QQ群：<br/>
 22200020<br/>
 
--------------------------------------<br/>
示例1.1.1::入门级<br/>
```java
//DbContext db  = new DbContext("user","proxool.xxx_db");
DbContext db  = new DbContext("user","jdbc:mysql://x.x.x:3306/user","root","1234",null);

//简易.查询示例
db.table("user_info").where("user_id<?", 10).count();

db.table("user_info").where("user_id<?", 10).exists();
  
db.table("user_info") 
  .where("user_id<?", 10)
  .select("user_id,name,sex")
  .getDataList(); //.getList(new UserInfoModel());

//简易.关联查询示例
db.table("user_info u")
  .innerJoin("user_ex e").on("u.useer_id = e.user_id")
  .where("u.user_id<?", 10)
  .select("u.user_id,u.name,u.sex")
  .getDataList();
                
//简易.插入示例
db.table("$.test")
  .set("log_time", "$DATE(NOW())")
  .insert();

//简易.批量插入示例
db.table("test")
  .insertList(list,(d,m)->{
      m.set("log_time", "$DATE(NOW())");
      m.set("name",d.name);
  });

//简易.更新示例
db.table("test")
  .set("txt", "NOW()xx")
  .set("num", 44)
  .where("id IN (?...)", new int[] { 15,14,16}) //数据参数
  .update();

//简易.存储过程调用示例，及使用使用示例
db.call("user_get")
  .set("xxx", 1)  //保持与存储过程参数的序顺一致
  .getItem(new UserInfoModel()); 

//简易.查询过程调用示例，及使用使用示例
db.call("select * from user where user_id=@userID")
  .set("@userID", 1) 
  .caching(cache)//使用缓存
  .usingCache(60 * 100) //缓存时间
  .getItem(new UserInfoModel()); 

//简易.存储过程调用示例，及使用事务示例
db.tran(tran->{
    db.call("$.user_set").set("xxx", 1) 
      .tran(tran) //使用事务
      .execute();
});
```

示例1.1.2::不确定因素的连式处理支持<br/>
```java
//连式处理::对不确定的条件拼装
db.table("test")
  .expre(tb -> {
      tb.where("1=1");

      if (1 == 2) {
          tb.and("mobile=?", "xxxx");
      } else {
          tb.and("icon=?", "xxxx");
      }
  }).select("*");
  
//连式处理::对不确定字段的插入
db.table("test")
  .expre(tb -> {
      tb.set("name", "xxx");

      if (1 == 2) {
          tb.set("mobile", "xxxx");
      } else {
          tb.set("icon", "xxxx");
      }
  }).insert(); 
```

示例1.2::事务控制<br/>
```java
//demo1:: //事务组
db.tran((t) => {
    //以下操作在同一个事务里执行
    t.db().sql("insert into test(txt) values(?)", "cc").tran(t).execute();
    t.db().sql("insert into test(txt) values(?)", "dd").tran(t).execute();
    t.db().sql("insert into test(txt) values(?)", "ee").tran(t).execute();

    t.db().sql("update test set txt='1' where id=1").tran(t).execute();
});

//demo2:: //事务队列
DbTranQueue queue = new DbTranQueue();

db.tran().join(queue).execute((t) => {
    db.sql("insert into test(txt) values(?)", "cc").tran(t).execute();
    db.sql("insert into test(txt) values(?)", "dd").tran(t).execute();
    db.sql("insert into test(txt) values(?)", "ee").tran(t).execute();
});

db2.tran().join(queue).execute((t) => {
    db2.sql("insert into test(txt) values(?)", "gg").tran(t).execute();
});

queue.complete();

//demo2.1:: //事务队列简化写法
new DbTranQueue().execute(qt->{
     for (order_add_sync_stone sp : processList) {
         sp.tran(qt).execute();
     }
 });
```
示例1.3::缓存控制<br/>
```java
//最简单的缓存控制
db.call("user_get").set("xxx", 1)
    .caching(cache)
    .usingCache(60 * 1000)
    .getItem(new UserInfoModel());
    
//根据查询结果控制缓存
db.call("user_get").set("xxx",1)
    .caching(cache)
    .usingCache(60 * 100)
    .getItem(new UserInfoModel(), (cu, t) => { 
        if (t.user_id == 0)
            cu.usingCache(false);
});

//通过tag维护缓存
//1.缓存并添加简易标签
db.call("user_get").set("xxx", 1)
    .caching(cache)
    .cacheTag("user_"+ 1)
    .usingCache(60 * 1000)
    .getItem(new UserInfoModel());

CacheTags tags = new CacheTags(cache);
//2.1.可根据标签清除缓存
tags.clear("user_" + 1);

//2.2.可根据标签更新缓存
tags.update<UserInfoModel>("user_" + 1, (m)=>{
    m.name = "xxx";
    return m;
});
```

示例2::数据模型类（或叫实体类等）<br/>
```java
public class UserInfoModel implements IBinder {
    public long user_id;
    public int role;
    public String mobile;
    public String udid;
    public int city_id;
    public String name;
    public String icon;


    public void bind(GetHandlerEx s) {
        user_id = s.get("user_id").value(0l);
        role    = s.get("role").value(0);
        mobile  = s.get("mobile").value("");
        udid    = s.get("udid").value("");
        city_id = s.get("city_id").value(0);
        name    = s.get("name").value("");
        icon    = s.get("icon").value("");

    }

    public IBinder clone() {
        return new UserInfoModel();
    }
}

```

示例3.1::[存储过程]映射类<br/>
```java
public class user_get_by_id extends DbStoredProcedure
{
    public user_get_by_id()
    {
        super(Config.user);
        call("user_get_by_id");

        //set("{colname}", ()->{popname});
        //
        set("_user_id", ()->user_id);
    }

    public long user_id;
}

//使用示例
user_get_by_id sp = new user_get_by_id();
sp.user_id = 1;
sp.caching(cache)
  .getItem(new UserInfoModel());
```

示例3.2::[查询过程]映身类<br/>
```java
public class user_get_by_id extends DbQueryProcedure
{
    public user_get_by_id()
    {
        super(Config.user);
        sql("SELECT * FROM `user` where user_id = @user_id;");

        //set("{colname}", ()->{popname});
        //
        set("@user_id", ()->user_id);
    }

    public long user_id;
}

//使用示例
user_get_by_id sp = new user_get_by_id();
sp.user_id = 1;
sp.caching(cache)
  .getItem(new UserInfoModel());
```

示例3.3::[数据表]映射类<br/>
```java
public class UserM extends DbTable {
    public UserM() {
        super(DbConfig.test);

        table("users u");
        set("UserID", () -> UserID);
        set("Nickname", () -> Nickname);
        set("Sex", () -> Sex);
        set("Icon", () -> Icon);
        set("City", () -> City);
    }

    public Long UserID;
    public String Nickname;
    public Integer Sex;
    public String Icon;
    public String City;
}

//使用示例1
UserM m = new UserM();
m.UserID=1;
m.Icon="http:///xxxx";
m.insert();

//使用示例2
UserM m = new UserM();
m.icon="http:///xxxx";
m.where("UserID=?",1).update();

//使用示例3
UserM m = new UserM();
m.where("sex=?",1)
 .select("*")
 .getList(new UserInfoModel());

```

示例4::全局控制<br/>
```java
//开始debug模式，会有更多类型检查
WeedConfig.isDebug = true; 

//执行前检查代码 //不充许select 代码没有 limit 限制
WeedConfig.onExecuteBef((cmd)->{
    String sqltmp = cmd.text.toLowerCase();
    if(sqltmp.indexOf("select ")>=0 && sqltmp.indexOf(" limit ")< 0&&sqltmp.indexOf("count")<0) {
        return false;
    }else{
        return true;
    }
});

//执行后打印代码
WeedConfig.onExecuteAft((cmd) -> {
    System.out.println(cmd.text); //执行后打印日志
});

//对执行进行日志处理
WeedConfig.onLog((cmd) -> {
    //....
});
db.table("user").set("sex",1).log(true).update(); //.log(true) 执行后进行onLog日志处理

```

更多高级示例请参考Weed3Demo <br/>
--------------------------------------<br/>

by noear
