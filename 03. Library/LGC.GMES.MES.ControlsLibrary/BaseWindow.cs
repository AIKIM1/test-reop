using System;
using C1.WPF;

namespace LGC.GMES.MES.ControlsLibrary
{
    public class BaseWindow : C1Window
    {
        public event AfterOpenWidowEventHandler AfterOpenWidow;
        
        /// <summary>
        /// After Open Window
        /// </summary>
        /// <param name="sender"></param>
        public delegate void AfterOpenWidowEventHandler(object sender);

        private System.Timers.Timer openTimer;

        static BaseWindow()
        {
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.Loaded += BaseWindow_Loaded;           
        }

        private void BaseWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (openTimer == null)
            {
                openTimer = new System.Timers.Timer(1000);
                openTimer.Elapsed += OpenTimer_Elapsed;
                openTimer.Start();
            }
            this.Loaded -= BaseWindow_Loaded;
        }

        private void OpenTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                openTimer.Stop();
                Dispatcher.BeginInvoke(new Action(() => AfterOpenWidow?.Invoke(this)));
            }
            catch
            {
                openTimer.Stop();
            }
        }

    }
}
