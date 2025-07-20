/***************************************************************
 Created Date : 2024.01.30 
      Creator : 
   Decription : 전지GMES 구축 - SKID 바코드 미리보기 팝업
----------------------------------------------------------------
 [Change History]
  2024.01.30   김도형 : Initial Created. [E20240115-000103] Slitter history card -> small tag
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
    /// CMM_ELEC_SKID_BARCODE_PREV.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_ELEC_SKID_BARCODE_PREV : C1Window, IWorkArea
    {
        C1.C1Report.C1Report cr = null;

        private string _LOTID = string.Empty;
        private string _PROCID = string.Empty;
        private DataRowView dataRowView; 
        DataTable dtLotItem01;

        Util _Util = new Util();
         
        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_SKID_BARCODE_PREV()
        {
            InitializeComponent();
        }

        public CMM_ELEC_SKID_BARCODE_PREV(TextBox target)
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
            if (_LOTID != null )
            { 
                 
                GetLotInfo(_LOTID); // SKID 상세정보(라벨관련정보) 

            }
            else
            {
                Util.MessageValidation("SFU1195"); // Lot 정보가 없습니다.
                this.DialogResult = MessageBoxResult.Cancel;
                return;
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
            try
            {
                if (dtLotItem01 != null && dtLotItem01.Rows.Count > 0)
                { 
                    cr = new C1.C1Report.C1Report();
                    cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A6; 

                    string filename = string.Empty;
                    string reportname = string.Empty;

                    reportname = "SlittingSkidBarcodeLabel_Prev_Elec"; 
                    filename = reportname + ".xml";

                    System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                    using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.CMM001.Report." + filename))
                    {
                        cr.Load(stream, reportname);

                        cr.Fields["ITEM001"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_PRODID") +  " : " + dtLotItem01.Rows[0]["PRODID"].ToString();
                        cr.Fields["ITEM002"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_PRJT_NAME") +  " : " + dtLotItem01.Rows[0]["PRJT_NAME"].ToString();
                        cr.Fields["ITEM003"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_PROD_VER_CODE") + " : " + dtLotItem01.Rows[0]["PROD_VER_CODE"].ToString();
                        cr.Fields["ITEM004"].Text = dtLotItem01.Rows[0]["SKID_ID"].ToString();
                        cr.Fields["ITEM005"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_ELEC_TYPE") + " : " + dtLotItem01.Rows[0]["ELEC_TYPE"].ToString();
                        cr.Fields["ITEM006"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_SKID_ID") + " : " + dtLotItem01.Rows[0]["SKID_ID"].ToString();
                        cr.Fields["ITEM007"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_LANE_QTY") +  " : " + dtLotItem01.Rows[0]["LANE_QTY"].ToString();
                        cr.Fields["ITEM008"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_QTY_UNIT") + " : " + dtLotItem01.Rows[0]["OUTPUT_CR0_QTY"].ToString() + dtLotItem01.Rows[0]["UNIT_CODE_NAME"].ToString();
                        cr.Fields["ITEM009"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_COATING_LINE") + " : " + dtLotItem01.Rows[0]["COATING_LINE"].ToString();
                        cr.Fields["ITEM010"].Text = ObjectDic.Instance.GetObjectName("SKID_BARCODE_VLD_DATE") + "  : " + dtLotItem01.Rows[0]["VLD_DATE"].ToString();  
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
        
        private void GetLotInfo(string sLotid)
        {
            try
            { 
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string)); 

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["LOTID"] = sLotid; 

                IndataTable.Rows.Add(Indata);

                DataTable result = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PROCESS_SKID_LABEL_ELEC", "INDATA", "RSLTDT", IndataTable);

                if (result != null && result.Rows.Count > 0)
                {
                   
                    dtLotItem01 = result; 
                    return ;
                }
            }
            catch (Exception ex)
            {
                return ;
            }

            return ;
        }
        
    }
}