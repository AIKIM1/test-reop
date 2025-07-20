/*************************************************************************************
 Created Date : 2018.09.20
      Creator : 오화백
   Decription : CNV 상태변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.20  DEVELOPER : Initial Created.
    
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace LGC.GMES.MES.MCS001
{
    /// <summary>
    /// MCS001_006_CHANGE_PORT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MCS001_006_CHANGE_CNV : C1Window, IWorkArea
    {

        #region Initialize
        private bool _load = true;
        /// <summary>
        /// 생성자
        /// </summary>
        public MCS001_006_CHANGE_CNV()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }

        /// <summary>
        /// 폼로드
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_load)
            {
                //사용자 권한별로 버튼 숨기기
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSelect);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                //콤보셋팅
                this.InitCombo();
           
                _load = false;
            }
        }

        #endregion

        #region Event

        #region 컨베어 변경 버튼 : btnChange_Click()
        /// <summary>
        /// 컨베어 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, System.Windows.RoutedEventArgs e)
        {

            if (!ValidationReturn())
                return;
            // 컨베이어 상태를 변경하시겠습니까?
            Util.MessageConfirm("SFU5067", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    PortChange();
                }
            });
        }

        #endregion

        #region 팝업 닫기 : btnClose_Click()
        /// <summary>
        /// 팝업닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region  설비선택이벤트 : cboCNV_SelectedItemChanged()
        private void cboCNV_SelectedItemChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            if (cboCNV.Items.Count > 0 && cboCNV.SelectedValue != null && !cboCNV.SelectedValue.Equals(""))
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANG", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANG"] = LoginInfo.LANGID;
                dr["EQPTID"] = cboCNV.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_MCS_SEL_CNV_DIRECTION", "RQSTDT", "RSLTDT", RQSTDT);
                cboCNVMode.SelectedValue = dtResult.Rows[0]["EQPT_WRK_MODE"].ToString();

            }
            else
            {

                cboCNVMode.SelectedIndex = 0;
            }
        }

        #endregion

        #endregion

        #region UserMethod

        #region 콤보박스 셋팅 : InitCombo()
        /// <summary>
        /// 콤보박스 셋팅
        /// </summary>
        private void InitCombo()
        {

            CommonCombo _combo = new CommonCombo();
            //컨베어 방향
            String[] sFilter1 = { "EQPT_WRK_MODE" };
            _combo.SetCombo(cboCNVMode, CommonCombo.ComboStatus.SELECT, sFilter: sFilter1, sCase: "COMMCODE");

            // 컨베어 설비 조회
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANG", typeof(string));

            DataRow drRow = RQSTDT.NewRow();
            drRow["LANG"] = LoginInfo.LANGID;
            RQSTDT.Rows.Add(drRow);

            new ClientProxy().ExecuteService("DA_MCS_SEL_CNV_EQPT", "RQSTDT", "RSLTDT", RQSTDT, (result, Exception) =>
            {
                if (Exception != null)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(Exception), null, "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
                    return;
                }
                DataRow dRow = result.NewRow();

                dRow["EQPTID"] = "";
                dRow["EQPTNAME"] = "-SELECT-";
                result.Rows.InsertAt(dRow, 0);

                cboCNV.ItemsSource = DataTableConverter.Convert(result);
                if (result.Rows.Count > 0)
                {
                    cboCNV.SelectedIndex = 0;
                }
                else if (result.Rows.Count == 0)
                {
                    cboCNV.SelectedItem = null;
                }
            });


        }
        #endregion

        #region  Validation : ValidationReturn()
        /// <summary>
        /// Validation
        /// </summary>
        /// <returns></returns>
        private bool ValidationReturn()
        {

            if (cboCNVMode.SelectedIndex <= 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("CNV방향"));
                return false;
            }

            if (cboCNV.SelectedIndex <= 0)
            {
                // %1(을)를 선택하세요.
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("설비"));
                return false;
            }
            return true;
        }

        #endregion

        #region 컨베어 변경  : PortChange()
        /// <summary>
        /// 컨베어 변경 
        /// </summary>
        private void PortChange()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                DataTable inTable = new DataTable();
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("EQPT_WRK_MODE", typeof(string));
                inTable.Columns.Add("UPDUSER", typeof(string));
                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["EQPTID"] = cboCNV.SelectedValue.ToString();
                newRow["EQPT_WRK_MODE"] = cboCNVMode.SelectedValue.ToString();
                newRow["UPDUSER"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);
                new ClientProxy().ExecuteService("BR_MCS_REG_CNV_WAY", "INDATA", null, inTable, (result, searchException) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }

        #endregion

        #endregion

        
    }
}
