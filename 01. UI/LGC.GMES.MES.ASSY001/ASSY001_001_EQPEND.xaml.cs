/*************************************************************************************
 Created Date : 2016.08.23
      Creator : INS 김동일K
   Decription : 전지 5MEGA-GMES 구축 - Notching 공정진척 화면 - 장비완료 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2016.08.23  INS 김동일K : Initial Created.
  
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
    /// ASSY001_001_EQPEND.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY001_001_EQPEND : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipStat = string.Empty;
        private string _ProdID = string.Empty;
        private string _MountPstsID = string.Empty;

        DateTime dtNowTime;

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

        public ASSY001_001_EQPEND()
        {
            InitializeComponent();
        }

        private void InitializeControls()
        {
            if (dgProd.ItemsSource != null && dgProd.Rows.Count > 0 && DateTime.TryParse(Util.NVC(DataTableConverter.GetValue(dgProd.Rows[dgProd.Rows.Count - 1].DataItem, "DTTM_NOW")), out dtNowTime))
            {
            }
            else
            {
                dtNowTime = System.DateTime.Now;
            }
            if (ldpDatePicker != null)
                ldpDatePicker.SelectedDateTime = dtNowTime;
            if (teTimeEditor != null)
                teTimeEditor.Value = new TimeSpan(dtNowTime.Hour, dtNowTime.Minute, dtNowTime.Second);
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 6)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _LotID = Util.NVC(tmps[2]);
                _WipStat = Util.NVC(tmps[3]);
                _ProdID = Util.NVC(tmps[4]);
                _MountPstsID = Util.NVC(tmps[5]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _LotID = "";
                _WipStat = "";
                _ProdID = "";
                _MountPstsID = "";
            }
            ApplyPermissions();
            
            GetInputLotInfo();
            GetResultLotInfo();

            InitializeControls();
        }

        private void btnEqpend_Clicked(object sender, RoutedEventArgs e)
        {
            if (!CanEqpend())
                return;
                        
            Util.MessageConfirm("SFU1865", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    EqpEnd();                    
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void txtOutQty_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutQty.Text, 0))
                {
                    txtOutQty.Text = "";
                    return;
                }

                if (e.Key == Key.Enter)
                {
                    txtQtyChange();
                }
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region Mehod

        #region [BizCall]

        private void GetInputLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_INPUTLOT_INFO_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["LOTID"] = _LotID;
                newRow["EQPTID"] = _EqptID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_INPUTLOT_INFO_NT", "INDATA", "OUTDATA", inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        //dgInProd.ItemsSource = DataTableConverter.Convert(searchResult);
                        Util.GridSetData(dgInProd, searchResult, FrameOperation, true);
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

        private void GetResultLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = _Biz.GetDA_PRD_SEL_RSLTLOT_INFO_NT();

                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["PR_LOTID"] = _LotID;
                newRow["PROCID"] = Process.NOTCHING;

                inTable.Rows.Add(newRow);

                DataTable searchResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_RSLTLOT_INFO_NT", "INDATA", "OUTDATA", inTable);

                Util.GridSetData(dgProd, searchResult, FrameOperation, true);
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

        private void EqpEnd()
        {
            try
            {
                ShowLoadingIndicator();

                DateTime dtTime;
                TimeSpan spn;
                if (teTimeEditor.Value.HasValue)
                {
                    spn = ((TimeSpan)teTimeEditor.Value);
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                        spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                }
                else
                {
                    dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day);
                }

                DataSet indataSet = _Biz.GetBR_PRD_REG_EQPEND_NT();
                DataTable inTable = indataSet.Tables["IN_EQP"];

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["END_DTTM"] = dtTime;

                inTable.Rows.Add(newRow);
                newRow = null;

                DataTable inMtrlTable = indataSet.Tables["IN_INPUT"];
                newRow = inMtrlTable.NewRow();
                newRow["INPUT_LOTID"] = Util.NVC(DataTableConverter.GetValue(dgInProd.Rows[dgInProd.Rows.Count - 1].DataItem, "LOTID"));
                newRow["INPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgProd.Rows[dgProd.Rows.Count - 1].DataItem, "WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgProd.Rows[dgProd.Rows.Count - 1].DataItem, "WIPQTY")));
                newRow["OUTPUT_QTY"] = Util.NVC(DataTableConverter.GetValue(dgProd.Rows[dgProd.Rows.Count - 1].DataItem, "WIPQTY")).Equals("") ? 0 : double.Parse(Util.NVC(DataTableConverter.GetValue(dgProd.Rows[dgProd.Rows.Count - 1].DataItem, "WIPQTY")));

                newRow["EQPT_MOUNT_PSTN_ID"] = _MountPstsID;
                newRow["EQPT_MOUNT_PSTN_STATE"] = "A";

                inMtrlTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_EQPT_END_LOT_NT_UI", "IN_EQP,IN_INPUT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.

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
                }, indataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [Validation]

        private bool CanEqpend()
        {
            bool bRet = false;
            
            if (dgInProd.Rows.Count - dgInProd.TopRows.Count < 1)
            {
                Util.MessageValidation("SFU1945");
                return bRet;
            }

            if(dgProd.Rows.Count < 1)
            {
                //Util.Alert("실적 LOT이 없습니다.");
                Util.MessageValidation("SFU1703");
                return bRet;
            }

            if (_MountPstsID.Equals(""))
            {
                Util.MessageValidation("SFU1543");
                return bRet;
            }

            double dTmp = 0;
            double.TryParse(Util.NVC(DataTableConverter.GetValue(dgProd.Rows[dgProd.Rows.Count - 1].DataItem, "WIPQTY")), out dTmp);

            if (dTmp < 1)
            {
                Util.MessageValidation("SFU1802");  //입력 수량이 잘못 되었습니다.
                return bRet;
            }
            
            bRet = true;

            return bRet;
        }


        #endregion

        #region [Func]

        private void txtQtyChange()
        {
            if (!Util.CheckDecimal(txtOutQty.Text, 0))
            {
                txtOutQty.Text = "";
                return;
            }

            double rsltQty = double.Parse(txtOutQty.Text);
            double inputQtyEA = dgInProd.Rows.Count > 0 ? double.Parse(Util.NVC(DataTableConverter.GetValue(dgInProd.Rows[dgInProd.Rows.Count - 1].DataItem, "WIPQTY_EA"))) : 0;
            double inputQtyM = dgInProd.Rows.Count > 0 ? double.Parse(Util.NVC(DataTableConverter.GetValue(dgInProd.Rows[dgInProd.Rows.Count - 1].DataItem, "WIPQTY"))) : 0;
            
            for (int i = 0; i < dgProd.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgProd.Rows[i].DataItem, "WIPQTY", txtOutQty.Text);
            }

            txtOutQty.Text = "";
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnEqpend);

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

        private void ldpDatePicker_SelectedDataTimeChanged(object sender, SelectionChangedEventArgs e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    LGCDatePicker dtPik = (sender as LGCDatePicker);

                    DateTime dtTime;
                    TimeSpan spn;
                    if (teTimeEditor.Value.HasValue && ldpDatePicker != null)
                    {
                        spn = ((TimeSpan)teTimeEditor.Value);
                        dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                            spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                    }
                    else
                    {
                        dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day);
                    }

                    string sStartDTTM = dgProd.Rows.Count > 0 ? Util.NVC(DataTableConverter.GetValue(dgProd.Rows[0].DataItem, "WIPDTTM_ST")) : "";
                    if (!sStartDTTM.Equals("") && dtTime != null)
                    {
                        double dMinute = Math.Truncate(dtTime.Subtract(DateTime.Parse(sStartDTTM)).TotalSeconds);

                        if (dMinute < 0)
                        {
                            //Util.Alert("시작시간보다 이전은 선택할 수 없습니다.");
                            Util.MessageValidation("SFU3089");
                            // 시작시간보다 작으면 초기화.
                            InitializeControls();                            
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void teTimeEditor_ValueChanged(object sender, C1.WPF.DateTimeEditors.NullablePropertyChangedEventArgs<TimeSpan> e)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    if (sender == null)
                        return;

                    C1.WPF.DateTimeEditors.C1TimeEditor dtPik = (sender as C1.WPF.DateTimeEditors.C1TimeEditor);

                    DateTime dtTime;
                    TimeSpan spn;
                    if (teTimeEditor.Value.HasValue && ldpDatePicker != null)
                    {
                        spn = ((TimeSpan)teTimeEditor.Value);
                        dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day,
                            spn.Hours, spn.Minutes, spn.Seconds, DateTimeKind.Local);
                    }
                    else
                    {
                        dtTime = new DateTime(ldpDatePicker.SelectedDateTime.Year, ldpDatePicker.SelectedDateTime.Month, ldpDatePicker.SelectedDateTime.Day);
                    }

                    string sStartDTTM = dgProd.Rows.Count > 0 ? Util.NVC(DataTableConverter.GetValue(dgProd.Rows[0].DataItem, "WIPDTTM_ST")) : "";

                    if (!sStartDTTM.Equals("") && dtTime != null)
                    {
                        double dMinute = Math.Truncate(dtTime.Subtract(DateTime.Parse(sStartDTTM)).TotalSeconds);

                        if (dMinute < 0)
                        {
                            //Util.Alert("시작시간보다 이전은 선택할 수 없습니다.");
                            Util.MessageValidation("SFU3089");
                            // 시작시간보다 작으면 초기화.
                            InitializeControls();
                        }
                    }
                }
                catch (Exception ex)
                {
                }
            }));
        }

        private void txtOutQty_LostFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!Util.CheckDecimal(txtOutQty.Text, 0))
                {
                    txtOutQty.Text = "";
                    return;
                }

                txtQtyChange();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
    }
}
