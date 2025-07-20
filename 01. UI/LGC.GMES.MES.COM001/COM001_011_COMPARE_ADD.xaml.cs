/*************************************************************************************
 Created Date :
      Creator :
   Decription :
--------------------------------------------------------------------------------------
 [Change History]





 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_011_COMPARE_ADD : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo _combo = new CommonCombo();

        string _OBJ_AREAID = "";
        string _OBJ_STCK_CNT_YM = "";
        string _OBJ_STCK_CNT_SEQNO = "";
        string _STCK_CNT_GR = "";

        public COM001_011_COMPARE_ADD()
        {
            InitializeComponent();
            InitCombo();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion

        #region Initialize
        private void InitCombo()
        {
            //대상 동
            _combo.SetCombo(cboObjArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            //대상 차수
            object[] objSeqParent = { cboObjArea, ldpObjMonth };
            String[] sFilterAllObj = { "" };
            _combo.SetComboObjParent(cboObjSeq, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objSeqParent, sFilter: sFilterAllObj);
      }

        #endregion

        #region Event

        #region C1Window_Loaded
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    _STCK_CNT_GR = Util.NVC(tmps[0]);
                    _OBJ_AREAID = Util.NVC(tmps[1]);
                    _OBJ_STCK_CNT_YM = Util.NVC(tmps[2]);
                    _OBJ_STCK_CNT_SEQNO = Util.NVC(tmps[3]);

                    cboObjArea.SelectedValue = _OBJ_AREAID;
                    ldpObjMonth.SelectedDateTime = Convert.ToDateTime(_OBJ_STCK_CNT_YM);
                    cboObjSeq.SelectedValue = _OBJ_STCK_CNT_SEQNO;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region 차수조회
        private void ldpObjMonth_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboObjSeq);
        }

        #endregion

        #region 선택재고 생성
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            //추가하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2965"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    AddStcok();
                }
            }
            );
        }
        #endregion

        #endregion

        #region Mehod
        private bool Validation()
        {
            if (string.IsNullOrEmpty(txtLotId.Text))
            {
                //LOT ID 가 없습니다.
                Util.MessageValidation("SFU1361");
                return false;
            }

            if (txtInputQty.Value <= 0)
            {
                //수량은 0 보다 커야 합니다.
                Util.MessageValidation("100057");
                return false;
            }

            return true;
        }

        private void AddStcok()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("STCK_CNT_YM", typeof(string));
                inData.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                inData.Columns.Add("LOTID", typeof(string));
                inData.Columns.Add("STOCK_QTY", typeof(decimal));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["AREAID"] = Util.NVC(cboObjArea.SelectedValue);
                row["STCK_CNT_YM"] = Util.GetCondition(ldpObjMonth);
                row["STCK_CNT_SEQNO"] = Util.NVC(cboObjSeq.SelectedValue);
                row["LOTID"] = txtLotId.Text.Trim();
                row["STOCK_QTY"] = txtInputQty.Value;
                row["USERID"] = LoginInfo.USERID;

                indataSet.Tables["INDATA"].Rows.Add(row);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_STOCKCNT_COMPARE", "INDATA", null, indataSet);                

                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

    }
}
