/*************************************************************************************
 Created Date : 2024.11.19
      Creator : 이승신
   Decription : Hold Releae Date 변경 PopUp(PACK)
--------------------------------------------------------------------------------------
 [Change History]
 2024.11.20     Initial Created
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_103_POPUP.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_103_POPUP : C1Window, IWorkArea
    {
        private DataSet _dsHoldLotList;
        public PACK001_103_POPUP()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();

            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters == null)
                return;

            _dsHoldLotList = new DataSet();
            _dsHoldLotList = parameters[0] as DataSet;

        }
        private void InitCombo()
        {

        }

        private void Init()
        {
            dtExpected.SelectedDateTime = DateTime.Now.AddDays(30);
        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
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

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));
        }


        private void btnComplete_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                DateTime baseDate = DateTime.Now;
                if (Convert.ToDecimal(baseDate.ToString("yyyyMMdd")) > Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")))
                {
                    dtExpected.Text = baseDate.ToLongDateString();
                    dtExpected.SelectedDateTime = baseDate;
                    Util.MessageValidation("SFU1738");  //오늘 이전 날짜는 선택할 수 없습니다.
                    return;
                }

                //저장하시겠습니까?
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                "SFU1241"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        DataSave();
                    }
                });

            }

            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }


        private void dtExpected_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtExpected.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                Util.AlertInfo("SFU1740");  //오늘 이후 날짜만 지정 가능합니다.
                dtExpected.SelectedDateTime = DateTime.Now;
            }
        }

        private void DataSave()
        {
            DataSet inData = new DataSet();

            DataTable inDataTable = inData.Tables.Add("INDATA");
            inDataTable.Columns.Add("SRCTYPE", typeof(string));
            inDataTable.Columns.Add("LANGID", typeof(string));
            inDataTable.Columns.Add("USERID", typeof(string));
            inDataTable.Columns.Add("LOTID", typeof(string));
            inDataTable.Columns.Add("UNHOLD_SCHD_DATE", typeof(string));
            inDataTable.Columns.Add("HOLD_DTTM", typeof(string));

            DataRow row = null;

            for (int i = 0; i < _dsHoldLotList.Tables[0].Rows.Count; i++)
            {
                row = inDataTable.NewRow();
                row["SRCTYPE"] = Util.NVC(_dsHoldLotList.Tables[0].Rows[i]["SRCTYPE"].ToString());
                row["LANGID"] = Util.NVC(_dsHoldLotList.Tables[0].Rows[i]["LANGID"].ToString());
                row["USERID"] = Util.NVC(_dsHoldLotList.Tables[0].Rows[i]["USERID"].ToString());
                row["LOTID"] = Util.NVC(_dsHoldLotList.Tables[0].Rows[i]["LOTID"].ToString());
                row["HOLD_DTTM"] = Util.NVC(_dsHoldLotList.Tables[0].Rows[i]["HOLD_DTTM"].ToString());
                row["UNHOLD_SCHD_DATE"] = Util.GetCondition(dtExpected);

                inDataTable.Rows.Add(row);
            }

            try
            {
                //저장 처리
                DataSet dsRslt = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_HOLD_LOT_RELEASE_DATE_CHANGE_PACK", "INDATA", null, inData);

                Util.AlertInfo("SFU1270");  //저장되었습니다

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.AlertByBiz("BR_PRD_UPD_HOLD_LOT_RELEASE_DATE_CHANGE_PACK", ex.Message, ex.ToString());
            }
        }
    }
}
