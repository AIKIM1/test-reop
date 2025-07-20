/*************************************************************************************
 Created Date : 2018.04.11
      Creator : 오화백 K
   Decription : 활성화 대차 생성/삭제/복원/변경  : INBOX 추가
--------------------------------------------------------------------------------------
 [Change History]
   2018.04.11   오화백  : Initial Created.

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

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_227_CREATE_INBOX : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        BizDataSet _bizRule = new BizDataSet();
            
        private string _CTNR_ID = string.Empty;
        private string _INBOX_TYPE = string.Empty;
        private string _PROCID = string.Empty;
        private string _AREAID = string.Empty;
        private Double _MAXCELL_QTY = 0;
        private string _WIP_QLTY_TYPE_CODE = string.Empty;


        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion
        
        public COM001_227_CREATE_INBOX()
        {
            InitializeComponent();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            //용량등급
            String[] sFilterCapa = { "", "CAPA_GRD_CODE" };
            _combo.SetCombo(cboCapa, CommonCombo.ComboStatus.ALL, sFilter: sFilterCapa, sCase: "COMMCODES");

            SetAreaInboxType();

            //불량그룹
            String[] sFilterProcess = { _PROCID };
            _combo.SetCombo(cboDefect_Group, CommonCombo.ComboStatus.ALL, sCase: "POLYMER_DEFECT_GROUP", sFilter: sFilterProcess);
        }

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _CTNR_ID = tmps[0] as string;             //대차ID
            _INBOX_TYPE = tmps[1] as string;          //INBOX TYPE
            _PROCID = tmps[2] as string;
            _AREAID = tmps[3] as string;
            _WIP_QLTY_TYPE_CODE = tmps[4] as string;  //품질정보코드  
            // INIT COMBO
            InitCombo();

            if(_WIP_QLTY_TYPE_CODE == "N") // 불량
            {
                Inboxtype.Visibility = Visibility.Collapsed;
                cboInboxType.Visibility = Visibility.Collapsed;

                DefectGroup.Visibility = Visibility.Visible;
                cboDefect_Group.Visibility = Visibility.Visible;

            }
            else  //양품
            {
                Inboxtype.Visibility = Visibility.Visible;
                cboInboxType.Visibility = Visibility.Visible;

                DefectGroup.Visibility = Visibility.Collapsed;
                cboDefect_Group.Visibility = Visibility.Collapsed;
            }

            //INBOX 셋팅
            if (_INBOX_TYPE != string.Empty)
            {
                cboInboxType.SelectedValue = _INBOX_TYPE;
                cboInboxType.IsEnabled = false;
            }
            else
            {
                cboInboxType.IsEnabled = true;
                //Inbox Type
                SetAreaInboxType();

            }

            // INBOX Type  전체수량
            SetEqptInboxTypeQTY();
            txtCtnr.Text = _CTNR_ID;

            this.BringToFront();
        }
        #endregion

        #region [INBOX 생성 btnCreate_Click]

        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidationCreate()) return;

            // Inbox 생성
            InputInbox();
        }


        #endregion

        #region [닫기]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion

        #region Mehod
      
        #region [INBOX TYPE SetAreaInboxType()]

        private void SetAreaInboxType()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inTable.Rows.Add(newRow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_AREA_INBOX_TYPE_PC", "INDATA", "OUTDATA", inTable);
                DataRow dr = dtResult.NewRow();
                dr[cboInboxType.SelectedValuePath] = "SELECT";
                dr[cboInboxType.DisplayMemberPath] = "-SELECT-";
                dtResult.Rows.InsertAt(dr, 0);

                cboInboxType.ItemsSource = dtResult.Copy().AsDataView();
                cboInboxType.SelectedIndex = 0;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }


        #endregion

        #region [INBOX TYPE 수량 SetEqptInboxTypeQTY()]

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


        #endregion

        #region [INBOX 생성 InputInbox]

        private void InputInbox()
        {
            //try
            //{

            //    DataSet inDataSet = new DataSet();

            //    DataTable inTable = inDataSet.Tables.Add("INDATA");
            //    inTable.Columns.Add("SRCTYPE", typeof(string));
            //    inTable.Columns.Add("IFMODE", typeof(string));
            //    inTable.Columns.Add("EQPTID", typeof(string));
            //    inTable.Columns.Add("PROD_LOTID", typeof(string));
            //    inTable.Columns.Add("USERID", typeof(string));
            //    inTable.Columns.Add("SHIFT", typeof(string));
            //    inTable.Columns.Add("WRK_USERID", typeof(string));
            //    inTable.Columns.Add("WRK_USER_NAME", typeof(string));
            //    inTable.Columns.Add("CTNR_ID", typeof(string));
            //    inTable.Columns.Add("VISL_INSP_USERID", typeof(string));
            //    inTable.Columns.Add("MOD_FLAG", typeof(string));
            //    inTable.Columns.Add("PROCID", typeof(string));

            //    DataTable inBox = inDataSet.Tables.Add("INBOX");
            //    inBox.Columns.Add("CAPA_GRD_CODE", typeof(string));
            //    inBox.Columns.Add("VLTG_GRD_CODE", typeof(string));
            //    inBox.Columns.Add("CELL_QTY", typeof(decimal));
            //    inBox.Columns.Add("INBOX_QTY", typeof(decimal));
            //    inBox.Columns.Add("INBOX_TYPE_CODE", typeof(string));

            //    DataRow newRow = inTable.NewRow();
            //    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
            //    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
            //    newRow["EQPTID"] = _EQPT_ID;
            //    newRow["PROD_LOTID"] = _LOTID;
            //    newRow["USERID"] = LoginInfo.USERID;
            //    newRow["SHIFT"] = _SHIFT;
            //    newRow["WRK_USERID"] = _WRK_USERID;
            //    newRow["WRK_USER_NAME"] = _WRK_USER_NAME;
            //    newRow["CTNR_ID"] = cboCtnr_ID.SelectedValue.ToString();
            //    newRow["VISL_INSP_USERID"] = string.Empty;
            //    newRow["MOD_FLAG"] = "Y";
            //    newRow["PROCID"] = _PROCID;
            //    inTable.Rows.Add(newRow);

            //    DataRow dr = inBox.NewRow();
            //    dr["CAPA_GRD_CODE"] = cboCapa.SelectedValue.ToString();
            //    dr["CELL_QTY"] = txtCellQty.Value;
            //    dr["INBOX_QTY"] = 1;
            //    dr["INBOX_TYPE_CODE"] = cboInboxType.SelectedValue.ToString();
            //    inBox.Rows.Add(dr);

            //    //string xml = inDataSet.GetXml();

            //    new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CREATE_INBOX", "INDATA,INBOX", "OUTDATA", (bizResult, bizException) =>
            //    {
            //        try
            //        {

            //            if (bizException != null)
            //            {
            //                Util.MessageException(bizException);
            //                return;
            //            }

            //            this.DialogResult = MessageBoxResult.OK;
            //        }
            //        catch (Exception ex)
            //        {

            //            Util.MessageException(ex);
            //        }
            //    }, inDataSet);

            //}
            //catch (Exception ex)
            //{
            //    Util.MessageException(ex);
            //}
        }


        #endregion

        #region [VALLDATION]

        /// <summary>
        ///  생성 Valldation
        /// </summary>
        /// <returns></returns>
        private bool ValidationCreate()
        {

            if (cboCapa.SelectedIndex == 0)
            {
                Util.MessageValidation("SFU4482"); //용량등급 정보를 선택하세요.
                return false;
            }

            if (txtCellQty.Value.ToString() == "NaN")
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
            if (txtCellQty.Value > _MAXCELL_QTY)
            {
                Util.MessageValidation("SFU4485"); //현재 Cell 수량이 INBOX 유형에 대한 최대 Cell 수량보다 큽니다.
                return false;
            }


            return true;
        }


        #endregion

        #endregion






    }
}
