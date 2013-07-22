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
using yyService.DBupdate1;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;



namespace yyService
{

    public partial class DBsyn : Form
    {

        public DBsyn()
        {
            InitializeComponent();



        }

        //信息传递标志
        public const int IM_closed = 0x0295;

        //传参对象
        connect con = null;
        //线程对象
        Thread newpro;
        //控制线程开停的对象
        public static bool runAble;

        //线程是否运行的标志位
        public bool isRun;
        //	检测网络链接是否成功的值
        public bool netIsOk = false;

        //	数据库名称
        String _DBname;

        //	数据库用户名
        String _name;

        //	数据库密码
        String _pwd;

        //	数据库表名
        String _table;

        //	数据库地址
        String _ip;

        //	数据库端口
        String _port;

        //	服务器地址
        String _Sip;

        //	服务器端口
        String _Sport;

        //睡眠时间
        int sptime;

        //图标对象
        Icon istr, isto;
        
        //检查网络连接的引入函数
        [DllImport("wininet.dll")]
        public static extern bool InternetGetConnectedState(ref int dwFlag, int dwReserved);

        //数据库操作
        SqlConnection sc1 = null;
        SqlConnection sc2 = null;
        SqlConnection sc3 = null;
 
        //	数据库连接状态
        SqlCommand scd1 = null;
        SqlCommand scd2 = null;
        SqlCommand scd3 = null;

        //数据库执行结果
        SqlDataReader sdr1 = null;
        SqlDataReader sdr2 = null;


        //窗体载入时的操作
        private void Form1_Load(object sender, EventArgs e)
        {
            
            istr = new Icon("播放2.ico");
            isto = new Icon("停止2.ico");
            con = new connect();
            getini();
            DBinit();
            this.textBox1.Text = _name;
            this.textBox2.Text = _pwd;
            this.textBox3.Text = _Sip;
            this.textBox4.Text = _Sport;
            runAble = true;
            isRun = false;
            startService();
        }

        //读取配置文件
        public void getini()
        {
            //读取xml
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("config.xml");
            XmlNode root = xmlDoc.SelectSingleNode("dataconfig");
            XmlNodeList xnl = root.ChildNodes;
            foreach (XmlNode xn1 in xnl)
            {
                XmlElement xe = (XmlElement)xn1;
                switch (xe.Name)
                {
                    case "DBname":
                        _DBname = xe.InnerText;
                        break;
                    case "table":
                        _table = xe.InnerText;
                        break;
                    case "name":
                        _name = xe.InnerText;
                        break;
                    case "password":
                        _pwd = xe.InnerText;
                        break;
                    case "ip":
                        _ip = xe.InnerText;
                        break;
                    case "port":
                        _port = xe.InnerText;
                        break;
                    case "serverIp":
                        _Sip = xe.InnerText;
                        break;
                    case "serverPort":
                        _Sport = xe.InnerText;
                        break;
                    case "sleeptime":
                        sptime =int.Parse(xe.InnerText);
                        break;
                }
            }

        }

        //开始按钮监听器
        private void button1_Click(object sender, EventArgs e)
        {
            startService();
            
        }

        //开始子线程函数
        void startService()
        {
            checkNet();
            _name = this.textBox1.Text;
            _pwd = this.textBox2.Text;
            _Sip = this.textBox3.Text;
            _Sport = this.textBox4.Text;
            if (netIsOk)
            {
                None.Icon = istr;
                isRun = true;
                runAble = true;
                con.init(_DBname, _table, _ip, _name, _pwd, _Sip, _Sport, sptime);
                con.runAble = true;
                newpro = new Thread(new ParameterizedThreadStart(run));
                newpro.Start(con);
                this.textBox1.Enabled = false;
                this.textBox2.Enabled = false;
                this.textBox3.Enabled = false;
                this.textBox4.Enabled = false;
                this.button1.Enabled = false;
                this.button2.Enabled = true;
                this.启动ToolStripMenuItem.Enabled = false;
                this.暂停ToolStripMenuItem.Enabled = true;
            }
            
            //con.setName("同步服务");
        }

