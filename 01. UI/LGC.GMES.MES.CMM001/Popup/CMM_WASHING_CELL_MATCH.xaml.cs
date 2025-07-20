/*************************************************************************************
 Created Date : 2019.10.04
      Creator : Lee sang jun
   Decription : 전지 5MEGA-GMES 구축 - 소형조립 공정진척(Assembly용) Washing Lot 으로 등록된 마지막 Cell ID 조회 
--------------------------------------------------------------------------------------
 [Change History]
  2019.10.21  이상준 : Initial Created.
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
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_WASHING_CELL_MATCH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WASHING_CELL_MATCH : C1Window, IWorkArea
    {
        #region Declaration & Constructor         
        
        private string _previewQtyValue = string.Empty;
        private readonly BizDataSet _bizDataSet = new BizDataSet();
        private bool _load = true;

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
        public CMM_WASHING_CELL_MATCH()
        {
            InitializeComponent();
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
                if (_load)
                {
                    SetControl();
                    _load = false;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetControl()
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);
            
                if (!Util.NVC(tmps[5]).Equals(""))
                {
                    txtCellID.Text = Util.NVC(tmps[5]);
                }
                txtBarcode.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveCellMatch();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Mehod

        #region [BizCall]

        private void SaveCellMatch()
        {
            try
            {
                ShowLoadingIndicator();

                if (txtCellID.Text == null || txtCellID.Text == "")
                {
                    // SFU3552 저장 할 DATA가 없습니다.
                    Util.MessageValidation("SFU3552");
                    return;
                }

                string bizRuleName = "BR_PRD_REG_CELL_SCAN_WS";

                object[] tmps = C1WindowExtension.GetParameters(this);

                //Cell Match 검사 정보 저장
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");

                inTable.Columns.Add("SUBLOTID", typeof(string));
                inTable.Columns.Add("SCAN_SUBLOTID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("CSTID", typeof(string));
                inTable.Columns.Add("CSTSLOT", typeof(decimal));
                inTable.Columns.Add("SCAN_USERID", typeof(string));

                DataRow newRow = inTable.NewRow();
                newRow["SUBLOTID"] = Util.NVC(tmps[5]);
                newRow["SCAN_SUBLOTID"] = txtBarcode.Text.Trim();
                newRow["LOTID"] = Util.NVC(tmps[3]);
                newRow["CSTID"] = Util.NVC(tmps[1]);                
                newRow["CSTSLOT"] = Util.NVC(tmps[4]);
                newRow["SCAN_USERID"] = Util.NVC(tmps[6]);


                inTable.Rows.Add(newRow);


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA", (result, bizException) =>
                {
                    HiddenLoadingIndicator();
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        if (result != null && result.Tables["OUTDATA"].Rows.Count > 0)
                        {
                            if (result.Tables["OUTDATA"].Rows[0]["RESULT"].ToString() == "OK")
                            {   //Util.AlertInfo("일치 : CELL ID 일치합니다.");
                                Util.MessageInfo("SFU8244");

                                this.DialogResult = MessageBoxResult.OK;
                            }
                            else
                            {   //Util.AlertInfo("불일치 : CELL ID 불일치합니다.");
                                Util.MessageInfo("SFU8245");

                                this.DialogResult = MessageBoxResult.OK;
                            }
         
                        }                                            
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }


        #endregion

        #region [Validation]


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

        private void txtBarcode_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.LeftCtrl)
                {
                    Util.MessageValidation("SFU3180");  //복사 및 붙여넣기 금지.
                    return;
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }

        }

        private void txtBarcode_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Util.MessageValidation("SFU3180");  //복사 및 붙여넣기 금지.
            txtBarcode.IsEnabled = false;
            txtBarcode.IsEnabled = true;
            return;
        }

        private void txtBarcode_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Util.MessageValidation("SFU3180");  //복사 및 붙여넣기 금지.
            txtBarcode.IsEnabled = false;
            txtBarcode.IsEnabled = true;
            return;

        }
    }
}
