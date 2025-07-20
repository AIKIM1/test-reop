/*************************************************************************************
 Created Date : 2016.08.22
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching 공정진척 화면 - 교체처리 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.22  INS 김동일K : Initial Created.
  
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
    /// <summary>
    /// ASSY001_001_PAN_STATUS_CHG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_001_PAN_STATUS_CHG : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        //private string _LotID = string.Empty;

        Util _Util = new Util();
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

        public ASSY001_001_PAN_STATUS_CHG()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps.Length >= 2)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                //_LotID = Util.NVC(tmps[2]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                //_LotID = "";
            }

            //txtPancakeID.Text = _LotID;

            ApplyPermissions();
        }

        private void txtPancakeID_TextChanged(object sender, TextChangedEventArgs e)
        {
            //GetPancakeInfo();
        }

        private void txtQty_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Util.CheckDecimal(txtQty.Text, 0))
            {
                txtQty.Text = "";
                return;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SaveStatusChange();            
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]        

        private void SaveStatusChange()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetBR_PRD_REG_WIPSTATCHG_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = txtPancakeID.Text;
                newRow["OUTQTY"] = Convert.ToDecimal(txtQty.Text);

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        this.DialogResult = MessageBoxResult.OK;

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
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }

        }


        #endregion

        #region [Validation]

        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnSave);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

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