        //停止子线程函数
        void stopService()
        {
            if (isRun)
            {
                None.Icon = isto;
                runAble = false;
                newpro.Interrupt();
                this.textBox1.Enabled = true;
                this.textBox2.Enabled = true;
                this.textBox3.Enabled = true;
                this.textBox4.Enabled = true;
                this.启动ToolStripMenuItem.Enabled = true;
                this.暂停ToolStripMenuItem.Enabled = false;
                this.button1.Enabled = true;
                this.button2.Enabled = false;
                isRun = false;
            }
        }

        //检查用友端数据库是否有新库添加，有的话新建触发器
        void DBinit()
        {
            string con = "Server=" + _ip + ";DataBase=JLWSYN;uid=" + _name + ";pwd=" + _pwd;
            string query = "select name from master.dbo.sysdatabases where name like 'UFDATA%' and name not in (select name from JLWSYN.dbo.ACCOUNT_TRIGGERED)";
            try
            {
                sc1 = new SqlConnection(con);
                sc1.Open();
                scd1 = new SqlCommand(query, sc1);
                sdr1 = scd1.ExecuteReader();
                StreamReader sr;
                string que2 = null;
                string name = null;
                string con2 = null;

                while (sdr1.Read())
                {
                    sr = new StreamReader("RdRecord.jlw");
                    que2 = sr.ReadToEnd();
                    name = sdr1["name"].ToString();
                    con2 = "Server=" + _ip + ";DataBase=" + name + ";uid=" + _name + ";pwd=" + _pwd;
                    sc2 = new SqlConnection(con2);
                    sc2.Open();
                    scd2 = new SqlCommand(que2, sc2);
                    scd2.ExecuteNonQuery();
                    sr.Close();

                    sr = new StreamReader("RdRecords_I.jlw");
                    que2 = sr.ReadToEnd();
                    scd2 = new SqlCommand(que2, sc2);
                    scd2.ExecuteNonQuery();
                    sr.Close();

                    sr = new StreamReader("RdRecords_D.jlw");
                    que2 = sr.ReadToEnd();
                    scd2 = new SqlCommand(que2, sc2);
                    scd2.ExecuteNonQuery();
                    sr.Close();

                    sr = new StreamReader("RdRecords_U.jlw");
                    que2 = sr.ReadToEnd();
                    scd2 = new SqlCommand(que2, sc2);
                    scd2.ExecuteNonQuery();
                    sr.Close();

                    sc2.Close();

                    sc3 = new SqlConnection(con);
                    sc3.Open();
                    string que = "insert JLWSYN.dbo.ACCOUNT_TRIGGERED (name) values ('" + name + "')";
                    scd3 = new SqlCommand(que, sc3);
                    scd3.ExecuteNonQuery();
                    sc3.Close();
                }
                sdr1.Close();
                sc1.Close();
            }
            catch(SqlException e)
            {              
                MessageBox.Show("用友数据库未能正确连接！\r\n请确保数据库服务开启和网络通畅后再尝试打开程序");
                //SendMessage(this.Handle, IM_closed, 80, 0);
                this.Close();
            }
            
        }

        //	检查网络连接的函数
        public void checkNet()
        {
            int flag=0;
            
            netIsOk = InternetGetConnectedState(ref flag,0);
            if (netIsOk)
            {
                DBupdateService a = new DBupdateService(_Sip + ":" + _Sport);
                try
                {
                    int i = a.test();
                    netIsOk = true;
                }
                catch (WebException e)
                {
                    netIsOk = false;
                    MessageBox.Show("优时服务器未开启。请检查服务器电脑网络连接，并查看服务是否开启！");
                    //SendMessage(this.Handle, IM_closed, 80, 0);
                }
            }
            else
            {
                MessageBox.Show("未连接网络。请连接好网络后再尝试打开服务！");
            }
        }

