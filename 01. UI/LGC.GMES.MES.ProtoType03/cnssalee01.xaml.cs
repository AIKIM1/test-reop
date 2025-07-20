/*************************************************************************************
 Created Date : 2016.09.19
      Creator : 이슬아D
   Decription : 대차 모니터링
--------------------------------------------------------------------------------------
 [Change History]
  2016.09.19  이슬아D : Initial Created.
  
**************************************************************************************/
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
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

namespace LGC.GMES.MES.ProtoType03
{

    /// <summary>
    /// cnssalee01.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class cnssalee01 : UserControl, IWorkArea
    {
        #region Variable 
        private static class RFID_Status
        {
            /// <summary>
            /// 맵핑
            /// </summary>
            public static readonly string MAPPING = "MAPPING";
            /// <summary>
            /// 맵핑 해제
            /// </summary>
            public static readonly string UNMAPPING = "UNMAPPING";
            /// <summary>
            /// 게이트 통과
            /// </summary>
            public static readonly string MOVING = "MOVING";
        }
        #endregion

        #region Initialize   
        public cnssalee01()
        {
            InitializeComponent();
        }

        #endregion
        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

       

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ApplyPermissions();
            InitControls();        
        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SetWaitCartList();
            SetMappingPouchLotList();
            SetMapplingCartPouch();
            SetRFIDInfo();
        }

        private void txtEQPTID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    txtCartID.Text = getCartID();
                    InitLotCombo();
                    SetWaitCartList();
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
        private void SetWaitCartList()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("CART_ID", typeof(string));

                DataRow newRow = inDataTable.NewRow();
                if (!string.IsNullOrEmpty(txtEQPTID.Text))
                    newRow["EQPTID"] = txtEQPTID.Text;
                if (!string.IsNullOrEmpty(txtCartID.Text))
                    newRow["CART_ID"] = txtCartID.Text;
                inDataTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_RTLS_PROD_WAIT_CART", "INDATA", "OUTDATA", inDataTable);

                dgWaitCartList.ItemsSource = DataTableConverter.Convert(searchResult);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(ex.Message, null, "Error", MessageBoxButton.OK, ControlsLibrary.MessageBoxIcon.Warning);
            }
        }

        private string getCartID()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("LANGID", typeof(string));
            RQSTDT.Columns.Add("EQPTID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQPTID"] = Util.GetCondition(txtEQPTID, "EQPTID 필수입니다.");
            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_RTLS_PROD_WAIT_CART", "RQSTDT", "RSLTDT", RQSTDT);

            return dtResult.AsEnumerable().Select(c => c.Field<string>("CART_ID")).FirstOrDefault();
        }

        private void InitControls()
        {
            txtEQPTID.Text = "EQPT1";
        }

        private void InitLotCombo()
        {
            try
            {
                cboLOTID.Text = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = Util.GetCondition(txtEQPTID, "EQPTID 필수입니다.");
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_WIP_BY_EQPTID", "RQSTDT", "RSLTDT", RQSTDT);
                cboLOTID.DisplayMemberPath = "LOTID";
                cboLOTID.SelectedValuePath = "LOTID";
                cboLOTID.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void cboLOTID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            txtPouchID.Text = string.Empty;
            SetMappingPouchLotList();
            SetMapplingCartPouch();
        }
        private void SetMappingPouchLotList()
        {
            try
            {             
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("POUCH_ID", typeof(string));
                RQSTDT.Columns.Add("LOTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if(!string.IsNullOrEmpty(txtPouchID.Text)) dr["POUCH_ID"] = txtPouchID.Text;
                if (!string.IsNullOrEmpty(cboLOTID.Text)) dr["LOTID"] = cboLOTID.SelectedValue;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_RTLS_POUCH_LOT", "RQSTDT", "RSLTDT", RQSTDT);
                dgMappingPouchLotList.ItemsSource = DataTableConverter.Convert(dtResult);
                txtPouchID.Text = dtResult.AsEnumerable().Select(c => c.Field<string>("POUCH_ID")).FirstOrDefault();                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }

        }

        private void MappingCartPouch()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("RFID_STAT", typeof(string)); 
                inDataTable.Columns.Add("POUCH_ID", typeof(string));
                inDataTable.Columns.Add("CART_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RFID_STAT"] = RFID_Status.MAPPING;
                dr["POUCH_ID"] = txtPouchID.Text;
                dr["CART_ID"] = txtCartID.Text;
                dr["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_RTLS_REG_BY_STATUS", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("맵핑 완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

                });
                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void UnMappingCartPouch()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("RFID_STAT", typeof(string));
                inDataTable.Columns.Add("POUCH_ID", typeof(string));
                inDataTable.Columns.Add("CART_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RFID_STAT"] = RFID_Status.UNMAPPING;
                dr["POUCH_ID"] = txtPouchID.Text;
                dr["CART_ID"] = txtCartID.Text;
                dr["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_RTLS_REG_BY_STATUS", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("맵핑해제 완료"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

                });              
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void MovingGate()
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("RFID_STAT", typeof(string));
                inDataTable.Columns.Add("POUCH_ID", typeof(string));
                inDataTable.Columns.Add("CART_ID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataRow dr = inDataTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RFID_STAT"] = RFID_Status.MOVING;
                dr["POUCH_ID"] = txtPouchID.Text;
                dr["CART_ID"] = txtCartID.Text;
                dr["USERID"] = LoginInfo.USERID;

                inDataTable.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_RTLS_REG_BY_STATUS", "INDATA", "RSLTDT", inDataTable, (result, ex) =>
                {
                    if (ex != null)
                    {
                        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("예외발생" + ex.ToString()), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("게이트 통과"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.Warning);

                });                
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetMapplingCartPouch()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("POUCH_ID", typeof(string));
                RQSTDT.Columns.Add("CART_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                if (!string.IsNullOrEmpty(txtPouchID.Text)) dr["POUCH_ID"] = txtPouchID.Text;
                if (!string.IsNullOrEmpty(txtCartID.Text)) dr["CART_ID"] = txtCartID.Text;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_RTLS_CART_POUCH", "RQSTDT", "RSLTDT", RQSTDT);
                dgMappingCartPouchList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }

        private void SetRFIDInfo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("RFID_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["RFID_ID"] = txtPouchID.Text + (txtPouchID.Text==string.Empty? txtCartID.Text : "," + txtCartID.Text);
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_RTLS_RFID_PSTN", "RQSTDT", "RSLTDT", RQSTDT);
                dgRFIDList.ItemsSource = DataTableConverter.Convert(dtResult);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnMapping_Click(object sender, RoutedEventArgs e)
        {
            MappingCartPouch();
            SetMapplingCartPouch();
        }

        private void btnUnMapping_Click(object sender, RoutedEventArgs e)
        {
            UnMappingCartPouch();
            SetMapplingCartPouch();
        }

        private void btnMoving_Click(object sender, RoutedEventArgs e)
        {
            MovingGate();
            SetRFIDInfo();
        }
    }
}
