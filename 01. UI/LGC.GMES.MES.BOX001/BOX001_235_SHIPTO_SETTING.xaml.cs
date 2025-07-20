/*************************************************************************************
 Created Date : 2018.08.08
      Creator : 오화백K
   Decription : 출하처 설정 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.08.08  오화백K : 최초생성
  2023.05.11  이병윤 : E20230425-000509 라벨타입 선택 개선_popShipto_ValueChanged 추가
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
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_235_SHIPTO_SETTING : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string _EQSGID = string.Empty;
        private string _EQPTID = string.Empty;
        private string _SHIPTO = string.Empty;
        //private string _LABEL = string.Empty;

        public BOX001_235_SHIPTO_SETTING()
        {
            InitializeComponent();
        }

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
            

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, null, sFilter: new string[] { _EQSGID, Process.CELL_BOXING, null }, sCase: "EQUIPMENT");
            _combo.SetCombo(cboLabelType, CommonCombo.ComboStatus.SELECT, sFilter: new string[] { "MOBILE_INBOX_LABEL_TYPE" }, sCase: "COMMCODE_WITHOUT_CODE");

        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            cboEquipment.SelectedValue = _EQPTID;
            txtBeforeShipto.Text = _SHIPTO;
            cboEquipment.IsEnabled = false;
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _EQSGID = Util.NVC(tmps[0]); // 라인정보
            _EQPTID = Util.NVC(tmps[1]); // 설비정보
            _SHIPTO = Util.NVC(tmps[2]); // 출하처 정보
            //_LABEL  = Util.NVC(tmps[3]); // 라벨타입 정보
            txtBeforeLabel.Text = Util.NVC(tmps[3]); // 라벨타입 정보
            if(Util.NVC(tmps[4]) == "Y")
            {
                chkFIFO.IsChecked = true;
            }
            else
            {
                chkFIFO.IsChecked = false;
            }

            InitCombo();
            InitControl();
            setShipToPopControl();
     }

        #endregion

        #region Mehod

        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void setShipToPopControl()
        {
            const string bizRuleName = "DA_PRD_SEL_SHIPTO_CBO";
            string[] arrColumn = { "SHOPID", "LANGID" };
            string[] arrCondition = { LoginInfo.CFG_SHOP_ID, LoginInfo.LANGID };
            CommonCombo.SetFindPopupCombo(bizRuleName, popShipto, arrColumn, arrCondition, (string)popShipto.SelectedValuePath, (string)popShipto.DisplayMemberPath);
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            if (popShipto.SelectedValue == null)
            {
                Util.MessageValidation("SFU4096");
                return;
            }

            if (cboLabelType.SelectedValue.ToString() == "SELECT" || cboLabelType.SelectedValue.ToString() == "")
            {
                // 라벨 타입을 선택하세요
                Util.MessageValidation("SFU1522");
                return;
            }


            Util.MessageConfirm("SFU5001", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    RunStart();
                }
            });

        }

        private void RunStart()
        {
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("EQPTID");
                inDataTable.Columns.Add("BOXING_SHIPTO_ID");
                inDataTable.Columns.Add("USERID");
                inDataTable.Columns.Add("BOXING_LABEL_ID");
                inDataTable.Columns.Add("BOXING_FIFO_FLAG");

                DataRow newRow = inDataTable.NewRow();
                newRow["EQPTID"] = _EQPTID;
                newRow["BOXING_SHIPTO_ID"] = popShipto.SelectedValue.ToString();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["BOXING_LABEL_ID"] = cboLabelType.SelectedValue.ToString();
                newRow["BOXING_FIFO_FLAG"] = chkFIFO.IsChecked == true ? "Y" : "N";
                inDataTable.Rows.Add(newRow);
               
                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_EIOATTR_SHIPTO_LABEL_ID_NJ", "INDATA", null, inDataSet);
                this.DialogResult = MessageBoxResult.OK;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }

        private void popShipto_ValueChanged(object sender, EventArgs e)
        {
            //E20230425-000509 라벨타입 선택 개선
            // MMD : 출하처 인쇄관리 항목(Cell) 조회
            DataTable dtPrtDt = new DataTable();
            dtPrtDt.TableName = "RQSTDT";
            dtPrtDt.Columns.Add("LANGID", typeof(string));
            dtPrtDt.Columns.Add("SHOPID", typeof(string));
            dtPrtDt.Columns.Add("SHIPTO_ID", typeof(string));

            DataRow drPrtrow = dtPrtDt.NewRow();
            drPrtrow["LANGID"] = LoginInfo.LANGID;
            drPrtrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
            drPrtrow["SHIPTO_ID"] = popShipto.SelectedValue;

            dtPrtDt.Rows.Add(drPrtrow);
            DataTable dtMmdEx = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LABELCODE_BY_PRODID_CBO", "RQSTDT", "RSLTDT", dtPrtDt);

            string attr4 = string.Empty;
            if (dtMmdEx.Rows.Count == 0)
            {
                // 초기화일 경우 경고메세지 보이지 않게 하기
                if (!string.IsNullOrEmpty(popShipto.SelectedValue.ToString()))
                {
                    //SFU9994 : Contact engineers to configure the label
                    Util.MessageValidation("SFU9994");
                }

                attr4 = ",";
            }
            else
            {
                for (int i = 0; i < dtMmdEx.Rows.Count; i++)
                {
                    attr4 += dtMmdEx.Rows[i]["CBO_CODE"].ToString() + ",";
                }
            }

            // 라벨타입조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
            RQSTDT.Columns.Add("ATTRIBUTE4", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["CMCDTYPE"] = "MOBILE_INBOX_LABEL_TYPE";
            dr["ATTRIBUTE4"] = attr4;
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMCODE_ATTR_CBO_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

            // SELECT 추가
            DataRow drResult = dtResult.NewRow();
            drResult["CBO_NAME_ONLY"] = "-SELECT-";
            drResult["CBO_CODE"] = "SELECT";
            dtResult.Rows.InsertAt(drResult, 0);
            // 콤보박스 세팅
            cboLabelType.DisplayMemberPath = "CBO_NAME_ONLY";
            cboLabelType.SelectedValuePath = "CBO_CODE";
            cboLabelType.ItemsSource = dtResult.Copy().AsDataView();
            if (dtResult.Rows.Count <= 1)
            {
                cboLabelType.SelectedIndex = 0;
            }
            else
            {
                cboLabelType.SelectedValue = dtResult.Rows[1]["CBO_CODE"].ToString();

                if (cboLabelType.SelectedIndex < 0)
                    cboLabelType.SelectedIndex = 0;
            }
        }
    }
}
