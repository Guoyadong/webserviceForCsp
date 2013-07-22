using System;
using yyService.DBupdate1;
using System.Data.SqlClient;
using System.Web.Services;

using System.Diagnostics;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Threading;


namespace yyService
{

    class connect
    {
        // webservice 对象
       public DBupdateService dbd;
        
        //	数据库名称
       public string _DBname;

        //	数据表名称
       public string _TBname;

        //	ip地址
       public string _IP;

        //	数据库用户名
       public string _username;

        //	数据库密码
       public string _userpwd;

        //	数据库登陆地址
       public string _dbURL;

        //	服务器IP
       public string _SIp;

        //	服务器端口号
       public string _SPort;

        //	数据库连接
        //Connection con=null;

       public SqlConnection con = null;
       public SqlConnection con1 = null;
        //	数据库连接状态
       public SqlCommand sta = null;
       public SqlCommand scd = null;
        
        //数据库执行结果
       public SqlDataReader sqlread = null;
               
        //	执行许可
        public bool runAble = true;

        //	关闭许可
        public bool closeAble = false;

        //	间隔时间
        public int sleeptime;

        //	对象申请，此为默认初始化

        //	不带参数的对象申请
        public connect()
        {
            //	 _DBname="UFDATA_401_2013";
            //	 _TBname="RdRecords_Temp";
            //	 _IP="192.168.10.105:1433";
            //	 _username="sa";
            //	 _userpwd="sa";
            //	 _dbURL="jdbc:sqlserver://"+_IP+";DatabaseName="+_DBname;
            //	 sleeptime=60000;
            //	 dbs=new DBupdateService();
            //	 dbd=dbs.getDBupdate();
            
        }
    
    //带参数的初始化
    public  connect(String DatabaseName,String TableName,String IP,String UserName,String UserPassword,String ServerIP,String ServerPort,int sptime=5000)
    {
	_DBname=DatabaseName;
	_TBname=TableName;
	_IP=IP;
	_username=UserName;
	_userpwd=UserPassword;
	_SIp=ServerIP;
	_SPort=ServerPort;
	_dbURL="jdbc:sqlserver://"+_IP+";DatabaseName="+_DBname;
    dbd=new DBupdate1.DBupdateService(_SIp+":"+_SPort);
	sleeptime=sptime;
}  

    /// <summary>
    /// 初始化类内申明的对象
    /// </summary>
    /// <param name="DatabaseName">数据库名</param>
    /// <param name="TableName">表名</param>
    /// <param name="IP">IP名</param>
    /// <param name="UserName">数据库用户名</param>
    /// <param name="UserPassword">密码</param>
    /// <param name="ServerIP">服务端IP</param>
    /// <param name="ServerPort">服务端端口</param>
    /// <param name="sptime">睡眠时间</param>
    public void init(String DatabaseName, String TableName, String IP, String UserName, String UserPassword, String ServerIP, String ServerPort,int sptime=60000)
    {
        _DBname = DatabaseName;
        _TBname = TableName;
        _IP = IP;
        _username = UserName;
        _userpwd = UserPassword;
        _SIp = ServerIP;
        _SPort = ServerPort;
        _dbURL = "jdbc:sqlserver://" + _IP + ";DatabaseName=" + _DBname;
        dbd = new DBupdate1.DBupdateService(_SIp + ":" + _SPort);
        sleeptime = sptime;
    }

        //	线程运行程序。已废弃
//        public void run(){





////			正式程序段        
//    string driverName="com.microsoft.jdbc.sqlserver.SQLServerDriver";
//    string sqlconnetdata="server="+_IP+";uid="+_username+";pwd="+ _userpwd+";database="+_DBname;
//    string query="select * from "+_TBname;
//    try{
////		外层循环，每隔sleeptime毫秒运行一次
//        while(runAble){
//            Thread.Sleep(sleeptime);
		
//            //wait(sleeptime);
//            //System.out.println("start drivername");
//             con=new SqlConnection(sqlconnetdata);
//             sta =new SqlCommand(query,con);  
//             Console.WriteLine("start drivername");
//             sqlread=sta.ExecuteReader();
//            while(sqlread.Read()&&runAble)
//            {
//            int sign=(int)sqlread["Operation"];
//            int did=0;
//           closeAble=false;
//                while(did==0 && runAble)
//                {	
//                    switch(sign)
//                    {
//                    case 0:
						
//                        did=dbd.outboundUpdateMain((int)sqlread["RdRecordsID"], (int)sqlread["bRdFlag"], sqlread["dDate"].ToString(),sqlread["cBatch"].ToString(), sqlread["cInvName"].ToString(), (double)sqlread["iQuantity"], sqlread["cInvStd"].ToString(), (decimal)sqlread["iPrice"], (double)sqlread["iUnitCost"]);
					
//                        break;
//                    case 1:
						
//                            did=dbd.outboundInsertMain((int)sqlread["RdRecordsID"],(int)sqlread["bRdFlag"], sqlread["dDate"].ToString(),sqlread["cBatch"].ToString(), sqlread["cInvName"].ToString(),sqlread["cWhCode"].ToString(), (double)sqlread["iQuantity"],sqlread["cInvStd"].ToString(),sqlread["cVenName"].ToString(),sqlread["cCode"].ToString(), sqlread["cDepName"].ToString(), sqlread["cBusType"].ToString(), sqlread["cMaker"].ToString(), (decimal)sqlread["iPrice"],(double)sqlread["iUnitCost"]);
//                        break;
//                    case 2:
						
//                        did=dbd.outboundDeleteMain((int)sqlread["RdRecordsID"]);
						
//                        break;
//                    case 3:	
					
//                        did=dbd.outboundUpdateMainOrder((int)sqlread["RdRecordsID"], sqlread["cCode"].ToString(), sqlread["dDate"].ToString(), sqlread["cBusType"].ToString(), sqlread["cVenName"].ToString(), sqlread["cMemo"].ToString(),sqlread["cDepName"].ToString());
					
//                        break;
					      				
//                    }
//                }
//                    if(!runAble)
//                    break;
//                //完成这一条数据的同步，进行删除
//                int ss=(int)sqlread["AutoID"];
//                String deleteQ="delete from "+_TBname+" where AutoID='"+ss+"'";
//                sta=new SqlCommand(deleteQ,con);
//                sta.ExecuteNonQuery();
//                closeAble=true;
//            }
//            con.Close();
//        }
//    }
//    catch (SqlException e) {
//        Console.WriteLine("sql error");
//    }
//    catch (Exception e)
//    {
//        Console.WriteLine("error");
//        try{
//            con.Close();
//        //	res.close();
//        }
//        catch(Exception o){			
//        }
//    }
//    Console.WriteLine("Run run over!");
//}
    }
}