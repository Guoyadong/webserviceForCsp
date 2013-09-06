using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using yyService.DBupdate1;
using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Data.SqlClient;
using System.Xml;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
namespace yyService
{
    class syn
    {


        String strConnection ;

        SqlConnection conn, con2;
        SqlCommand com, com2;
        SqlDataReader read, read1;
        SqlTransaction stc, stc2;
        string _ip,_dbname,_pwd,_uid;




        public syn(string uid,string pwd,string dbname,string ip)
        {
            _ip = ip;
            _pwd = pwd;
            _dbname = dbname;
            _uid = uid;
            strConnection = "uid="+uid+"; password="+pwd+"; initial catalog="+dbname+"; Server="+ip;
        }
        public syn()
        {
            strConnection = "uid=sa; password=sa; initial catalog=JLWSYN; Server=locahost";
        }
        public int eosInsert(int DjLsh, int DjBth, String XSBM, String KHMC, String WPMC, String ZDR, String GG, String PH, decimal QRSL, String QRRQ, String CKH, String BZ)
        {

            string data = QRRQ.Substring(0, 4);
            decimal iQuantity = QRSL;


            try
            {

                String query = "select * from GSDZB where CKH='" + CKH + "'";
                Console.WriteLine("调用insert接口，开始工作");
                conn = new SqlConnection(strConnection);
                conn.Open();
                stc = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "SQLTransaction");
                Console.WriteLine("数据库连接成功！");
                com = new SqlCommand(query, conn);
                com.Connection = conn;
                com.Transaction = stc;
                read = com.ExecuteReader();
                String dbname = "", ZTH = read["ZTH"].ToString();
                while (read.Read())
                {
                    dbname = "UFDATA_" +ZTH+ "_" + data;
                    break;
                }
                read.Close();
                //conn.Close();
                Console.WriteLine("数据库为：" + dbname);

                string connectionstr = "uid="+_uid+"; password="+_pwd+"; initial catalog=" + dbname + "; Server="+_ip;
                conn = new SqlConnection(connectionstr);
                conn.Open();


                ///id:主表号；autoid：明细表号
                int id = 0, autoid = 0;
                ///cCode：销售单号;cDepCode:部门编号;cCusCode:客户编号;cInvCode:物品号
                String cCode = null, code_temp = null, cDepCode = null, cCusCode = null, cInvCode = null,cwhcode=null,crdcode=null;

                //获取主表id，明细表autoid

                query = "declare @P1 int declare @P2 int exec sp_GetId '', '"+ZTH+"', 'rd', 1, @P1 output, @P2 output select @P1 id, @P2 autoid ";
                com = new SqlCommand(query, conn);
                read = com.ExecuteReader();
                // res = com.executeQuery(query,conn);
                if (read.Read())
                {
                    id = (int)read["id"];
                    autoid = (int)read["autoid"];
                }
                read.Close();
               

                
                //获取部门号
                query = "select * from Department where cDepName='" + XSBM + "'";
                Console.WriteLine(query);
                com = new SqlCommand(query, conn);
                read = com.ExecuteReader();
                if (read.Read())
                {
                    cDepCode = read["cDepCode"].ToString();
                }
                read.Close();
                //获取公司号
                query = "select * from Customer where cCusName='" + KHMC + "'";
                com = new SqlCommand(query, conn);
                read = com.ExecuteReader();
                while (read.Read())
                {
                    cCusCode = read["cCusCode"].ToString();
                    break;
                }
                read.Close();

                //获取物品号
                if (GG == "" || GG == null)
                {
                    query = "select * from Inventory where cInvName='" + WPMC + "' and cInvStd is null";
                }
                else
                {
                    query = "select * from Inventory where cInvName='" + WPMC + "' and cInvStd='" + GG + "'";
                }

                com = new SqlCommand(query, conn);
                read = com.ExecuteReader();
                Console.WriteLine(query);
                while (read.Read())
                {
                    cInvCode = read["cInvCode"].ToString();
                    break;
                }
                read.Close();
                Console.WriteLine("获取信息完毕，开始插入");
                query = "select count(*) num from JLWSYN..MXDZB where DjLsh=" + DjLsh;
                com = new SqlCommand(query, conn);
                int num = (int)com.ExecuteScalar();

                String dataIn;
                if (num == 0)
                {
                    Console.WriteLine("相关条数为0，开始插入新出库单");

                    //获取单号
                    if (CKH == "401" || CKH=="301")
                    {
                        query = "select * from VoucherHistory Where CardNumber='0303' and cContent='仓库' and cSeed='5' ";
                    }
                    else if (CKH == "105")
                    {
                        query = "select * from VoucherHistory Where iRdFlagSeed='2' and cContent is null ";
                    }
                    com = new SqlCommand(query, conn);
                    read = com.ExecuteReader();
                    if (read.Read())
                    {
                        code_temp = read["cNumber"].ToString();
                    }
                    read.Close();
                    //int tmp=Integer.valueOf(code_temp).intValue()+1;
                    int tem = int.Parse(code_temp);
                    tem++;
                    //String s=String.valueOf(tmp);
                    string s = tem.ToString();
                    if (CKH == "401" || CKH == "301")
                    {
                        cCode = "5";
                    }
                    else if (CKH == "105")
                    {
                        cCode = "0";
                    }
                    cCode = "5";
                    for (int i = 0; i < 9 - s.Length; i++)
                        cCode += "0";
                    cCode += s;
                    if (CKH == "401" || CKH == "301")
                    {
                        query = "Update VoucherHistory set cNumber='" + s + "' Where CardNumber='0303' and cContent='仓库' and cSeed='5'";
                    }
                    else if (CKH == "105")
                    {
                        query = "Update VoucherHistory set cNumber='" + s + "'  Where iRdFlagSeed='2' and cContent is null ";
                    }
                    
                    com = new SqlCommand(query, conn);
                    com.ExecuteNonQuery();

                    query = "select * from warehouse where cwhname='成品仓库'";
                    com = new SqlCommand(query, conn);
                    read = com.ExecuteReader();
                    read.Read();
                    cwhcode=read["cWhCode"].ToString();
                    read.Close();

                    query = "select * from rd_style where cRdName='销售出库'";
                    com = new SqlCommand(query, conn);
                    read = com.ExecuteReader();
                    read.Read();
                    crdcode = read["cRdCode"].ToString();
                    read.Close();

                    dataIn = "Insert Into Rdrecord(id,brdflag,cvouchtype,cbustype,cbuscode,imquantity,darvdate,cchkcode,dchkdate,cchkperson,vt_id,bisstqc,csource,cwhcode,ddate,ccode,crdcode,cdepcode,cpersoncode,cstcode,ccuscode,cvencode,cordercode,cbillcode,cdlcode,cmemo,cmaker,caccounter,chandler,cdefine1,cdefine2,cdefine3,cdefine4,cdefine5,cdefine6,cdefine7,cdefine8,cdefine9,cdefine10,cdefine11,cdefine12,cdefine13,cdefine14,cdefine15,cdefine16,dveridate,gspcheck,bpufirst,biafirst,ipurorderid,ipurarriveid,iproorderid,iarriveid,isalebillid) "
                            + "Values (" + id + ",0,'32','普通销售',NULL,0,NULL,NULL,NULL,NULL,87,0,'库存','"+cwhcode+"','" + QRRQ + "','" + cCode + "','"+crdcode+"','" + cDepCode + "',NULL,NULL,'" + cCusCode + "',NULL,NULL,NULL,NULL,'" + BZ + "','" + ZDR + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL)";
                    Console.WriteLine(dataIn);
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("主表插入完毕");
                    dataIn = "Insert Into Rdrecords(id,cinvcode,cfree1,cfree2,cfree3,cfree4,cfree5,cfree6,cfree7,cfree8,cfree9,cfree10,inquantity,innum,cassunit,cbatch,inum,cbarcode,iquantity,iunitcost,iprice,ipunitcost,ipprice,dvdate,cposition,autoid,cvouchcode,isoutquantity,isoutnum,cdefine22,cdefine23,cdefine24,cdefine25,cdefine26,cdefine27,cdefine28,cdefine29,cdefine30,cdefine31,cdefine32,cdefine33,cdefine34,cdefine35,cdefine36,cdefine37,citemcode,citem_class,idlsid,dmadedate,isbsid,isendquantity,isendnum,cname,impoids,icheckids,itrids,cgspstate,citemcname,iensid,cbvencode,imassdate,cinvouchcode)"
                            + " Values (" + id + ",'" + cInvCode + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,0,NULL,'" + PH + "',0,NULL," + iQuantity + ",1," + iQuantity + ",NULL,NULL,NULL,''," + autoid + ",NULL,0,0,NULL,NULL,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL) ";
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("子表插入完毕");
                    dataIn = " Update  CurrentStock Set  iQuantity =iQuantity-" + iQuantity + " Where cWhcode='05'  And  cInvCode ='" + cInvCode + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + PH + "' ";
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("库存表更新完毕");
                    dataIn = " insert into JLWSYN..MXDZB (DjLsh,DjBth,RdRecordID,RdRecordsID,cInvCode,cCode,cDepCode,cCusCode,iQuantity,DBname,cBatch) values (" + DjLsh + "," + DjBth + "," + id + "," + autoid + ",'" + cInvCode + "','" + cCode + "','" + cDepCode + "','" + cCusCode + "'," + iQuantity + ",'" + dbname + "','" + PH + "')";
                    Console.WriteLine(dataIn);
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    dataIn = " insert into JLWSYN..MXDZB (DjLsh,DjBth,RdRecordID,RdRecordsID,cCode,cDepCode,cCusCode,iQuantity,DBname) values (" + DjLsh + ",-1," + id + ",-1,'" + cCode + "','" + cDepCode + "','" + cCusCode + "',0,'" + dbname + "')";
                    Console.WriteLine(dataIn);
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("明细对照表插入完毕");
                }
                else
                {
                    query = "select * from JLWSYN..MXDZB where DjLsh=" + DjLsh;

                    com = new SqlCommand(query, conn);
                    read = com.ExecuteReader();
                    while (read.Read())
                    {
                        id = (int)read["RdRecordID"];
                        cCode=read["cCode"].ToString();
                        break;
                    }
                    read.Close();

                    Console.WriteLine("相关条数为：" + num + "条，开始原有出库单插入新明细");
                    dataIn = "Insert Into Rdrecords(id,cinvcode,cfree1,cfree2,cfree3,cfree4,cfree5,cfree6,cfree7,cfree8,cfree9,cfree10,inquantity,innum,cassunit,cbatch,inum,cbarcode,iquantity,iunitcost,iprice,ipunitcost,ipprice,dvdate,cposition,autoid,cvouchcode,isoutquantity,isoutnum,cdefine22,cdefine23,cdefine24,cdefine25,cdefine26,cdefine27,cdefine28,cdefine29,cdefine30,cdefine31,cdefine32,cdefine33,cdefine34,cdefine35,cdefine36,cdefine37,citemcode,citem_class,idlsid,dmadedate,isbsid,isendquantity,isendnum,cname,impoids,icheckids,itrids,cgspstate,citemcname,iensid,cbvencode,imassdate,cinvouchcode)"
                            + " Values (" + id + ",'" + cInvCode + "',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,0,NULL,'" + PH + "',0,NULL," + iQuantity + ",1," + iQuantity + ",NULL,NULL,NULL,''," + autoid + ",NULL,0,0,NULL,NULL,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,0,0,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL) ";
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("子表插入完毕");
                    dataIn = " Update  CurrentStock Set  iQuantity =iQuantity-" + iQuantity + " Where cWhcode='05'  And  cInvCode ='" + cInvCode + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + PH + "' ";
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("库存表更新完毕");
                    dataIn = "insert into JLWSYN..MXDZB (DjLsh,DjBth,RdRecordID,RdRecordsID,cInvCode,cCode,cDepCode,cCusCode,iQuantity,DBname,cBatch) values (" + DjLsh + "," + DjBth + "," + id + "," + autoid + ",'" + cInvCode + "','" + cCode + "','" + cDepCode + "','" + cCusCode + "'," + iQuantity + ",'" + dbname + "','" + PH + "')";
                    com = new SqlCommand(dataIn, conn);
                    com.ExecuteNonQuery();
                    Console.WriteLine("明细对照表插入完毕");
                }

