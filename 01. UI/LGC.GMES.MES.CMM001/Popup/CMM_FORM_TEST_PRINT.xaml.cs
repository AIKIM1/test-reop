/*************************************************************************************
 Created Date : 2017.03.04
      Creator : Shin Kwang Hee
   Decription : 전지 5MEGA-GMES 구축 - 활성화 후 공정 - 테스트 발행
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.23  Shin Kwang Hee : 
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_FORM_TEST_PRINT.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_FORM_TEST_PRINT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private readonly Util _util = new Util();
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
        public CMM_FORM_TEST_PRINT()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {   

        }
        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
                InitializeControls();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnGoodLabel_Click(object sender, RoutedEventArgs e)
        {
            //LBL0106 : 양품 태그 라벨,  LBL0107 : 불량 태그 라벨
            const string labelCode = "LBL0106";
            if (!ValidationDefectPrint(labelCode)) return;

            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));
            dtLabelItem.Columns.Add("ITEM011", typeof(string));


            for (int i = 0; i < numAddCount.Value.GetInt(); i++)
            {
                DataRow dr = dtLabelItem.NewRow();
                dr["LABEL_CODE"] = labelCode;
                dr["ITEM001"] = string.Empty;
                dr["ITEM002"] = string.Empty;
                dr["ITEM003"] = string.Empty;
                dr["ITEM004"] = string.Empty;
                dr["ITEM005"] = string.Empty;
                dr["ITEM006"] = string.Empty;
                dr["ITEM007"] = string.Empty;
                dr["ITEM008"] = string.Empty;
                dr["ITEM009"] = string.Empty;
                dr["ITEM010"] = string.Empty;
                dr["ITEM011"] = string.Empty;
                dtLabelItem.Rows.Add(dr);
            }

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);
            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다. IT 담당자에게 문의하세요.
                Util.MessageValidation("SFU3150");
            }

        }

        private void btnNoGoodLabel_Click(object sender, RoutedEventArgs e)
        {
            //LBL0106 : 양품 태그 라벨,  LBL0107 : 불량 태그 라벨
            const string labelCode = "LBL0107";
            if (!ValidationDefectPrint(labelCode)) return;

            DataTable dtLabelItem = new DataTable();
            dtLabelItem.Columns.Add("LABEL_CODE", typeof(string));  //LABEL CODE
            dtLabelItem.Columns.Add("ITEM001", typeof(string));     //PROCESSNAME
            dtLabelItem.Columns.Add("ITEM002", typeof(string));     //MODEL
            dtLabelItem.Columns.Add("ITEM003", typeof(string));     //PKGLOT
            dtLabelItem.Columns.Add("ITEM004", typeof(string));     //DEFECT
            dtLabelItem.Columns.Add("ITEM005", typeof(string));     //QTY
            dtLabelItem.Columns.Add("ITEM006", typeof(string));     //MKTTYPE
            dtLabelItem.Columns.Add("ITEM007", typeof(string));     //PRODDATE
            dtLabelItem.Columns.Add("ITEM008", typeof(string));     //EQUIPMENT
            dtLabelItem.Columns.Add("ITEM009", typeof(string));     //INSPECTOR
            dtLabelItem.Columns.Add("ITEM010", typeof(string));
            dtLabelItem.Columns.Add("ITEM011", typeof(string));

            for (int i = 0; i < numAddCount.Value.GetInt(); i++)
            {
                DataRow dr = dtLabelItem.NewRow();
                dr["LABEL_CODE"] = labelCode;
                dr["ITEM001"] = string.Empty;
                dr["ITEM002"] = string.Empty;
                dr["ITEM003"] = string.Empty;
                dr["ITEM004"] = string.Empty;
                dr["ITEM005"] = string.Empty;
                dr["ITEM006"] = string.Empty;
                dr["ITEM007"] = string.Empty;
                dr["ITEM008"] = string.Empty;
                dr["ITEM009"] = string.Empty;
                dr["ITEM010"] = string.Empty;
                dr["ITEM011"] = string.Empty;
                dtLabelItem.Rows.Add(dr);
            }

            string printType;
            string resolution;
            string issueCount;
            string xposition;
            string yposition;
            string darkness;
            DataRow drPrintInfo;

            if (!_util.GetConfigPrintInfo(out printType, out resolution, out issueCount, out xposition, out yposition, out darkness, out drPrintInfo))
                return;

            bool isLabelPrintResult = Util.PrintLabelPolymerForm(FrameOperation, loadingIndicator, dtLabelItem, printType, resolution, issueCount, xposition, yposition, darkness, drPrintInfo);
            if (!isLabelPrintResult)
            {
                //라벨 발행중 문제가 발생하였습니다. IT 담당자에게 문의하세요.
                Util.MessageValidation("SFU3150");
            }
        }

        private void btnCartPrint_Click(object sender, RoutedEventArgs e)
        {
            CMM_POLYMER_FORM_TEMP_TAG_PRINT popupTempTagPrint = new CMM_POLYMER_FORM_TEMP_TAG_PRINT();
            popupTempTagPrint.FrameOperation = this.FrameOperation;
            C1WindowExtension.SetParameters(popupTempTagPrint, null);
            popupTempTagPrint.Closed += popupTempTagPrint_Closed;
            this.Dispatcher.BeginInvoke(new Action(() => popupTempTagPrint.ShowModal()));
        }
        private void popupTempTagPrint_Closed(object sender, EventArgs e)
        {
            //throw new NotImplementedException();
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }


        #endregion

        #region Mehod

        #region [BizCall]

        #endregion

        #region [Validation]
        private bool ValidationDefectPrint(string labelCode)
        {

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Count < 1)
            {
                Util.MessageValidation("SFU2003"); //프린트 환경 설정값이 없습니다.
                return false;
            }

            var query = (from t in LoginInfo.CFG_SERIAL_PRINT.AsEnumerable()
                         where t.Field<string>("LABELID") == labelCode
                         select t).ToList();

            if (!query.Any())
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            if (LoginInfo.CFG_SERIAL_PRINT.Rows.Cast<DataRow>().Any(itemRow => string.IsNullOrEmpty(itemRow[CustomConfig.CONFIGTABLE_SERIALPRINTER_LABELID].GetString())))
            {
                Util.MessageValidation("SFU4339"); //프린터 환경설정에 라벨정보 항목이 없습니다.
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]

        private void ShowLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if (loadingIndicator != null)
                loadingIndicator.Visibility = Visibility.Collapsed;
        }


        #endregion

        #endregion


    }
}
