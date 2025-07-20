/*************************************************************************************
 Created Date : 2021.01.20
      Creator : 
   Decription : GQMS BLOCK 기준정보
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.20  DEVELOPER : Initial Created.
  2023.03.22  이주홍  활성화 UI에서 조립 PROCID를 가져오지 못해서 GQMS 품질 인터락 기준정보를 조회할 수 없음.
                     전체 공정을 선택한 경우(All) 조회 조건에서 PROCID를 제외하도록 수정함.
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using System.Net;
using System.Reflection;
using System.Collections;
using System.Globalization;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_349.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_349 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        CommonCombo combo = new CommonCombo();
        Util _util = new Util();

        public COM001_349()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }

        #endregion

        #region Initialize
        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
            InitCombo();
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            combo.SetCombo(cboArea, CommonCombo.ComboStatus.NONE, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, null, cbParent: cboEquipmentSegmentParent);

            // 차단 유형 코드
            string[] sFilter1= { "BLOCK_TYPE_CODE" };
            combo.SetCombo(cboBlockType, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);

            //Rev NO
            SetCombo_BlockRevNo(cboRevNo);
            cboRevNo.SelectedIndex = cboRevNo.Items.Count - 1;

            //모델유형
            string[] sFilter3 = { "CELL_PRDT_TYPE_CODE" };
            combo.SetCombo(cboModel, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter3);

            //모델유형 All 추가(GQMS BLOCK 기준정보 'ALL' DATA 존재)
            DataTable dtCombo = new DataTable();
            dtCombo.Columns.Add(new DataColumn() { ColumnName = "CBO_CODE", DataType = typeof(string) });
            dtCombo.Columns.Add(new DataColumn() { ColumnName = "CBO_NAME", DataType = typeof(string) });

            dtCombo = DataTableConverter.Convert(cboModel.ItemsSource);

            DataRow Row = dtCombo.NewRow();

            Row = dtCombo.NewRow();
            Row["CBO_CODE"] = "ALL";
            Row["CBO_NAME"] = "ALL : " + ObjectDic.Instance.GetObjectName("MODEL_TYPE_CD") + "(" + ObjectDic.Instance.GetObjectName("전체") + ")";
            dtCombo.Rows.InsertAt(Row, cboModel.Items.Count + 1);

            cboModel.ItemsSource = DataTableConverter.Convert(dtCombo);
        }

        #endregion

        #region Event
        /// <summary>
        /// Initializing 이후에 FormLoad시 Event를 생성.
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboArea.SelectedIndex < 0 || cboArea.SelectedValue.ToString().Trim().Equals("SELECT"))
            {
                Util.MessageValidation("SFU3203");  //동은필수입니다.
                return;
            }

            SearchData();
        }

        #endregion

        #region Method
        /// <summary>
        /// 조회
        /// </summary>
        private void SearchData()
        {
            try
            {
                //초기화
                Util.gridClear(dgBlockResult);

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
				RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("BLOCK_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("REVNO", typeof(string));
                RQSTDT.Columns.Add("MODELTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue.ToString();
				// dr["PROCID"] = cboProcess.SelectedItemsToString;
				dr["PROCID"] = cboProcess.GetBindValue(); // 2023.03.02 이주홍
				dr["BLOCK_TYPE_CODE"] = Util.GetCondition(cboBlockType) == "" ? null : Util.GetCondition(cboBlockType);
                dr["REVNO"] = Util.GetCondition(cboRevNo) == "" ? null : Util.GetCondition(cboRevNo);
                dr["MODELTYPE"] = Util.GetCondition(cboModel) == "" ? null : Util.GetCondition(cboModel);

                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_BAS_SEL_TB_MMD_QMS_BLOCK_BAS", "RQSTDT", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        string[] sColumnName = null;
                        
                        Util.GridSetData(dgBlockResult, searchResult, FrameOperation, true);
                        sColumnName = new string[] { "AREANAME", "BLOCKTYPENAME" };
                        _util.SetDataGridMergeExtensionCol(dgBlockResult, sColumnName, DataGridMergeMode.VERTICALHIERARCHI); //VERTICALHIERARCHI);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
            }
        }

        /// <summary>
        /// CommonCombo 추가시 최신파일로 컴파일이 오류로 소스에서 처리
        /// </summary>
        /// <param name="cbo"></param>
        private void SetCombo_BlockRevNo(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = cboArea.SelectedValue.ToString();

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_QMS_BLOCK_BAS_REVNO_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";
                cbo.ItemsSource = AddStatus(dtResult, CommonCombo.ComboStatus.ALL, "CBO_CODE", "CBO_NAME").Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private DataTable AddStatus(DataTable dt, CommonCombo.ComboStatus cs, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();

            switch (cs)
            {
                case CommonCombo.ComboStatus.ALL:
                    dr[sDisplay] = "-ALL-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.SELECT:
                    dr[sDisplay] = "-SELECT-";
                    dr[sValue] = "SELECT";
                    dt.Rows.InsertAt(dr, 0);
                    break;

                case CommonCombo.ComboStatus.NA:
                    dr[sDisplay] = "-N/A-";
                    dr[sValue] = "";
                    dt.Rows.InsertAt(dr, 0);
                    break;
            }

            return dt;
        }

        private void SetProcess()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboArea.SelectedValue;
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_PROCESS_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboProcess.ItemsSource = DataTableConverter.Convert(dtResult);

                for (int i = 0; i < dtResult.Rows.Count; i++)
                {
                    cboProcess.Check(i);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion



        private void cboEquipmentSegment_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (cboEquipmentSegment.Items.Count > 0 && cboEquipmentSegment.SelectedValue != null && !cboEquipmentSegment.SelectedValue.Equals("SELECT"))
            {
                SetProcess();

                //Rev NO
                SetCombo_BlockRevNo(cboRevNo);
                cboRevNo.SelectedIndex = cboRevNo.Items.Count - 1;

                ////Rev NO
                //string[] sFilter2 = { cboArea.SelectedValue.ToString() };
                //combo.SetCombo(cboRevNo, CommonCombo.ComboStatus.ALL, sCase: "BLOCK_REVNO", sFilter: sFilter2);
                //cboRevNo.SelectedIndex = cboRevNo.Items.Count - 1;
            }
        }
        
    }
}
