/*************************************************************************************
 Created Date : 2020.11.10
      Creator : Dooly
   Decription : Tray 선택
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.10  DEVELOPER : Initial Created.
  2023.09.14  이의철 : E등급 SPEC 추가
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.FCS001
{
    public partial class FCS001_019_TRAY_SEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _LotID = string.Empty;
        private string _GRADE = string.Empty;
        
        public FCS001_019_TRAY_SEL()
        {
            InitializeComponent();

        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        #endregion

        #region Initialize

        
        #endregion

        #region Event
        
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _LotID = tmps[0] as string;

            //E등급 SPEC 추가
            if (tmps.Length > 1)
            {
                _GRADE = tmps[1] as string;
            }            
            GetList();
        }

        private void dgTraySel_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();
            if (e.Row.Index != dgTraySel.Rows.Count) //마지막 RowHeader 표시 x
            {
                tb.Text = (e.Row.Index + 1 - dgTraySel.TopRows.Count).ToString();
                tb.VerticalAlignment = VerticalAlignment.Center;
                tb.HorizontalAlignment = HorizontalAlignment.Center;
                e.Row.HeaderPresenter.Content = tb;
            }
        }
        #endregion

        #region Mehod
        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("DAY_GR_LOTID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["DAY_GR_LOTID"] = _LotID;
                dtRqst.Rows.Add(dr);

                //E등급 SPEC 추가
                string sBiz = "DA_SEL_W_LOT_CELL_ID";
                if(_GRADE.Equals("E"))
                {
                    sBiz = "DA_SEL_E_LOT_CELL_ID";
                }

                ShowLoadingIndicator();
                //DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_W_LOT_CELL_ID", "INDATA", "OUTDATA", dtRqst);
                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgTraySel, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }
        #endregion

        
    }
}
