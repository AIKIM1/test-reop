/*************************************************************************************
 Created Date : 2023.09.08
      Creator : 백광영
   Decription : Cell 공급 수량 변경
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using Microsoft.Win32;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.CMM001;
using System.Collections;
using LGC.GMES.MES.COM001;
using System.Windows.Controls;
using C1.WPF.DataGrid;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK003_045_CHANGEQTY.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK003_045_CHANGEQTY : C1Window, IWorkArea
    {
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();

        string _sReqID = string.Empty;
        decimal _dReqQty = 0;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }

            _sReqID = Util.NVC(tmps[0]);
            _dReqQty = Util.NVC_Decimal(tmps[1]);

            txtSourceQty.Value = Convert.ToDouble(_dReqQty);

            this.Loaded -= C1Window_Loaded;
        }

        public PACK003_045_CHANGEQTY()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            if (txtTargetQty.Value == 0 || txtTargetQty.Value.ToString() == "NaN")
            {
                Util.MessageValidation("SFU1683");  // 수량은 0보다 커야 합니다.
                return;
            }

            // Pallet 수량 변경 처리하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ms.AlertRetun("SFU3652"), null, "Information", MessageBoxButton.OKCancel, MessageBoxIcon.None, (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    _ChangeQty();
                }
            });
        }

        private void _ChangeQty()
        {
            ShowLoadingIndicator();
            try
            {
                DataSet inDataSet = null;
                inDataSet = new DataSet();
                DataTable INDATA = inDataSet.Tables.Add("INDATA");
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("CELL_SPLY_REQ_ID", typeof(string));
                INDATA.Columns.Add("CHG_QTY", typeof(decimal));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CELL_SPLY_REQ_ID"] = _sReqID;
                dr["CHG_QTY"] = txtTargetQty.Value;
                dr["USERID"] = LoginInfo.USERID;
                INDATA.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("BR_PRD_UPD_CELL_SPLY_REQ_CHANGE_QTY", "INDATA", null, INDATA);
                this.DialogResult = MessageBoxResult.OK;
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                //Util.MessageInfo(ex.Message.ToString());
                Util.MessageInfo(ex.Data["CODE"].ToString());
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.OK;
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
    }
}
