/*************************************************************************************
 Created Date : 2018.05.02
      Creator : 최승혁
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]  
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class; 
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace LGC.GMES.MES.MNT001
{
    public partial class MNT001_011_HoldList : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _AREAID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PRODID = string.Empty;
        private string _MODLID = string.Empty;
        private string _PJT    = string.Empty; 
         

        public MNT001_011_HoldList()
        {
            InitializeComponent();

            this.Width =  Screen.PrimaryScreen.Bounds.Width;  
            this.Height =  Screen.PrimaryScreen.Bounds.Height;
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _AREAID = (string)tmps[0];
            _EQSGID = (string)tmps[1];
            _PRODID = (string)tmps[2]; 
            _PJT = (string)tmps[3]; 
 
            GetLotList();

        }
 

        #endregion

        #region Event

        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;          
        }

        #endregion


        #region Mehod


        public void GetLotList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("PRODID", typeof(string));
                dtRqst.Columns.Add("MODLID", typeof(string));
                dtRqst.Columns.Add("PJT", typeof(string));
                dtRqst.Columns.Add("SKIDID", typeof(string));

                DataRow dr = dtRqst.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _AREAID;
                dr["EQSGID"] = _EQSGID;
                dr["PRODID"] = _PRODID;
                //dr["MODLID"] = _MODLID;
                dr["PJT"] = _PJT;

                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOLD_LOT_LIST_MNT", "INDATA", "OUTDATA", dtRqst);

                if (dtRslt.Rows.Count == 0)
                {
                    
                }
                else
                {
                    // dgListHold.ItemsSource = DataTableConverter.Convert(dtRslt);
                    Util.GridSetData(dgListHold, dtRslt, FrameOperation, true);
                } 
               
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }


        #endregion
    }
}
