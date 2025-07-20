/*************************************************************************************
 Created Date : 2016.06.16
      Creator : 
   Decription : 생산계획 등록
--------------------------------------------------------------------------------------
 [Change History]
  2016.06.16  DEVELOPER : Initial Created.


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
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.CMM001.Popup
{
    public partial class CMM_HOPR_SCLE_SOLD_CONT_RATE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        private string sEQPTID = string.Empty;
        private string sPROCID = string.Empty;
        DataTable dtHopper = new DataTable();
        DataTable dtiUse = new DataTable();


        public CMM_HOPR_SCLE_SOLD_CONT_RATE()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps == null)
            {
                this.DialogResult = MessageBoxResult.Cancel;
                this.Close();
            }
            sEQPTID = Util.NVC(tmps[0]);
            sPROCID = Util.NVC(tmps[1]);
            //txtPRod.Text = Util.NVC(tmps[0]);

            setDataTable();
            InitCombo();

            //const string bizRuleName = "DA_BAS_SEL_SLURRY_PRODUCT_PRJT_CBO";
            //string[] arrColumn = { "PRODID" };
            //string[] arrCondition = { null };
            //string selectedValueText = (string)((PopupFindDataColumn)dgList.Columns["PRODID"]).SelectedValuePath;
            //string displayMemberText = (string)((PopupFindDataColumn)dgList.Columns["PRODID"]).DisplayMemberPath;

            //SetFindGridCombo(bizRuleName, dgList.Columns["PRODID"], arrColumn, arrCondition, selectedValueText, displayMemberText);

            btnSearch_Click(null, null);

            ApplyPermissions();

            if (!IsPersonByAuth())
                btnSave.IsEnabled = false;

        }
        #endregion

        #region Event
        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            SetMixerScaleSolidcontRate();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                this.dgList.BeginNewRow();
                this.dgList.EndNewRow(true);

            }
            catch (Exception ex)
            {
                FrameOperation.PrintMessage(MessageDic.Instance.GetMessage(ex));
            }
        }
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            DataGrid01RowDelete(dgList);
        }
        #endregion
        #region Mehod
        private void DataGrid01RowDelete(C1.WPF.DataGrid.C1DataGrid dg)
        {
            try
            {
                if (dg.Rows.Count > 0)
                {
                    DataRowView drv = dg.SelectedItem as DataRowView;
                    if (drv != null && (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached))
                    {
                        if (dg.SelectedIndex > -1)
                        {
                            dg.EndNewRow(true);
                            dg.RemoveRow(dg.SelectedIndex);
                        }
                    }
                }
                else
                {
                    //삭제할 항목이 없습니다.
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1597"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None);
                    return;
                }
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                return;
            }
        }
        #endregion
        //private void txtProd_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.Key == Key.Enter)
        //    {
        //        if (string.IsNullOrEmpty(txtPRod.Text))
        //        {
        //            txtPRod.Focus();
        //            return;
        //        }
        //        btnSearch_Click(sender, e);
        //    }
        //}
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            GetMixerScaleSolidContRate();
        }
        private void GetMixerScaleSolidContRate()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = dtRqst.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQPTID"] = sEQPTID;

                dtRqst.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_MIXER_SCALE_SOLID_CONT_RATE", "RQSTDT", "RSLTDT", dtRqst);

                Util.GridSetData(dgList, dtMain, FrameOperation, true);
                (dgList.Columns["USE_FLAG"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtiUse.Copy());
                (dgList.Columns["HOPPER_SCALE"] as C1.WPF.DataGrid.DataGridComboBoxColumn).ItemsSource = DataTableConverter.Convert(dtHopper.Copy());
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
        private void setDataTable()
        {
            dtiUse = GetCommonCode("IUSE");
            dtHopper = GetHopperCombo();
        }
        private DataTable GetCommonCode(string sType)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["CMCDTYPE"] = sType;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }

        private DataTable GetHopperCombo()
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PROCID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["PROCID"] = sPROCID;
                dr["EQPTID"] = sEQPTID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_HOPPER_SCALE_CBO", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return dtResult;
            }
            catch (Exception ex) { }

            return new DataTable();
        }


        private void SetFindGridCombo(string bizRuleName, C1.WPF.DataGrid.DataGridColumn dgcol, string[] arrColumn, string[] arrCondition, string selectedValueText, string displayMemberText)
        {
            try
            {
                DataTable inDataTable = new DataTable();
                inDataTable.TableName = "RQSTDT";

                if (arrColumn != null)
                {
                    // 동적 컬럼 생성 및 Row 추가
                    foreach (string col in arrColumn)
                    {
                        inDataTable.Columns.Add(col, typeof(string));
                    }

                    DataRow dr = inDataTable.NewRow();
                    for (int i = 0; i < inDataTable.Columns.Count; i++)
                    {
                        dr[inDataTable.Columns[i].ColumnName] = arrCondition[i];
                    }
                    inDataTable.Rows.Add(dr);
                }

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "RQSTDT", "RSLTDT", inDataTable);

                DataTable dtBinding = dtResult.DefaultView.ToTable(false, new string[] { selectedValueText, displayMemberText });

                PopupFindDataColumn column = dgcol as PopupFindDataColumn;
                column.AddMemberPath = "PRODID";
                column.ItemsSource = DataTableConverter.Convert(dtBinding);



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void dgList_BeginningEdit(object sender, C1.WPF.DataGrid.DataGridBeginningEditEventArgs e)
        {
            DataRowView drv = e.Row.DataItem as DataRowView;

            if (drv["CHK"].ToString() != "1" && e.Column != this.dgList.Columns["CHK"])
            {
                e.Cancel = true;
                return;
            }
            if (drv.Row.RowState == DataRowState.Added || drv.Row.RowState == DataRowState.Detached)
            {
                e.Cancel = false;
            }
            else
            {
                if (e.Column != this.dgList.Columns["CHK"]
                 && e.Column != this.dgList.Columns["USE_FLAG"]
                 && e.Column != this.dgList.Columns["SOLID_CONT_RATE"])
                {
                    e.Cancel = true;
                }
                else
                {
                    e.Cancel = false;
                }
            }
        }
        private void dgList_BeginningNewRow(object sender, DataGridBeginningNewRowEventArgs e)
        {
            e.Item.SetValue("CHK", "1");
            e.Item.SetValue("USE_FLAG", "Y");
            e.Item.SetValue("AREAID", LoginInfo.CFG_AREA_ID);
            e.Item.SetValue("EQPTID", sEQPTID);
            e.Item.SetValue("INSDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("UPDDTTM", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            e.Item.SetValue("INSUSER", LoginInfo.USERNAME);
            e.Item.SetValue("UPDUSER", LoginInfo.USERNAME);
        }
        private bool IsPersonByAuth()
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("USERID", typeof(string));
                RQSTDT.Columns.Add("AUTHID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["USERID"] = LoginInfo.USERID;
                dr["AUTHID"] = "ELEC_MANA";
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_PERSON_BY_AUTH_MULTI", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult != null && dtResult.Rows.Count > 0)
                    return true;
            }
            catch (Exception ex) { }

            return false;
        }
        private void SetMixerScaleSolidcontRate()
        {
            Util.MessageConfirm("SFU1241", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    try
                    {
                        //ShowLoadingIndicator();

                        DataTable dt = ((DataView)dgList.ItemsSource).Table;
                        DataSet indataSet = new DataSet();

                        DataTable inDATA = indataSet.Tables.Add("INDATA");
                        inDATA.Columns.Add("AREAID");
                        inDATA.Columns.Add("EQPTID");
                        inDATA.Columns.Add("HOPPER_SCALE");
                        inDATA.Columns.Add("SOLID_CONT_RATE");
                        inDATA.Columns.Add("USE_FLAG");
                        inDATA.Columns.Add("INSUSER");
                        inDATA.Columns.Add("INSDTTM");
                        inDATA.Columns.Add("UPDUSER");
                        inDATA.Columns.Add("UPDDTTM");

                        foreach (DataRow inRow in dt.Rows)
                        {
                            if (Convert.ToBoolean(inRow["CHK"]))
                            {
                                //if (string.IsNullOrEmpty(Util.NVC(inRow["PRODID"])))
                                //{
                                //    Util.MessageValidation("SFU2949");  //제품ID를 입력하세요.
                                //    return;
                                //}
                                if (string.IsNullOrEmpty(Util.NVC(inRow["HOPPER_SCALE"])))
                                {
                                    Util.MessageValidation("SFU2087");  //계량기를 입력하세요.
                                    return;
                                }
                                if (string.IsNullOrEmpty(Util.NVC(inRow["SOLID_CONT_RATE"])))
                                {
                                    Util.MessageValidation("SFU2088");  //고형분 비율을 입력하세요.
                                    return;
                                }
                                DataRow newRow = inDATA.NewRow();
                                newRow["AREAID"] = Util.NVC(inRow["AREAID"]);
                                newRow["EQPTID"] = Util.NVC(inRow["EQPTID"]);
                                newRow["HOPPER_SCALE"] = Util.NVC(inRow["HOPPER_SCALE"]);
                                newRow["SOLID_CONT_RATE"] = Util.NVC(inRow["SOLID_CONT_RATE"]);
                                newRow["USE_FLAG"] = Util.NVC(inRow["USE_FLAG"]);
                                newRow["INSUSER"] = LoginInfo.USERID;
                                newRow["INSDTTM"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                newRow["UPDUSER"] = LoginInfo.USERID;
                                newRow["UPDDTTM"] = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); 
                                inDATA.Rows.Add(newRow);
                            }
                        }
                        if (inDATA.Rows.Count < 1)
                        {
                            Util.MessageValidation("MMD0002"); //저장할 데이터가 존재하지 않습니다.
                            return;
                        }

                        new ClientProxy().ExecuteService_Multi("DA_PRD_REG_TB_SFC_MIXER_SCALE_SOLID_CONT_RATE", "INDATA", null, (bizResult, bizException) =>
                        {
                            try
                            {
                                if (bizException != null)
                                {
                                    HiddenLoadingIndicator();
                                    Util.MessageException(bizException);
                                    return;
                                }

                                GetMixerScaleSolidContRate();
                                Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
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
                        Util.MessageException(ex);
                    }
                }
            });
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
