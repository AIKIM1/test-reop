/*************************************************************************************
 Created Date : 2018.12.27
      Creator : 오화백
   Decription : 자재공급
--------------------------------------------------------------------------------------
 [Change History]
  2018.12.27  오화백 : Initial Created.
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_021_SUPPLY : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        Util _Util = new Util();
        private string _EQPTID = string.Empty;
        private bool _load = true;
      
        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_021_SUPPLY()
        {
            InitializeComponent();
        }


        private void InitializeUserControls()
        {
        }
        /// <summary>
        /// LOAD
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                _load = false;
            }

        }
        /// <summary>
        ///  팝업 셋팅
        /// </summary>
        private void SetControl()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            _EQPTID = parameters[0] as string; // 설비ID
            txtR_Eqpt.Text = parameters[1] as string; //설비명
            txtWo.Text = parameters[2] as string; //WO
            txtFromPort.Text = parameters[3] as string; //FromPort
            txtToPort.Text = parameters[4] as string; //ToPort
        }
        #endregion

        #region Event
        
        #region CSTID ID 바코드 영문 적용 : txtCSTID_GotFocus()
        /// <summary>
        /// 바코드 영문 적용
        /// </summary>
        private void txtCSTID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region Cell 정보 조회 및 입력  : txtCSTID_KeyDown()
        /// <summary>
        /// Cell 정보 조회 및 입력
        /// </summary>
        private void txtCSTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Validation())
                    {
                        return;
                    }

                    string sLotid = txtCSTID.Text.Trim();

                    if (dgMLOT.GetRowCount() > 0)
                    {
                        DataTable dt = DataTableConverter.Convert(dgMLOT.ItemsSource);
                        DataRow[] drSelect = dt.Select("MLOT = '" + sLotid + "'");

                        if (drSelect.Length > 0)
                        {
                            int idx = dt.Rows.IndexOf(drSelect[0]);
                            dgMLOT.SelectedIndex = idx;
                            dgMLOT.ScrollIntoView(idx, dgMLOT.Columns["MLOT"].Index);
                            txtCSTID.Focus();
                            txtCSTID.Text = string.Empty;
                            return;
                        }

                    }
                    if (dgMLOT.Rows.Count == 0)
                    {
                        DataTable inMLot = new DataTable();
                        inMLot.Columns.Add("MLOT", typeof(string));

                        // inMLot
                        DataRow newRow = inMLot.NewRow();
                        newRow["MLOT"] = txtCSTID.Text;
                        inMLot.Rows.Add(newRow);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgMLOT.ItemsSource);
                        DataRow newRow = null;
                        newRow = dtSource.NewRow();
                        newRow["MLOT"] = txtCSTID.Text;
                        dtSource.Rows.Add(newRow);
                        Util.GridSetData(dgMLOT, dtSource, FrameOperation);
                    }
                    txtCSTID.Focus();
                    txtCSTID.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region MLOTID 바코드 영문 적용 : txtCSTID_GotFocus()
        /// <summary>
        /// 바코드 영문 적용
        /// </summary>
        private void txtMLOTID_GotFocus(object sender, RoutedEventArgs e)
        {
            InputMethod.Current.ImeConversionMode = ImeConversionModeValues.Alphanumeric;
        }
        #endregion

        #region MLOT 정보 조회 및 입력  : txtCSTID_KeyDown()
        /// <summary>
        /// MLOT 정보 조회 및 입력
        /// </summary>
        private void txtMLOTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (!Validation())
                    {
                        return;
                    }

                    string sLotid = txtMLOTID.Text.Trim();

                    if (dgMLOT.GetRowCount() > 0)
                    {
                        DataTable dt = DataTableConverter.Convert(dgMLOT.ItemsSource);
                        DataRow[] drSelect = dt.Select("MLOT = '" + sLotid + "'");

                        if (drSelect.Length > 0)
                        {
                            int idx = dt.Rows.IndexOf(drSelect[0]);
                            dgMLOT.SelectedIndex = idx;
                            dgMLOT.ScrollIntoView(idx, dgMLOT.Columns["MLOT"].Index);
                            txtMLOTID.Focus();
                            txtMLOTID.Text = string.Empty;
                            return;
                        }

                    }
                    if (dgMLOT.Rows.Count == 0)
                    {
                        DataTable inMLot = new DataTable();
                        inMLot.Columns.Add("MLOT", typeof(string));

                        // inMLot
                        DataRow newRow = inMLot.NewRow();
                        newRow["MLOT"] = txtMLOTID.Text;
                        inMLot.Rows.Add(newRow);
                    }
                    else
                    {
                        DataTable dtSource = DataTableConverter.Convert(dgMLOT.ItemsSource);
                        DataRow newRow = null;
                        newRow = dtSource.NewRow();
                        newRow["MLOT"] = txtMLOTID.Text;
                        dtSource.Rows.Add(newRow);
                        Util.GridSetData(dgMLOT, dtSource, FrameOperation);
                    }
                    txtMLOTID.Focus();
                    txtMLOTID.Text = string.Empty;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion
        
        #region  공급요청 : btnSupply_Click()
        // 불량 Cell 삭제
        private void btnSupply_Click(object sender, RoutedEventArgs e)
        {
            if (!Validation())
            {
                return;
            }
       
        }

        #endregion

        #region [닫기]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion


        #endregion

        #region Mehod

        #region Validation()
        // <summary>
        /// Validation
        /// </summary>
        private bool Validation()
        {
          

                return false;
        }




        #endregion

        #endregion
    }
}