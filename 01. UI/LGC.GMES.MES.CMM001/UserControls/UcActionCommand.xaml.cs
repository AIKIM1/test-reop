/*************************************************************************************
 Created Date : 2021.06.17
      Creator : 조영대
   Decription : Action Command
--------------------------------------------------------------------------------------
 [Change History]
 2021.06.17  조영대 : Initial Created.
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System;
using System.Windows.Input;
using System.Collections.Generic;

namespace LGC.GMES.MES.CMM001.UserControls
{
    public partial class UcActionCommand : UserControl
    {
        #region Properties

        public event CommandButtonClickEventHandler ButtonClick;
        public delegate void CommandButtonClickEventHandler(object sender, string buttonName);

        public event CommandButtonLoadedPresenterEventHandler ButtonLoadedPresenter;
        public delegate void CommandButtonLoadedPresenterEventHandler(object sender, Button button);

        readonly Dictionary<string, Button> buttonList = new Dictionary<string, Button>();
        
        public Common.IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        #endregion

        public UcActionCommand()
        {
            InitializeComponent();            
        }

        #region Event

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SetScroll();
        }

        private void UserControl_Initialized(object sender, EventArgs e)
        {
            InitializeControls();
        }

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue.Equals(true))
            {
                foreach (Button btn in buttonList.Values)
                {
                    ButtonLoadedPresenter?.Invoke(this, btn);
                }                
            }
        }

        private void svButtonScroll_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetScroll();
        }
        
        private void btnLeftScroll_Click(object sender, RoutedEventArgs e)
        {
            svButtonScroll.ScrollToHorizontalOffset(svButtonScroll.HorizontalOffset - 50);
        }

        private void btnRightScroll_Click(object sender, RoutedEventArgs e)
        {
            svButtonScroll.ScrollToHorizontalOffset(svButtonScroll.HorizontalOffset + 50);
        }

        private void spButtonPanel_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                svButtonScroll.ScrollToHorizontalOffset(svButtonScroll.HorizontalOffset - 50);
            }
            else if (e.Delta < 0)
            {
                svButtonScroll.ScrollToHorizontalOffset(svButtonScroll.HorizontalOffset + 50);
            }
        }

        private void btnCommon_Click(object sender, RoutedEventArgs e)
        {
            Button clickButton = sender as Button;

            ButtonClick?.Invoke(clickButton, clickButton.Name);
        }
        #endregion

        #region Public Method

        public void ClearButton()
        {
            foreach (Button btn in buttonList.Values)
            {
                if (spButtonPanel.Children.Contains(btn)) spButtonPanel.Children.Remove(btn);
            }
            buttonList.Clear();
        }

        public void AddButton(string buttonName, string buttonText, bool enabled = true, Visibility visibility = Visibility.Visible)
        {
            if (FrameOperation == null || FrameOperation.AUTHORITY.Equals("R")) return;

            if (!buttonList.ContainsKey(buttonName))
            {
                if (buttonList.Count > 0)
                {
                    Button addBorder = new Button();
                    addBorder.Width = 3;
                    addBorder.BorderThickness = new Thickness(0);
                    addBorder.Cursor = Cursors.Arrow;
                    spButtonPanel.Children.Add(addBorder);
                }

                Button addButton = new Button();
                addButton.Name = buttonName;
                addButton.Content = Common.ObjectDic.Instance.GetObjectName(buttonText);
                addButton.Style = btnLeftScroll.Style;
                addButton.IsEnabled = enabled;
                addButton.Visibility = visibility;

                addButton.Click += btnCommon_Click;

                spButtonPanel.Children.Add(addButton);

       

                buttonList.Add(buttonName, addButton);
            }
        }

        public void RemoveButton(string buttonName)
        {
            if (buttonList.ContainsKey(buttonName))
            {
                spButtonPanel.Children.Remove(buttonList[buttonName]);
                buttonList.Remove(buttonName);
            }
        }

        #endregion

        #region Method

        private void InitializeControls()
        {

        }
        
        private void SetScroll()
        {
            if (spButtonPanel.ActualWidth > svButtonScroll.ActualWidth)
            {
                btnLeftScroll.Visibility = Visibility.Visible;
                btnRightScroll.Visibility = Visibility.Visible;
            }
            else
            {
                btnLeftScroll.Visibility = Visibility.Collapsed;
                btnRightScroll.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

   
    }
}