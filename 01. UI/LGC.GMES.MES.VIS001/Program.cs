using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace LGC.GMES.MES.VIS001
{
    static class Program
    {
        /// <summary>
        /// 해당 응용 프로그램의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool bnew;
            Mutex mutex = new Mutex(true, "LGC.GMES.MES.Vision", out bnew);
            if (bnew)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainWindows());
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("프로그램이 실행중입니다.");
                Application.Exit();
            }

        }
    }
}
