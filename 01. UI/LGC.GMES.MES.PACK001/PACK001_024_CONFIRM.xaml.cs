/*************************************************************************************
 Created Date : 2016.06.16
      Creator :
   Decription :
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16 DEVELOPER : Initial Created.
  2022.07.16 정용석 CSR ID C20220504-000287 : DA_PRD_SEL_RETURN_AS_PACK_LIST -> 이거 호출안하고 부모폼에서 넘겨받은 IssueID 목록을 그대로 표출
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Documents;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK001_024_CONFIRM : C1Window, IWorkArea
    {
        #region Member Variable Lists...
        private int iReturnCellQty = 0;
        private string returnNote = string.Empty;
        #endregion

        #region Constructor...
        public PACK001_024_CONFIRM()
        {
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }

        public PACK001_024_CONFIRM(int returnCellQty)
        {
            iReturnCellQty = returnCellQty;
            InitializeComponent();
            this.Loaded += Window_Loaded;
        }
        #endregion

        #region Property
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string RETURNNOTE
        {
            get
            {
                return returnNote;
            }
            set
            {
                returnNote = value;
            }
        }
        #endregion

        #region Member Function List
        private void Initialize()
        {
            object[] obj = C1WindowExtension.GetParameters(this);
            DataTable dt = (obj != null && obj.Length >= 0) ? (DataTable)obj[0] : null;
            this.Loaded -= Window_Loaded;
            if (CommonVerify.HasTableRow(dt))
            {
                Util.GridSetData(this.dgConfirmReturn, dt, FrameOperation);
            }
        }

        private void Confirm()
        {
            try
            {
                TextRange textRange = new TextRange(this.txtNote.Document.ContentStart, this.txtNote.Document.ContentEnd);
                if (string.IsNullOrEmpty(textRange.Text.Trim()))
                {
                    Util.AlertInfo("SFU1554");  // 반품사유를 입력하세요
                    return;
                }

                returnNote = textRange.Text;
                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region Event Lists...
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Initialize();
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            this.Confirm();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
    }
}