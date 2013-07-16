using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Data.SqlClient;
using yyService.DBupdate1;
using System.Xml;
using System.IO;


namespace yyService
{

    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();



        }
    
        //传参对象
        connect con = null;
        //线程对象
        Thread newpro;
        //控制线程开停的对象
        public static bool runAble;

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

        //窗体载入时的操作
        private void Form1_Load(object sender, EventArgs e)
        {
            getini();
            this.textBox1.Text = _name;
            this.textBox2.Text = _pwd;
            this.textBox3.Text = _Sip;
            this.textBox4.Text = _Sport;
            runAble = true;
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
            checkNet();
            _name = this.textBox1.Text;
            _pwd = this.textBox2.Text;
            _Sip = this.textBox3.Text;
            _Sport = this.textBox4.Text;
            checkNet();
            if (netIsOk)
            {
                runAble = true;
                startService();
                con.runAble = true;
                newpro = new Thread(new ParameterizedThreadStart(run));
                newpro.Start(con);
                this.button1.Enabled = false;
                this.button2.Enabled = true;
                this.启动ToolStripMenuItem.Enabled = false;
                this.暂停ToolStripMenuItem.Enabled = true;
            }
        }
        //传参对象初始化函数
        void startService()
        {

            con.init(_DBname, _table, _ip, _name, _pwd, _Sip, _Sport,sptime);
            //con.setName("同步服务");
        }
        //	检查网络连接的函数
        public void checkNet()
        {
            Ping p = new Ping();//创建Ping对象p
            PingReply pr = p.Send(_Sip);//向指定IP或者主机名的计算机发送ICMP协议的ping数据包
            if (pr.Status == IPStatus.Success)//如果ping成功
            {
                Console.WriteLine("网络连接成功, 执行下面任务...");
                netIsOk = true;
            }
            else
            {
                int times = 0;//重新连接次数;
                do
                {
                    if (times >= 1)
                    {
                        MessageBox.Show("重新尝试连接超过12次,连接失败程序结束");
                        netIsOk = false;
                        return;
                    }
                    //  Thread.Sleep(5000);//等待十分钟(方便测试的话，你可以改为1000)
                    pr = p.Send(_Sip);
                    Console.WriteLine(pr.Status);
                    times++;
                }
                while (pr.Status != IPStatus.Success);
                 MessageBox.Show("连接成功");
                netIsOk = true;
                times = 0;//连接成功，重新连接次数清为0;
            }
        }

        //子线程函数
        public void run(object A)
        {
            StreamWriter sw = new StreamWriter("dlog.txt", true, Encoding.Default);
            sw.WriteLine("线程开始运行");
            
            connect a = A as connect;
            //			正式程序段        

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
                                    did = a.dbd.outboundInsertMain((int)a.sqlread["RdRecordsID"], (int)a.sqlread["bRdFlag"], a.sqlread["dDate"].ToString(), a.sqlread["cBatch"].ToString(), a.sqlread["cInvName"].ToString(), a.sqlread["cWhCode"].ToString(), (double)a.sqlread["iQuantity"], a.sqlread["cInvStd"].ToString(), a.sqlread["cVenName"].ToString(), a.sqlread["cCode"].ToString(), a.sqlread["cDepName"].ToString(), a.sqlread["cBusType"].ToString(), a.sqlread["cMaker"].ToString(), (decimal)a.sqlread["iPrice"], (double)a.sqlread["iUnitCost"]);
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
                Console.WriteLine("lalala");
                //MessageBox.Show("zhongduan tuichu");
                sw.Close();
            }
            catch (ThreadAbortException e)
            {
                
            }
            catch (SqlException e)
            {

                Console.WriteLine("sql error");
                MessageBox.Show(e.ToString());
                sw.Close();

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
            runAble = false;
            newpro.Interrupt();
            //newpro.Abort();
            this.button1.Enabled = true;
            this.button2.Enabled = false;
            this.启动ToolStripMenuItem.Enabled = true;
            this.暂停ToolStripMenuItem.Enabled = false;
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
            if (this.button1.Enabled == false)
            {
                runAble = false;
                newpro.Interrupt();
                //newpro.Abort();
                this.启动ToolStripMenuItem.Enabled = true;
                this.暂停ToolStripMenuItem.Enabled = false;
                this.button1.Enabled = true;
                this.button2.Enabled = false;
            }
        }
        //托盘退出按钮
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            runAble = false;
            newpro.Interrupt();
            this.Close();
        }

        //托盘启动按钮
        private void 启动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            checkNet();
            _name = this.textBox1.Text;
            _pwd = this.textBox2.Text;
            _Sip = this.textBox3.Text;
            _Sport = this.textBox4.Text;
            checkNet();
            if (netIsOk)
            {
                runAble = true;
                startService();
                con.runAble = true;
                newpro = new Thread(new ParameterizedThreadStart(run));
                newpro.Start(con);
                this.button1.Enabled = false;
                this.button2.Enabled = true;
                this.启动ToolStripMenuItem.Enabled = false;
               this.暂停ToolStripMenuItem.Enabled = true;
            }
            
        }

        //托盘双击操作监听器
        private void None_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            //this.None.ContextMenuStrip = contextMenuStrip1;
            if (e.Button == MouseButtons.Left)
                this.Show();
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
        //托盘帮助按钮
        private void 帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("开发商：景联文科技有限公司\r\n联系电话：0571-8877xxxx\r\n版本：v1.0");
        }
        //窗体关闭按钮
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            runAble = false;
            newpro.Interrupt();
            this.Close();
        }

       





    }
}
