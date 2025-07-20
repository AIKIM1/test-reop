/*************************************************************************************
 Created Date : 2021.01.18
      Creator : 오화백K
   Decription : 활성화 트레이 공급/중지
--------------------------------------------------------------------------------------
 [Change History]
  2021.01.18  INS 오화백K : Initial Created.

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


namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY004_007_FORMATION_REQUEST_BLOCK.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_007_FORMATION_REQUEST_BLOCK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        //설비ID 변수
        private string _EqptID = string.Empty;
       
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
        /// <summary>
        /// 생성자
        /// </summary>
        public ASSY004_007_FORMATION_REQUEST_BLOCK()
        {
            InitializeComponent();
        }
        //LOAD
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //부모 화면에서 넘어온 파라미터 셋팅
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                _EqptID = Util.NVC(tmps[0]);

                GetTrayInfo();
            }
            else
            {
                _EqptID = "";

            }
            //ApplyPermissions();

        }
        #endregion

        #region Event

        #region 공급중지 버튼 이벤트  : btnStop_Click()
        /// <summary>
        /// 공급중지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {

            //공급중지 하시겠습니까?
            Util.MessageConfirm("SFU8320", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        try
                        {
                            DataTable dt = new DataTable();
                            dt.TableName = "INDATA";
                            dt.Columns.Add("EQPTID", typeof(string));
                            dt.Columns.Add("UPDUSER", typeof(string));

                            DataRow dr = dt.NewRow();
                            dr["EQPTID"] = _EqptID;
                            dr["UPDUSER"] = LoginInfo.USERID;
                            dt.Rows.Add(dr);
                            ////new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOP_TRAY_FOR_PKG", "INDATA", "OUTDATA", dt);
                            new ClientProxy().ExecuteServiceSync("BR_PRD_REG_STOP_TRAY_FOR_PKG", "INDATA", null, dt);

                            GetTrayInfo();
                           

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        HideLoadingIndicator();


                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);

                    }

                }
            });
        }


        #endregion

        #region 변경 버튼 이벤트  : btnChange_Click()
        /// <summary>
        /// 변경
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {


            if (cboTrayType.SelectedIndex == 0)
            {
                //Util.Alert("Tray Type을 선택하세요");
                Util.MessageValidation("SFU4925", ObjectDic.Instance.GetObjectName("Tray Type"));
                return;
            }

            //변경 하시겠습니까?
            Util.MessageConfirm("SFU8321", result =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        ShowLoadingIndicator();

                        try
                        {

                            DataSet indataSet = new DataSet();

                            DataTable inData = indataSet.Tables.Add("INDATA");
                            inData.Columns.Add("EQPTID", typeof(string));
                            inData.Columns.Add("TRAY_TYPE_CD", typeof(string));
                            inData.Columns.Add("UPDUSER", typeof(string));

                            DataRow row = inData.NewRow();
                            row["EQPTID"] = _EqptID;
                            row["TRAY_TYPE_CD"] = Util.NVC(cboTrayType.SelectedValue);
                            row["UPDUSER"] = LoginInfo.USERID;
                            indataSet.Tables["INDATA"].Rows.Add(row);



                            new ClientProxy().ExecuteService_Multi("BR_PRD_REG_CHG_TRAY_TYPE_FOR_PKG", "INDATA", null, (bizResult, bizException) =>
                            {
                                try
                                {
                                    HideLoadingIndicator();
                                    if (bizException != null)
                                    {
                                        Util.MessageException(bizException);
                                        return;
                                    }
                                    //재조회
                                    GetTrayInfo();

                                    // 변경되었습니다..
                                    Util.MessageInfo("SFU1166");
                                }
                                catch (Exception ex)
                                {
                                    HideLoadingIndicator();
                                    Util.MessageException(ex);
                                }

                            }, indataSet);

                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                        HideLoadingIndicator();


                    }
                    catch (Exception ex)
                    {
                        HideLoadingIndicator();
                        Util.MessageException(ex);

                    }

                }
            });
        }


        #endregion

        #region 닫기 버튼 이벤트 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region Mehod

        #region 팝업시작시 Tray Type 콤보박스 조회  : GetTrayInfo()
        private void GetTrayInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("PKG_EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["PKG_EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_INF_PFC3_SEL_TRAY_TYPE_EQPTID_CBO", "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (bizResult == null || bizResult.Rows.Count == 0)
                        {
                            return;
                        }
                        else
                        {

                            //팝업DA의 OUTPUT 컬럼이  TRAY_TYPE_CD  하나여서 DataTable을 만들어서 콤보박스에 바인딩

                            DataRow row = null;
                            DataTable BindData = new DataTable();
                            BindData.Columns.Add("CBO_NAME", typeof(string));
                            BindData.Columns.Add("CBO_CODE", typeof(string));

                            for (int i = 0; i < bizResult.Rows.Count; i++)
                            {
                                row = BindData.NewRow();
                                row["CBO_NAME"] = bizResult.Rows[i]["TRAY_TYPE_CD"].ToString();
                                row["CBO_CODE"] = bizResult.Rows[i]["TRAY_TYPE_CD"].ToString();
                                BindData.Rows.Add(row);
                            }
                            row = BindData.NewRow();
                            row["CBO_CODE"] = "";
                            row["CBO_NAME"] = "-SELECT-";
                            BindData.Rows.InsertAt(row, 0);

                            cboTrayType.ItemsSource = BindData.Copy().AsDataView();
                            cboTrayType.SelectedIndex = 0;

                            //Tray Type이 조회가 된뒤  설비 상세 조회 
                            GetEqptInfo();

                        }
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
        }

        #endregion

        #region 현재 PKG 설비에 설정된 Tray 정보 조회 : GetEqptInfo()
        private void GetEqptInfo()
        {
            try
            {
                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_SEL_TRAY_SUPPLY_INFO_BY_EQPTID", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        if (bizResult != null && bizResult.Rows.Count > 0)
                        {
                            txtEqptid.Text = bizResult.Rows[0]["EQPTNAME"].ToString();
                            txtEqptid.Tag = bizResult.Rows[0]["EQPTID"].ToString();
                            txtModelLot.Text = bizResult.Rows[0]["MDLLOT"].ToString();
                            txtSupply.Text = bizResult.Rows[0]["SUPPLY_YN"].ToString();
                            cboTrayType.SelectedValue = bizResult.Rows[0]["TRAY_TYPE_CD"].ToString();
                        }
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
        }
        #endregion
        
        #region 버튼 권한 셋팅(주석처리중)  : ApplyPermissions() 
        //private void ApplyPermissions()
        //{
        //    List<Button> listAuth = new List<Button>();
        //    listAuth.Add(btnStop);
        //    listAuth.Add(btnChange);

        //    Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        //}


        #endregion

        #region 프로그래스 바 관련 : ShowLoadingIndicator(), HideLoadingIndicator()

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
