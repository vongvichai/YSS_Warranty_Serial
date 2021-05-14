using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace YSS_Warranty_Serial
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");
        [STAThread]
        
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FormMain());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("only one instance at a time");
                return;
            }
          

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());


        }
    }
}




       