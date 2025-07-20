/*************************************************************************************
Created Date : 2018.06.21
      Creator : 손홍구
   Decription : 공정별 작업지시
--------------------------------------------------------------------------------------
 [Change History]
  2018.06.21  DEVELOPER : Initial Created.
  2018.11.09 손우석 CWA.PACK5호, 모듈5호, 모듈9호 증설 및 모듈2호 개조 GMES구축 - 작업지시 등록 시작 시간 및 종료 시간 추가
  2019.05.07 염규범 공정별 W/O 시 제품 비교 Validation 부분 제거
  2019.06.12 손우석 작업지시 등록시 시작 시간 및 종료 시간 설정 누락 로직 구현
  2019.06.26 손우석 작업지시 등록시 시작 시간 및 종료 시간 설정 오류 수정
  2019.10.10 염규범 작업지시 해제시, GMES LOGIN 된 작업자 ID 저장 추가
  2019.10.17 손우석 공정별 W/O에서 종료 일자 기간 설정(다음 날 05:59)
  2019.12.23 염규범 W/O 선택시 주석처리
  2019.12.24 손우석 W/O 선택시 오창 요구사항으로 분기 처리로 변경
  2019.12.24 염규범 빈값 클릭후  그룹 W/O 해제시, 전체 W/O 해체시 오류건 수정
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_033 : UserControl, IWorkArea
    {
        #region Declaration & Constructor
        DataTable dtwoNotePre = new DataTable();
        Util _Util = new Util();
        DataTable dtOrgWO = new DataTable();

        private C1.WPF.DataGrid.DataGridRow drSelectedDataGrid;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        public PACK001_033()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialize
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
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnWoClear_All);
                listAuth.Add(btnWoClear_One);
                //listAuth.Add(btnWoClose);
                listAuth.Add(btnWoSave);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                //2019.10.17
                dtpStrtDate.SelectedDateTime = DateTime.Today;
                dtpEndDate.SelectedDateTime = DateTime.Today.AddDays(1);

                //2018.11.08
                Util.Set_Pack_cboTimeList(stHour, "CBO_NAME", "CBO_CODE", "06:00:00");
                //2019.10.17
                //Util.Set_Pack_cboTimeList(edHour, "CBO_NAME", "CBO_CODE", "23:00:00");
                Util.Set_Pack_cboTimeList(edHour, "CBO_NAME", "CBO_CODE", "05:00:00");

                Util.Set_Pack_cboTimeList2(stMinute, "CBO_NAME", "CBO_CODE", "05:00:00");
                //2019.10.17
                //Util.Set_Pack_cboTimeList2(edMinute, "CBO_NAME", "CBO_CODE", "23:59:00");
                Util.Set_Pack_cboTimeList2(edMinute, "CBO_NAME", "CBO_CODE", "05:59:00");

                setComboBox();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        #endregion Initialize

        #region Event

        #region Button

        private void btnWorkOrderSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Refresh();

                setDgOrderList();
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnWoSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (saveValidation())
                {
                    selWorkOrder_DateCheck();
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void sWoSave_CHK()
        {
            try
            {
                //2019.12.24
                if (LoginInfo.CFG_SHOP_ID != "A040")
                {
                    #region [ 기존 로직 - 주석 이유 확인 필요 ]
                    // 2019.10.17   -> 2019.12.24 주석 해제
                    string sEQSGID = Util.NVC(txtSeletedEQSG.Tag);
                    string sPCSGID = Util.NVC(txtSeletedPCSG.Tag);
                    string sPRODID = txtSeletedPRODUCT.Text;
                    string sWOID = txtSeletedWo.Text;
                    string sWOTYPE = txtSeletedWoType.Text;

                    DataTable dtWo = new DataTable();
                    dtWo = DataTableConverter.Convert(dgWorkOrderList.ItemsSource);

                    foreach (DataRow drRow in dtWo.Rows)
                    {
                        string sCHK = drRow["CHK"].ToString();
                        string sProdIDOri = drRow["PRODID"].ToString();
                        string sWoTypeOri = drRow["WOTYPE"].ToString();

                        if (sCHK == "1")
                        {
                            if (sProdIDOri == sPRODID)
                            {
                                //W/O 타입이 다릅니다. W/O를 변경하시겠습니까?
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3660"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                {
                                    if (result != MessageBoxResult.OK)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID);
                                    }
                                });
                            }
                            else
                            {
                                //제품코드가 다릅니다. W/O를 변경하시겠습니까?
                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3659"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                {
                                    if (result != MessageBoxResult.OK)
                                    {
                                        return;
                                    }
                                    else
                                    {
                                        wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID);
                                    }
                                });
                            }
                        }

                    }

                }
                #endregion

                else
                {
                    // 2019-12-23 염규범
                    // 공정별 W/O는 W/O와 무관하게 걸려야함
                    // 같은 제품이라도 다른 W/O 사용할수 있음
                    // 기존로직과 완전 동일
                    // 특별한 사유가 있으면 해당 주석 해체 처리 필요

                    //2019.12.24  오창 분기 처리
                    GetWOrkrder();

                    if (dtOrgWO.Rows.Count > 0)
                    {
                        string OrgWoProd = dtOrgWO.Rows[0]["PRODID"].ToString();

                        if (txtSeletedPRODUCT.Text != OrgWoProd)
                        {
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU7342"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result1) =>
                            {
                                if (result1 != MessageBoxResult.OK)
                                {

                                }
                                else
                                {
                                    string sEQSGID = Util.NVC(txtSeletedEQSG.Tag);
                                    string sPCSGID = Util.NVC(txtSeletedPCSG.Tag);
                                    string sPRODID = txtSeletedPRODUCT.Text;
                                    string sWOID = txtSeletedWo.Text;
                                    string sWOTYPE = txtSeletedWoType.Text;

                                    DataTable dtWo = new DataTable();
                                    dtWo = DataTableConverter.Convert(dgWorkOrderList.ItemsSource);


                                    string StDate = dtpStrtDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + stHour.Text + ":" + stMinute.Text + ":00";
                                    string EdDate = dtpEndDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + edHour.Text + ":" + edMinute.Text + ":00";

                                    // 시작시간이 종료시간보다 빠르면 ERROR
                                    if (Util.NVC_Decimal(Convert.ToDateTime(StDate).ToString("yyyyMMddHHmmss")) >
                                        Util.NVC_Decimal(Convert.ToDateTime(EdDate).ToString("yyyyMMddHHmmss")))
                                    {
                                        Util.MessageValidation("SFU2954");  //종료시간이 시작시간보다 빠를 수는 없습니다.
                                        return;
                                    }

                                    if (!ChkPrdtRout(Util.NVC(cboRouteByPcsgid.SelectedValue), txtSeletedPRODUCT.Text).Equals(true))
                                    {
                                        Util.MessageValidation("SFU8138", sPRODID, Util.NVC(cboRouteByPcsgid.SelectedValue));
                                        return;
                                    }

                                    foreach (DataRow drRow in dtWo.Rows)
                                    {
                                        string sCHK = drRow["CHK"].ToString();
                                        string sProdIDOri = drRow["PRODID"].ToString();
                                        string sWoTypeOri = drRow["WOTYPE"].ToString();

                                        if (sCHK == "1")
                                        {
                                            if (sProdIDOri == sPRODID)
                                            {
                                                //W/O 타입이 다릅니다. W/O를 변경하시겠습니까?
                                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3660"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                                {
                                                    if (result != MessageBoxResult.OK)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID);
                                                    }
                                                });
                                            }
                                            else
                                            {
                                                //제품코드가 다릅니다. W/O를 변경하시겠습니까?
                                                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3659"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                                                {
                                                    if (result != MessageBoxResult.OK)
                                                    {
                                                        return;
                                                    }
                                                    else
                                                    {
                                                        wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID);
                                                    }
                                                });
                                            }
                                        }

                                    }
                                }
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnWoClear_One_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);

                if (iwoListIndex == -1)
                {
                    ms.AlertWarning("SFU1741"); //오더해제 대상이 없습니다. 선택하세요.
                    return;
                }

                if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PRODID"))))
                {
                    ms.AlertWarning("SFU1741"); //오더해제 대상이 없습니다. 선택하세요.
                    return;
                }

                string strWOID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "WOID"));
                string strEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                string strROUTID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "ROUTID"));
                string strPROCID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PROCID"));
                string strPRODID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PRODID"));

                //선택된 개별 작업지시를 해제하시겠습니까?\n[LINE:{0} ,공정군:{1} ,작업지시:{2} ,경로:{3}]
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1647", txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedWo.Text, txtSeletedPRODUCT.Text, Util.NVC(cboRouteByPcsgid.SelectedValue)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        clearGroupWorkOrder(strWOID, strEQSGID, strROUTID, strPROCID, strPRODID, "N");

                        Refresh();

                        setDgOrderList();
                    }
                });
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private static DateTime GetSystemTime()
        {
            DateTime systemDateTime = new DateTime();

            const string bizRuleName = "BR_CUS_GET_SYSTIME";
            DataTable inDataTable = new DataTable("INDATA");
            DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

            if (CommonVerify.HasTableRow(dtResult))
            {
                systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
            }

            return systemDateTime;
        }
        private void btnWoClear_Group_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);

                if (iwoListIndex == -1)
                {
                    ms.AlertWarning("SFU1741"); //오더해제 대상이 없습니다. 선택하세요.
                    return;
                }

                string strWOID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "WOID"));
                string strEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                string strROUTID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "ROUTID"));
                string strPROCID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PROCID"));
                string strPRODID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PRODID"));

                // 그룹 워크오더 해제 하시겠습니까? 
                // 메세지 만들것
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1647", txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedWo.Text, txtSeletedPRODUCT.Text, Util.NVC(cboRouteByPcsgid.SelectedValue)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                {

                    if (string.IsNullOrWhiteSpace(Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PRODID"))))
                    {
                        ms.AlertWarning("SFU1741"); //오더해제 대상이 없습니다. 선택하세요.
                        return;
                    }


                    if (result == MessageBoxResult.OK)
                    {
                        clearGroupWorkOrder(strWOID, strEQSGID, strROUTID, strPROCID, strPRODID, "Y");

                        Refresh();

                        setDgOrderList();
                    }
                });
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void btnWoClear_All_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string strEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue).ToString();
                string strEQSGNAME = cboEquipmentSegment.Text.ToString();

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1285", strEQSGNAME), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //[LINE:{0}] 의 오더를 전체 해제하시겠습니까?
                {
                    if (!(dgWorkOrderList.Rows.Count > 0) || Util.gridFindIsDataRow(ref dgWorkOrderList, "PRODID", "", false) == -1)
                    {
                        ms.AlertWarning("SFU1905"); //조회된 Data가 없습니다.
                        return;
                    }

                    /*
                    if (iwoListIndex != -1)
                    {
                        ms.AlertWarning("SFU1741"); //오더해제 대상이 없습니다. 선택하세요.
                        return;
                    }
                    */

                    if (result == MessageBoxResult.OK)
                    {
                        clearGroupWorkOrderAll(strEQSGID, "N");

                        Refresh();

                        setDgOrderList();
                    }
                });


                /*
                if (!(dgWorkOrderList.Rows.Count > 0))
                {
                    ms.AlertWarning("SFU1905"); //조회된 Data가 없습니다.
                    return;
                }

                
                int [] iwoIndex = Util.getSelectedRows(dgWorkOrderList);
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);

                int temp = dgWorkOrderList.Rows.Count;
                    

                string strWOID   = string.Empty;
                string strEQSGID = string.Empty;
                string strROUTID = string.Empty;
                string strPROCID = string.Empty;
                string strPRODID = string.Empty;


                DataTable dtWoMuti = new DataTable();

                dtWoMuti.TableName = "INDATA";
                dtWoMuti.Columns.Add("WOID", typeof(string));
                dtWoMuti.Columns.Add("ROUTID", typeof(string));
                dtWoMuti.Columns.Add("EQSGID", typeof(string));
                dtWoMuti.Columns.Add("PROCID", typeof(string));
                dtWoMuti.Columns.Add("PRODID", typeof(string));
                dtWoMuti.Columns.Add("GROUP_YN", typeof(string));
                dtWoMuti.Columns.Add("USERID", typeof(string));

                for(int i=0; i <  temp; i++)
                {
                    strWOID   = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "WOID"));
                    strEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "EQSGID"));
                    strROUTID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "ROUTID"));
                    strPROCID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "PROCID"));
                    strPRODID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "PRODID"));

                    DataRow Indata = dtWoMuti.NewRow();
                    Indata["WOID"] = strWOID;
                    Indata["ROUTID"] = strROUTID;
                    Indata["EQSGID"] = strEQSGID;
                    Indata["PROCID"] = strPROCID;
                    Indata["PRODID"] = strPRODID;
                    Indata["GROUP_YN"] = "N";
                    Indata["USERID"] = LoginInfo.USERID;
                    dtWoMuti.Rows.Add(Indata);

                }

                if (iwoListIndex.Equals(null)){
                    strWOID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "WOID"));
                    strEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                }
                else
                {
                    strWOID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[0].DataItem, "WOID"));
                    strEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[0].DataItem, "EQSGID"));
                }
                
                clearGroupWorkOrderMuti(dtWoMuti);
                */

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        // W/O 선택시 전월 또는 익월인 W/O인 경우 인터락
        private void selWorkOrder_DateCheck()
        {
            try
            {
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);
                C1.WPF.DataGrid.DataGridRow drDataGrid = dgWorkOrderList.Rows[iwoListIndex];
                string AREAID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "AREAID"));

                DataTable dt = ((DataView)dgPlanWorkorderList.ItemsSource).ToTable();
                dt.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

                // 선택한 W/O의 계획시작일
                string sPLANSTDTTM = dt.Rows[0]["PLANSTDTTM"].ToString();
                DateTime cPLANSTDTTM = DateTime.Parse(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString(sPLANSTDTTM));

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("PLANSTDTTM", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["PLANSTDTTM"] = cPLANSTDTTM.ToString("yyyy-MM");
                dr["WOID"] = dt.Rows[0]["WOID"].ToString();
                dr["AREAID"] = AREAID;
                INDATA.Rows.Add(dr);

                new ClientProxy().ExecuteService("BR_PRD_CHK_DATE_FOR_WORKORDER_PACK", "INDATA", null, INDATA, (bizResult, bizException) =>
                {
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }
                    else
                    {
                        sWoSave_CHK();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        //private void btnWoClose_Click(object sender, RoutedEventArgs e)
        //{
        //    try
        //    {
        //        if (saveValidation())
        //        {
        //            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1647", txtSeletedWo.Text, txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedPRODUCT.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
        //                //선택된 정보로 작업지시를 마감하시겠습니까?\n[작업지시:{0} ,LINE:{1} ,공정군:{2} ,제품ID:{3} ]
        //                {
        //                    if (result == MessageBoxResult.OK)
        //                    {
        //                        string sSHOPID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "SHOPID"));
        //                        string sAREAID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "AREAID"));
        //                        string sEQSGID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "EQSGID"));
        //                        string sPCSGID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "PCSGID"));
        //                        closeWorkOrder(sSHOPID, sAREAID,txtSeletedWo.Text);

        //                        //초기화
        //                        Refresh();

        //                        //오더조회
        //                        setDgOrderList();

        //                        int iwoListIndex = 0;
        //                        for (int i = 0; i < dgWorkOrderList.Rows.Count; i++)
        //                        {
        //                            if (Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "AREAID")) == sAREAID
        //                            && Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "EQSGID")) == sEQSGID
        //                            && Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "PCSGID")) == sPCSGID
        //                            )
        //                            {
        //                                iwoListIndex = i;
        //                                break;
        //                            }
        //                        }
        //                        //마감전 선택한 오더선택정보 유지.
        //                        DataTableConverter.SetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "CHK", "1");

        //                        //setDgPlanWorkorderList(dgWorkOrderList.Rows[iwoListIndex]);
        //                    }
        //                });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //Util.Alert(ex.ToString());
        //        Util.MessageException(ex);
        //    }
        //}

        #endregion Button

        #region Date

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);

                txtSeletedPRODUCT.Text = "";
                txtSeletedWo.Text = "";
                txtSeletedWoType.Text = "";

                if (iwoListIndex == -1)
                {
                    return;
                }
                else
                {
                    setDgPlanWorkorderList(dgWorkOrderList.Rows[iwoListIndex]);
                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        #endregion Date
        #region Grid

        private void dgWorkOrderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWorkOrderList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    DataTableConverter.SetValue(dgWorkOrderList.Rows[cell.Row.Index].DataItem, "CHK", "1");
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgWorkOrderList_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);

                if (iwoListIndex == -1)
                {
                    return;
                }

                txtSeletedEQSG.Text = string.Empty;
                txtSeletedPCSG.Text = string.Empty;
                txtSeletedPROC.Text = string.Empty;
                txtSeletedPRODUCT.Text = string.Empty;
                txtSeletedWo.Text = string.Empty;
                txtSeletedWoType.Text = string.Empty;

                txtSeletedEQSG.Tag = string.Empty;
                txtSeletedPCSG.Tag = string.Empty;
                txtSeletedPROC.Tag = string.Empty;
                txtSeletedPRODUCT.Tag = string.Empty;
                txtSeletedWo.Tag = string.Empty;
                txtSeletedWoType.Tag = string.Empty;

                cboRouteByPcsgid.ItemsSource = null;
                cboRouteByPcsgid.SelectedValue = null;

                txtSeletedEQSG.Tag = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                txtSeletedPCSG.Tag = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));
                txtSeletedPROC.Tag = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PROCID"));
                txtSeletedEQSG.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGNAME"));
                txtSeletedPCSG.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGNAME"));
                txtSeletedPROC.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PROCNAME"));
                //string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                //string sPCSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));
                //string sPRJ_NAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PRJ_NAME"));

                //setComboBox_Route(sEQSGID, sPCSGID);

                setDgPlanWorkorderList(dgWorkOrderList.Rows[iwoListIndex]);

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }


        private void dgPlanWorkorderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPlanWorkorderList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    DataTableConverter.SetValue(dgPlanWorkorderList.Rows[cell.Row.Index].DataItem, "CHK", "1");
                }
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dgPlanWorkOrderListChoice_Checked(object sender, RoutedEventArgs e)
        {
            try
            {
                if (sender == null)
                    return;

                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);
                if (iwoListIndex == -1)
                {
                    return;
                }

                string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                string sPCSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
                {
                    int iwoPlanListIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                    for (int i = 0; i < dg.GetRowCount(); i++)
                    {
                        if (iwoPlanListIndex == i)
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                        else
                            DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                    }

                    //row 색 바꾸기
                    dgPlanWorkorderList.SelectedIndex = iwoListIndex;
                }

                if ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                {
                    int iwoPlanListIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                    if (iwoPlanListIndex == -1)
                    {
                        return;
                    }

                    string sPRJ_NAME = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRJ_NAME"));
                    string sPRODID = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRODID"));

                    string sWoStatCode = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WO_STAT_CODE"));

                    //작업지시상태가 20,40인경우만 선택가능함.
                    if (sWoStatCode == "20" || sWoStatCode == "40")
                    {
                        txtSeletedPRODUCT.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRODID"));
                        txtSeletedWo.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOID"));
                        txtSeletedWoType.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOTYPE"));
                    }
                    else
                    {
                        DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", "0");
                        ms.AlertWarning("SFU1512"); //등록가능한작업지시상태가아닙니다.\n선택가능상태: Release , Process
                    }
                    //수정1
                    setComboBox_Route(sEQSGID, sPCSGID, sPRJ_NAME, sPRODID);
                }

                /*
                 *   2019.05.07 염규범 공정별 W/O 시 제품 비교 Validation 부분 제거
                    if ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
                    {

                    int iwoPlanListIndex = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                        if (iwoPlanListIndex == -1)
                        {
                            return;
                        }

                        string sPRJ_NAME = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRJ_NAME"));
                        string sPRODID = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRODID"));


                        DataTable dtChkOutData = sChk_SelectOrder(sEQSGID, sPRODID);
                        if (dtChkOutData != null)
                        {
                            if (!(dtChkOutData.Rows.Count > 0))
                            {
                                DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", "0");
                            }
                            else
                            {

                            string sWoStatCode = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WO_STAT_CODE"));

                                //작업지시상태가 20,40인경우만 선택가능함.
                                if (sWoStatCode == "20" || sWoStatCode == "40")
                                {
                                    txtSeletedPRODUCT.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "PRODID"));
                                    txtSeletedWo.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOID"));
                                    txtSeletedWoType.Text = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "WOTYPE"));
                                }
                                else
                                {
                                    DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", "0");
                                    ms.AlertWarning("SFU1512"); //등록가능한작업지시상태가아닙니다.\n선택가능상태: Release , Process
                                }

                                setComboBox_Route(sEQSGID, sPCSGID, sPRJ_NAME);

                            }
                        }
                        else
                        {
                            DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", "0");
                            txtSeletedPRODUCT.Text = "";
                            txtSeletedWo.Text = "";
                            txtSeletedWoType.Text = "";
                        }
                    }
                */

                //int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }

        }

        private void dgPlanWorkorderList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                try
                {
                    int current_row = dgPlanWorkorderList.CurrentRow.Index;
                    int current_col = dgPlanWorkorderList.CurrentColumn.Index;
                    string value = dgPlanWorkorderList.CurrentCell.Value.ToString();
                    string old_value = (dtwoNotePre.Rows[current_row] as DataRow)["WO_NOTE"].ToString();

                    // W/O NOTE 값이 변경 됐는지 확인
                    if (value == old_value)
                    {
                        return;
                    }

                    string sWOID = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[current_row].DataItem, "WOID"));
                    string sWONOTE = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[current_row].DataItem, "WO_NOTE"));

                    DataTable RQSTDT = new DataTable();
                    RQSTDT.TableName = "INDATA";
                    RQSTDT.Columns.Add("WOID", typeof(string));
                    RQSTDT.Columns.Add("NOTE", typeof(string));
                    RQSTDT.Columns.Add("USERID", typeof(string));

                    DataRow Indata = RQSTDT.NewRow();
                    Indata["WOID"] = sWOID;
                    Indata["NOTE"] = sWONOTE;
                    Indata["USERID"] = LoginInfo.USERID;

                    RQSTDT.Rows.Add(Indata);

                    if (old_value == null || old_value.Length == 0) //insert
                    {
                        new ClientProxy().ExecuteServiceSync("DA_PRD_INS_WORKORDER_NOTE", "INDATA", "", RQSTDT, null);
                    }
                    else //update
                    {
                        new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_WORKORDER_NOTE", "INDATA", "", RQSTDT, null);
                    }


                }
                catch (Exception ex)
                {
                    //Util.AlertByBiz("DA_PRD_DEL_PCSG_WO_PACK", ex.Message, ex.ToString());
                    Util.MessageException(ex);
                }

            }
        }

        #endregion Grid

        #endregion Event

        #region Mehod
        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //shop
                String[] sFilter = { Area_Type.PACK };
                C1ComboBox[] cboShopChild = { cboAreaByAreaType };
                _combo.SetCombo(SHOP_AUTH, CommonCombo.ComboStatus.SELECT, cbChild: cboShopChild, sFilter: sFilter);

                //동
                C1ComboBox[] cboAreaParent = { SHOP_AUTH };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                //C1ComboBox[] cboAreaChild = { cboEquipmentSegment , cboPRJ_Model };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild, cbParent: cboAreaParent, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboProcessSegmentByEqsgid };
                //C1ComboBox[] cboLineChild = { cboPRJ_Model };
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.NONE, cbChild: cboLineChild, cbParent: cboLineParent);

                //공정군
                C1ComboBox[] cbProcessParent = { cboEquipmentSegment };
                _combo.SetCombo(cboProcessSegmentByEqsgid, CommonCombo.ComboStatus.ALL, cbParent: cbProcessParent);

                //모델
                //C1ComboBox[] cboPRJ_ModelParent = { cboAreaByAreaType,cboEquipmentSegment };
                //_combo.SetCombo(cboPRJ_Model, CommonCombo.ComboStatus.ALL, cbParent: cboPRJ_ModelParent, sCase: "PRJ_MODEL");


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private void setComboBox_Route(string sEQSGID, string sPCSGID, string sPRJ_NAME, string sPRODID)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //route
                String[] sFilter = { sEQSGID, sPCSGID, sPRJ_NAME, sPRODID };
                _combo.SetCombo(cboRouteByPcsgid, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "ROUTEBYPCSGMODLPRODID");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setDgOrderList()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(SHOP_AUTH.SelectedValue) == "" ? null : Util.NVC(SHOP_AUTH.SelectedValue);
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaByAreaType.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_PROC_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);


                Util.GridSetData(dgWorkOrderList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setDgPlanWorkorderList(C1.WPF.DataGrid.DataGridRow drDataGrid)
        {
            try
            {
                if (Convert.ToDecimal(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString("yyyyMMdd")) > Convert.ToDecimal(Convert.ToDateTime(dtpDateTo.SelectedDateTime).ToString("yyyyMMdd")))
                {
                    Util.Alert("SFU1913");      //종료일자가 시작일자보다 빠릅니다.
                    return;
                }

                drSelectedDataGrid = drDataGrid;
                string sSTRT_DTTM_FROM = dtpDateFrom.SelectedDateTime.ToString("yyyyMMdd");
                string sSTRT_DTTM_TO = dtpDateTo.SelectedDateTime.ToString("yyyyMMdd");
                string sSHOPID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "SHOPID"));
                string sAREAID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "AREAID"));
                string sEQSGID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "EQSGID"));
                string sPCSGID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "PCSGID"));
                string sMODELID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "PRJ_NAME"));
                string sWOID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "WOID"));

                //sMODELID = sMODELID == "" ? null : sMODELID;
                sMODELID = null;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM_FROM", typeof(string));
                RQSTDT.Columns.Add("STRT_DTTM_TO", typeof(string));
                RQSTDT.Columns.Add("SHOPID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("PRJ_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["STRT_DTTM_FROM"] = sSTRT_DTTM_FROM;
                dr["STRT_DTTM_TO"] = sSTRT_DTTM_TO;
                dr["SHOPID"] = sSHOPID;
                dr["AREAID"] = sAREAID;
                dr["EQSGID"] = sEQSGID;
                dr["PCSGID"] = sPCSGID;
                dr["PRJ_NAME"] = sMODELID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_PLAN_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                //dgPlanWorkorderList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgPlanWorkorderList, dtResult, FrameOperation, true);
                if (dtResult != null)
                {
                    if (dtResult.Rows.Count > 0)
                    {
                        if (sWOID.Length > 0)
                        {
                            int iwoListIndex = Util.gridFindDataRow(ref dgPlanWorkorderList, "WOID", sWOID, false);
                            if (iwoListIndex > -1)
                            {
                                DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoListIndex].DataItem, "CHK", "1");
                            }
                        }

                        dtwoNotePre = dtResult;
                    }
                }
            }

            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_WORKORDER_PLAN_LIST_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private DataTable sChk_SelectOrder(string sEQSGID, string sPRODID)
        {
            DataTable dtTemp = null;
            try
            {
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["EQSGID"] = sEQSGID;
                drINDATA["PRODID"] = sPRODID;
                INDATA.Rows.Add(drINDATA);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_WORKORDER_PACK", "INDATA", "OUTDATA", dsInput, null);
                if (dsResult.Tables["OUTDATA"] != null)
                {
                    if (dsResult.Tables["OUTDATA"].Rows.Count > 0)
                    {
                        dtTemp = dsResult.Tables["OUTDATA"];
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_CHK_WORKORDER_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
                return dtTemp;
            }
            return dtTemp;
        }

        private DataTable sChk_SelectOrder_Save(string sEQSGID, string sPRODID, string sPCSGID)
        {
            DataTable dtTemp = null;
            try
            {
                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PCSGID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["EQSGID"] = sEQSGID;
                drINDATA["PRODID"] = sPRODID;
                drINDATA["PCSGID"] = sPCSGID;
                INDATA.Rows.Add(drINDATA);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_CHK_WORKORDER_SAVE_PACK", "INDATA", "OUTDATA,CHK", dsInput, null);
                if (dsResult.Tables["CHK"] != null)
                {
                    if (dsResult.Tables["CHK"].Rows.Count > 0)
                    {
                        dtTemp = dsResult.Tables["CHK"];
                    }
                }
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("BR_PRD_CHK_WORKORDER_SAVE_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
                return dtTemp;
            }
            return dtTemp;
        }

        private bool saveValidation()
        {
            bool bReturnValue = true;

            try
            {
                if (!(txtSeletedEQSG.Text.Length > 0) || !(txtSeletedPCSG.Text.Length > 0))
                {
                    ms.AlertWarning("SFU1509"); //등록 대상 공정군이 없습니다.
                    bReturnValue = false;
                    return bReturnValue;
                }

                if (!(txtSeletedWo.Text.Length > 0))
                {
                    ms.AlertWarning("SFU1510"); //등록 대상 작업지시가 없습니다.
                    bReturnValue = false;
                    return bReturnValue;
                }

                if (!(Util.NVC(cboRouteByPcsgid.SelectedValue).Length > 0))
                {
                    ms.AlertWarning("1018", param1: txtSeletedPRODUCT.Text); //Product %1의 Route 흐름 정보가 존재하지 않습니다.
                    //ms.AlertWarning("SFU1455"); //경로를 선택 하세요.
                    bReturnValue = false;
                    return bReturnValue;
                }

                //4호기 B10인경우 서로다른 모델이.. MBOM정보에존재 .. MBOM기준으로 체크되도록 변경..
                //string sWoPRJ_NAME = string.Empty;
                //if (dgPlanWorkorderList.Rows.Count>0 && dgWorkOrderList.Rows.Count > 0)
                //{
                //    int iPlanWoListIndex = Util.gridFindDataRow(ref dgPlanWorkorderList, "CHK", "1", false);
                //    string sPlanWoPRJ_NAME = Util.NVC(DataTableConverter.GetValue(dgPlanWorkorderList.Rows[iPlanWoListIndex].DataItem, "PRJ_NAME"));

                //    bool bCheck = false;
                //    for(int i = 0; i < dgWorkOrderList.Rows.Count; i++)
                //    {
                //        sWoPRJ_NAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "PRJ_NAME"));
                //        if(sPlanWoPRJ_NAME != sWoPRJ_NAME && sWoPRJ_NAME != "")
                //        {
                //            bCheck = true;
                //            break;
                //        }
                //    }
                //    if (bCheck)
                //    {
                //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("다른 모델이 등록 되어있습니다.\n{0}을 해제 후 등록해주세요.", sWoPRJ_NAME), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                //        bReturnValue = false;
                //        return bReturnValue;
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return bReturnValue;
        }

        private void saveWorkOrder()
        {
            try
            {
                //BR_PRD_REG_WO_PACK
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("SRCTYPE", typeof(string));
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("PCSGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("ROUTID", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("VLD_STRT_DTTM", typeof(DateTime));
                INDATA.Columns.Add("VLD_END_DTTM", typeof(DateTime));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PCSGID"] = Util.NVC(txtSeletedPCSG.Tag);
                drINDATA["EQSGID"] = Util.NVC(txtSeletedEQSG.Tag);
                drINDATA["ROUTID"] = Util.NVC(cboRouteByPcsgid.SelectedValue);
                drINDATA["WOID"] = txtSeletedWo.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["PRODID"] = txtSeletedPRODUCT.Text;
                drINDATA["PROCID"] = txtSeletedPROC.Tag;

                string StDate = dtpStrtDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + stHour.Text + ":" + stMinute.Text + ":00";
                string EdDate = dtpEndDate.SelectedDateTime.ToString("yyyy-MM-dd") + " " + edHour.Text + ":" + edMinute.Text + ":00";

                drINDATA["VLD_STRT_DTTM"] = Convert.ToDateTime(StDate);
                drINDATA["VLD_END_DTTM"] = Convert.ToDateTime(EdDate);

                INDATA.Rows.Add(drINDATA);

                new ClientProxy().ExecuteService("BR_PRD_REG_WO_PROC_PACK", "INDATA", null, INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }

                    ms.AlertInfo("SFU1275"); //정상처리되었습니다.
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 2019-12-23 염규범
        /// 미사용 처리 ( 이력 남기는 BR 로 변경 )
        /// </summary>
        /// <param name="sEQSGID"></param>
        /// <param name="sPROCID"></param>
        private void clearWorkOrder(string sEQSGID, string sPROCID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));

                DataRow Indata = RQSTDT.NewRow();
                Indata["EQSGID"] = sEQSGID;
                Indata["PROCID"] = sPROCID;
                RQSTDT.Rows.Add(Indata);

                new ClientProxy().ExecuteServiceSync("DA_PRD_DEL_PROC_WO_PACK", "INDATA", "", RQSTDT, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void clearGroupWorkOrder(string strWO, string strEQSGID, string strROUTID, string strPROCID, string strPRODID, string strGROUP_YN)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("WOID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("GROUP_YN", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow Indata = RQSTDT.NewRow();
                Indata["WOID"] = strWO.Trim();
                Indata["ROUTID"] = strROUTID.Trim();
                Indata["EQSGID"] = strEQSGID.Trim();
                Indata["PROCID"] = strPROCID.Trim();
                Indata["PRODID"] = strPRODID.Trim();
                Indata["GROUP_YN"] = strGROUP_YN.Trim();
                Indata["USERID"] = LoginInfo.USERID.Trim();
                RQSTDT.Rows.Add(Indata);

                new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_WO_PROC_PACK", "INDATA", "", RQSTDT, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void clearGroupWorkOrderAll(string strEQSGID, string strGROUP_YN)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("GROUP_YN", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow Indata = RQSTDT.NewRow();
                Indata["EQSGID"] = strEQSGID.Trim();
                Indata["GROUP_YN"] = strGROUP_YN.Trim();
                Indata["USERID"] = LoginInfo.USERID.Trim();
                RQSTDT.Rows.Add(Indata);

                new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_WO_PROC_PACK", "INDATA", "", RQSTDT, null);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgWorkOrderList);
                Util.gridClear(dgPlanWorkorderList);

                txtSeletedEQSG.Text = string.Empty;
                txtSeletedPCSG.Text = string.Empty;
                txtSeletedPRODUCT.Text = string.Empty;
                txtSeletedWo.Text = string.Empty;
                txtSeletedWoType.Text = string.Empty;

                txtSeletedEQSG.Tag = string.Empty;
                txtSeletedPCSG.Tag = string.Empty;
                txtSeletedPRODUCT.Tag = string.Empty;
                txtSeletedWo.Tag = string.Empty;
                txtSeletedWoType.Tag = string.Empty;

                //cboRouteByPcsgid.ItemsSource = null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void wo_Save(string sEQSGID, string sPRODID, string sPCSGID, string sWOID)
        {
            DataTable dtChkOutData = sChk_SelectOrder_Save(sEQSGID, sPRODID, sPCSGID);

            if (dtChkOutData.Rows.Count > 0)
            {
                if (Util.NVC(dtChkOutData.Rows[0]["CHK_WO"]) == "N")
                {

                    //등록된 W/O 와 등록하려는 W/O[%1]은 상이합니다.\n작업 진행 하시겠습니까?
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3421", sWOID), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            saveWorkOrder();

                            Refresh();

                            setDgOrderList();
                        }
                    });
                }
                else
                {
                    //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(
                    //    "선택된 정보로 작업지시를 등록하시겠습니까?\n[LINE:{0} ,공정군:{1} ,작업지시:{2} ,제품ID:{3} ,경로:{4}]", txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedWo.Text, txtSeletedPRODUCT.Text, Util.NVC(cboRouteByPcsgid.SelectedValue)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1648", txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedWo.Text, txtSeletedPRODUCT.Text, Util.NVC(cboRouteByPcsgid.SelectedValue)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                    //선택된 정보로 작업지시를 등록하시겠습니까?\n[LINE:{0} ,공정군:{1} ,작업지시:{2} ,제품ID:{3} ,경로:{4}]
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            saveWorkOrder();

                            Refresh();

                            setDgOrderList();
                        }
                    });
                }
            }
        }

        //2019.10.17
        //
        private void GetWOrkrder()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["EQSGID"] = Util.NVC(txtSeletedEQSG.Tag) == "" ? null : Util.NVC(txtSeletedEQSG.Tag);
                dr["PCSGID"] = Util.NVC(txtSeletedPCSG.Tag) == "" ? null : Util.NVC(txtSeletedPCSG.Tag);
                dr["ROUTID"] = Util.NVC(cboRouteByPcsgid.SelectedValue) == "" ? null : Util.NVC(cboRouteByPcsgid.SelectedValue);
                RQSTDT.Rows.Add(dr);

                dtOrgWO = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_PCSG_WO_INFO", "RQSTDT", "RSLTDT", RQSTDT);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion Method

        #region ChkPrdtRout
        private Boolean ChkPrdtRout(string strRoutID, string strProdID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["ROUTID"] = strRoutID;
                dr["PRODID"] = strProdID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_SHOP_PRDT_ROUT", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }
        #endregion 

    }
}
