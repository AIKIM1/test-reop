/*************************************************************************************
 Created Date : 2017.08.25
      Creator : 신광희
   Decription : 전지 5MEGA-GMES 구축 - X-Ray , Washing 재작업 공정진척 완성Pallet 발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.10.13  신광희 : Initial Created.
**************************************************************************************/
using System;
using System.Windows;
using System.Data;
using System.IO;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ASSY_PALLET_PRINT : C1Window, IWorkArea
    {
        #region Declaration
        DataTable _dtRunCard = null;
        C1.C1Report.C1Report _rePort = null;
        private string _palletId = string.Empty;
        private string _processCode = string.Empty;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        public CMM_ASSY_PALLET_PRINT()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Form Load Event
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            // Runcard 발행 DataSet
            _dtRunCard = tmps[0] as DataTable;
            _palletId = tmps[1].GetString();
            _processCode = tmps[2].GetString();

            this.Loaded -= Window_Loaded;
            // 미리보기
            PrintView();


        }
        #endregion

        #region Button Event
        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            SetPalletPrintCount();
            //var pm = new C1.C1Preview.C1PrintManager();
            //pm.Document = _rePort;
            //System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
            //pm.Print(ps, ps.DefaultPageSettings);
            //this.Close();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion

        #region SizeChanged
        private void C1Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            c1DocumentViewer.FitToWidth();
            c1DocumentViewer.FitToHeight();

        }
        #endregion

        #region User Method
        private void PrintView()
        {
            try
            {
                _rePort = new C1.C1Report.C1Report();
                _rePort.Layout.PaperSize = System.Drawing.Printing.PaperKind.A4;

                System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();

                string targetFile = string.Equals(_processCode, Process.XRAY_REWORK) ? "LGC.GMES.MES.CMM001.Report.XRAY_REWORK.xml" : "LGC.GMES.MES.CMM001.Report.WASHING_REWORK.xml";
                string reportName = string.Equals(_processCode, Process.XRAY_REWORK) ? "XRAY_REWORK" : "WASHING_REWORK";

                using (Stream stream = a.GetManifestResourceStream(targetFile))
                {
                    _rePort.Load(stream, reportName);

                    // 임시 다국어 처리
                    for (int cnt = 0; cnt < _rePort.Fields.Count; cnt++)
                    {
                        if(_rePort.Fields[cnt].Name == null) continue;
                        if (_rePort.Fields[cnt].Name.IndexOf("Text", StringComparison.Ordinal) > -1)
                        {
                            _rePort.Fields[cnt].Text = ObjectDic.Instance.GetObjectName(Util.NVC(_rePort.Fields[cnt].Text));
                        }
                    }

                    for (int col = 0; col < _dtRunCard.Columns.Count; col++)
                    {
                        string strColName = _dtRunCard.Columns[col].ColumnName;

                        if (_rePort.Fields.Contains(strColName))
                        {
                            double dValue = 0;
                            int nValue = 0;

                            if (strColName.Equals("WIPQTY"))
                            {
                                if (double.TryParse(Util.NVC(_dtRunCard.Rows[0][strColName]), out dValue))
                                    _rePort.Fields[strColName].Text = dValue.ToString("N0");
                            }
                            else if (strColName.IndexOf("BARCODE", StringComparison.Ordinal) > -1)
                            {
                                _rePort.Fields[strColName].Text = _dtRunCard.Rows[0][strColName].ToString();
                                _rePort.Fields[strColName + "_TXT"].Text = _dtRunCard.Rows[0][strColName].ToString();
                            }
                            else
                            {
                                _rePort.Fields[strColName].Text = _dtRunCard.Rows[0][strColName].ToString();
                            }
                            
                        }
                    }

                }

                c1DocumentViewer.Document = _rePort.FixedDocumentSequence;

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetPalletPrintCount()
        {
            try
            {
                string bizRuleName = string.Equals(_processCode, Process.XRAY_REWORK) ? "BR_PRD_REG_PALLET_PRINT_CNT_ASSY_XR" : "BR_PRD_REG_PALLET_PRINT_CNT_ASSY_PL";
                DataTable inDataTable = new DataTable("INDATA");
                inDataTable.Columns.Add("PALLET_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                DataRow dr = inDataTable.NewRow();
                dr["PALLET_ID"] = _palletId;
                dr["USERID"] = LoginInfo.USERID;
                inDataTable.Rows.Add(dr);
                new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", null, inDataTable);

                var pm = new C1.C1Preview.C1PrintManager {Document = _rePort};
                System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
                pm.Print(ps, ps.DefaultPageSettings);
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion



    }
}