        //消息发送函数
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(
               IntPtr hWnd,　　　// handle to destination window
               int Msg,　　　 // message
               int wParam,　// first message parameter
               int lParam // second message parameter
         );

        //消息接收函数
        protected override void DefWndProc(ref System.Windows.Forms.Message m)
        {
            switch (m.Msg)
            {
                case IM_closed:
                    ///string与MFC中的CString的Format函数的使用方法有所不同
                    
                    string message = string.Format("收到消息!参数为:{0},{1}", m.WParam, m.LParam);
                    MessageBox.Show(message);///显示一个消息框
                    Thread.Sleep(10000);
                    startService();
                    break;
                default:
                    base.DefWndProc(ref m);///调用基类函数处理非自定义消息。
                    break;
            }
        }

        //子线程函数
        public void run(object A)
        {
            StreamWriter sw = new StreamWriter("dlog.txt", true, Encoding.Default);
            sw.WriteLine("线程开始运行");
            
            connect a = A as connect;
            //			正式程序段        
            sw.WriteLine("sleeptime:"+a.sleeptime);
            string sqlconnetdata = "Server="+a._IP+";DataBase=" + a._DBname + ";uid=" + a._username + ";pwd=" + a._userpwd;
            //string sqlconnetdata = "Server=192.168.1.112;Initial Catalog=UFDATA_401_2013;User ID=sa;Password=sa";
           // string sqlconnetdata = "Server=192.168.1.112:1433/SQLEXPRESS;Initial Catalog=UFDATA_401_2013;Integrated Security=True";
            string query = "select * from " + a._TBname;

            try
            {
                a.con = new SqlConnection(sqlconnetdata);
                a.con1 = new SqlConnection(sqlconnetdata);
                sw.WriteLine("con连接正常，开始大循环");
                //		外层循环，每隔sleeptime毫秒运行一次
                while (runAble)
                {
                    Thread.Sleep(a.sleeptime);
                    //wait(sleeptime);
                    //System.out.println("start drivername");
                    a.con.Open();
                    a.con1.Open();
                    a.sta = new SqlCommand(query, a.con);

                    sw.WriteLine("sta创建正常");
                    a.sqlread = a.sta.ExecuteReader();
                    sw.WriteLine("开始read循环");
                    while (a.sqlread.Read() && runAble)
                    {
                        int sign = (int)a.sqlread["Operation"];
                        int did = 0;
                        a.closeAble = false;
                        while (did == 0 && a.runAble)
                        {
                            switch (sign)
                            {
                                case 0:
                                    sw.WriteLine("引用1号接口");
                                    did = a.dbd.outboundUpdateMain((int)a.sqlread["RdRecordsID"], (int)a.sqlread["bRdFlag"], a.sqlread["dDate"].ToString(), a.sqlread["cBatch"].ToString(), a.sqlread["cInvName"].ToString(), (double)a.sqlread["iQuantity"], a.sqlread["cInvStd"].ToString(), (decimal)a.sqlread["iPrice"], (double)a.sqlread["iUnitCost"]);
                                    sw.WriteLine("1号接口引用结束");
                                    break;
                                case 1:
                                    sw.WriteLine("引用2号接口");
                                    did = a.dbd.outboundInsertMain((int)a.sqlread["RdRecordsID"], (int)a.sqlread["bRdFlag"], a.sqlread["dDate"].ToString(), a.sqlread["cBatch"].ToString(), a.sqlread["cInvName"].ToString(), a.sqlread["cAcc_Name"].ToString(), (double)a.sqlread["iQuantity"], a.sqlread["cInvStd"].ToString(), a.sqlread["cVenName"].ToString(), a.sqlread["cCode"].ToString(), a.sqlread["cDepName"].ToString(), a.sqlread["cBusType"].ToString(), a.sqlread["cMaker"].ToString(), (decimal)a.sqlread["iPrice"], (double)a.sqlread["iUnitCost"]);
                                    sw.WriteLine("2号接口引用结束");
                                    break;
                                case 2:
                                    sw.WriteLine("引用3号接口");
                                    did = a.dbd.outboundDeleteMain((int)a.sqlread["RdRecordsID"]);
                                    sw.WriteLine("3号接口引用结束");
                                    break;
                                case 3:
                                    sw.WriteLine("引用4号接口");
                                    did = a.dbd.outboundUpdateMainOrder((int)a.sqlread["RdRecordsID"], a.sqlread["cCode"].ToString(), a.sqlread["dDate"].ToString(), a.sqlread["cBusType"].ToString(), a.sqlread["cVenName"].ToString(), a.sqlread["cMemo"].ToString(), a.sqlread["cDepName"].ToString());
                                    sw.WriteLine("4号接口引用结束");
                                    break;

                            }
                        }
                        if (!runAble)
                        {
                            a.sqlread.Close();
                            break;
                        }

                        //完成这一条数据的同步，进行删除

                        int ss = (int)a.sqlread["AutoID"];

                        sw.WriteLine("开始delete");
                        String deleteQ = "delete from " + a._TBname + " where AutoID='" + ss + "'";
                        a.scd = new SqlCommand(deleteQ, a.con1);
                        a.scd.ExecuteNonQuery();
                        sw.WriteLine("delete结束");
                        a.closeAble = true;

                    }
                    a.con1.Close();
                    a.con.Close();
                    sw.WriteLine("while 大循环结束");
                }
                if(!runAble)
                    sw.WriteLine("runable=false!!lo");
            }
            catch (ThreadInterruptedException e)
            {
                Console.WriteLine("interrupt");
                //MessageBox.Show("zhongduan tuichu");
                sw.Close();
            }
            catch (ThreadAbortException e)
            {
                
            }
            catch(WebException e)
            {
                SendMessage(this.Handle, IM_closed, 80, 0);
            }
            catch (SqlException e)
            {

                Console.WriteLine("sql error");
                MessageBox.Show("本地数据库连接异常！请检查数据库服务是否开启，并检查用户名密码正确输入！");
                sw.Close();
                SendMessage(this.Handle, IM_closed, 80, 0);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                sw.Close();
                Console.WriteLine("error");
                try
                {
                    a.con.Close();
                    //	res.close();
                }
                catch (Exception o)
                {
                }
            }
            Console.WriteLine("线程关闭!");
            sw.Close();
        }

