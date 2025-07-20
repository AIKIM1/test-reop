
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ASSY001
{
    /// <summary>
    /// ASSY001_011_DUMMY_LOT_CHG.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_011_DUMMY_LOT_CHG : C1Window, IWorkArea
    {
        public ASSY001_011_DUMMY_LOT_CHG()
        {
            InitializeComponent();
        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                ApplyPermissions();

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CanSave())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.DialogResult = MessageBoxResult.Cancel;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();

            listAuth.Add(btnSave);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }
        
        private bool CanSave()
        {
            bool bRet = false;

            

            bRet = true;

            return bRet;
        }

        private void ChgCarrier()
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("SRCTYPE", typeof(string));
                inDataTable.Columns.Add("IFMODE", typeof(string));
                inDataTable.Columns.Add("PROCID", typeof(string));
                inDataTable.Columns.Add("FROM_CSTID", typeof(string));
                inDataTable.Columns.Add("TO_LOTID", typeof(string));
                inDataTable.Columns.Add("TO_CSTID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow param = inDataTable.NewRow();
                //param["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                //param["IFMODE"] = IFMODE.IFMODE_OFF;
                //param["PROCID"] = _Procid;
                //param["FROM_CSTID"] = txtFromCstIdHidden.Text;
                //param["TO_LOTID"] = txtToLotId.Text;
                //param["TO_CSTID"] = txtToCstIdHidden.Text;
                //param["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(param);

                new ClientProxy().ExecuteService("", "INDATA", null, inDataTable, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");      //정상 처리 되었습니다.                        
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                Util.MessageException(ex);
            }
        }
    }
}
