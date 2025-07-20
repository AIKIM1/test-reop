/*************************************************************************************
 Created Date : 2016.08.01
      Creator : Jeong Hyeon Sik
   Decription : Pack 작업지시 선택 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.01  Jeong Hyeon Sik : Initial Created.





 
**************************************************************************************/

using System;
using System.Data;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_014_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }       

        public string TEXT
        {
            get
            {
                return stxt;
            }

            set
            {
                stxt = value;
            }
        }
       
        private string stxt = "";      

        public PACK001_014_INPUT()
        {
            InitializeComponent();
        }

        #endregion

        #region Initialize

        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {               
                object[] tmps = C1WindowExtension.GetParameters(this);
                if (tmps != null)
                {
                    DataTable dtText = tmps[0] as DataTable;

                    if (dtText.Rows.Count > 0)
                    {
                        stxt = Util.NVC(dtText.Rows[0]["SCRP_RSN_NOTE"]);

                        txtInput.Text = stxt;
                    }
                }

                txtInput.Focus();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
      

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (txtInput.Text.Length > 0)
                {
                    TEXT = Util.GetCondition(txtInput);

                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();
                }
            }
        }

        #endregion


    }
}
