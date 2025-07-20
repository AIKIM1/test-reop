/*************************************************************************************
 Created Date : 2017.07.01
      Creator : 이대영D
   Decription : Winding 공정진척(초소형) 대기Pancake조회
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.01   INS 이대영D : Initial Created.
   2023.03.27        이홍주 : 소형활성화MES 복사
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Data;
using System.Diagnostics.Eventing.Reader;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_322_CREATE_INBOX : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();

        private DataTable selectDt = null;
        public DataRow DrSelectedDataRow = null;
        private string _EQPT_ID = string.Empty;
        private string _LOTID = string.Empty;
        private string _SHIFT = string.Empty;
        private string _INBOX_TYPE_CODE = string.Empty;
        private string _INBOX_TYPE_NAME = string.Empty;
        private string _AREAID = string.Empty;
        private string _EQSGID = string.Empty;
        private string _PROCID = string.Empty;
        private string _WRK_USERID = string.Empty;
        private string _WRK_USER_NAME = string.Empty;
        private Double _MAXCELL_QTY = 0;



        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion
        
        public FCS002_322_CREATE_INBOX()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
  
            string[] sFilterCapa = { _AREAID, _EQSGID, "G" };
            _combo.SetCombo(cboCapa, CommonCombo.ComboStatus.SELECT, sCase: "FORM_GRADE_TYPE_CODE_LINE", sFilter: sFilterCapa);
            string[] sFilterLOT = { _LOTID };
            _combo.SetCombo(cboCtnr_ID, CommonCombo.ComboStatus.SELECT, sCase: "PROD_LOT_CTNR", sFilter: sFilterLOT);

            // Inbox type
            string[] sFilter = { _AREAID, _PROCID };
            _combo.SetCombo(cboInboxType, CommonCombo.ComboStatus.SELECT, sFilter: sFilter);

        }

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _LOTID = tmps[0] as string;             // 생산LOTID
            _EQPT_ID = tmps[1] as string;           // 설비정보
            _INBOX_TYPE_CODE = tmps[2] as string;   // INBOX 유형 코드
            _INBOX_TYPE_NAME = tmps[3] as string;   // INBOX 유형 명
            _AREAID = tmps[4] as string;   // 동 정보
            _EQSGID = tmps[5] as string;   // 라인정보
            _PROCID = tmps[6] as string;   // 공정정보
            _SHIFT  = tmps[7] as string;   // 작업조
            _WRK_USERID = tmps[8] as string;   // 작업자 ID
            _WRK_USER_NAME = tmps[9] as string;   // 작업자 명

            // INIT COMBO
            InitCombo();
            if (_INBOX_TYPE_CODE != string.Empty)
            {
                cboInboxType.SelectedValue = _INBOX_TYPE_CODE;
                cboInboxType.IsEnabled = false;
            }
            else
            {
                cboInboxType.IsEnabled = true;
                // 설비에 설정된 Inbox Type
                SetEqptInboxType();

            }
            //txtInboxType.Text = _INBOX_TYPE_NAME;
            //txtInboxType.Tag = _INBOX_TYPE_CODE;
            // INBOX Type  전체수량
            SetEqptInboxTypeQTY(); 
         

            this.BringToFront();
        }
        #endregion
  

        #region [닫기]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion


        private void SetEqptInboxTypeQTY()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));
                inTable.Columns.Add("INBOX_TYPE_CODE", typeof(string));
                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _AREAID;
                newRow["PROCID"] = _PROCID;
                newRow["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue.ToString();

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_INBOX_TYPE_QTY", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    _MAXCELL_QTY = Convert.ToDouble(dtResult.Rows[0]["INBOX_LOAD_QTY"].ToString()); 
                    txtCellQty.Value = Convert.ToDouble(dtResult.Rows[0]["INBOX_LOAD_QTY"].ToString());

                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            // Inbox 생성
            InputInbox();
        }

        private bool ValidationCreate()
        {

            if (cboCapa.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4482"); //용량등급 정보를 선택하세요.
                return false;
            }
            if (cboCtnr_ID.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4483"); //적재대차 정보를 선택하세요.
                return false;
            }

           if(txtCellQty.Value.ToString() == "NaN")
            {
                Util.MessageValidation("SFU4484"); //Cell 정보를 입력하세요
                return false;
            }
            if (txtCellQty.Value == 0)
            {
                Util.MessageValidation("SFU4484"); //Cell 정보를 입력하세요
                return false;
            }
            if (Convert.ToDecimal(txtCellQty.Value) < 0)
            {
                Util.MessageValidation("SFU1683"); //수량은 0보다 커야 합니다.
                return false;
            }
            if (txtCellQty.Value > _MAXCELL_QTY )
            {
                Util.MessageValidation("SFU4485"); //현재 Cell 수량이 INBOX 유형에 대한 최대 Cell 수량보다 큽니다.
                return false;
            }


            return true;
        }

        private void InputInbox()
        {
            try
            {
               
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("CTNR_ID", typeof(string));
                inTable.Columns.Add("VISL_INSP_USERID", typeof(string));
                inTable.Columns.Add("MOD_FLAG", typeof(string));
                inTable.Columns.Add("PROCID", typeof(string));

                DataTable inBox = inDataSet.Tables.Add("INBOX");
                inBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
                inBox.Columns.Add("VLTG_GRD_CODE", typeof(string));
                inBox.Columns.Add("CELL_QTY", typeof(decimal));
                inBox.Columns.Add("INBOX_QTY", typeof(decimal));
                inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EQPT_ID;
                newRow["PROD_LOTID"] = _LOTID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["SHIFT"] = _SHIFT;
                newRow["WRK_USERID"] = _WRK_USERID;
                newRow["WRK_USER_NAME"] = _WRK_USER_NAME;
                newRow["CTNR_ID"] = cboCtnr_ID.SelectedValue.ToString();
                newRow["VISL_INSP_USERID"] = string.Empty;
                newRow["MOD_FLAG"] = "Y";
                newRow["PROCID"] = _PROCID;
                inTable.Rows.Add(newRow);
                      
                DataRow dr = inBox.NewRow();
                dr["CAPA_GRD_CODE"] = cboCapa.SelectedValue.ToString();
                dr["CELL_QTY"] = txtCellQty.Value;
                dr["INBOX_QTY"] = 1;
                dr["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue.ToString();
                inBox.Rows.Add(dr);
              
                //string xml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_INBOX", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
                {
                    try
                    {
                       
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                    
                        Util.MessageException(ex);
                    }
                }, inDataSet);

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetEqptInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = _EQPT_ID;

                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_EQPT_INBOX_TYPE", "INDATA", "OUTDATA", inTable);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    if (cboInboxType.Items.Count > 0)
                        cboInboxType.SelectedValue = dtResult.Rows[0]["INBOX_TYPE"].ToString();
                }

                if (cboInboxType.SelectedValue == null)
                    cboInboxType.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
    }
}