                stc.Commit();
                conn.Close();
            }
            catch (Exception e)
            {
                stc.Rollback();
                conn.Close();
                MessageBox.Show(e.ToString());
                try
                {
                    
                }
                catch (SqlException e1)
                {
                    // TODO Auto-generated catch block
                    
                }
                return 0;
            }
            Console.WriteLine("接口完成，退出");

            return 1;
        }

        public int eosUpdate(int DjLsh, int DjBth, String WPMC, String GG, String PH, decimal QRSL)
        {


            String dbname = null, cInvCode = null, cBatch = null;
            int id = 0, autoid = 0;
            decimal iQuantity=0;
            try
            {
                Console.WriteLine("开始update接口");

                conn = new SqlConnection(strConnection);
                Console.WriteLine("Connection Successful!");
                conn.Open();
                

                String query = "select * from MXDZB where DjLsh=" + DjLsh + " and DjBth=" + DjBth;
                stc = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "SQLTransaction");
                Console.WriteLine("数据库连接成功！");
                com = new SqlCommand(query, conn);
                com.Connection = conn;
                com.Transaction = stc;
                read = com.ExecuteReader();
                while (read.Read())
                {
                    dbname = read["DBname"].ToString();
                    Console.WriteLine(dbname);
                    id = (int)read["RdRecordID"];
                    autoid = (int)read["RdRecordsID"];
                    iQuantity = Convert.ToDecimal(read["iQuantity"].ToString());
                    cInvCode = read["cInvCode"].ToString();
                    cBatch = read["cBatch"].ToString();
                    break;
                }
                read.Close();
                //con2.Close();

                //dbURL="jdbc:sqlserver://localhost:1433;DatabaseName="+dbname;
                //conn = DriverManager.getConnection(dbURL, userName, userPwd);
                //conn.setAutoCommit(false);
                //System.out.println("Connection Successful!");
                //sta = conn.createStatement();
                string connectionstr = "uid=" + _uid + "; password=" + _pwd + "; initial catalog=" + dbname + "; Server=" + _ip;
                con2 = new SqlConnection(connectionstr);
                con2.Open();
                stc2 = con2.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "SQLTransaction");

               
                if (GG == "" || GG == null)
                {
                    query = "select * from Inventory where cInvName='" + WPMC + "' and cInvStd is null";
                }
                else
                {
                    query = "select * from Inventory where cInvName='" + WPMC + "' and cInvStd='" + GG + "'";
                }
                com = new SqlCommand(query, con2);
                com.Connection = con2;
                com.Transaction = stc2;
                read = com.ExecuteReader();
                string cInvCode2 = "";
                while (read.Read())
                {
                    cInvCode2 = read["cInvCode"].ToString();
                    break;
                }
                read.Close();
                Console.WriteLine("相关数据获取完毕，开始更新");
                String upd = "update  Rdrecords set cinvcode='" + cInvCode2 + "',cBatch='" + PH + "',iquantity=" + QRSL + ",iprice=" + QRSL + " where id=" + id + " and autoId=" + autoid;
                com.CommandText = upd;
                //com = new SqlCommand(upd, conn);
                com.ExecuteNonQuery();
                Console.WriteLine("子表更新完毕");
                upd = "Update  CurrentStock Set  iQuantity =iQuantity+" + iQuantity + " Where cWhcode='05'  And  cInvCode ='" + cInvCode + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + cBatch + "' ";
                com.CommandText = upd;
                //com = new SqlCommand(upd, conn);
                com.ExecuteNonQuery();

                upd = "Update  CurrentStock Set  iQuantity =iQuantity-" + QRSL + " Where cWhcode='05'  And  cInvCode ='" + cInvCode2 + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + PH + "' ";
                com.CommandText = upd;
                //com = new SqlCommand(upd, conn);
                com.ExecuteNonQuery();
                Console.WriteLine("库存表更新完毕");

                upd = "update JLWSYN..MXDZB set iQuantity=" + QRSL + ",cInvCode='" + cInvCode2 + "',cBatch='" + PH + "' where RdRecordID=" + id + " and RdrecordsID=" + autoid;
                com.CommandText = upd;
                //com = new SqlCommand(upd, conn);
                com.ExecuteNonQuery();
                stc.Commit();
                stc2.Commit();
                conn.Close();
                con2.Close();
            }
            catch (Exception e)
            {
                stc2.Rollback();
                stc.Rollback();
                conn.Close();
                MessageBox.Show(e.ToString());
                try
                {
                    
                }
                catch (SqlException e1)
                {
                    // TODO Auto-generated catch block
                    
                }
                

                return 0;
            }
            Console.WriteLine("接口完成，退出");

            return 1;
        }

        public int eosDelete(int DjLsh, int DjBth)
        {
            String dbname = null, cInvCode = null, cBatch = null;
            int id = 0, autoid = 0;
            decimal iQuantity=0;
            try
            {
                Console.WriteLine("开始delete接口");
                
                conn = new SqlConnection(strConnection);
                Console.WriteLine("Connection Succesful!");
                conn.Open();
                stc = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "SQLTransaction");
                Console.WriteLine("数据库连接成功！");

                int kk = 0;
                int id2 = 0;

                if (DjBth == -1)
                {
                    Console.WriteLine("检测到云交单废弃，开始删除出库单");
                    String query = "select * from MXDZB where DjLsh=" + DjLsh;
                    //com.CommandText = query;
                    com = new SqlCommand(query, conn);
                    com.Connection = conn;
                    com.Transaction = stc;
                    read = com.ExecuteReader();
                    String del = null;
                    
                    while (read.Read())
                    {
                        dbname = read["DBname"].ToString();
                        id = (int)read["RdRecordID"];
                        autoid = (int)read["RdRecordsID"];
                        iQuantity = (decimal)read["iQuantity"];
                        cInvCode = read["cInvCode"].ToString();
                        cBatch = read["cBatch"].ToString();
                        int DjBth2 = (int)read["DjBth"];
                        id2 = id;
                        //dbURL="jdbc:sqlserver://localhost:1433;DatabaseName="+dbname;
                        //con2 = DriverManager.getConnection(dbURL, userName, userPwd);

                        //System.out.println("Connection Successful!");
                        //sta2 = con2.createStatement();


                        string connectstirng = "uid=" + _uid + "; password=" + _pwd + "; initial catalog=" + dbname + "; Server=" + _ip;
                        //sta2.execute("alter TABLE Rdrecords  DISABLE TRIGGER Rdrecords_delete ");
                        if(kk==0)
                        {   
                            con2 = new SqlConnection(connectstirng);
                            con2.Open();
                            stc2 = con2.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "SQLTransaction");
                        }
                        kk++;
                        del = "delete from Rdrecords where id=" + id + " and autoId=" + autoid;
                        com2 = new SqlCommand(del, con2);
                        com2.Connection = con2;
                        com2.Transaction = stc2;
                        com2.ExecuteNonQuery();
                        Console.WriteLine("子表删除完毕");

                        del = "Update  CurrentStock Set  iQuantity =iQuantity+" + iQuantity + " Where cWhcode='05'  And  cInvCode ='" + cInvCode + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + cBatch + "' ";
                        com2.CommandText = del;
                        com2.ExecuteNonQuery();
                        Console.WriteLine("库存更新完毕");
                        del = "delete from JLWSYN..MXDZB where DjLsh=" + DjLsh + " and DjBth=" + DjBth2;
                        com2.CommandText = del;
                        //com2 = new SqlCommand(del, con2);
                        com2.ExecuteNonQuery();
                        //sta2.execute("alter TABLE Rdrecords ENABLE TRIGGER Rdrecords_delete ");

                        
                    }
                    read.Close();
                    del = "delete from Rdrecord where id=" + id2;
                    if (kk > 0)
                    {
                        com2.CommandText = del;
                        //com2 = new SqlCommand(del, con2);
                        com2.ExecuteNonQuery();
                        Console.WriteLine("主表删除完毕");
                    }
                }
                else
                {
                    String query = "select * from MXDZB where DjLsh=" + DjLsh + " and DjBth=" + DjBth;
                    //com.CommandText = query;
                    com = new SqlCommand(query, conn);
                    com.Connection = conn;
                    com.Transaction = stc;
                    read = com.ExecuteReader();
                    if (read.Read())
                    {
                        dbname = read["DBname"].ToString();
                        id = (int)read["RdRecordID"];
                        autoid = (int)read["RdRecordsID"];
                        iQuantity = (decimal)read["iQuantity"];
                        cInvCode = read["cInvCode"].ToString();
                        cBatch = read["cBatch"].ToString();
                    }
                    read.Close();
                    query = "select count(*) num from MXDZB where DjLsh=" + DjLsh;
                    com.CommandText = query;
                    //com = new SqlCommand(query, conn);
                    int num = (int)com.ExecuteScalar();
                    //conn.Close();

                    string connectstring = "uid=" + _uid + "; password=" + _pwd + "; initial catalog=" + dbname + "; Server=" + _ip;
                    con2 = new SqlConnection(connectstring);
                    con2.Open();
                    stc2 = con2.BeginTransaction(System.Data.IsolationLevel.ReadCommitted, "SQLTransaction");
                    Console.WriteLine("相关数据获取完毕，开始删除操作");
                    String del = null;
                    kk++;
                    if (num >1)
                    {
                        Console.WriteLine("明细对照表找到多条数据，只删除明细表");
                        del = "delete from Rdrecords where id=" + id + " and autoId=" + autoid + "";
                        com = new SqlCommand(del, con2);
                        com.Connection = con2;
                        com.Transaction = stc2;
                        com.ExecuteNonQuery();
                        Console.WriteLine("子表删除完毕");
                        del = "Update  CurrentStock Set  iQuantity =iQuantity+" + iQuantity + " Where cWhcode='05'  And  cInvCode ='" + cInvCode + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + cBatch + "' ";
                        com.CommandText = del;
                        //com = new SqlCommand(del, conn);
                        com.ExecuteNonQuery();
                        Console.WriteLine("库存更新完毕");

                        del = "delete from JLWSYN..MXDZB where DjLsh=" + DjLsh + " and DjBth=" + DjBth;
                        com.CommandText = del;
                        //com = new SqlCommand(del, conn);
                        com.ExecuteNonQuery();
                    }
                    //else
                    //{
                    //    Console.WriteLine("只找到一条相关数据，开始删除主表及子表");
                    //    del = "delete from Rdrecords where id=" + id + " and autoId=" + autoid + "";
                    //    com = new SqlCommand(del, con2);
                    //    com.Connection = con2;
                    //    com.Transaction = stc2;
                    //    com.ExecuteNonQuery();
                    //    Console.WriteLine("子表删除完毕");
                    //    del = "delete from Rdrecord where id=" + id;
                    //    com.CommandText = del;
                    //    //com = new SqlCommand(del, conn);
                    //    com.ExecuteNonQuery();
                    //    Console.WriteLine("主表删除完毕");
                    //    del = "Update  CurrentStock Set  iQuantity =iQuantity+" + iQuantity + " Where cWhcode='05'  And  cInvCode ='" + cInvCode + "' And cFree1 ='' And cFree2 ='' And cFree3 ='' And cFree4 ='' And cFree5 ='' And cFree6 ='' And cFree7 ='' And cFree8 ='' And cFree9 ='' And cFree10 =''  And cBatch ='" + cBatch + "' ";
                    //    com.CommandText = del;
                    //    //com = new SqlCommand(del, conn);
                    //    com.ExecuteNonQuery();
                    //    Console.WriteLine("库存更新完毕");
                    //    del = "delete from JWLSYN..MXDZB where DjLsh=" + DjLsh + " and DjBth=" + DjBth;
                    //    com.CommandText = del;
                    //    //com = new SqlCommand(del, conn);
                    //    com.ExecuteNonQuery();
                    //}
                }
                if (kk > 0)
                {
                    stc2.Commit();
                    con2.Close();
                }
                stc.Commit();
                conn.Close();
                
            }
            catch (Exception e)
            {
                stc2.Rollback();
                stc.Rollback();
                conn.Close();
                con2.Close();
                MessageBox.Show(e.ToString());
                try
                {
                }
                catch (SqlException e1)
                {
                    // TODO Auto-generated catch block

                }
                return 0;
            }
            Console.WriteLine("接口完成，退出");

            return 1;
        }

    }
}
