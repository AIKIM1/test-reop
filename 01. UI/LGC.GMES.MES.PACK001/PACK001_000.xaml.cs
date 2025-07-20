/*************************************************************************************
Created Date : 2016.08.00
      Creator : Jeong HyeonSik
   Decription : 작업지시
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16 DEVELOPER : Initial Created.
  2017.07.25 손우석 W/O 자동 변경 사용 여부 표시 추가
  2018.07.18 손우석 CSR ID 3704751 [1.업무변경/개선] GMES 화면에 제품 정보 추가 요청 요청번호 C20180604_04751
  2019.05.08 손우석  WO 종료 버튼 현장 PC에서 삭제 요청
  2019.10.10 염규범 작업지시 해제시, GMES LOGIN 된 작업자 ID 저장 추가
  2020.01.03 염규범 W/O 변경시, 라벨 미리보기 기능 추가건
  2020.01.04 염규범 빈값 클릭후  그룹 W/O 해제시, 전체 W/O 해체시 오류건 수정
  2020.03.11 염규범 다국어처리 SFU8182 (공정 PC에서는, W/O 설정이 불가능합니다. )
  2020.04.20 손우석 서비스 번호 53563 공정 PC w/o end 기능 삭제 요청의 건 [요청번호] C20200418-000019
  2020.11.24 김민석 선택 W/O BOM LIST 조회 화면 및 메서드 추가 [요청번호] C20200925-000312
  2021.08.20 김용준 W/O 변경인원 제한 적용(ESWA만 적용) [요청번호] 
  2024.04.29 김민석 W/O 생성 버튼 G182 하드 코딩 부분 공통코드 DIFFUSION_SITE로 변경 [요청번호] E20240408-000384
  2024.08.12 권성혁 W/O 날짜 비교 CUT_OFF 시간 기준으로 변경으로 인한 INDATA 추가 [요청번호]E20240812-000133
  2024.08.21 최평부 ESST PACK 증설 SI PROJECT INDATA 추가(제품 변경시 MES > 포트별 자재설정(UI) 자재 변경로직 추가)
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
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Windows.Controls.Primitives;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_000 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 
        DataTable dtwoNotePre = new DataTable();
        DataTable dtsiteValid = new DataTable();
        SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        Util _Util = new Util();

        string sTempEQSGID = string.Empty;

        string sWOID = string.Empty;

        //2024.08.21 BY 최평부
        string sCHGPRODFLAG = "N";
        string sORIPRODID = string.Empty;

        private C1.WPF.DataGrid.DataGridRow drSelectedDataGrid;
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        public PACK001_000()
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

        #endregion

        #region Event
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                this.RegisterName("redBrush", redBrush);

                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnWoClear_All);
                listAuth.Add(btnWoClear_One);
                listAuth.Add(btnWoClose);
                listAuth.Add(btnWoSave);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                #region 2020.04.20
                //2019.05.08
                //if (LoginInfo.CFG_SHOP_ID == "G481")
                //{
                //    if ((LoginInfo.USERID.ToString().Substring(0, 2) == "P7") || (LoginInfo.USERID.ToString().Substring(0, 2) == "P8"))
                //    {
                //        btnWoClose.Visibility = Visibility.Hidden;
                //    }
                //}

                if (LoginInfo.USERTYPE == "P")
                {
                    btnWoClose.Visibility = Visibility.Hidden;
                }
                #endregion 2020.04.20


                //2024.04.29 - G182 하드 코딩 부분 공통코드 DIFFUSION_SITE로 변경 - KIM MIN SEOK
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCODE"] = "CREATE_PACKING_WO";
                RQSTDT.Rows.Add(dr);

                dtsiteValid = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_DIFFUSION_SITE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtsiteValid != null && dtsiteValid.Rows[0]["CHECK_YN"].ToString() == "Y")
                {
                    btnCreateWO.Visibility = Visibility.Visible;
                }
                else
                {
                    btnCreateWO.Visibility = Visibility.Collapsed;
                }
                setComboBox();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void btnWorkOrderSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Refresh();

                setDgOrderList();
                sTempEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void woSave()
        {
            string sEQSGID = Util.NVC(txtSeletedEQSG.Tag);
            string sPCSGID = Util.NVC(txtSeletedPCSG.Tag);
            string sPRODID = txtSeletedPRODUCT.Text;
            string sWOID = txtSeletedWo.Text;
            string sWOTYPE = txtSeletedWoType.Text;
            string strRouteID = Util.NVC(cboRouteByPcsgid.SelectedValue.ToString());
            string strSeletedPCSG = txtSeletedPCSG.Text;
            bool passYn = true;

            DataTable dtWo = ((DataView)dgWorkOrderList.ItemsSource).ToTable();
            dtWo.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

            string sWOChangeFlag = dtWo.Rows[0]["WO_AUTO_CHG_FLAG"].ToString();
            string sProdIDOri = dtWo.Rows[0]["PRODID"].ToString();
            string sWoTypeOri = dtWo.Rows[0]["WOTYPE"].ToString();

            //2024.08.21 by 최평부
            sORIPRODID = dtWo.Rows[0]["PRODID"].ToString();

            if (sProdIDOri == sPRODID)
            {
                if (sWoTypeOri == sWOTYPE)
                {
                    if (sWOChangeFlag == "N")
                    {
                        ms.AlertWarning("SFU3656"); //W/O 자동 변경 모드입니다. 설정을 확인해주세요.
                        return;
                    }
                    else
                    {
                        wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID, strRouteID, strSeletedPCSG);
                    }
                }
                else
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
                            wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID, strRouteID, strSeletedPCSG);
                        }
                    });
                }
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
                        wo_Save(sEQSGID, sPRODID, sPCSGID, sWOID, strRouteID, strSeletedPCSG);
                    }
                });
            }
        }

        private void btnWoSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (saveValidation())
                {

                    DataTable dtWorkList = ((DataView)dgPlanWorkorderList.ItemsSource).ToTable();
                    dtWorkList.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

                    if (!dtWorkList.Rows[0]["DEMAND_TYPE"].Equals("MASS"))
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU8547", dtWorkList.Rows[0]["DEMAND_TYPE"].ToString()), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                        {
                            if (result != MessageBoxResult.OK)
                            {
                                return;
                            }
                            else
                            {
                                //woSave();
                                selWorkOrder_DateCheck();
                            }

                        });
                    }
                    else
                    {
                        //woSave();
                        selWorkOrder_DateCheck();
                    }

                }

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }


        private void wo_Save(string sEQSGID, string sPRODID, string sPCSGID, string sWOID, string strRouteId, string strSeletedPCSG)
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
                            //                            saveWorkOrder();

                            //                            setDgOrderList();

                            //                            Refresh();

                            //사용자권한별로 메뉴 숨기기

                            if (GetLableSqlYn(sEQSGID, sPCSGID, sPRODID) && GetLabelCheck(sEQSGID))
                            {
                                if (LoginInfo.USERTYPE.Equals("G"))
                                {
                                    PopUpPrintInfo(sEQSGID, sPRODID, strRouteId, strSeletedPCSG);
                                }
                                else
                                {
                                    Util.MessageInfo("SFU8182");
                                }

                            }
                            else
                            {
                                saveWorkOrder();

                                Refresh();

                                setDgOrderList();
                            }
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
                            //                            saveWorkOrder();

                            //                            setDgOrderList();

                            //                            Refresh();
                            if (GetLableSqlYn(sEQSGID, sPCSGID, sPRODID) && GetLabelCheck(sEQSGID))
                            {
                                if (LoginInfo.USERTYPE.Equals("G"))
                                {
                                    PopUpPrintInfo(sEQSGID, sPRODID, strRouteId, strSeletedPCSG);
                                }
                                else
                                {
                                    Util.MessageInfo("SUF8182");
                                }
                            }
                            else
                            {
                                saveWorkOrder();

                                Refresh();

                                setDgOrderList();
                            }
                        }
                    });
                }
            }
        }

        private void btnWoClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //2020.04.20
                //if (saveValidation())
                //{
                //    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1647", txtSeletedWo.Text, txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedPRODUCT.Text), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //    //선택된 정보로 작업지시를 마감하시겠습니까?\n[작업지시:{0} ,LINE:{1} ,공정군:{2} ,제품ID:{3} ]
                //    {
                //        if (result == MessageBoxResult.OK)
                //        {
                //            string sSHOPID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "SHOPID"));
                //            string sAREAID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "AREAID"));
                //            string sEQSGID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "EQSGID"));
                //            string sPCSGID = Util.NVC(DataTableConverter.GetValue(drSelectedDataGrid.DataItem, "PCSGID"));
                //            closeWorkOrder(sSHOPID, sAREAID, txtSeletedWo.Text);

                //            //초기화
                //            Refresh();

                //            //오더조회
                //            setDgOrderList();

                //            int iwoListIndex = 0;
                //            for (int i = 0; i < dgWorkOrderList.Rows.Count; i++)
                //            {
                //                if (Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "AREAID")) == sAREAID
                //                && Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "EQSGID")) == sEQSGID
                //                && Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[i].DataItem, "PCSGID")) == sPCSGID
                //                )
                //                {
                //                    iwoListIndex = i;
                //                    break;
                //                }
                //            }
                //            //마감전 선택한 오더선택정보 유지.
                //            DataTableConverter.SetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "CHK", "1");

                //            //setDgPlanWorkorderList(dgWorkOrderList.Rows[iwoListIndex]);
                //        }
                //    });
                //}
            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void dtpDate_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "True", false);
                //if (iwoListIndex == -1)
                //{
                //    return;
                //}
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
        private void dgWorkOrderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgWorkOrderList.GetCellFromPoint(pnt);

                if (cell != null)
                {
                    // MES 2.0 CHK 컬럼 Bool 오류 Patch
                    DataTableConverter.SetValue(dgWorkOrderList.Rows[cell.Row.Index].DataItem, "CHK", true);
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
                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "True", false);
                if (iwoListIndex == -1)
                {
                    return;
                }

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

                cboRouteByPcsgid.ItemsSource = null;
                cboRouteByPcsgid.SelectedValue = null;

                txtSeletedEQSG.Tag = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                txtSeletedPCSG.Tag = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));
                txtSeletedWo.Tag = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "WOID"));
                txtSeletedEQSG.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGNAME"));
                txtSeletedPCSG.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGNAME"));
                txtSeletedWo.Text = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "WOID"));
                //string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                //string sPCSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));
                //string sPRJ_NAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PRJ_NAME"));

                //setComboBox_Route(sEQSGID, sPCSGID);

                setDgPlanWorkorderList(dgWorkOrderList.Rows[iwoListIndex]);
                setDgWorkorderBOM(dgWorkOrderList.Rows[iwoListIndex]);

            }
            catch (Exception ex)
            {
                //Util.Alert(ex.ToString());
                Util.MessageException(ex);
            }
        }
        private void dgWorkOrderListChoice_Unchecked(object sender, RoutedEventArgs e)
        {

        }
        private void dgPlanWorkorderList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point pnt = e.GetPosition(null);
                C1.WPF.DataGrid.DataGridCell cell = dgPlanWorkorderList.GetCellFromPoint(pnt);
                if (cell != null)
                {
                    // MES 2.0 CHK 컬럼 Bool 오류 Patch
                    DataTableConverter.SetValue(dgPlanWorkorderList.Rows[cell.Row.Index].DataItem, "CHK", true);
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

                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "True", false);
                if (iwoListIndex == -1)
                {
                    return;
                }

                string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                string sPCSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));

                RadioButton rb = sender as RadioButton;

                if (rb.DataContext == null)
                    return;

                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if ((bool)rb.IsChecked && 
                    ((rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0") ||
                     (rb.DataContext as DataRowView).Row["CHK"].Equals(false)))
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

                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                if ((rb.DataContext as DataRowView).Row["CHK"].Equals(true) ||
                    (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("1"))
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
                            // MES 2.0 CHK 컬럼 Bool 오류 Patch
                            DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", false);
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
                                setDgWorkorderBOM(dgPlanWorkorderList.Rows[iwoPlanListIndex]);
                            }
                            else
                            {
                                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                                DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", false);
                                ms.AlertWarning("SFU1512"); //등록가능한작업지시상태가아닙니다.\n선택가능상태: Release , Process
                            }
                            setComboBox_Route(sEQSGID, sPCSGID, sPRJ_NAME, sPRODID);
                        }
                    }
                    else
                    {
                        // MES 2.0 CHK 컬럼 Bool 오류 Patch
                        DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoPlanListIndex].DataItem, "CHK", false);
                        cboRouteByPcsgid.ItemsSource = null;
                        cboRouteByPcsgid.SelectedIndex = 0;
                        txtSeletedPRODUCT.Text = "";
                        txtSeletedWo.Text = "";
                        txtSeletedWoType.Text = "";
                    }


                }

                //int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);


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
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "True", false);

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

                string sWOID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "WOID"));
                string sPCSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGID"));
                string sPCSGNAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "PCSGNAME"));
                string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                string sEQSGNAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGNAME"));
                string sROUTID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "ROUTID"));
                string sROUTNAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "ROUTNAME"));

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1647", txtSeletedEQSG.Text, txtSeletedPCSG.Text, txtSeletedWo.Text, txtSeletedPRODUCT.Text, Util.NVC(cboRouteByPcsgid.SelectedValue)), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //선택된 작업지시를 해제하시겠습니까?\n[LINE:{0} ,공정군:{1} ,작업지시:{2} ,경로:{3}]
                {
                    if (result == MessageBoxResult.OK)
                    {
                        clearWorkOrder(sWOID, sPCSGID, sEQSGID, sROUTID);

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
                /*
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);
                if (iwoListIndex == -1)
                {                   
                    ms.AlertWarning("SFU1741"); //오더해제 대상이 없습니다. 선택하세요.
                    return;
                }
                string sWOID = null;
                string sPCSGID = null;
                string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGID"));
                string sROUTID = null;
                string sEQSGNAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[iwoListIndex].DataItem, "EQSGNAME"));
                */
                if (!(dgWorkOrderList.Rows.Count > 0))
                {
                    ms.AlertWarning("SFU1905"); //조회된 Data가 없습니다.
                    return;
                }


                string sWOID = null;
                string sPCSGID = null;
                string sEQSGID = Util.NVC(cboEquipmentSegment.SelectedValue).ToString();
                //string sEQSGID = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[0].DataItem, "EQSGID"));
                string sROUTID = null;
                string sEQSGNAME = cboEquipmentSegment.Text.ToString();
                //string sEQSGNAME = Util.NVC(DataTableConverter.GetValue(dgWorkOrderList.Rows[0].DataItem, "EQSGNAME"));

                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU1285", sEQSGNAME), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
                //[LINE:{0}] 의 오더를 전체 해제하시겠습니까?
                {
                    if (!(dgWorkOrderList.Rows.Count > 0) || Util.gridFindIsDataRow(ref dgWorkOrderList, "PRODID", "", false) == -1)
                    {
                        ms.AlertWarning("SFU1905"); //조회된 Data가 없습니다.
                        return;
                    }

                    if (result == MessageBoxResult.OK)
                    {
                        clearWorkOrder(sWOID, sPCSGID, sEQSGID, sROUTID);

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


        private void btnCreateWO_Click(object sender, RoutedEventArgs e)
        {
            if (dtsiteValid != null && dtsiteValid.Rows[0]["CHECK_YN"].ToString() == "Y")
            {
                popup_CREATE_MENUAL_WO();

            }
        }

        ////W/O 초과생산 팝업
        //private void btnWoUnlimited_Click(object sender, RoutedEventArgs e)
        //{
        //    /* W/O 리스트를 컬럼들을 가져온다
        //    가져온 컬럼중에서 선택의 라디오 버튼이 ON된 것의 컬럼에서 W/O, W/O초과생산 값을 가져옴*/
        //    int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "1", false);
        //    if (iwoListIndex == -1)
        //    {
        //        Util.MessageInfo("SFU1635");
        //        return;
        //    }
        //    // 선택화면 띄우기
        //    this.ShowPopUp(e);
        //}

        //private void ShowPopUp(RoutedEventArgs e)
        //{
        //    // 메시지 내용 설정
        //    string message = MessageDic.Instance.GetMessage("SFU5952", txtSeletedWo.Tag);
        //    message = message.Replace("\\r\\n", Environment.NewLine).Replace("\\n", Environment.NewLine);
        //    txtPopupMessage.Text = message;


        //    // Popup 가운데 위치에 띄워주기
        //    this.popupSelect.Placement = PlacementMode.Center;
        //    this.popupSelect.IsOpen = true;
        //    this.rdoYes.IsChecked = true;
        //}

        //private void btnSelect_Click(object sender, RoutedEventArgs e)
        //{
        //    // 확인 누르는 버튼까지만 만들었음.
        //    this.popupSelect.IsOpen = false;

        //    if (rdoYes.IsChecked == true)
        //    {
        //        Util.MessageConfirm("SFU5954", result =>
        //        {
        //            if (result == MessageBoxResult.OK)
        //            {
        //                //뭐시기 저시기가 Y로 바뀐다. 이부분에 아마 쿼리가 사용될것 같아서 DB로 처리할 가능성이 높다.
        //            }
        //        }, txtSeletedWo.Tag);
        //    }
        //}

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
        private void grdTestMode_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Util.MessageConfirm("라벨 출력 상태를 확인 하셨습니까?", (result) =>
            {
                if (result == MessageBoxResult.OK)
                { HideTestMode(); }
            });
        }
        #endregion

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
                Util.MessageException(ex);
            }
        }

        private void setComboBox_Route(string sEQSGID, string sPCSGID)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //route
                String[] sFilter = { sEQSGID, sPCSGID };
                _combo.SetCombo(cboRouteByPcsgid, CommonCombo.ComboStatus.NONE, sFilter: sFilter);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setComboBox_Route(string sEQSGID, string sPCSGID, string sPRJ_NAME)
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                //route
                String[] sFilter = { sEQSGID, sPCSGID, sPRJ_NAME };
                _combo.SetCombo(cboRouteByPcsgid, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "ROUTEBYPCSGMODL");
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
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
                Util.MessageException(ex);
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
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("PRJ_NAME", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["SHOPID"] = Util.NVC(SHOP_AUTH.SelectedValue) == "" ? null : Util.NVC(SHOP_AUTH.SelectedValue);
                dr["AREAID"] = Util.NVC(cboAreaByAreaType.SelectedValue) == "" ? null : Util.NVC(cboAreaByAreaType.SelectedValue);
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedValue) == "" ? null : Util.NVC(cboEquipmentSegment.SelectedValue);
                dr["PCSGID"] = Util.NVC(cboProcessSegmentByEqsgid.SelectedValue) == "" ? null : Util.NVC(cboProcessSegmentByEqsgid.SelectedValue);
                dr["PRJ_NAME"] = null;// Util.NVC(cboPRJ_Model.SelectedValue) == "" ? null : Util.NVC(cboPRJ_Model.SelectedValue);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);

                //dgWorkOrderList.ItemsSource = DataTableConverter.Convert(dtResult);
                Util.GridSetData(dgWorkOrderList, dtResult, FrameOperation, true);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_WORKORDER_LIST_PACK", ex.Message, ex.ToString());
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
                                // MES 2.0 CHK 컬럼 Bool 오류 Patch
                                DataTableConverter.SetValue(dgPlanWorkorderList.Rows[iwoListIndex].DataItem, "CHK", true);
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

        //2020.11.24 선택 W/O BOM 조회 추가 - KIM MIN SEOK
        private void setDgWorkorderBOM(C1.WPF.DataGrid.DataGridRow drDataGrid)
        {
            try
            {
                drSelectedDataGrid = drDataGrid;
                //string sSHOPID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "SHOPID"));
                //string sAREAID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "AREAID"));
                //string sEQSGID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "EQSGID"));
                //string sPCSGID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "PCSGID"));
                //string sPRODID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "PRODID"));

                string sWOID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "WOID"));


                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                //RQSTDT.Columns.Add("SHOPID", typeof(string));
                //RQSTDT.Columns.Add("AREAID", typeof(string));
                //RQSTDT.Columns.Add("EQSGID", typeof(string));
                //RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                //dr["SHOPID"] = sSHOPID;
                //dr["AREAID"] = sAREAID;
                //dr["EQSGID"] = sEQSGID;
                //dr["PRODID"] = sPRODID;
                dr["WOID"] = sWOID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_WORKORDER_CHK_BOM_LIST_PACK", "RQSTDT", "RSLTDT", RQSTDT);
                Util.GridSetData(dgBOMList, dtResult, FrameOperation, true);
            }

            catch (Exception ex)
            {
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

                if (LoginInfo.CFG_SHOP_ID.Equals("G481"))  // 폴란드(ESWA) 한정 W/O 변경 권한 체크
                {
                    if (IsPersonByAuth(LoginInfo.USERID, "PACK_WO_CHANGE") == false) // 사용자 권한 'PACK_WO_CHANGE' 등록 여부 체크
                    {
                        ms.AlertWarning("SFU3520", LoginInfo.USERID, ObjectDic.Instance.GetObjectName("W/O선택")); //해당 USER[%1]는 권한[%2]을 가지고 있지 않습니다.
                        bReturnValue = false;
                        return bReturnValue;
                    }
                }

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

        private bool IsPersonByAuth(string sUserID, string sAuthID)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = sUserID;
                dr["AUTHID"] = sAuthID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }

        private void saveWorkOrder()
        {

            try
            {
                //2024.08.21 by 최평부, 제품이 달라지면..
                if (!string.IsNullOrWhiteSpace(sORIPRODID).Equals(txtSeletedPRODUCT.Text))
                {
                    sCHGPRODFLAG = "Y";
                }

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
                INDATA.Columns.Add("CHGPRODFLAG", typeof(string));//2024.08.21 by 최평부

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PCSGID"] = Util.NVC(txtSeletedPCSG.Tag);
                drINDATA["EQSGID"] = Util.NVC(txtSeletedEQSG.Tag);
                drINDATA["ROUTID"] = Util.NVC(cboRouteByPcsgid.SelectedValue);
                drINDATA["WOID"] = txtSeletedWo.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["PRODID"] = txtSeletedPRODUCT.Text;
                drINDATA["CHGPRODFLAG"] = sCHGPRODFLAG;//2024.08.21 by 최평부
                INDATA.Rows.Add(drINDATA);

                new ClientProxy().ExecuteService("BR_PRD_REG_WO_PACK", "INDATA", null, INDATA, (result, ex) =>
                {
                    //loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_REG_WO_PACK", ex.Message, ex.ToString());
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


        private void saveMenualWorkOrder(string sPcsgID, string sEqsgID, string sRoutID, string sWOID, string sProdID)
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

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PCSGID"] = sPcsgID;
                drINDATA["EQSGID"] = sEqsgID;
                drINDATA["ROUTID"] = sRoutID;
                drINDATA["WOID"] = sWOID;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["PRODID"] = sProdID;
                INDATA.Rows.Add(drINDATA);

                new ClientProxy().ExecuteService("BR_PRD_REG_WO_PACK", "INDATA", null, INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_REG_WO_PACK", ex.Message, ex.ToString());
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


        private void saveWorkOrderProdPreChkCntt(string strChkItem)
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
                INDATA.Columns.Add("PROD_PRE_CHK_CNTT", typeof(string));


                DataRow drINDATA = INDATA.NewRow();
                drINDATA["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                drINDATA["LANGID"] = LoginInfo.LANGID;
                drINDATA["PCSGID"] = Util.NVC(txtSeletedPCSG.Tag);
                drINDATA["EQSGID"] = Util.NVC(txtSeletedEQSG.Tag);
                drINDATA["ROUTID"] = Util.NVC(cboRouteByPcsgid.SelectedValue);
                drINDATA["WOID"] = txtSeletedWo.Text;
                drINDATA["USERID"] = LoginInfo.USERID;
                drINDATA["PRODID"] = txtSeletedPRODUCT.Text;
                drINDATA["PROD_PRE_CHK_CNTT"] = strChkItem;
                INDATA.Rows.Add(drINDATA);

                new ClientProxy().ExecuteService("BR_PRD_REG_WO_PACK", "INDATA", null, INDATA, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_REG_WO_PACK", ex.Message, ex.ToString());
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

        private void closeWorkOrder(string sShop, string sAreaId, string sWoId)
        {
            try
            {

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("SHOPID", typeof(string));
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("CLOSE_FLAG", typeof(string));
                IndataTable.Columns.Add("USERID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["WOID"] = sWoId;
                Indata["SHOPID"] = sShop;
                Indata["AREAID"] = sAreaId;
                Indata["CLOSE_FLAG"] = "Y";
                Indata["USERID"] = LoginInfo.USERID;
                IndataTable.Rows.Add(Indata);

                new ClientProxy().ExecuteService("BR_PRD_REG_CLOSE_WO_TO_ERP", "INDATA", null, IndataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        //Util.AlertByBiz("BR_PRD_REG_CLOSE_WO_TO_ERP", ex.Message, ex.ToString());
                        Util.MessageException(ex);
                        return;
                    }
                    ms.AlertInfo("SFU1889"); //정상 처리 되었습니다.
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void clearWorkOrder(string sWOID, string sPCSGID, string sEQSGID, string sROUTID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("PCSGID", typeof(string));
                RQSTDT.Columns.Add("EQSGID", typeof(string));
                RQSTDT.Columns.Add("ROUTID", typeof(string));
                RQSTDT.Columns.Add("WOID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow Indata = RQSTDT.NewRow();
                Indata["PCSGID"] = sPCSGID;
                Indata["EQSGID"] = sEQSGID;
                Indata["ROUTID"] = sROUTID;
                Indata["WOID"] = sWOID;
                Indata["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(Indata);

                //new ClientProxy().ExecuteServiceSync("DA_PRD_DEL_PCSG_WO_PACK", "RQSTDT", "", RQSTDT, null);
                new ClientProxy().ExecuteServiceSync("BR_PRD_DEL_WO_PACK", "INDATA", "", RQSTDT, null);
            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_DEL_PCSG_WO_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }

        private void Refresh()
        {
            try
            {
                //그리드 clear
                Util.gridClear(dgBOMList);
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

                cboRouteByPcsgid.ItemsSource = null;
            }
            catch (Exception ex)
            {
                throw ex;
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

        #region [ LOT 발번룰 SQL 존재 유무 조회 ]
        /// <summary>
        ///  SQL롤 된 LOT 발번룰을 가지고 LOT 발번, 
        /// </summary>
        /// <param name="strEqsgID"></param>
        /// <param name="strPCSG"></param>
        /// <param name="strProdId"></param>
        /// <returns></returns>
        private Boolean GetLableSqlYn(string strEqsgID, string strPCSG, string strProdId)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("EQSGID", typeof(string));
                //INDATA.Columns.Add("PRDT_CLSS_CODE", typeof(string));
                INDATA.Columns.Add("PACK_LOT_GNRT_LOGIC_TYPE", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("SHOPID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["EQSGID"] = strEqsgID;
                //drINDATA["PRDT_CLSS_CODE"] = strPCSG;
                drINDATA["PACK_LOT_GNRT_LOGIC_TYPE"] = "SQL";
                drINDATA["PRODID"] = strProdId;
                drINDATA["SHOPID"] = LoginInfo.CFG_SHOP_ID;

                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_LOTRULE", "RQSTDT", "RSLTDT", INDATA);

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

        #region [ POP UP ]
        private void PopUpPrintInfo(string strLineID, string strProdID, string strRouteID, string strSeletedPCSG)
        {
            try
            {

                PACK001_000_LABEL_CHECK popup = new PACK001_000_LABEL_CHECK();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = strLineID;
                    Parameters[1] = strProdID;
                    Parameters[2] = strRouteID;
                    Parameters[3] = strSeletedPCSG;
                    Parameters[4] = txtSeletedWo.Text;
                    Parameters[5] = txtSeletedEQSG.Text;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    //popup.Closed += new EventHandler(popup_Closed);
                    //this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));

                    popup.Closed -= popup_Closed;
                    popup.Closed += popup_Closed;
                    //grdMain.Children.Add(popup);
                    popup.ShowModal();
                    popup.CenterOnScreen();
                    popup.BringToFront();

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void popup_Closed(object sender, EventArgs e)
        {
            try
            {
                PACK001_000_LABEL_CHECK popup = sender as PACK001_000_LABEL_CHECK;
                if (popup.DialogResult == MessageBoxResult.OK)
                {
                    string strChkItem = string.Empty;
                    strChkItem = Util.NVC(popup.DataContext.ToString());

                    saveWorkOrderProdPreChkCntt(strChkItem);

                    Refresh();

                    setDgOrderList();
                }
                else if (popup.DialogResult == MessageBoxResult.Yes)
                {
                    saveWorkOrder();

                    Refresh();

                    setDgOrderList();

                    //ShowTestMode();
                }
                else if (popup.DialogResult == MessageBoxResult.Cancel)
                {
                    Util.MessageInfo("SFU1937");

                    //Refresh();

                    //setDgOrderList();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }






        private void popup_CREATE_MENUAL_WO()
        {
            try
            {
                DataTable dt = DataTableConverter.Convert(dgWorkOrderList.ItemsSource);
                if (dt.Rows.Count > 0)
                {
                    PACK001_000_MENUAL_WO_CREATE popup = new PACK001_000_MENUAL_WO_CREATE();
                    popup.FrameOperation = this.FrameOperation;

                    if (popup != null)
                    {
                        object[] Parameters = new object[6];
                        //DataRow[] dr = dt.Select();
                        if (sTempEQSGID.Equals("PDQ01"))
                        {
                            //타겟 W/O
                            DataRow[] rows = dt.Select("PCSGID='M'");
                            //파라미터 담기!
                            Parameters[0] = sTempEQSGID; // 라인 ID
                            Parameters[1] = rows[0];//제품 ID
                            Parameters[2] = "P5500"; //공정

                        }
                        C1WindowExtension.SetParameters(popup, Parameters);

                        //popup.Closed += new EventHandler(popup_Closed);
                        //this.Dispatcher.BeginInvoke(new Action(() => popup.ShowModal()));

                        popup.Closed -= popup_CREATE_MENUAL_WO_Closed;
                        popup.Closed += popup_CREATE_MENUAL_WO_Closed;
                        //grdMain.Children.Add(popup);
                        popup.ShowModal();
                        popup.CenterOnScreen();
                        popup.BringToFront();

                    }

                }
                else
                {
                    Util.MessageInfo("SFU1223");
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        void popup_CREATE_MENUAL_WO_Closed(object sender, EventArgs e)
        {
            string sPcsgID;
            string sEqsgID;
            string sRoutID;
            string sWOID;
            string sProdID;
            try
            {
                PACK001_000_MENUAL_WO_CREATE popup = sender as PACK001_000_MENUAL_WO_CREATE;
                if (popup.DialogResult == MessageBoxResult.OK)
                {

                    sWOID = popup.WOID;
                    sPcsgID = popup.PCSGID;
                    sEqsgID = popup.EQSGID;
                    sRoutID = popup.ROUTID;
                    sProdID = popup.PRODID;
                    if (popup.WOCHECK)
                    {
                        saveMenualWorkOrder(sPcsgID, sEqsgID, sRoutID, sWOID, sProdID);
                        //Util.MessageInfo("SFU1275");
                    }
                    else
                    {
                        Util.MessageInfo("SFU6342", sWOID);

                    }


                    Refresh();
                    setDgOrderList();


                }
                else if (popup.DialogResult == MessageBoxResult.Cancel)
                {
                    Util.MessageInfo("SFU1937");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        // W/O 선택시 전월 또는 익월인 W/O인 경우 인터락, W/O CUT_OFF 시간까지 당월
        private void selWorkOrder_DateCheck()
        {
            try
            {
                int iwoListIndex = Util.gridFindDataRow(ref dgWorkOrderList, "CHK", "True", false);
                C1.WPF.DataGrid.DataGridRow drDataGrid = dgWorkOrderList.Rows[iwoListIndex];
                string AREAID = Util.NVC(DataTableConverter.GetValue(drDataGrid.DataItem, "AREAID"));

                DataTable dt = ((DataView)dgPlanWorkorderList.ItemsSource).ToTable();
                dt.Select("CHK = 0").ToList<DataRow>().ForEach(row => row.Delete());

                //// 선택한 W/O의 계획시작일
                string sPLANSTDTTM = dt.Rows[0]["PLANSTDTTM"].ToString();
                //DateTime cPLANSTDTTM = DateTime.Parse(Convert.ToDateTime(dtpDateFrom.SelectedDateTime).ToString(sPLANSTDTTM));

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("PLANSTDTTM", typeof(string));
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["PLANSTDTTM"] = DateTime.Parse(sPLANSTDTTM).ToString("yyyy-MM"); // cPLANSTDTTM.ToString("yyyy-MM");
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
                        woSave();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        #region [ Animation ]
        private void showTestAnimationCompleted(object sender, EventArgs e)
        {
            ColorAnimationInredRectangle();
        }

        private void ShowTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (bTestMode) return;
                if (MainContents.RowDefinitions[4].Height.Value > 0) return;

                MainContents.RowDefinitions[4].Height = new GridLength(1, GridUnitType.Star);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(0, GridUnitType.Star);
                gla.To = new GridLength(1, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                gla.Completed += showTestAnimationCompleted;
                MainContents.RowDefinitions[4].BeginAnimation(RowDefinition.HeightProperty, gla);

                ColorAnimationInredRectangle();
            }));

            //bTestMode = true;
        }

        private void HideTestMode()
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                //if (!bTestMode) return;
                if (MainContents.RowDefinitions[1].Height.Value <= 0) return;

                MainContents.RowDefinitions[4].Height = new GridLength(0);
                LGC.GMES.MES.CMM001.GridLengthAnimation gla = new LGC.GMES.MES.CMM001.GridLengthAnimation();
                gla.From = new GridLength(1, GridUnitType.Star);
                gla.To = new GridLength(0, GridUnitType.Star);
                gla.Duration = new TimeSpan(0, 0, 0, 0, 500);
                //  gla.Completed += HideTestAnimationCompleted;
                MainContents.RowDefinitions[4].BeginAnimation(RowDefinition.HeightProperty, gla);
            }));


        }

        private void ColorAnimationInredRectangle()
        {
            recTestMode.Fill = redBrush;

            DoubleAnimation opacityAnimation = new DoubleAnimation();
            opacityAnimation.From = 1.0;
            opacityAnimation.To = 0.0;
            opacityAnimation.Duration = TimeSpan.FromSeconds(0.8);
            opacityAnimation.AutoReverse = true;
            opacityAnimation.RepeatBehavior = RepeatBehavior.Forever;
            Storyboard.SetTargetName(opacityAnimation, "redBrush");
            Storyboard.SetTargetProperty(
            opacityAnimation, new PropertyPath(SolidColorBrush.OpacityProperty));

            Storyboard mouseLeftButtonDownStoryboard = new Storyboard();
            mouseLeftButtonDownStoryboard.Children.Add(opacityAnimation);

            mouseLeftButtonDownStoryboard.Begin(this);

        }

        #endregion


        #region [ 임시 테스트용 ]
        /// <summary>
        /// 임시 테스트용
        /// </summary>
        /// <param name="strEqsgID"></param>
        /// <returns></returns>
        #region
        private Boolean GetLabelCheck(string strEqsgID)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "RQSTDT";
                INDATA.Columns.Add("CMCDTYPE", typeof(string));
                INDATA.Columns.Add("CMCODE", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["CMCDTYPE"] = "PACK_WO_LBL_CHECK";
                drINDATA["CMCODE"] = strEqsgID;

                INDATA.Rows.Add(drINDATA);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", INDATA);

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

        private void preViewLable_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("DPI", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LABEL_TYPE"] = "PACK";
                drINDATA["LABEL_CODE"] = "LBL0183";
                drINDATA["SAMPLE_FLAG"] = "Y";
                drINDATA["PRODID"] = "AMDAU0177B";
                drINDATA["DPI"] = "300";
                drINDATA["USERID"] = "cnsrend";

                INDATA.Rows.Add(drINDATA);

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                DataSet dsResult;
                string url = string.Empty;
                string zplText = string.Empty;
                string dpmm;

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPL_API", "INDATA", "OUTDATA,IMAGE", dsIndata, null);
                dpmm = ((dsResult.Tables["OUTDATA"].Rows[0])).ItemArray[3].ToString();
                url = (dsResult.Tables["IMAGE"].Rows[0]).ItemArray[1].ToString();
                zplText = ((dsResult.Tables["OUTDATA"].Rows[0])).ItemArray[0].ToString();

                if (((dsResult.Tables["IMAGE"].Rows[0])).ItemArray[0].Equals("OK"))
                {
                    Byte[] temp;
                    temp = (Byte[])(dsResult.Tables["IMAGE"].Rows[0]).ItemArray[2];

                    var bi = new BitmapImage();

                    bi.BeginInit();
                    if (temp != null) bi.StreamSource = new MemoryStream(temp);
                    bi.EndInit();
                }
                else
                {
                    byte[] zpl = System.Text.Encoding.UTF8.GetBytes(zplText);
                    byte[] IMG = null;

                    var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = zpl.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(zpl, 0, zpl.Length);
                    requestStream.Close();

                    var response = (System.Net.HttpWebResponse)request.GetResponse();
                    using (var responseStream = response.GetResponseStream())
                    {
                        MemoryStream stream_Buffer = new MemoryStream();
                        responseStream.CopyTo(stream_Buffer);

                        IMG = stream_Buffer.ToArray();

                        responseStream.Close();
                        stream_Buffer.Close();

                        var bi = new BitmapImage();

                        bi.BeginInit();
                        if (IMG != null) bi.StreamSource = new MemoryStream(IMG);
                        bi.EndInit();

                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            #endregion

        }
    }
}
