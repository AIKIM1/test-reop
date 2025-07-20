/*************************************************************************************
 Created Date : 2020.07.10
      Creator : 오화백K
   Decription : 고객인증그룹조회 
--------------------------------------------------------------------------------------
 [Change History]
  2020.07.10  오화백K : Initial Created.
  
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using C1.WPF.DataGrid;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_CUSTOMER_GROUP_SEARCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_CUSTOMER_GROUP_SEARCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _ProcID = string.Empty;
        private string _EqptID = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        #endregion

        #region Initialize    
        public CMM_ELEC_CUSTOMER_GROUP_SEARCH()
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

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 3)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _ProcID = Util.NVC(tmps[2]);
                   
                }
                else
                {
                    return;
                }
                SetEquipment();
                GetCustGroup(_EqptID);
   
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }


        #endregion

        #region Event

        #region 고객그룹정보조회 이벤트  : btnSearch_Click()
        /// <summary>
        /// 고객그룹정보조회 이벤트
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.SelectedIndex < 1)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요.
                return;
            }
            GetCustGroup(cboEquipment.SelectedValue.ToString());
        }
        #endregion

        #region 닫기 이벤트 : btnClose_Click()
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #endregion

        #region Mehod

        #region 고객그룹정보조회 함수  : GetCustGroup()
        /// <summary>
        /// 데이터 조회
        /// </summary>
        private void GetCustGroup(string eqptid)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("LANGID", typeof(string));         // 언어
                inDataTable.Columns.Add("AREAID", typeof(string));         // AREAID
                inDataTable.Columns.Add("PROCID", typeof(string));         // 공정
                inDataTable.Columns.Add("EQPTID", typeof(string));         // 설비
                inDataTable.Columns.Add("USE_FLAG", typeof(string));       // 사용여부
          
                DataRow newRow = inDataTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                newRow["PROCID"] = _ProcID;
                newRow["EQPTID"] = eqptid;
                newRow["USE_FLAG"] = "Y";
            

                inDataTable.Rows.Add(newRow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_ELTR_CERT_GR_EQPT", "INDATA", "OUTDATA", inDataTable);
                Util.GridSetData(dgEqptCond, dtRslt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        #endregion

        #region 설비정보조회 함수  : SetEquipment()
        /// <summary>
        /// 설비정보 조회
        /// </summary>
        private void SetEquipment( )
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("COATER_EQPT_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("EQPT_RSLT_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = _LineID;
                dr["PROCID"] = _ProcID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cboEquipment.DisplayMemberPath = "CBO_NAME";
                cboEquipment.SelectedValuePath = "CBO_CODE";

                DataRow drIns = dtResult.NewRow();
                drIns["CBO_NAME"] = "-SELECT-";
                drIns["CBO_CODE"] = "SELECT";
                dtResult.Rows.InsertAt(drIns, 0);

                cboEquipment.ItemsSource = dtResult.Copy().AsDataView();
                cboEquipment.SelectedValue = _EqptID;

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        #endregion

        #region 프로그래스바 관련 함수  : ShowLoadingIndicator(), HiddenLoadingIndicator()
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

        #endregion

        #endregion


      
    }
}