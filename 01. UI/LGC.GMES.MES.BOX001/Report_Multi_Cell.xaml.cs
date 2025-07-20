/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.
  2023.09.12  최도훈    : Box 수량이 21개를 초과할 때, 2페이지(22~42번)부터 출력 불가능한 문제 수정




 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.IO;
using System.Windows;

namespace LGC.GMES.MES.BOX001
{
    public partial class Report_Multi_Cell : C1Window, IWorkArea
    {
        object[] tmps;
        string tmmp01 = string.Empty;
        DataTable tmmp02;
        int iMAX_PAGE = 0;
        C1.C1Report.C1Report cr = null;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        
        public Report_Multi_Cell()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            tmps = C1WindowExtension.GetParameters(this);
            tmmp01 = tmps[0] as string;
            tmmp02 = tmps[1] as DataTable;

            this.Loaded -= Window_Loaded;                     
            txtPage.Text = "1";
           
            // Print 수량 셋팅하기
            if (tmps.Length > 2)
            {
                txtPrintQty.Value = Convert.ToInt32(tmps[2] as string);
            }
            else
            {
                txtPrintQty.Value = 1;
            }

            iMAX_PAGE = tmmp02.Rows.Count;
            txtPage_ValueChanged(Convert.ToInt32(txtPage.Text)-1);

        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int iCopies = int.Parse(txtPrintQty.Value.ToString());
                // Print수량만큼 출력함...
                for (int iPrint = 0; iPrint < iCopies; iPrint++)
                {
                    for (int i = 0; i < iMAX_PAGE; i++)
                    {
                        txtPage.Text = (i + 1).ToString();
                        txtPage_ValueChanged(i);
                        
                        var pm = new C1.C1Preview.C1PrintManager();
                        pm.Document = cr;
                        System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();

                        if (LoginInfo.CFG_GENERAL_PRINTER != null && LoginInfo.CFG_GENERAL_PRINTER.Rows.Count > 0 && LoginInfo.CFG_GENERAL_PRINTER.Rows[0] != null && !string.IsNullOrEmpty(LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString()))
                            ps.PrinterName = LoginInfo.CFG_GENERAL_PRINTER.Rows[0]?[CustomConfig.CONFIGTABLE_GENERALPRINTER_NAME].ToString();

                        pm.Print(ps);                        
                    }
                }
                this.DialogResult = MessageBoxResult.OK;
                this.Close();

            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        private void txtPage_ValueChanged(int iPage)
        {
            try
            {
                string filename = string.Empty;
                string reportname = string.Empty;

                //C1.C1Report.C1Report cr = new C1.C1Report.C1Report();
                cr = new C1.C1Report.C1Report();
                cr.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                reportname = tmmp01;
                filename = tmmp01 + ".xml";
                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                using (Stream stream = a.GetManifestResourceStream("LGC.GMES.MES.BOX001.Report." + filename))
                {
                    cr.Load(stream, reportname);

                    for (int col = 0; col < tmmp02.Columns.Count; col++)
                    {
                        string strColName = tmmp02.Columns[col].ColumnName;
                        if (cr.Fields.Contains(strColName)) cr.Fields[strColName].Text = tmmp02.Rows[iPage][strColName].ToString();
                    }

                    int nCount = cr.Fields.Count;
                    string strLabelName = string.Empty;
                    string strLabelTextName = string.Empty;
                    string strLabelFlag = string.Empty;
                    string strObjectID = string.Empty;
                    string strText = string.Empty;         
                    strLabelFlag = "lbl_";
                    for (int i = 0; i < nCount; i++)
                    {
                        strLabelName = cr.Fields[i].Name;
                        strLabelTextName = cr.Fields[i].Name;
                        if (strLabelName.Contains(strLabelFlag))
                        {
                            strObjectID = strLabelTextName.Remove(0, 4);

                            int idx = strObjectID.IndexOf("_");
                            if (idx >= 0 && strObjectID.Length > idx)
                            {
                                int num = 0;
                                string sNumber = strObjectID.Substring(idx+1);
                                if (int.TryParse(sNumber, out num))
                                {
                                    strObjectID = strObjectID.Substring(0, idx);
                                }
                            }
                            strText = ObjectDic.Instance.GetObjectName(strObjectID.Replace(" ",""));
                            cr.Fields[i].Text = strText;
                        }
                    }
                }

                c1DocumentViewer.Document = cr.FixedDocumentSequence;                
            }
            catch (Exception ex)
            {
                Util.AlertInfo(ex.Message);
            }

        }

        private void btnDown_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage > 1 && iCurPage <= iMAX_PAGE)
            {
                txtPage.Text = (iCurPage - 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }
                        
        }

        private void btnUp_Click(object sender, RoutedEventArgs e)
        {
            int iCurPage = Util.NVC_Int(txtPage.Text);
            if (iCurPage >= 1 && iCurPage < iMAX_PAGE)
            {
                txtPage.Text = (iCurPage + 1).ToString();
                txtPage_ValueChanged(Convert.ToInt32(txtPage.Text) - 1);
            }

        }
    }
}
