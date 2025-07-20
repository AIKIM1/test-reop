/*************************************************************************************
 Created Date : 2021.10.25
      Creator : 
   Decription : 권취 방향 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2021.10.25  조영대 : Initial Created. 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.CMM001
{
    public partial class CMM_ELEC_EM_SECTION_ROLL_DIRCTN : C1Window, IWorkArea
    {
        #region Initialize
        public IFrameOperation FrameOperation { get; set; }

        public CMM_ELEC_EM_SECTION_ROLL_DIRCTN()
        {
            InitializeComponent();
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplyPermissions();

                ClearControl();           

                SelectRollDirectionChange();

                txtSearchID.Focus();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region Event
        private void txtSearchID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && !txtSearchID.Equals(string.Empty))
            {
                SelectRollDirectionChange();
            }
        }
        
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            ClearControl();           
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
            {
                if (result == MessageBoxResult.OK)
                {
                    Save();
                }
            });
        }

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #region Method
        private void ClearControl()
        {
            txtRollDir.Text = ObjectDic.Instance.GetObjectName("WINDING_DIRCTN_CHANGE") + " : ";
            txtRollDirChange.Text = string.Empty;

            txtSearchID.Text = string.Empty;
            dgList.ClearRows();
        }

        private void SelectRollDirectionChange()
        {            
            DataTable IndataTable = new DataTable("RQSTDT");
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("SEARCHID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["SEARCHID"] = txtSearchID.Text.Trim();
            IndataTable.Rows.Add(Indata);

            new ClientProxy().ExecuteService("DA_PRD_SEL_ROLL_DIRCTN_CHANGE", "RQSTDT", "RSLTDT", IndataTable, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    if ( result == null || result.Rows.Count == 0)
                    {
                        if (txtSearchID.Text.Equals(string.Empty))
                        {
                            Util.GridSetData(dgList, result, FrameOperation, false);
                            return;
                        }

                        Util.MessageValidation("SFU3536");  //조회된 결과가 없습니다
                        txtSearchID.Clear();
                        return;
                    }

                    if (result.Rows.Count > 0)
                    {
                        DataRow drSelect = result.Rows[0];
                                                
                        if (Util.NVC(drSelect["LOTID"]).Equals(string.Empty) &&
                            Util.NVC(drSelect["CSTID"]).Equals(string.Empty))
                        {
                            Util.MessageValidation("SFU1886");  //정보가 없습니다.
                            txtSearchID.Clear();
                            return;
                        }

                        if (Util.NVC(drSelect["LOTID"]).Equals(string.Empty) &&
                           !Util.NVC(drSelect["CSTID"]).Equals(string.Empty))
                        {
                            Util.MessageValidation("SFU4566");  //해당 Carrier에 매핑된 Lot이 없습니다.
                            txtSearchID.Clear();
                            return;
                        }
                        
                        if (Util.NVC(drSelect["CSTID"]).Equals(string.Empty) &&
                           !Util.NVC(drSelect["LOTID"]).Equals(string.Empty))
                        {
                            Util.MessageValidation("SFU4564");  //캐리어 정보가 없습니다.
                            txtSearchID.Clear();
                            return;
                        }
                        
                        if (!Util.NVC(drSelect["WIPSTAT"]).Equals(Wip_State.WAIT))
                        {
                            Util.MessageValidation("SFU7004", Util.NVC(drSelect["LOTID"]));  //LOTID[%1]의 상태가 WAIT이 아닙니다.
                            txtSearchID.Clear();
                            return;
                        }

                        if (!Util.NVC(drSelect["CSTSTAT"]).Equals("U"))
                        {
                            Util.MessageValidation("SFU7002", Util.NVC(drSelect["CSTID"]), Util.NVC(drSelect["CSTSTAT_NAME"]));  //CSTID[%1] 이 상태가 %2 입니다.
                            txtSearchID.Clear(); 
                            return;
                        }
                    

                        DataTable dtGrid = DataTableConverter.Convert(dgList.ItemsSource);
                        if (dtGrid.AsEnumerable().Where(s => Util.NVC(s.Field<string>("CSTID")).Equals(Util.NVC(drSelect["CSTID"]))).Count() > 0)
                        {
                            Util.MessageValidation("SFU3471", Util.NVC(drSelect["CSTID"])); // [%1]은 이미 등록되었습니다.
                            return;
                        }

                        if (dtGrid.AsEnumerable().Where(s => Util.NVC(s.Field<string>("LOTID")).Equals(Util.NVC(drSelect["LOTID"]))).Count() > 0)
                        {
                            Util.MessageValidation("SFU3471", Util.NVC(drSelect["LOTID"])); // [%1]은 이미 등록되었습니다.
                            return;
                        }

                        if (Util.NVC(drSelect["ROLL_DIRCTN_NAME"]).Equals(string.Empty))
                        {
                            Util.MessageValidation("SFU9999", ObjectDic.Instance.GetObjectName("WINDING_DIRCTN"));  //%1  :  데이터가 없습니다.
                            txtSearchID.Clear();
                            return;
                        }

                        if (dtGrid.Rows.Count > 0 &&
                            dtGrid.AsEnumerable().Where(s => Util.NVC(s.Field<string>("ROLL_DIRCTN")).Equals(Util.NVC(drSelect["ROLL_DIRCTN"]))).Count().Equals(0))
                        {
                            Util.MessageValidation("SFU8290", ObjectDic.Instance.GetObjectName("WINDING_DIRCTN"), Util.NVC(drSelect["ROLL_DIRCTN_NAME"])); //%1 이(가) 맞지 않습니다. 확인해 주세요. - %2
                            return;
                        }

                        // MES 2.0 ItemArray 위치 오류 Patch
                        //dtGrid.Rows.Add(drSelect.ItemArray);
                        dtGrid.AddDataRow(drSelect);

                        Util.GridSetData(dgList, dtGrid, FrameOperation, false);

                        txtSearchID.Text = string.Empty;

                        if (dgList.Rows.Count == 1)
                        {
                            txtRollDir.Text = ObjectDic.Instance.GetObjectName("WINDING_DIRCTN_CHANGE") + " : " +
                            Util.NVC(drSelect["ROLL_DIRCTN_NAME"]) + " -> ";
                            txtRollDirChange.Text = Util.NVC(drSelect["CHANGE_ROLL_DIRCTN_NAME"]);
                        }
                    }
                }
                catch (Exception ex) { Util.MessageException(ex); }
            });
        }

        private void Save()
        {
            try
            {
                DataSet inData = new DataSet();

                DataTable inDataTable = inData.Tables.Add("IN_DATA");
                inDataTable.Columns.Add("LANGID", typeof(string));
                inDataTable.Columns.Add("LOTID", typeof(string));
                inDataTable.Columns.Add("HALF_SLIT_SIDE", typeof(string));
                inDataTable.Columns.Add("EM_SECTION_ROLL_DIRCTN", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));

                DataTable dt = dgList.GetDataTable();

                foreach (DataRow row in dt.Rows)
                {
                    DataRow newRow = inDataTable.NewRow();
                    newRow["LANGID"] = LoginInfo.LANGID;
                    newRow["LOTID"] = row["LOTID"];
                    newRow["HALF_SLIT_SIDE"] = null;
                    newRow["EM_SECTION_ROLL_DIRCTN"] = row["CHANGE_ROLL_DIRCTN"];
                    newRow["USERID"] = LoginInfo.USERID;
                    inDataTable.Rows.Add(newRow);
                }

                ShowLoadingIndicator();

                new ClientProxy().ExecuteService("BR_PRD_REG_LOT_SLIT_SIDE_ROLL_DIR_UI", "IN_DATA", null, inDataTable, (result, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                        
                        Util.MessageInfo("SFU1275"); // 정상처리되었습니다.

                        ClearControl();
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }
        #endregion

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
    }
}
