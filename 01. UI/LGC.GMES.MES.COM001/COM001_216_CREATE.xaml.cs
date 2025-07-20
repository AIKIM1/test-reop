/*************************************************************************************
 Created Date : 2018.01.12
      Creator : 
   Decription :
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;
using System.Windows.Input;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;

using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_216_CREATE : C1Window, IWorkArea
    {
        #region Declaration
        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
     
        private bool _load = true;
        private readonly Util _util = new Util();
        private readonly BizDataSet _bizDataSet = new BizDataSet();

        DataTable dtiUse = new DataTable();
        DataTable dtType = new DataTable();


        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize


        public COM001_216_CREATE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            Initialize();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetEvent();
        }

        private void Initialize()
        {
            CommonCombo combo = new CommonCombo();

            //동
            combo.SetCombo(cboCreateArea, CommonCombo.ComboStatus.NONE, sCase: "AREA");
            //CST 유형
            String[] sFilter1 = { "", "CARRIER_TYPE" };
            combo.SetCombo(cboCreateCstType, CommonCombo.ComboStatus.NONE, sFilter: sFilter1, sCase: "COMMCODES");

            String[] sFilter4 = { "", "CARRIER_ELEC" };
            combo.SetCombo(cboElecType, CommonCombo.ComboStatus.NONE, sFilter: sFilter4, sCase: "COMMCODES");

            String[] sFilter5 = { "", "CARRIER_PROD" };
            combo.SetCombo(cboProdType, CommonCombo.ComboStatus.NONE, sFilter: sFilter5, sCase: "COMMCODES");
        }

        private void SetEvent()
        {
            this.Loaded -= C1Window_Loaded;

            dtiUse = CommonCodeS("IUSE");

            if (dtiUse != null && dtiUse.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtiUse.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtiUse.Rows[i]["CBO_CODE"].ToString(), dtiUse.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }
            }

            dtType = CommonCodeS("CARRIER_TYPE");

            if (dtType != null && dtType.Rows.Count > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_CODE");
                dt.Columns.Add("CBO_NAME");

                DataRow newRow = null;

                for (int i = 0; i < dtType.Rows.Count; i++)
                {
                    newRow = dt.NewRow();
                    newRow.ItemArray = new object[] { dtType.Rows[i]["CBO_CODE"].ToString(), dtType.Rows[i]["CBO_NAME"].ToString() };
                    dt.Rows.Add(newRow);
                }
            }
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region User Method

        #region [BizCall]
        #endregion
   

        #region [Func]
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
        private DataTable CommonCodeS(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMM_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
                else
                    return null;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void chkOnlyOne_Click(object sender, RoutedEventArgs e)
        {
            bool bCheck = (bool)chkOnlyOne.IsChecked;
            if (bCheck)
            {
                tbFrom.Text = ObjectDic.Instance.GetObjectName("순번");
                txtTo.IsEnabled = false;
                txtTo.Value = 0;
            }
            else
            {
                tbFrom.Text = "From";
                txtTo.IsEnabled = true;
            }
        }
        #endregion

        #endregion

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if ((Convert.ToDecimal(txtFrom.Value.ToString()) < 0) || (Convert.ToDecimal(txtFrom.Value.ToString()) == 0))
                {
                    Util.MessageValidation("SFU4562");	//순번을 확인 하세요.
                    return;
                }

                bool bCheck = (bool)chkOnlyOne.IsChecked;
                if (!bCheck)
                {
                    if ((Convert.ToDecimal(txtTo.Value.ToString()) < 0) || (Convert.ToDecimal(txtTo.Value.ToString()) == 0))
                    {
                        Util.MessageValidation("SFU4562");  //순번을 확인 하세요.
                        return;
                    }

                    if (Convert.ToDecimal(txtFrom.Value.ToString()) > Convert.ToDecimal(txtTo.Value.ToString()))
                    {
                        Util.MessageValidation("SFU4562");  //순번을 확인 하세요.
                        return;
                    }

                    if (Convert.ToDecimal(txtFrom.Value.ToString()) == Convert.ToDecimal(txtTo.Value.ToString()))
                    {
                        Util.MessageValidation("SFU4562");  //순번을 확인 하세요.
                        return;
                    }

                }

                ShowLoadingIndicator();

                string sCSTID = string.Empty;

                DataTable dtBasicInfo = new DataTable();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("CSTOWNER", typeof(string));
                inTable.Columns.Add("CSTTYPE", typeof(string));
                inTable.Columns.Add("FROM_SEQ", typeof(Int32));
                inTable.Columns.Add("TO_SEQ", typeof(Int32));
                inTable.Columns.Add("ELEC", typeof(string));
                inTable.Columns.Add("PROD", typeof(string));
                inTable.Columns.Add("INSUSER", typeof(string));
                inTable.Columns.Add("ONLY_CHK", typeof(string));
                inTable.Columns.Add("SRCTYPE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["CSTOWNER"] = cboCreateArea.SelectedValue.ToString();
                newRow["CSTTYPE"] = cboCreateCstType.SelectedValue.ToString();
                newRow["FROM_SEQ"] = txtFrom.Value;
                newRow["TO_SEQ"] = txtTo.Value;
                newRow["ELEC"] = cboElecType.SelectedValue.ToString();
                newRow["PROD"] = cboProdType.SelectedValue.ToString();
                newRow["INSUSER"] = LoginInfo.USERID;
                newRow["ONLY_CHK"] = chkOnlyOne.IsChecked == true ? "Y" : "N";
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;

                inTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CREATE_RFID_CARRIER", "INDATA", null, inTable);

                Util.MessageInfo("SFU1270");      //저장되었습니다.          

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnCreate_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
