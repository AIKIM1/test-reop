/*************************************************************************************
 Created Date : 2017.05.30
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2017.05.30  DEVELOPER : Initial Created.
  2023.04.03  이홍주      소형활성화MES 복사




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Globalization;
using System.Windows;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_332_STOCKCNT_RSLT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        CommonCombo _combo = new CommonCombo();

        const int _iGridColsCnt = 5; //Grid Display Colums 수

        string _OBJ_AREAID = "";
        string _OBJ_STCK_CNT_YM = "";
        string _OBJ_STCK_CNT_SEQNO = "";
        string _STCK_CNT_GR = "";

        DataTable _dtRSLT = new DataTable();

        public FCS002_332_STOCKCNT_RSLT()
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
            //C1ComboBox[] cboAreaShotChild = { cboObjSeq, cboTargetSeq };
            _combo.SetCombo(cboObjArea, CommonCombo.ComboStatus.SELECT, sCase: "AREA");

            //대상 차수
            object[] objSeqParent = { cboObjArea, ldpObjMonth };
            String[] sFilterAllObj = { "" };
            _combo.SetComboObjParent(cboObjSeq, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: objSeqParent, sFilter: sFilterAllObj);

            //변경 차수
            object[] targetSeqParent = { cboObjArea, ldpTargetMonth };
            String[] sFilterAllTarget = { "" };
            _combo.SetComboObjParent(cboTargetSeq, CommonCombo.ComboStatus.NONE, sCase: "STOCKSEQ", objParent: targetSeqParent, sFilter: sFilterAllTarget);
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

                    DataRow[] drs = tmps[4] as DataRow[];
                    _dtRSLT.Columns.Add("AREAID", typeof(string));
                    _dtRSLT.Columns.Add("STCK_CNT_YM", typeof(string));
                    _dtRSLT.Columns.Add("STCK_CNT_SEQNO", typeof(string));
                    _dtRSLT.Columns.Add("PROCID", typeof(string));
                    _dtRSLT.Columns.Add("LOTID", typeof(string));
                   

                    for (int idx = 0; idx < drs.Length; idx++)
                    {
                        DataRow dr = _dtRSLT.NewRow();
                        dr["AREAID"] = drs[idx]["AREAID"].ToString();
                        dr["STCK_CNT_YM"] = drs[idx]["STCK_CNT_YM"].ToString();
                        dr["STCK_CNT_SEQNO"] = drs[idx]["STCK_CNT_SEQNO"].ToString();
                        dr["PROCID"] = drs[idx]["PROCID"].ToString();
                        dr["LOTID"] = _STCK_CNT_GR == "MCP" ? drs[idx]["SCAN_ID"].ToString() : drs[idx]["LOTID"].ToString();

                        _dtRSLT.Rows.Add(dr);
                    }

                    //Grid에 바인딩.
                    if (_dtRSLT.Rows.Count > 0)
                    {
                        int iLotCnt = 0;
                        int iLotRemainderCnt = _dtRSLT.Rows.Count % _iGridColsCnt;
                        decimal dLotRowsCnt = (decimal)_dtRSLT.Rows.Count / _iGridColsCnt;
                        int iTrayRowsCnt = (int)Math.Ceiling(dLotRowsCnt);

                        DataTable dtData = new DataTable();
                        dtData.Columns.Add("COL1");
                        dtData.Columns.Add("COL2");
                        dtData.Columns.Add("COL3");
                        dtData.Columns.Add("COL4");
                        dtData.Columns.Add("COL5");

                        for (int i = 0; i < iTrayRowsCnt; i++)
                        {
                            DataRow newRow = null;
                            newRow = dtData.NewRow();

                            if (i == iTrayRowsCnt - 1 && iLotRemainderCnt > 0)
                            {
                                for (int iRemainderCol = 0; iRemainderCol < iLotRemainderCnt; iRemainderCol++)
                                {
                                    newRow[string.Format("COL{0}", iRemainderCol + 1)] = _dtRSLT.Rows[iLotCnt]["LOTID"].ToString();
                                    iLotCnt++;
                                }
                            }
                            else
                            {
                                for (int iCol = 0; iCol < _iGridColsCnt; iCol++)
                                {
                                    newRow[string.Format("COL{0}", iCol + 1)] = _dtRSLT.Rows[iLotCnt]["LOTID"].ToString();
                                    iLotCnt++;
                                }
                            }

                            dtData.Rows.Add(newRow);
                            dtData.AcceptChanges();
                        }

                        dgData.BeginEdit();
                        dgData.ItemsSource = DataTableConverter.Convert(dtData);
                        dgData.EndEdit();

                    }
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

        private void ldpTargetMonth_SelectedDataTimeChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _combo.SetCombo(cboTargetSeq);
        }
        #endregion

        #region 선택재고 변경
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation()) return;

            //실물 재고 변경을 하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("MMD0106"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result.ToString().Equals("OK"))
                {
                    Exclude_RSLT();
                }
            }
            );
        }
        #endregion

        #endregion

        #region Mehod
        private bool Validation()
        {
            if (string.IsNullOrEmpty(Util.NVC(cboTargetSeq.SelectedValue)))
            {
                Util.MessageInfo("SFU2958");//차수는 필수입니다.
                return false;
            }

            if (Util.GetCondition(ldpObjMonth) == Util.GetCondition(ldpTargetMonth) && Util.NVC(cboObjSeq.SelectedValue) == Util.NVC(cboTargetSeq.SelectedValue))
            {
                Util.MessageInfo("MMD0107");//같은 기준월, 같은 차수로 변경할수 없습니다.
                return false;
            }

            return true;
        }

        private void Exclude_RSLT()
        {
            try
            {
                DataSet indataSet = new DataSet();
                DataTable inData = indataSet.Tables.Add("INDATA");
                inData.Columns.Add("STCK_CNT_GR", typeof(string));
                inData.Columns.Add("AREAID", typeof(string));
                inData.Columns.Add("TARGET_STCK_CNT_YM", typeof(string));
                inData.Columns.Add("TARGET_STCK_CNT_SEQNO", typeof(string));
                inData.Columns.Add("USERID", typeof(string));

                DataRow row = inData.NewRow();
                row["STCK_CNT_GR"] = _STCK_CNT_GR;
                row["AREAID"] = _OBJ_AREAID;
                row["TARGET_STCK_CNT_YM"] = Util.GetCondition(ldpTargetMonth);
                row["TARGET_STCK_CNT_SEQNO"] = Util.NVC(cboTargetSeq.SelectedValue);
                row["USERID"] = LoginInfo.USERID;
                indataSet.Tables["INDATA"].Rows.Add(row);

                _dtRSLT.TableName = "INLOT";
                indataSet.Tables.Add(_dtRSLT);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_STOCKCNT_RSLT", "INDATA,INLOT", null, indataSet);                

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
