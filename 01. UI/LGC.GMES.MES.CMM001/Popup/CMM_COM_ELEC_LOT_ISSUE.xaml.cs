/*************************************************************************************
 Created Date : 2017.03.02
      Creator : 유관수
   Decription : 전지 5MEGA-GMES 구축 - 이전작업특이사항팝업
--------------------------------------------------------------------------------------
 [Change History]
  2017.03.02  유관수 : Initial Created. - 이전 공정에서 입력한 특이사항 이력 조회
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_COM_ELEC_LOT_ISSUE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_COM_ELEC_LOT_ISSUE : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _lineCode = string.Empty;
        private string _eqptCode = string.Empty;
        private string _eqptName = string.Empty;
        private string _processCode = string.Empty;
        private string _lotid = string.Empty;
        private string _wipSeq = string.Empty;
        private string _shiftCode = string.Empty;

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

        public CMM_COM_ELEC_LOT_ISSUE()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 5)
            {
                _lineCode = Util.NVC(tmps[0]);
                _eqptCode = Util.NVC(tmps[1]);
                _processCode = Util.NVC(tmps[2]);
                _lotid = Util.NVC(tmps[3]);
                _wipSeq = Util.NVC(tmps[4]);
                _eqptName = Util.NVC(tmps[5]);

                txtLOTId.Text = _lotid;
            }

            SelectEquiptMentNote();
            Loaded -= C1Window_Loaded;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SelectEquiptMentNote();
        }

        #endregion

        #region Mehod
        private void SelectEquiptMentNote()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;
                Util.gridClear(dgEquipmentNote);
                const string bizRuleName = "DA_PRD_SEL_LOT_HISTORY_WIPNOTE";

                DataTable inDataTable = new DataTable();

                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));


                DataRow inData = inDataTable.NewRow();
                inData["LANGID"] = LoginInfo.LANGID;
                inData["LOTID"] = txtLOTId.Text;
                inDataTable.Rows.Add(inData);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    loadingIndicator.Visibility = Visibility.Collapsed;
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    dgEquipmentNote.ItemsSource = DataTableConverter.Convert(result);
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
        #endregion
    }
}