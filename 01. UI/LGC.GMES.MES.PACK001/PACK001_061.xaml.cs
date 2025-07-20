/*************************************************************************************
 Created Date : 2020.05.18
      Creator : 강호운
   Decription : 2nd OCV 대상 LOT 조회 / 2nd OCV 검사 이력 조회 / 2nd OCV 검사 상세 데이타 조회 Export Data 는 검사 이력 조회 내역의 데이타와 함께 다운로드
--------------------------------------------------------------------------------------
 [Change History]
 2020.11.03    강호운   2nd 검사 결과 조회시 모델 , 제품 콤보 박스 내역 않보이는 현상 수정 처리
 2021.10.06    강호운   2nd 이력 삭제 여부 변경 기능 추가
 2021.10.27    염규범   2nd Ocv 엑셀 다운로드 수정 및 배포를 위한 임시 조치
 2024.04.02    김길용   검사완료이력Tab 콤보박스 및 조회날짜 변경 E20240328-001588 [생산PI] GMES 내 2nd OCV Target Search UI 개선 요청 건
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
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_061 : UserControl, IWorkArea
    {
        private object lockObject = new object();
        private string sMODELID = string.Empty;
        private string sLINEID = string.Empty;
        string sBoxSetEqsgId = "";
        private bool bMultilotByKeyboard = false;

        #region Declaration & Constructor 
        public PACK001_061()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion Declaration & Constructor 

        #region Initialize
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                setComboBox();

                ApplyPermissions();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void setComboBox()
        {
            try
            {
                CommonCombo _combo = new CommonCombo();

                ////동
                String[] sFilterTabLine = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };

                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.ALL, sFilter: sFilterTabLine, sCase: "AREA_AREATYPE");
                cboEquipmentSegment.ApplyTemplate();
                Set_Combo_Eqsgid(cboEquipmentSegment);

                cboProduct.ApplyTemplate();
                Set_Combo_Product(cboProduct, cboEquipmentSegment);

                cboProductModel.ApplyTemplate();
                Set_Combo_ProductModel(cboProductModel, cboEquipmentSegment);


                _combo.SetCombo(cboAreaByAreaType1, CommonCombo.ComboStatus.ALL, sFilter: sFilterTabLine, sCase: "AREA_AREATYPE");
                cboEquipmentSegment1.ApplyTemplate();
                Set_Combo_Eqsgid(cboEquipmentSegment1);

                cboProduct1.ApplyTemplate();
                Set_Combo_Product(cboProduct1, cboEquipmentSegment1);

                //cboProductModel1.ApplyTemplate();
                //Set_Combo_ProductModel(cboProductModel1, cboEquipmentSegment1);

                // 배포를 위한 임시 조치
                // 염규범 선임
                //DataTable cboDT = new DataTable();
                //cboDT.TableName = "RQSTDT";
                //cboDT.Columns.Add("CBO_NAME", typeof(string));
                //cboDT.Columns.Add("CBO_CODE", typeof(string));
                //
                //DataRow newRow = cboDT.NewRow();
                //newRow["CBO_NAME"] = "Y";
                //newRow["CBO_CODE"] = "Y";
                //cboDT.Rows.Add(newRow);
                //newRow = null;
                //newRow = cboDT.NewRow();
                //newRow["CBO_NAME"] = "N";
                //newRow["CBO_CODE"] = "N";
                //cboDT.Rows.Add(newRow);
                //(dgLineHistoryTabSearch.Columns["DEL_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(cboDT.Copy());

                setUseYN();
                setInspFlag();
                setDayType();
                setInspcount();

                //날자 초기값 세팅
                dtpDateFrom.SelectedDateTime = (DateTime)System.DateTime.Now.AddDays(-7); //일주일 전 날짜
                dtpDateTo.SelectedDateTime = (DateTime)System.DateTime.Now;//오늘 날짜

                //2024.04.02
                Util.Set_Pack_cboTimeList(cboTimeFrom, "CBO_NAME", "CBO_CODE", "06:00:00");
                Util.Set_Pack_cboTimeList(cboTimeTo, "CBO_NAME", "CBO_CODE", "23:59:59");

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void Set_Combo_Eqsgid(MultiSelectionBox cboMulti)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_EQUIPMENTSEGMENT_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMulti.Uncheck(-1);
                    }
                    else
                    {
                        //cboMulti.isAllUsed = true;
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; ++i)
                        {
                            cboMulti.Uncheck(i - 1);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Set_Combo_Product(MultiSelectionBox cboMulti, MultiSelectionBox cboMulti_target)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("PRODTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                drnewrow["EQSGID"] = Convert.ToString(cboMulti_target.SelectedItemsToString);
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["AREA_TYPE_CODE"] = "P";
                drnewrow["PRODTYPE"] = "PROD";

                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_PRODTYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMulti.Uncheck(-1);
                    }
                    else
                    {
                        //cboMulti.isAllUsed = true;
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; ++i)
                        {
                            cboMulti.Uncheck(i - 1);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void Set_Combo_ProductModel(MultiSelectionBox cboMulti, MultiSelectionBox cboMulti_target)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PRODTYPE", typeof(string));

                string[] sEqsgid = cboEquipmentSegment.SelectedItemsToString.Split(',');

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["AREAID"] = cboAreaByAreaType.SelectedValue.ToString();
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["EQSGID"] = Convert.ToString(cboMulti_target.SelectedItemsToString);
                drnewrow["PRODTYPE"] = "MODEL";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_PRODTYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                if (dtResult.Rows.Count != 0)
                {
                    if (dtResult.Rows.Count == 1)
                    {
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        cboMulti.Uncheck(-1);
                    }
                    else
                    {
                        //cboMulti.isAllUsed = true;
                        cboMulti.ItemsSource = DataTableConverter.Convert(dtResult);
                        for (int i = 0; i < dtResult.Rows.Count; ++i)
                        {
                            cboMulti.Uncheck(i - 1);
                        }
                    }
                }
                else
                {
                    cboMulti.ItemsSource = null;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void setUseYN()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "-ALL-";
                dr_["CBO_CODE"] = "";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y : " + ObjectDic.Instance.GetObjectName("대상");
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N : " + ObjectDic.Instance.GetObjectName("미대상");
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboUseFlag.ItemsSource = DataTableConverter.Convert(dt);
                cboUseFlag.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void setInspcount()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CBO_NAME", typeof(string));
                RQSTDT.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = RQSTDT.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "";
                RQSTDT.Rows.Add(dr_);

                DataRow dr = RQSTDT.NewRow();
                dr["CBO_NAME"] = "1차";
                dr["CBO_CODE"] = "1";
                RQSTDT.Rows.Add(dr);

                DataRow dr1 = RQSTDT.NewRow();
                dr1["CBO_NAME"] = "2차";
                dr1["CBO_CODE"] = "2";
                RQSTDT.Rows.Add(dr1);

                DataRow dr2 = RQSTDT.NewRow();
                dr2["CBO_NAME"] = "3차";
                dr2["CBO_CODE"] = "3";
                RQSTDT.Rows.Add(dr2);

                DataRow dr3 = RQSTDT.NewRow();
                dr3["CBO_NAME"] = "4차";
                dr3["CBO_CODE"] = "4";
                RQSTDT.Rows.Add(dr3);

                DataRow dr4 = RQSTDT.NewRow();
                dr4["CBO_NAME"] = "5차";
                dr4["CBO_CODE"] = "5";
                RQSTDT.Rows.Add(dr4);

                RQSTDT.AcceptChanges();

                cboinspcnt.ItemsSource = DataTableConverter.Convert(RQSTDT);
                cboinspcnt.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private void setInspFlag()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "OK";
                dr["CBO_CODE"] = "OK";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "NG";
                dr1["CBO_CODE"] = "NG";
                dt.Rows.Add(dr1);

                DataRow dr2 = dt.NewRow();
                dr2["CBO_NAME"] = "OTHER";
                dr2["CBO_CODE"] = "OTHER";
                dt.Rows.Add(dr2);

                dt.AcceptChanges();

                cboINSPFlag.ItemsSource = DataTableConverter.Convert(dt);
                cboINSPFlag.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void setDayType()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = ObjectDic.Instance.GetObjectName("시작일");
                dr_["CBO_CODE"] = "S";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = ObjectDic.Instance.GetObjectName("종료일");
                dr["CBO_CODE"] = "E";
                dt.Rows.Add(dr);

                DataRow dr__ = dt.NewRow();
                dr__["CBO_NAME"] = ObjectDic.Instance.GetObjectName("기준일");
                dr__["CBO_CODE"] = "B";
                dt.Rows.Add(dr__);

                dt.AcceptChanges();

                cboDayType.ItemsSource = DataTableConverter.Convert(dt);
                cboDayType.SelectedIndex = 0; //default Y
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Initialize

        #region Method


        private void Set_Combo_ProductModel1(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("LANGID", typeof(string));
                dtRQSTDT.Columns.Add("PRODTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["AREAID"] = cboAreaByAreaType1.SelectedValue.ToString();
                drnewrow["EQSGID"] = Convert.ToString(cboEquipmentSegment1.SelectedItemsToString);
                drnewrow["LANGID"] = LoginInfo.LANGID;
                drnewrow["PRODTYPE"] = "MODEL";
                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_PRODTYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                DataRow dRow = dtRslt.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = "";
                dtRslt.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void Set_Combo_Product1(C1ComboBox cbo)
        {
            try
            {
                DataTable dtRQSTDT = new DataTable();
                dtRQSTDT.TableName = "RQSTDT";
                dtRQSTDT.Columns.Add("AREAID", typeof(string));
                dtRQSTDT.Columns.Add("EQSGID", typeof(string));
                dtRQSTDT.Columns.Add("SHOPID", typeof(string));
                dtRQSTDT.Columns.Add("AREA_TYPE_CODE", typeof(string));
                dtRQSTDT.Columns.Add("PRODTYPE", typeof(string));

                DataRow drnewrow = dtRQSTDT.NewRow();
                drnewrow["AREAID"] = cboAreaByAreaType1.SelectedValue.ToString();
                drnewrow["EQSGID"] = Convert.ToString(cboEquipmentSegment1.SelectedItemsToString);
                drnewrow["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                drnewrow["AREA_TYPE_CODE"] = "P";
                drnewrow["PRODTYPE"] = "PROD";

                dtRQSTDT.Rows.Add(drnewrow);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PRODUCT_BY_PRODTYPE_CBO", "RQSTDT", "RSLTDT", dtRQSTDT);

                DataRow dRow = dtRslt.NewRow();

                dRow["CBO_NAME"] = "-ALL-";
                dRow["CBO_CODE"] = "";
                dtRslt.Rows.InsertAt(dRow, 0);

                cbo.ItemsSource = DataTableConverter.Convert(dtRslt);
                cbo.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ShowLoadingIndicator()
        {

        }

        private void HiddenLoadingIndicator()
        {

        }

        #endregion Method

        #region Event
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbCellListCount.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Search();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnHistorySearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                tbCellListCount1.Text = "[ 0 " + ObjectDic.Instance.GetObjectName("건") + " ]";
                Search1();
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        //2020.01.06

        private void Search()
        {
            try
            {
                //ShowLoadingIndicator();

                string strEQSG = string.Empty;
                string strModel = string.Empty;
                string strPRODID = string.Empty;
                string strCMPL = string.Empty;
                string strAREA = string.Empty;
                string strEnable = string.Empty;

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("INSP_ENABLE_YN", typeof(string));
                
                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;


                sBoxSetEqsgId = Convert.ToString(cboEquipmentSegment.SelectedItemsToString);    

                if (string.IsNullOrEmpty(sBoxSetEqsgId))
                {
                    Util.MessageInfo("SFU1223");
                    return;
                }

                dr["EQSGID"] = Util.NVC(cboEquipmentSegment.SelectedItemsToString); 
                dr["MODLID"] = Util.NVC(cboProductModel.SelectedItemsToString).Equals("") ?  null : Util.NVC(cboProductModel.SelectedItemsToString); 
                dr["PRODID"] = Util.NVC(cboProduct.SelectedItemsToString).Equals("") ? null : Util.NVC(cboProduct.SelectedItemsToString);

                switch (cboAreaByAreaType.SelectedValue.ToString())
                {
                    case "":
                        strAREA = null;
                        break;
                    default:
                        strAREA = cboAreaByAreaType.SelectedValue.ToString();
                        break;
                }
                dr["AREAID"] = strAREA;

                switch (cboUseFlag.SelectedValue.ToString())
                {
                    case "":
                        strEnable = null;
                        break;
                    default:
                        strEnable = cboUseFlag.SelectedValue.ToString();
                        break;
                }
                dr["INSP_ENABLE_YN"] = strEnable;

                INDATA.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_2ND_OCV_INSP_LIST", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    bMultilotByKeyboard = false;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgLineTabSearch, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount, Util.NVC(dtResult.Rows.Count));
                });

            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void Search1()
        {
            try
            {
                if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                {
                    Util.MessageValidation("SFU2042", "31");
                    return;
                }
                //DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
                DateTime dtEndTime = DateTime.Parse(dtpDateTo.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeTo.SelectedValue.ToString());
                //DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);
                DateTime dtStartTime = DateTime.Parse(dtpDateFrom.SelectedDateTime.ToString("yyyy-MM-dd") + " " + cboTimeFrom.SelectedValue.ToString());

                if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                {
                    //종료일자가 시작일자보다 빠릅니다.
                    Util.MessageValidation("SFU1913");
                    return;
                }

                string strEQSG = string.Empty;
                string strModel = string.Empty;
                string strPRODID = string.Empty;
                string strCMPL = string.Empty;
                string strAREA = string.Empty;
                string strInspseq = string.Empty;
                string strINSPFlag = string.Empty;

                DataSet dsInput = new DataSet();
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("INSP_SEQS", typeof(string));
                //INDATA.Columns.Add("MODLID", typeof(string));
                INDATA.Columns.Add("INSP_JUDG_VALUE", typeof(string));
                INDATA.Columns.Add("JUDG_PENDING_FLAG", typeof(string));
                INDATA.Columns.Add("VISUAL_INSP_FLAG", typeof(string));
                
                if (cboINSPFlag.SelectedValue.ToString().Equals("OTHER"))
                {
                    if (cboDayType.SelectedValue.Equals("S"))
                    {
                        INDATA.Columns.Add("SSTDTTM", typeof(DateTime));
                        INDATA.Columns.Add("SEDDTTM", typeof(DateTime));
                    }
                    else
                    {
                        Util.MessageValidation("SFU8361");
                        return;
                    }
                    
                }
                else
                {
                    if (cboDayType.SelectedValue.Equals("S"))
                    {
                        INDATA.Columns.Add("SSTDTTM", typeof(DateTime));
                        INDATA.Columns.Add("SEDDTTM", typeof(DateTime));
                    }
                    else if (cboDayType.SelectedValue.Equals("E"))
                    {
                        INDATA.Columns.Add("STDTTM", typeof(DateTime));
                        INDATA.Columns.Add("EDDTTM", typeof(DateTime));
                    }
                    else if (cboDayType.SelectedValue.Equals("B"))
                    {
                        INDATA.Columns.Add("BSTDTTM", typeof(DateTime));
                        INDATA.Columns.Add("BEDDTTM", typeof(DateTime));
                    }
                }
                

                sBoxSetEqsgId = Convert.ToString(cboEquipmentSegment1.SelectedItemsToString);

                if (string.IsNullOrEmpty(sBoxSetEqsgId))
                {
                    Util.MessageInfo("SFU1223");
                    return;
                }

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQSGID"] = Util.NVC(cboEquipmentSegment1.SelectedItemsToString);
                //dr["MODLID"] = Util.NVC(cboProductModel1.SelectedItemsToString).Equals("") ?  null : Util.NVC(cboProductModel1.SelectedItemsToString); 
                dr["PRODID"] = Util.NVC(cboProduct1.SelectedItemsToString).Equals("") ? null : Util.NVC(cboProduct1.SelectedItemsToString);
                dr["INSP_SEQS"] = Util.NVC(cboinspcnt.SelectedValue.ToString()).Equals("") ? null : Util.NVC(cboinspcnt.SelectedValue.ToString());

                switch (cboAreaByAreaType1.SelectedValue.ToString())
                {
                    case "":
                        strAREA = null;
                        break;
                    default:
                        strAREA = cboAreaByAreaType1.SelectedValue.ToString();
                        break;
                }
                dr["AREAID"] = strAREA;
                
                //외관 검사 NG 대상 조회
                //기존 대상 조회
                {

                    if (dtStartTime != null && dtEndTime != null)
                    {
                        switch (cboINSPFlag.SelectedValue.ToString())
                        {
                            case "":
                            case "OTHER":
                                strINSPFlag = null;
                                break;
                            default:
                                strINSPFlag = cboINSPFlag.SelectedValue.ToString();
                                break;
                        }
                        dr["INSP_JUDG_VALUE"] = strINSPFlag;

                        if (cboINSPFlag.SelectedValue.ToString().Equals("OTHER"))
                        {

                            dr["JUDG_PENDING_FLAG"] = "Y";
                            dr["VISUAL_INSP_FLAG"] = chkNg.IsChecked == true ? "N": null ;
                            dr["SSTDTTM"] = dtStartTime;
                            dr["SEDDTTM"] = dtEndTime;
                        }
                        else {
                            if (cboDayType.SelectedValue.Equals("S"))
                            {
                                dr["SSTDTTM"] = dtStartTime;
                                dr["SEDDTTM"] = dtEndTime;
                            }
                            else if (cboDayType.SelectedValue.Equals("E"))
                            {
                                dr["STDTTM"] = dtStartTime;
                                dr["EDDTTM"] = dtEndTime;
                            }
                            else if (cboDayType.SelectedValue.Equals("B"))
                            {
                                dr["BSTDTTM"] = dtStartTime;
                                dr["BEDDTTM"] = dtEndTime;
                            }
                        }
                    }
                }
               

                INDATA.Rows.Add(dr);
                loadingIndicator.Visibility = Visibility.Visible;
                new ClientProxy().ExecuteService("DA_PRD_SEL_2ND_OCV_INSP_HIST_LIST", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                {
                    bMultilotByKeyboard = false ;
                    loadingIndicator.Visibility = Visibility.Collapsed;

                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.GridSetData(dgLineHistoryTabSearch, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount1, Util.NVC(dtResult.Rows.Count));
                });
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }

        private void btnCellDownLoad_clctitem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!bMultilotByKeyboard)
                {
                    if ((dtpDateTo.SelectedDateTime - dtpDateFrom.SelectedDateTime).TotalDays > 31)
                    {
                        //Util.AlertInfo("SFU2042", new object[] { "7" });   //기간은 {0}일 이내 입니다.
                        Util.MessageValidation("SFU2042", "31");
                        return;
                    }
                    DateTime dtEndTime = new DateTime(dtpDateTo.SelectedDateTime.Year, dtpDateTo.SelectedDateTime.Month, dtpDateTo.SelectedDateTime.Day);
                    DateTime dtStartTime = new DateTime(dtpDateFrom.SelectedDateTime.Year, dtpDateFrom.SelectedDateTime.Month, dtpDateFrom.SelectedDateTime.Day);

                    if (!(Math.Truncate(dtEndTime.Subtract(dtStartTime).TotalSeconds) >= 0))
                    {
                        //종료일자가 시작일자보다 빠릅니다.
                        Util.MessageValidation("SFU1913");
                        return;
                    }

                    string strEQSG = string.Empty;
                    string strModel = string.Empty;
                    string strPRODID = string.Empty;
                    string strCMPL = string.Empty;
                    string strAREA = string.Empty;
                    string strINSPFlag = string.Empty;

                    DataSet dsInput = new DataSet();
                    DataTable INDATA = new DataTable();

                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("EQSGID", typeof(string));
                    INDATA.Columns.Add("PRODID", typeof(string));
                    INDATA.Columns.Add("AREAID", typeof(string));
                    //INDATA.Columns.Add("MODLID", typeof(string));
                    INDATA.Columns.Add("INSP_JUDG_VALUE", typeof(string));
                    INDATA.Columns.Add("JUDG_PENDING_FLAG", typeof(string));
                    INDATA.Columns.Add("VISUAL_INSP_FLAG", typeof(string));
                    if (cboINSPFlag.SelectedValue.ToString().Equals("OTHER"))
                    {
                        if (cboDayType.SelectedValue.Equals("S"))
                        {
                            INDATA.Columns.Add("SSTDTTM", typeof(string));
                            INDATA.Columns.Add("SEDDTTM", typeof(string));
                        }
                        else
                        {
                            Util.MessageValidation("SFU8361");
                            return;
                        }

                    }
                    else if (cboDayType.SelectedValue.Equals("S"))
                    {
                        INDATA.Columns.Add("SSTDTTM", typeof(string));
                        INDATA.Columns.Add("SEDDTTM", typeof(string));
                    }
                    else if (cboDayType.SelectedValue.Equals("E"))
                    {
                        INDATA.Columns.Add("STDTTM", typeof(string));
                        INDATA.Columns.Add("EDDTTM", typeof(string));
                    }
                    else if (cboDayType.SelectedValue.Equals("B"))
                    {
                        INDATA.Columns.Add("BSTDTTM", typeof(string));
                        INDATA.Columns.Add("BEDDTTM", typeof(string));
                    }
                    DataRow dr = INDATA.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["EQSGID"] = Util.NVC(cboEquipmentSegment1.SelectedItemsToString);
                    //dr["MODLID"] = Util.NVC(cboProductModel1.SelectedItemsToString).Equals("") ? null : Util.NVC(cboProductModel1.SelectedItemsToString);
                    dr["PRODID"] = Util.NVC(cboProduct1.SelectedItemsToString).Equals("") ? null : Util.NVC(cboProduct1.SelectedItemsToString);

                    switch (cboAreaByAreaType1.SelectedValue.ToString())
                    {
                        case "":
                            strAREA = null;
                            break;
                        default:
                            strAREA = cboAreaByAreaType1.SelectedValue.ToString();
                            break;
                    }
                    dr["AREAID"] = strAREA;

                    switch (cboINSPFlag.SelectedValue.ToString())
                    {
                        case "":
                        case "OTHER":
                            strINSPFlag = null;
                            break;
                        default:
                            strINSPFlag = cboINSPFlag.SelectedValue.ToString();
                            break;
                    }

                    dr["INSP_JUDG_VALUE"] = strINSPFlag;

                    if (dtpDateFrom != null && dtpDateTo != null)
                    {
                        if (cboINSPFlag.SelectedValue.ToString().Equals("OTHER"))
                        {
                            dr["JUDG_PENDING_FLAG"] = "Y";
                            dr["VISUAL_INSP_FLAG"] = chkNg.IsChecked == true ? "N" : null;
                            dr["SSTDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                            dr["SEDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                        }
                        else if (cboDayType.SelectedValue.Equals("S"))
                        {
                            dr["SSTDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                            dr["SEDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                        }
                        else if (cboDayType.SelectedValue.Equals("E"))
                        {
                            dr["STDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                            dr["EDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                        }
                        else if (cboDayType.SelectedValue.Equals("B"))
                        {
                            dr["BSTDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                            dr["BEDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                        }
                    }

                    INDATA.Rows.Add(dr);
                    loadingIndicator.Visibility = Visibility.Visible;
                    new ClientProxy().ExecuteService("DA_PRD_SEL_2ND_OCV_INSP_DATACOLLECT_LIST", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        Dictionary<string, string> dicHeader = new Dictionary<string, string>();


                        if (dtResult.Rows.Count > 0)

                        {
                            foreach (DataColumn dc in dtResult.Columns)
                            {
                                dicHeader.Add(dc.ColumnName.ToString(), dc.ColumnName.ToString());
                            }

                        }
                        //C1DataGrid에 뿌려주는 시간 단축을 위해서, DataTable을 다운로드하는 형태로 변경 처리

                        new ExcelExporter().DtToExcel(dtResult, "2NDOCV_INSP_DATA_HISTORY" , dicHeader);
                         
                        //Util.GridSetData(dgLineHistoryDetailTabSearch, dtResult, FrameOperation, true);
                        //new LGC.GMES.MES.Common.ExcelExporter().Export(dgLineHistoryDetailTabSearch, "2NDOCV_INSP_DATA_HISTORY" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    });
                }
                else
                {
                    DataSet dsInput = new DataSet();
                    DataTable INDATA = new DataTable();

                    INDATA.TableName = "INDATA";
                    INDATA.Columns.Add("LANGID", typeof(string));
                    INDATA.Columns.Add("LOTID", typeof(string));
                    DataRow dr = INDATA.NewRow();


                    DataTable dt = DataTableConverter.Convert(dgLineHistoryTabSearch.ItemsSource);
                    string sLotid = null;
                    for(int i = 0; dt.Rows.Count > i; i++)
                    {
                        if (string.IsNullOrWhiteSpace(sLotid))
                        {
                            sLotid = dt.Rows[i]["LOTID"].ToString();
                        }else 
                        {
                            sLotid = (sLotid + "," + dt.Rows[i]["LOTID"].ToString() );

                        }
                    }
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["LOTID"] = sLotid;
                    INDATA.Rows.Add(dr);
                    new ClientProxy().ExecuteService("DA_PRD_SEL_2ND_OCV_INSP_DATACOLLECT_LIST", "RQSTDT", "RSLTDT", INDATA, (dtResult, ex) =>
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;

                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }

                        if (dtResult.Rows.Count > 0)
                        {
                            Dictionary<string, string> dicHeader = new Dictionary<string, string>();

                            foreach (DataColumn dc in dtResult.Columns)
                            {
                                dicHeader.Add(dc.ColumnName.ToString(), dc.ColumnName.ToString());
                            }

                            new ExcelExporter().DtToExcel(dtResult, "2NDOCV_INSP_DATA_HISTORY", dicHeader);

                        }
                        //C1DataGrid에 뿌려주는 시간 단축을 위해서, DataTable을 다운로드하는 형태로 변경 처리

                        Util.GridSetData(dgLineHistoryDetailTabSearch, dtResult, FrameOperation, true);
                        new LGC.GMES.MES.Common.ExcelExporter().Export(dgLineHistoryDetailTabSearch, "2NDOCV_INSP_DATA_HISTORY" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                    });
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        
        private void dgLineTabSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    string judgValue = DataTableConverter.GetValue(dataGrid.Rows[e.Cell.Row.Index].DataItem, "COLOR_FLAG").ToString();

                    if (string.IsNullOrEmpty(judgValue)) return;

                    if (e.Cell.Column.Index == 12)
                    {
                        switch (judgValue)
                        {
                            case "0":
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.LightGray);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                break;
                            case "1":
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.YellowGreen);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                break;
                            case "2":
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Orange);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                break;
                            case "3":
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.Red);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.White);
                                break;
                            default:
                                e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                                e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                                break;
                        }
                    }else
                    {
                        e.Cell.Presenter.Background = new SolidColorBrush(Colors.White);
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));            
        }

        private void dgLineHistoryTabSearch_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                {
                    return;
                }

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
  
                    if (e.Cell.Column.Name == "LOTID")
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Black);
                    }
                }
            }));
        }

        private void dgLineTabSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = null;
                    }
                }
            }));
        }

        private void dgLineHistoryTabSearch_UnloadedCellPresenter(object sender, DataGridCellEventArgs e)
        {
            if (sender == null)
                return;

            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid?.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter != null)
                {
                    if (e.Cell.Row.Type == DataGridRowType.Item)
                    {
                        e.Cell.Presenter.Background = null;
                        e.Cell.Presenter.Foreground = null;
                    }
                }
            }));
        }
        #endregion Event

        private void btnCellDownLoad_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLineTabSearch, "2NDOCV_TARGET_LIST" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnCellDownLoad1_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //염규범
                new LGC.GMES.MES.Common.ExcelExporter().Export(dgLineHistoryTabSearch, "2NDOCV_HISTORY" + "_" + DateTime.Now.ToString("yyyyMMddHHmmss")); 
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void popUpOpenPalletInfo(string sLotid, string sInsp_seqs)
        {
            try
            {
                PACK001_061_POPUP popup = new PACK001_061_POPUP();
                popup.FrameOperation = this.FrameOperation;

                if (popup != null)
                {
                    object[] Parameters = new object[2];
                    Parameters[0] = sLotid;
                    Parameters[1] = sInsp_seqs;
                    C1WindowExtension.SetParameters(popup, Parameters);

                    popup.ShowModal();
                    popup.CenterOnScreen();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void dgLineHistoryTabSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(null);
            C1.WPF.DataGrid.DataGridCell cell = dgLineHistoryTabSearch.GetCellFromPoint(pnt);
            if (cell != null)
            {
                if (cell.Row.Index > -1)
                {
                    string sLotid = Util.NVC(DataTableConverter.GetValue(dgLineHistoryTabSearch.Rows[cell.Row.Index].DataItem, "LOTID"));
                    string sInsp_seqs = Util.NVC(DataTableConverter.GetValue(dgLineHistoryTabSearch.Rows[cell.Row.Index].DataItem, "INSP_SEQS"));

                    if (cell.Column.Name == "LOTID")
                    {
                        popUpOpenPalletInfo(sLotid, sInsp_seqs);
                    }                    
                }
            }
        }

        private void cboEquipmentSegment_SelectionChanged(object sender, EventArgs e)
        {
            Set_Combo_ProductModel(cboProductModel, cboEquipmentSegment);
            Set_Combo_Product(cboProduct, cboEquipmentSegment);
        }

        private void cboEquipmentSegment1_SelectionChanged(object sender, EventArgs e)
        {
            //Set_Combo_ProductModel(cboProductModel1, cboEquipmentSegment1);
            Set_Combo_Product(cboProduct1, cboEquipmentSegment1);
        }


        private void cboINSPFlag_SelectedIndexChanged(object sender, PropertyChangedEventArgs<int> e)
        {
            if (e.NewValue.Equals(3))
            {
                txtCheckNg.Visibility = Visibility.Visible; //chkNg
                chkNg.Visibility = Visibility.Visible;
            }
            else
            {
                txtCheckNg.Visibility = Visibility.Hidden; //chkNg
                chkNg.Visibility = Visibility.Hidden;
                chkNg.IsChecked = false;

            }
        }

        private void txtMLotID_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            
            switch (e.Key)
            {
                case Key.V:
                  if (Keyboard.IsKeyDown(Key.LeftCtrl))
                    {
                        // 줄바꿈 자르기
                        string[] stringSeparators = new string[] { "\r\n" };
                        string sPasteString = Clipboard.GetText();
                        string[] sPasteStrings = sPasteString.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
                        string sLotid = null;
                        tbLotID.Text = null;
                        //없을시
                        if (sPasteStrings.Length == 0)
                        {
                            Util.MessageInfo("SFU1190");
                            HiddenLoadingIndicator();
                            return;
                        }
                        //100개 이상시 에러발생
                        else if (sPasteStrings.Length > 100)
                        {
                            Util.MessageValidation("SFU3695");   //최대 100개 까지 가능합니다.
                            HiddenLoadingIndicator();
                            return;
                        }
                        else if (sPasteStrings.Length > 0 && sPasteStrings.Length <= 100)
                        {
                            bMultilotByKeyboard = true;
                            ShowLoadingIndicator();
                            dgLineHistoryTabSearch.ClearRows();
                            DataTable dtResult = DataTableConverter.Convert(dgLineHistoryDetailTabSearch.ItemsSource);
                            for (int i  = 0; sPasteStrings.Length > i;i++)
                            {
                                //컴마로 LOTID 묶기
                                if (string.IsNullOrWhiteSpace(sLotid))
                                {
                                    sLotid = sPasteStrings[i];
                                }
                                else
                                {
                                    sLotid = (sLotid + "," + sPasteStrings[i]);

                                }
                               
                            }
                            dtResult = search_LOT_2ND_OCV_INSP_HIST(sLotid);
                            Util.GridSetData(dgLineHistoryTabSearch, dtResult, FrameOperation, true);
                            Util.SetTextBlockText_DataGridRowCount(tbCellListCount1, Util.NVC(dtResult.Rows.Count));
                            HiddenLoadingIndicator();
                            
                        }
                    }
                    break;
            }
            
        }
        private void txtMLotID_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow drLOT = RQSTDT.NewRow();
                drLOT["LOTID"] = tbLotID.Text.Trim();
                RQSTDT.Rows.Add(drLOT);
                //LOTID 찾기
                DataTable dtLOTResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_MAPPINGLOTID", "RQSTDT", "RSLTDT", RQSTDT);
                if(dtLOTResult.Rows.Count > 0)
                {
                    dgLineHistoryTabSearch.ClearRows();
                    bMultilotByKeyboard = true;

                    DataTable dtResult = DataTableConverter.Convert(dgLineHistoryTabSearch.ItemsSource);
                    //2nd OCV 조회
                    dtResult.Merge(search_LOT_2ND_OCV_INSP_HIST(dtLOTResult.Rows[0]["LOTID"].ToString()));
                    //UI에 넣기
                    Util.GridSetData(dgLineHistoryTabSearch, dtResult, FrameOperation, true);
                    Util.SetTextBlockText_DataGridRowCount(tbCellListCount1, Util.NVC(dtResult.Rows.Count));
                    tbLotID.Clear();
                    HiddenLoadingIndicator();
                }
                else
                {
                    Util.MessageValidation("SFU1905");
                }
                

            }

        }
        private DataTable search_LOT_2ND_OCV_INSP_HIST(string LOTID)
        {
            try
            {
                
                DataTable INDATA = new DataTable();
                
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("LOTID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["LOTID"] = LOTID; 

                INDATA.Rows.Add(dr);
                DataTable OUTDATA = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_2ND_OCV_INSP_HIST_LIST", "RQSTDT", "RSLTDT", INDATA,null);
                return OUTDATA;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private void dgLineHistoryTabSearch_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            // 배포를 위한 임시 조치
            // 염규범 선임
            //DataRowView drv = e.Row.DataItem as DataRowView;
            //if (drv["CHK"].SafeToString() != "True" && e.Column != dgLineHistoryTabSearch.Columns["CHK"])
            //{
            //    e.Cancel = true;
            //    return;
            //}
            //if (e.Column != this.dgLineHistoryTabSearch.Columns["CHK"]
            //                && e.Column != this.dgLineHistoryTabSearch.Columns["DEL_FLAG"])
            //{
            //    e.Cancel = true;
            //}
            //else
            //{
            //    e.Cancel = false;
            //}
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            ShowLoadingIndicator();
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new System.Threading.ThreadStart(delegate { }));

            try
            {
                string bizRuleName = "DA_PRD_UPD_TB_SFC_PRDT_2ND_OCV_JUDG_FOR_DEL_FLAG";
                DataTable isCreateTable = new DataTable();
                isCreateTable = DataTableConverter.Convert(dgLineHistoryTabSearch.GetCurrentItems());
                if (!CommonVerify.HasDataGridRow(dgLineHistoryTabSearch)) return;

                this.dgLineHistoryTabSearch.EndEdit();
                this.dgLineHistoryTabSearch.EndEditRow(true);

                DataSet indataSet = new DataSet();

                DataTable inDataTable = indataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("INSP_SEQS", typeof(string));
                inDataTable.Columns.Add("INSP_BAS_DTTM", typeof(string));
                inDataTable.Columns.Add("DEL_FLAG", typeof(string));
                inDataTable.Columns.Add("UPDUSER", typeof(string));

                foreach (object modified in dgLineHistoryTabSearch.GetModifiedItems())
                {
                    if (DataTableConverter.GetValue(modified, "CHK").Equals("True"))
                    {
                        DataRow param = inDataTable.NewRow();
                        param["LOTID"] = DataTableConverter.GetValue(modified, "LOTID");
                        param["INSP_BAS_DTTM"] = DataTableConverter.GetValue(modified, "INSP_BAS_DTTM");
                        param["INSP_SEQS"] = DataTableConverter.GetValue(modified, "INSP_SEQS");
                        param["DEL_FLAG"] = DataTableConverter.GetValue(modified, "DEL_FLAG");
                        param["UPDUSER"] = LoginInfo.USERID;
                        inDataTable.Rows.Add(param);
                    }
                }

                if (inDataTable.Rows.Count < 1)
                {
                    Util.MessageValidation("SFU3538");
                    return;
                }

                new ClientProxy().ExecuteServiceSync_Multi(bizRuleName, "INDATA", null, indataSet);
                Util.MessageInfo("SFU2056", inDataTable.Rows.Count);
                Util.gridClear(dgLineHistoryTabSearch);

                inDataTable = new DataTable();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            if (FrameOperation.AUTHORITY.ToString().Equals("R"))
            {
                this.dgLineHistoryTabSearch.Columns["DEL_FLAG"].Visibility = Visibility.Collapsed;
                this.dgLineHistoryTabSearch.Columns["CHK"].Visibility = Visibility.Collapsed;
            }
        }
    }
}