        //停止按钮监听器
        private void button2_Click(object sender, EventArgs e)
        {
            stopService();
        }

        //最小化按钮监听器
        private void button3_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        //最小化按钮
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.None.Visible = true;
            } 
        }
        //托盘图标单击
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {

            this.None.ContextMenuStrip = contextMenuStrip1;
            if (e.Button == MouseButtons.Right)
            {
                this.contextMenuStrip1.Visible = true;
            }
            //else
            //{

            //    this.Show();
            //    this.Focus();
            //    //this.None.Visible = false;
               
            //}

        }

        //托盘停止按钮
        private void 暂停ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            stopService();
        }

        //托盘退出按钮
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (isRun)
            {
                runAble = false;
                newpro.Interrupt();
            }
            this.Close();
        }

        //托盘启动按钮
        private void 启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            startService();
            
        }

        //托盘双击操作监听器
        private void None_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //this.None.ContextMenuStrip = contextMenuStrip1;
            if (e.Button == MouseButtons.Left)
            {
                this.TopMost = true;
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.TopMost = false;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
        //托盘帮助按钮
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("开发商：景联文科技有限公司\r\n联系电话：0571-88942537\r\n版本：v1.0");
        }

        //窗体关闭按钮
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (isRun)
            {
                runAble = false;
                newpro.Interrupt();
            }
            contextMenuStrip1.Close();
            //this.Close();
        }

       





    }
}
