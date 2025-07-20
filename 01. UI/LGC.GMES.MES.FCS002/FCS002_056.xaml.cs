/*************************************************************************************
 Created Date : 2020.12.
      Creator : 
   Decription : 수동 실적 레포트
--------------------------------------------------------------------------------------
 [Change History]
  2020.  DEVELOPER : Initial Created.





 
**************************************************************************************/
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using System.Collections;

namespace LGC.GMES.MES.FCS002
{
    /// <summary>
    /// TSK_710.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class FCS002_056 : UserControl, IWorkArea
    {
        #region [Declaration & Constructor]
        #endregion
        #region [Initialize]
        public FCS002_056()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

           /* InitCombo();
            InitControl();
            object[] parameters = this.FrameOperation.Parameters;
            if (parameters != null && parameters.Length >= 1)
            {
                DateTime nowDate = DateTime.Now;
                
                txtLotID.Text = Util.NVC(parameters[0]);
                cboLine.SelectedValue = Util.NVC(parameters[1]);
                cboModel.SelectedValue = Util.NVC(parameters[2]);
                cboRoute.SelectedValue = Util.NVC(parameters[3]);
                cboOper.SelectedValue = Util.NVC(parameters[4]);

                dtpFromDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(parameters[5]));
                dtpFromTime.DateTime = Util.StringToDateTime(nowDate.ToString("yyyyMMdd") + " " + Util.NVC(parameters[6]), "yyyyMMdd HHmmss");
                dtpToDate.SelectedDateTime = Util.StringToDateTime(Util.NVC(parameters[7]));
                dtpToTime.DateTime = Util.StringToDateTime(nowDate.ToString("yyyyMMdd") + " " + Util.NVC(parameters[8]), "yyyyMMdd HHmmss");
                chkHistory.IsChecked = (bool)parameters[9];
                GetList();
            }*/
        }
        private void InitCombo()
        {
            CommonCombo_Form _combo = new CommonCombo_Form();
            C1ComboBox[] cboLineChild = { cboModel };
            _combo.SetCombo(cboLine, CommonCombo_Form.ComboStatus.ALL, sCase: "LINE", cbChild: cboLineChild);

            C1ComboBox[] cboModelParent = { cboLine };
            _combo.SetCombo(cboModel, CommonCombo_Form.ComboStatus.ALL, sCase: "LINEMODEL", cbParent: cboModelParent);

            string[] sFilter1 = { "COMBO_SHIFT" };
            _combo.SetCombo(cboShift, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            string[] sFilter2 = { "WRKLOG_TYPE_CODE" };
            _combo.SetCombo(cboType, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter2);

            string[] sFilter3 = { "WRKLOG_GR_LOT_TYPE_CODE" };
            _combo.SetCombo(cboAttr, CommonCombo_Form.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);
        }

        private void InitControl()
        {
            dtpFromDate.SelectedDateTime = DateTime.Now.AddDays(-1);
            dtpToDate.SelectedDateTime = DateTime.Now;
        }
        #endregion
        #region [Method]
        private void GetList()
        {
           try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("FROM_DATE", typeof(string));
                dtRqst.Columns.Add("TO_DATE", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("MDLLOTID", typeof(string));
                dtRqst.Columns.Add("SHIFT", typeof(string));
                dtRqst.Columns.Add("WC_TYPE", typeof(string));
                dtRqst.Columns.Add("LOT_TYPE", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["FROM_DATE"] = Util.GetCondition(dtpFromDate);
                dr["TO_DATE"] = Util.GetCondition(dtpToDate);
                dr["MODEL_ID"] = Util.GetCondition(cboModel, bAllNull: true);
                dr["LINE_ID"] = Util.GetCondition(cboLine, bAllNull: true);
                dr["SHIFT"] = Util.GetCondition(cboShift, bAllNull: true);
                dr["WC_TYPE"] = Util.GetCondition(cboType, bAllNull: true);
                dr["LOT_TYPE"] = Util.GetCondition(cboAttr, bAllNull: true);
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_WORKSHEET_LIST", "RQSTDT", "RSLTDT", dtRqst);
                Util.GridSetData(dgList, dtRslt, this.FrameOperation);

                /*for (int i = 0; i < fpsMaintResult.ActiveSheet.RowCount; i++)
                {
                    if (string.IsNullOrEmpty(fpsMaintResult.GetValue(i, "WORK_DATE").ToString()))
                    {
                        fpsMaintResult.SetValue(i, "LOT_ATTR_NAME", ObjectDic.GetObjectName("ALL_SUM"));  //전체합계
                        fpsMaintResult.ActiveSheet.Rows[i].BackColor = Color.Peru;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(fpsMaintResult.GetValue(i, "SHIFT_NAME").ToString()))
                        {
                            fpsMaintResult.SetValue(i, "LOT_ATTR_NAME", ObjectDic.GetObjectName("MODEL_SUM"));  //모델합계
                            fpsMaintResult.ActiveSheet.Rows[i].BackColor = Color.PeachPuff;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(fpsMaintResult.GetValue(i, "LOT_ATTR_NAME").ToString()))
                            {
                                fpsMaintResult.SetValue(i, "LOT_ATTR_NAME", ObjectDic.GetObjectName("SHIFT_EQP_SUM"));  //작업조,설비별합계
                                fpsMaintResult.ActiveSheet.Rows[i].BackColor = Color.Lavender;
                            }
                        }
                    }
                }*/
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region [Event]
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }
    
        #endregion

    }
}
