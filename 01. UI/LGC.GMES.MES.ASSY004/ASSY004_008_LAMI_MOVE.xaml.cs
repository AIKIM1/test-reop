/*************************************************************************************
 Created Date : 2020.11.03
      Creator : 오화백K
   Decription : CWA3동 증설 - Lami랏이동
--------------------------------------------------------------------------------------
 [Change History]
  2020.11.03  오화백K : Initial Created.

**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using System.Linq;
using System.Reflection;
using C1.WPF.DataGrid;
using System.Windows.Media;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_008_LAMI_MOVE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_008_LAMI_MOVE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
       
        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();
        private string _AreaID = string.Empty;

        #endregion

        #region Initialize 
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        public ASSY004_008_LAMI_MOVE()
        {
            InitializeComponent();
        }
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _AreaID = Util.NVC(tmps[0]);

            }
            else
            {
                _AreaID = "";

            }
            txtWaitPancakeLot.Focus();
            ApplyPermissions();
            Forcing_Move_Visible();
            SetLine(cboLine);

        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnRework);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Event

        #region 랏 조회 (버튼)  : btnSearch_Click()
        /// <summary>
        /// 랏 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                GetMoveLot();
                txtWaitPancakeLot.Clear();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 랏 조회 (텍스트박스)  : txtWaitPancakeLot_KeyDown()

        /// <summary>
        /// 라 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtWaitPancakeLot_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    GetMoveLot();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 닫기 : btnClose_Click()
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        #endregion

        #region 라미 라인이동 : btnRework_Click()

        /// <summary>
        /// 라미라인이동
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnRework_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }
            MoveLamiWail();
        }

        #endregion

        #region 시생산 체크여부 : chkPilot_Checked(), chkPilot_Unchecked()

        private void chkPilot_Checked(object sender, RoutedEventArgs e)
        {
            cboLine_All.Visibility = Visibility.Visible;
        }

        private void chkPilot_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkForcingMove.IsChecked == false)
            {
                cboLine_All.Visibility = Visibility.Collapsed;
            }
        }

        #endregion

        #region 강제이동 체크여부 : chkForcingMove_Checked(), chkForcingMove_Unchecked()
        private void chkForcingMove_Checked(object sender, RoutedEventArgs e)
        {
            cboLine_All.Visibility = Visibility.Visible;
        }

        private void chkForcingMove_Unchecked(object sender, RoutedEventArgs e)
        {
            if (chkPilot.IsChecked == false)
            {
                cboLine_All.Visibility = Visibility.Collapsed;
            }
        }
        #endregion

        #endregion

        #region Mehod

        #region 초기화 : InitControl()
        /// <summary>
        /// 초기화
        /// </summary>
        private void InitControl()
        {
            // HOLD 사유 코드
            // 그리드
            Util.gridClear(dgWaitLot);
            cboLine.SelectedIndex = 0;
            txtWaitPancakeLot.Focus();
        }
        #endregion

        #region 랏 조회 : GetMoveLot()

        /// <summary>
        /// 랏 조회
        /// </summary>
        private void GetMoveLot()
        {
            try
            {
                if (txtWaitPancakeLot.Text.Trim().Equals(""))
                {
                    // 조회할 LOT이 없습니다.
                    Util.MessageValidation("SFU1190", (result) =>
                    {
                        txtWaitPancakeLot.Focus();

                    }, txtWaitPancakeLot.Text);
                    return;
                }

                ShowLoadingIndicator();
                //Column 생성
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));

                //Row추가
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtWaitPancakeLot.Text.Trim().ToString();
                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_MOVE_LOT_VD_TO_LAM_L", "RQSTDT", "RSLTDT", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.GridSetData(dgWaitLot, searchResult, null, true);

                        txtWaitPancakeLot.Text = string.Empty;
                        txtWaitPancakeLot.Focus();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HideLoadingIndicator();
                    }
                }
                );
            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Lami 라인이동  : MoveLamiWail()
        private void MoveLamiWail()
        {
            try
            {
                Util.MessageConfirm("SFU1763", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {

                        string ToEqsgid = string.Empty;

                        if (cboLine.IsVisible == true)
                        {
                            ToEqsgid = cboLine.SelectedValue.ToString();
                        }
                   


                        DataTable inTable = new DataTable();
                        inTable.Columns.Add("SRCTYPE");
                        inTable.Columns.Add("IFMODE");
                        inTable.Columns.Add("USERID");
                        inTable.Columns.Add("AREAID");
                        inTable.Columns.Add("FROM_EQSGID");
                        inTable.Columns.Add("FROM_PROCID");
                        inTable.Columns.Add("LOTID");
                        inTable.Columns.Add("TO_EQSGID");

                        DataRow newRow = inTable.NewRow();
                        newRow["SRCTYPE"] = "UI";
                        newRow["IFMODE"] = "OFF";
                        newRow["USERID"] = LoginInfo.USERID;
                        newRow["AREAID"] = _AreaID;
                        newRow["FROM_EQSGID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[0].DataItem, "EQSGID"));
                        newRow["FROM_PROCID"] = "A6000";
                        newRow["LOTID"] = Util.NVC(DataTableConverter.GetValue(dgWaitLot.Rows[0].DataItem, "LOTID"));
                        newRow["TO_EQSGID"] = ToEqsgid == string.Empty ? null : ToEqsgid;

                        inTable.Rows.Add(newRow);

                        new ClientProxy().ExecuteService("BR_PRD_REG_MOVE_LOT_VD_TO_LAM_L", "INDATA", null, inTable, (RSLTDT, ex) =>
                        {
                            try
                            {
                                if (ex != null)
                                {
                                    Util.MessageException(ex);
                                    return;
                                }

                                //정상 처리 되었습니다.
                                Util.MessageInfo("SFU1275");
                                InitControl();
                            }
                            catch (Exception ex1)
                            {
                                Util.MessageException(ex1);
                            }
                            finally
                            {

                            }
                        });
                    }
                });

            }
            catch (Exception ex)
            {
                HideLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region 시생산 라인아이디 조회 : SetLine()

        private void SetLine(C1ComboBox cbo)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _AreaID;
                dr["PROCID"] = "A7000";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_BY_PROCID_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                cbo.DisplayMemberPath = "CBO_NAME";
                cbo.SelectedValuePath = "CBO_CODE";

                DataRow drALL = dtResult.NewRow();
                drALL["CBO_NAME"] = "-SELECT-";
                drALL["CBO_CODE"] = "";
                dtResult.Rows.InsertAt(drALL, 0);

                cbo.ItemsSource = dtResult.Copy().AsDataView();

                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Validation : Validation()
        private bool Validation()
        {
            if (chkPilot.IsChecked == true)
            {
                if (cboLine.SelectedIndex == 0)
                {
                    Util.MessageValidation("SFU1223"); //라인을 선택하세요
                    return false;
                }
            }


            if (dgWaitLot.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537"); //조회된 데이터가 없습니다.
                return false;
            }
            return true;
        }
        #endregion

        #region 강제이동 체크박스 Visible : Forcing_Move_Visible()
        /// <summary>
        /// 공통코드에 등록된 권한에 따라 강제이동 체크박스 
        /// </summary>

        private void Forcing_Move_Visible()
        {

            try
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("CMCODE", typeof(string));
                RQSTDT.Columns.Add("CMCDIUSE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = "LAMI_FORCING_MOVE";
                dr["CMCDIUSE"] = "Y";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_USE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    for(int i=0; i< dtResult.Rows.Count; i++)
                    {

                        DataTable Auth = new DataTable("RQSTDT");
                        Auth.Columns.Add("USERID", typeof(string));
                        Auth.Columns.Add("AUTHID", typeof(string));

                        DataRow drAuth = Auth.NewRow();
                        drAuth["USERID"] = LoginInfo.USERID;
                        drAuth["AUTHID"] = dtResult.Rows[i]["CMCODE"] ;
                        Auth.Rows.Add(drAuth);

                        DataTable dtDrauthResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", Auth);

                        if(dtDrauthResult.Rows.Count>0)
                        {
                            chkForcingMove.Visibility = Visibility.Visible;
                            break;
                        }

                    }

                }
                else
                {
                    chkForcingMove.Visibility = Visibility.Collapsed;
                }


            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }



        #endregion


        #region [Func]


        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HideLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #endregion
   



        
    }
}
