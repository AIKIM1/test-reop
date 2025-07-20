/*************************************************************************************
 Created Date : 2020.03.24
      Creator : 
   Decription : 21700 Tesla 자동포장기 OUTBOX 라벨 재발행 화면
--------------------------------------------------------------------------------------
 [Change History]
  날짜        버젼  수정자   CSR              내용
 -------------------------------------------------------------------------------------
 2020.03.24  0.1   이현호       21700 자동포장기 OUTBOX 라벨 재발행 화면 개발.


**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{
    public partial class BOX001_242_REPRINT : C1Window, IWorkArea
    {
        Util _Util = new Util();

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_242_REPRINT()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();

        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #region [Events]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnReprint_Click(object sender, RoutedEventArgs e)
        {
            Reprint();
        }

        private void txtOutBoxID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                Reprint();
        }
        #endregion

        #region [Method]
        private void Reprint()
        {
            string zplCode = string.Empty;
            string lblCode = string.Empty;

            string sPrt = string.Empty;
            string sRes = string.Empty;
            string sCopy = string.Empty;
            string sXpos = string.Empty;
            string sYpos = string.Empty;
            string sDark = string.Empty;
            DataRow drPrtInfo = null;

            if (!_Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo))
                return;
            try
            {
                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("BOXID");
                inData.Columns.Add("LANGID");
                DataRow inDataRow = inData.NewRow();
                inDataRow["BOXID"] = txtOutBoxID.Text;
                inDataRow["LANGID"] = LoginInfo.LANGID;
                inData.Rows.Add(inDataRow);

                DataTable inPrintTable = inDataSet.Tables.Add("INPRINT");
                inPrintTable.Columns.Add("PRMK");
                inPrintTable.Columns.Add("RESO");
                inPrintTable.Columns.Add("PRCN");
                inPrintTable.Columns.Add("MARH");
                inPrintTable.Columns.Add("MARV");
                inPrintTable.Columns.Add("DARK");
                DataRow inPrintDr = inPrintTable.NewRow();
                inPrintDr["PRMK"] = sPrt;
                inPrintDr["RESO"] = sRes;
                inPrintDr["PRCN"] = sCopy;
                inPrintDr["MARH"] = sXpos;
                inPrintDr["MARV"] = sYpos;
                inPrintDr["DARK"] = sDark;
                inPrintTable.Rows.Add(inPrintDr);

                DataSet resultDS = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_OUTBOX_REPRINT_AUTO", "INDATA,INPRINT", "OUTDATA", inDataSet);
                if ((resultDS.Tables.IndexOf("OUTDATA") > -1) && resultDS.Tables["OUTDATA"].Rows.Count > 0)
                {
                    zplCode = resultDS.Tables["OUTDATA"].Rows[0]["ZPLCODE"].GetString();
                    if (zplCode.Split(',')[0].Equals("1"))
                    {
                        ControlsLibrary.MessageBox.Show(zplCode.Split(',')[1], "", "Info", MessageBoxButton.OK, MessageBoxIcon.None, null);

                    }
                    PrintLabel(zplCode, drPrtInfo);
                }
            }
            catch (Exception ex)
            {

                Util.MessageException(ex);
            }
        }
        private bool PrintLabel(string zpl, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, zpl);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }
        #endregion
    }
}
