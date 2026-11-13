using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using rakan_eservices.Properties;

namespace rakan_eservices
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        private static eService objService;

        private static NotifyIcon notifyIcon;

        static async Task Main(string[] args)
        {
            try
            {
                System.Windows.Forms.Application.EnableVisualStyles();
                System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
                // We need to dispose here, or the icon will not remove until the 
                // system tray is updated.
                System.Windows.Forms.Application.ApplicationExit += delegate { notifyIcon.Dispose(); };
                CreateNotifyIcon();

                var handle = GetConsoleWindow();
                ShowWindow(handle, SW_HIDE);
                string strFileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "LOG", System.DateTime.Today.ToString("yyyyMMdd"), "APP.LOG");
                Logging.InitLogFile(strFileName);
                objService = new eService();
                objService.InfoLog += REDA_InfoLog;

                if (objService != null)
                {
                    await Task.Run(() => objService.SendStudentInfoToMyEG());
                }
                BalloonTip();
            }
            catch (Exception ex)
            {
                Logging.WriteLine(ex.Message, System.Diagnostics.EventLogEntryType.Error);
            }

        }
        private static void CreateNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = Resources.services,
            };
            notifyIcon.Visible = true;
        }
        /// <summary>
        /// Show bubble when task is already finish execute.
        /// Does not 
        /// </summary>
        private static void BalloonTip()
        {
            notifyIcon.Visible = true;
            notifyIcon.Icon = SystemIcons.Information;
            notifyIcon.ShowBalloonTip(740, "Sync to MyEG backend finish.", "REDA e-Services", ToolTipIcon.Info);
        }
        private static void REDA_InfoLog(string info)
        {
            AppendTextBox(info, false);
        }
        public static void AppendTextBox(string value, bool isError)
        {
            if (isError)
                Logging.WriteLine(value, System.Diagnostics.EventLogEntryType.Error);
            else
                Logging.WriteLine(value, System.Diagnostics.EventLogEntryType.Information);
        }
    }
}
