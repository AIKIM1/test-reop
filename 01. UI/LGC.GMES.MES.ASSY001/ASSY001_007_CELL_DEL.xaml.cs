/*************************************************************************************
 Created Date : 2016.09.21
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Packaging 공정진척 화면 - Tray 생성 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2004.05.31  이병윤        E20240528-000578 NG Cell 추가, 삭제시 Caution 팝업 호출
  
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

namespace LGC.GMES.MES.ASSY001
{
    public partial class ASSY001_007_CELL_DEL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _CellID = string.Empty;

        BizDataSet _Biz = new BizDataSet();
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
        public ASSY001_007_CELL_DEL()
        {
            InitializeComponent();
        }
        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _CellID = Util.NVC(tmps[0]);

            txtCellId.Text = _CellID;
            txtCellId.IsReadOnly = true;
            txtNg.IsReadOnly = true;
            txtNg.Foreground = new SolidColorBrush(Colors.Red);
            txtNg.FontWeight = FontWeights.Bold;

            GetNgCell();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizRule]

        private void GetNgCell()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("SUBLOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["SUBLOTID"] = Util.NVC(_CellID);

                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CELL_NG_DEL_NJ", "INDATA", "OUTDATA", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                {
                    string msg = string.Empty;
                    for (int i = 0; i < dtResult.Rows.Count; i++)
                    {
                        string sNg = Convert.ToString(dtResult.Rows[i]["JUDGE"]);

                        if (sNg.Equals("AC"))
                        {
                            msg += "ACOH null; ";
                        }
                        else if (sNg.Equals("UT"))
                        {
                            msg += "UTAP null; ";
                        }
                        else if (sNg.Equals("TH"))
                        {
                            msg += "Thickness null; ";
                        }
                        else if (sNg.Equals("TA"))
                        {
                            msg += "Tape null; ";
                        }
                        else if (sNg.Equals("FI"))
                        {
                            msg += "Film Length null; ";
                        }
                        else if (sNg.Equals("NI"))
                        {
                            msg += "FILLING null; ";
                        }
                        else
                        {
                            msg += "";
                        }
                    }
                    txtNg.Text = msg;
                }
                else
                {
                    txtNg.Text = "";
                }

                HiddenLoadingIndicator();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                HiddenLoadingIndicator();
            }
        }
        
        #endregion

        #region [Validation]
       
        #endregion

        #region[Func]

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
