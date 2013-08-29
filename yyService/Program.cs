using System;
using System.Collections.Generic;

using System.Windows.Forms;

using System.Data.SqlClient;
using yyService.DBupdate1;





using System.Web.Services;

using System.Diagnostics;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Threading;
using System.Xml;

namespace yyService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {


            Process[] processes = Process.GetProcessesByName("yyService");
            if (processes.Length >= 2)
            {
                MessageBox.Show("程序已经执行!", "提醒", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new DBsyn());
            }
        }

    }
}
