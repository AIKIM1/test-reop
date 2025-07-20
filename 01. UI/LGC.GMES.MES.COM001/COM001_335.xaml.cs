/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2017.10.25  주건태 C20171011_02560 : [CSR ID:3502560] GMES 자주검사 조희 개선 요청 건
  2019.04.29  정문교 폴란드3동 대응 Carrier ID(CSTID) 조회조건, 조회칼럼 추가




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_335 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        private BizDataSet _Biz = new BizDataSet();
        public COM001_335()
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
        /// <summary>
        /// 화면내 combo 셋팅
        /// </summary>
        private void InitCombo()
        {

            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();


            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, cbParent: cboEquipmentSegmentParent);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.SELECT, cbChild: cboProcessChild, cbParent: cboProcessParent);


            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, cbParent: cboEquipmentParent);



        


            DataTable dtNest = new DataTable();
            dtNest.Columns.Add("CBO_NAME", typeof(string));
            dtNest.Columns.Add("CBO_CODE", typeof(string));

            for (int i = 1; i < 9; i++)
            {
                DataRow dr = dtNest.NewRow();
                dr["CBO_NAME"] = "Nest" + i.ToString();
                dr["CBO_CODE"] = @"ko-KR\Nest" + i.ToString();

                dtNest.Rows.Add(dr);
            }

        
        }


        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //사용자 권한별로 버튼 숨기기
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
            //여기까지 사용자 권한별로 버튼 숨기기

        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetList();
        }




        #endregion

        #region Mehod
        #region [작업대상 가져오기]
        public void GetList()
        {
            try
            {
                //동을 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboArea, "SFU1499")))
                    return;

                //라인을 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboEquipmentSegment, "SFU1223")))
                    return;
                //공정을 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboProcess, "SFU1459")))
                    return;
                //설비를 선택하세요.
                if (string.IsNullOrWhiteSpace(Util.GetCondition(cboEquipment, "SFU1153")))
                    return;

                if (c1tabDefault.Visibility.Equals(Visibility.Visible))
                {

                    GetQuality();
                }

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



        public void GetQuality()
        {
            try
            {
                string bizRuleName = "DA_PRD_SEL_CELL_SCAN_CHK_HIST";

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("INSDTTM", typeof(string));
                dtRqst.Columns.Add("INSDTTM_TO", typeof(string));
                dtRqst.Columns.Add("PROD_LOTID", typeof(string));
                dtRqst.Columns.Add("TRAYID", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["AREAID"] = Util.GetCondition(cboArea, "SFU1499"); //동을 선택하세요.
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, "SFU1223"); //라인을 선택하세요.
                dr["EQPTID"] = Util.GetCondition(cboEquipment, "SFU1153"); //설비를 선택하세요.
                if (string.IsNullOrWhiteSpace(txtLotID.Text) && string.IsNullOrWhiteSpace(txtTrayID.Text))
                {
                    dr["INSDTTM"] = dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dr["INSDTTM_TO"] = dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd HH:mm:ss");
                    dtRqst.Rows.Add(dr);

                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(txtLotID.Text)) 
                    {
                        dr["PROD_LOTID"] = txtLotID.Text;
                        dtRqst.Rows.Add(dr);
                    }
                    else
                    {
                        // Tray ID로 조회시
                        dr["TRAYID"] = txtLotID.Text;
                        dtRqst.Rows.Add(dr);
                    }
                }

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", dtRqst);

                Util.GridSetData(dgQualityInfo, dtRslt, FrameOperation);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

       

        #endregion

        #endregion




      
        
        private void btnSearch_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            loadingIndicator.Visibility = Visibility.Visible;

        }

        //private void DataGridTemplateColumn_GettingCellValue(object sender, C1.WPF.DataGrid.DataGridGettingCellValueEventArgs e)
        //{
        //    //C20171011_02560 : DataGridTemplateColumn 일 경우 엑셀 파일에 값 나오게 하기 위해. DataGridTemplateColumn 일 경우 필요함.
        //    DataRowView drvTmp = (e.Row.DataItem as DataRowView);
        //    //System.Windows.MessageBox.Show((!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "");
        //    e.Value = (!drvTmp["CLCTVAL01"].Equals(DBNull.Value) && !drvTmp["CLCTVAL01"].Equals("-")) ? drvTmp["CLCTVAL01"].ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "") : "";
        //}

        private void DataGridTemplateColumn_GettingCellValue(object sender, C1.WPF.DataGrid.DataGridGettingCellValueEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.DataGridTemplateColumn dgtc = sender as C1.WPF.DataGrid.DataGridTemplateColumn;
                System.Data.DataRowView drv = e.Row.DataItem as System.Data.DataRowView;

                if (dgtc != null && drv != null && dgtc.Name != null)
                {
                    e.Value = drv[dgtc.Name].ToString();
                }
            }
            catch (Exception ex)
            {
                //오류 발생할 경우 아무 동작도 하지 않음. try catch 없으면 이 로직에서 오류날 경우 복사 자체가 안됨
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
