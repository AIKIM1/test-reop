/***************************************************************
 Created Date : 2023.04.05 
      Creator : 
   Decription : 전지GMES 구축 - LANE별 바코드 미리보기 팝업
----------------------------------------------------------------
 [Change History]
  2023.04.05   김도형 : Initial Created. [E20230328-000520]Lot Label print 미리보기 개선건
  2024.09.23   김도형 : [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
****************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Collections.Generic;


namespace LGC.GMES.MES.CMM001.Popup
{
    /// <summary>
    /// CMM_ELEC_LANE_BARCODE_PREV.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_LANE_BARCODE_PREV : C1Window, IWorkArea
    {
        C1.C1Report.C1Report cr = null;

        private string _LOTID = string.Empty;
        private string _PROCID = string.Empty;
        private DataRowView dataRowView;
        DataTable dtLot01;
        DataTable dtLotItem01;

        Util _Util = new Util();
         
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_LANE_BARCODE_PREV()
        {
            InitializeComponent();
        }

        public CMM_ELEC_LANE_BARCODE_PREV(TextBox target)
        {
            InitializeComponent();
            ApplyPermissions();
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 3)
            {
                _LOTID = tmps[0].GetString();
                _PROCID = tmps[1].GetString();
                dataRowView = tmps[2] as DataRowView;

            }
            else
            {
                _LOTID = string.Empty;
                _PROCID = string.Empty;
                dataRowView = null;
            }
            GetCutIdList(_LOTID); //Line별 LOT 목록
            if (dtLot01 != null && dtLot01.Rows.Count > 0)
            { 
                for (int i = 0; i < dtLot01.Rows.Count; i++)
                {
                    GetLotInfo(Util.NVC(dtLot01.Rows[i]["LOTID"])); //Line별 Lot 상세정보(라벨관련정보)
                }

            }
            else
            {
                Util.MessageValidation("SFU1195"); // Lot 정보가 없습니다.
                this.DialogResult = MessageBoxResult.Cancel;
            } 
            
            BarCodeLabelPreview();  //BarCode Label Preview

            return;
        }

        
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void BarCodeLabelPreview()
        {
            int idxRow = 0;
            try
            {


                if (dtLotItem01 != null && dtLotItem01.Rows.Count > 0)
                {

                    cr = new C1.C1Report.C1Report();
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                    string filename = string.Empty;
                    string reportname = string.Empty;

                    string sEltrSltLotThicknessViewYn = GetEltrSltLotThicknessViewYn(_PROCID);       // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                    string sSltLotThickness = string.Empty;                                          // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                    string sSltLotid = string.Empty;

                    if (sEltrSltLotThicknessViewYn.Equals("Y") && _PROCID.Equals(Process.SLITTING )) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                    {
                        reportname = "SlittingBarcodeLabel_Prev_Thick_Elec";

                    }
                    else
                    {
                        reportname = "SlittingBarcodeLabel_Prev_Elec";
                    }
                     
                    filename = reportname + ".xml";

                    System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                    using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.CMM001.Report." + filename))
                    {
                        cr.Load(stream, reportname);
                        for (int row = 0; row < dtLotItem01.Rows.Count; row++)
                        {

                            idxRow = idxRow + 1;
                            if (idxRow > 30)
                            {
                                Util.MessageValidation("SFU8217", 30); //최대 [%1]까지 미리보기 가능 합니다.

                            }
                            else
                            {
                                if (sEltrSltLotThicknessViewYn.Equals("Y") && _PROCID.Equals(Process.SLITTING)) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                                {
                                    sSltLotid = "";
                                    sSltLotThickness = "";
                                    sSltLotid = dtLotItem01.Rows[row]["ITEM004"].ToString();            // LOTID
                                    sSltLotThickness = "T/K " +  GetEltrSltLotThickness(sSltLotid);     // 두께  ITEM007
                                }

                                cr.Fields["Line1" + "_" + idxRow.ToString()].Visible = true;
                                cr.Fields["Line2" + "_" + idxRow.ToString()].Visible = true;
                                cr.Fields["Line3" + "_" + idxRow.ToString()].Visible = true;
                                cr.Fields["Line4" + "_" + idxRow.ToString()].Visible = true;


                                for (int col = 0; col < dtLotItem01.Columns.Count; col++)
                                {
                                    string strColName = dtLotItem01.Columns[col].ColumnName.ToString();
                                    if (cr.Fields.Contains(strColName + "_" + idxRow.ToString()))
                                    {
                                       if (sEltrSltLotThicknessViewYn.Equals("Y") && _PROCID.Equals(Process.SLITTING) && strColName.Equals("ITEM007")) // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
                                       {
                                            cr.Fields[strColName + "_" + idxRow.ToString()].Text = sSltLotThickness;   // ITEM007 두께
                                       }
                                       else
                                       {
                                          cr.Fields[strColName + "_" + idxRow.ToString()].Text = dtLotItem01.Rows[row][strColName].ToString();
                                       }
                                       cr.Fields[strColName + "_" + idxRow.ToString()].Visible = true;
                                    }
                                }
                            }
                        }

                    }
                  

                        c1DocumentViewer.Document = cr.FixedDocumentSequence;

                }
                else
                {
                    Util.MessageValidation("SFU1195"); // Lot 정보가 없습니다.
                    this.DialogResult = MessageBoxResult.Cancel; 
                }
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
                return;
            }

            return;

        }
        private void GetCutIdList(string sLotid)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string)); 

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotid; 
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_SLIT_LOT_LANE_PREV", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    dtLot01 = result;
                    return;
                }
            }
            catch (Exception ex)
            {
                return;
            }

            return;
             
        }
        private void GetLotInfo(string sLotid)
        {
            try
            { 
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));
                IndataTable.Columns.Add("PROCID", typeof(string));
                IndataTable.Columns.Add("SAMPLE_COMPANY", typeof(string));
                IndataTable.Columns.Add("CLCTVAL", typeof(string)); 

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = sLotid;
                Indata["PROCID"] = _PROCID;
                Indata["SAMPLE_COMPANY"] = "";
                Indata["CLCTVAL"] = "";
                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_LOT_LABEL_ELEC_PREV", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                    if (dtLotItem01 != null && dtLotItem01.Rows.Count > 0)
                    {

                        DataRow resultRow = dtLotItem01.NewRow();
                        resultRow = result.Rows[0]; 
                        dtLotItem01.ImportRow(result.Rows[0]); 
                    }else
                    { 
                        dtLotItem01 = result;
                    }
                     
                    return ;
                }
            }
            catch (Exception ex)
            {
                return ;
            }

            return ;
        }

          // [E20240716-000333] [생산PI.MES팀] 롤프레스 설비 두께데이터→GMES바코드 정보전송
        private string GetEltrSltLotThicknessViewYn(string sProcid)
        {
            string sEltrSltLotThicknessViewYn = string.Empty;

            string sOpmodeCheck = string.Empty;
            string sCodeType;
            string sCmCode;
            string[] sAttribute = null;

            sCodeType = "ELTR_SLT_LOT_THICKNESS_VIEW_YN";  // 전극 슬리터 LOT 두께 보기 여부
            sCmCode = sProcid;                             // 공정 

            try
            {
                string[] sColumnArr = { "ATTR1", "ATTR2", "ATTR3", "ATTR4", "ATTR5" };

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));
                RQSTDT.Columns.Add("USE_FLAG", typeof(string));

                for (int i = 0; i < sColumnArr.Length; i++)
                    RQSTDT.Columns.Add(sColumnArr[i], typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = sCodeType;
                dr["COM_CODE"] = (sCmCode == string.Empty ? null : sCmCode);
                dr["USE_FLAG"] = "Y";

                if (sAttribute != null)
                {
                    for (int i = 0; i < sAttribute.Length; i++)
                        dr[sColumnArr[i]] = string.IsNullOrEmpty(sAttribute[i]) ? (object)DBNull.Value : sAttribute[i];
                }
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE_ATTR", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrSltLotThicknessViewYn = "Y";
                }
                else
                {
                    sEltrSltLotThicknessViewYn = "N";
                }

                return sEltrSltLotThicknessViewYn;
            }
            catch (Exception ex)
            {
                sEltrSltLotThicknessViewYn = "N";
                //Util.MessageException(ex);
                return sEltrSltLotThicknessViewYn;
            }
        }

        private string GetEltrSltLotThickness(string sLotId)
        {
            string sEltrSltLotThickness = string.Empty;

            try
            {
               
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("CLCTTYPE", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID; ;
                dr["LOTID"] = sLotId;   
                dr["CLCTTYPE"] = "E";  //설비수집항목
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_EQP_SEL_TB_SFC_EQPT_DATA_CLCT_INFO_COLLECT_SLT_THICK", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    sEltrSltLotThickness = dtResult.Rows[0]["PV_VALUE"].ToString() ;
                }
                else
                {
                    sEltrSltLotThickness = "";
                }

                return sEltrSltLotThickness;
            }
            catch (Exception ex)
            {
                sEltrSltLotThickness = "";
                //Util.MessageException(ex);
                return sEltrSltLotThickness;
            }
        }

    }
}