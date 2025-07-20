/*************************************************************************************
 Created Date : 2017.11.20
      Creator : 이슬아
   Decription : 전지 5MEGA-GMES 구축 - 출하HOLD 관리 - HOLD 등록 팝업
--------------------------------------------------------------------------------------
 [Change History]

 2023.03.13  LEEHJ     : 소형활성화 MES 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;
using C1.WPF.DataGrid.Summaries;
using System.Windows.Media;
using static LGC.GMES.MES.CMM001.Class.CommonCombo;
using LGC.GMES.MES.CMM001.Popup;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// BOX001_201_RUNSTART.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_307_UPDATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtInfo;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public FCS002_307_UPDATE()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {      
            InitControl();
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {   
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            dtInfo = (DataTable)tmps[0];

            if (dtInfo == null)
            {
                //SFU1645 선택된 작업대상이 없습니다.
                Util.MessageValidation("SFU1645");
                return;
            }
        }

        #endregion

        #region 저장/닫기 버튼 이벤트

        /// <summary>
        /// HOLD 수정
        /// BIZ : BR_PRD_REG_MODIFY_ASSY_HOLD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            //{
            //    // SFU1740  오늘 이후 날짜만 지정 가능합니다.
            //    Util.MessageValidation("SFU1740");
            //}

            int iHoldRegQty;
            if (!string.IsNullOrWhiteSpace(txtHoldRegQty.Text) && !int.TryParse(txtHoldRegQty.Text, out iHoldRegQty))
            {
                //SFU3435	숫자만 입력해주세요
                Util.MessageInfo("SFU3435");
            }

            if (string.IsNullOrEmpty(txtUser.Text))
            {
                //SFU4350 해제 예정 담당자를 선택하세요.
                Util.MessageValidation("SFU4350");
                return;
            }

            if (string.IsNullOrEmpty(txtNote.Text))
            {
                //SFU4300 Hold 사유를 입력하세요.
                Util.MessageValidation("SFU4300");
                return;
            }

            if (dtInfo.Rows.Count < 1)
            {
                //SFU3552	저장 할 DATA가 없습니다.	
                Util.MessageValidation("SFU3552");
                return;
            }


            //SFU4340	수정 하시겠습니까?	
            Util.MessageConfirm("SFU4340", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    Update();
                }
            });
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region Biz

        /// <summary>
        /// Hold 수정 
        /// </summary>
        private void Update()
        {
            try
            {   
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("UNHOLD_SCHD_DATE");
                inDataTable.Columns.Add("UNHOLD_CHARGE_USERID"); 
                inDataTable.Columns.Add("HOLD_NOTE");
                inDataTable.Columns.Add("HOLD_REG_QTY");


                DataTable inHoldTable = inDataSet.Tables.Add("INHOLD");
                inHoldTable.Columns.Add("HOLD_ID");

                DataTable inTable = inDataSet.Tables["INDATA"];
                DataRow newRow = inDataTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["UNHOLD_SCHD_DATE"] = dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd");
                newRow["UNHOLD_CHARGE_USERID"] = txtUser.Tag;
                newRow["HOLD_NOTE"] = txtNote.Text;
                newRow["HOLD_REG_QTY"] = string.IsNullOrEmpty(Util.NVC(txtHoldRegQty.Text)) ? null : Util.NVC(txtHoldRegQty.Text);
                inDataTable.Rows.Add(newRow);
                newRow = null;

                for (int row = 0; row < dtInfo.Rows.Count; row++)
                {
                    newRow = inHoldTable.NewRow();
                    newRow["HOLD_ID"] = dtInfo.Rows[row]["HOLD_ID"];
                    inHoldTable.Rows.Add(newRow);
                }
                //BR_PRD_REG_MODIFY_ASSY_HOLD
                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_MODIFY_ASSY_HOLD_F", "INDATA,INHOLD", null, (result, exception) =>
                {
                    try
                    {
                        if (exception != null)
                        {
                            Util.MessageException(exception);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                        this.DialogResult = MessageBoxResult.Cancel;
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }    
        #endregion

        #region Method
        private void GetUserWindow()
        {
            CMM_PERSON wndPerson = new CMM_PERSON();
            wndPerson.FrameOperation = FrameOperation;

            if (wndPerson != null)
            {
                object[] Parameters = new object[1];
                Parameters[0] = txtUser.Text;
                C1WindowExtension.SetParameters(wndPerson, Parameters);

                wndPerson.Closed += new EventHandler(wndUser_Closed);
                //grdMain.Children.Add(wndPerson);
                //wndPerson.BringToFront();

                foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
                {
                    if (tmp.Name == "grdMain")
                    {
                        tmp.Children.Add(wndPerson);
                        wndPerson.BringToFront();
                        break;
                    }
                }
            }
        }

        private void wndUser_Closed(object sender, EventArgs e)
        {
            CMM_PERSON wndPerson = sender as CMM_PERSON;
            if (wndPerson.DialogResult == MessageBoxResult.OK)
            {

                txtUser.Text = wndPerson.USERNAME;
                txtUser.Tag = wndPerson.USERID;
                txtDept.Text = wndPerson.DEPTNAME;
                txtDept.Tag = wndPerson.DEPTID;

            }

            foreach (Grid tmp in Util.FindVisualChildren<Grid>(Application.Current.MainWindow))
            {
                if (tmp.Name == "grdMain")
                {
                    tmp.Children.Remove(wndPerson);
                    break;
                }
            }
        }

        private DataTable AddStatus(DataTable dt, ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case ComboStatus.ALL:
                    dr[sDisplay] = "";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        #endregion

        private void dgHold_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            if (e.Column.Name == "STRT_SUBLOTID" || e.Column.Name == "END_SUBLOTID")
            {
                string sLotType = Util.NVC(dataGrid.GetCell(e.Row.Index, dataGrid.Columns["HOLD_TRGT_CODE"].Index).Value);               

                if (string.IsNullOrWhiteSpace(sLotType))
                {
                    //	SFU4349		보류범위 먼저 선택하세요.	
                    Util.MessageValidation("SFU4349");
                    e.Cancel = true;
                }
                else if (sLotType == "LOT")
                    e.Cancel = true;               
            }
        }

        private void btnUser_Click(object sender, RoutedEventArgs e)
        {
            GetUserWindow();
        }

        private void dtpSchdDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Convert.ToDecimal(dtpSchdDate.SelectedDateTime.ToString("yyyyMMdd")) < Convert.ToDecimal(DateTime.Now.ToString("yyyyMMdd")))
            {
                //SFU1740  오늘이후날짜만지정가능합니다.
                Util.MessageValidation("SFU1740");
                dtpSchdDate.SelectedDateTime = DateTime.Now;
            }
        }

        private void txtUser_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.Enter))
            {
                GetUserWindow();

                e.Handled = true;
            }
        }
    }
}
