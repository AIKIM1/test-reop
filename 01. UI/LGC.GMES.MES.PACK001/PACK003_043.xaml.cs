/*************************************************************************************
 Created Date : 2023.05.02
      Creator : 김선준
   Decription : Partial ILT Rack/Lot Info
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.02  김선준 : Initial Created.
***************************************************************************************/

using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    public partial class PACK003_043 : UserControl, IWorkArea
    {
        #region #1. Member Variable Lists... 
        LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM ms = new LGC.GMES.MES.PACK001.Class.MESSAGE_PARAM();
        Util util = new Util();

        /// <summary>
        /// Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        #endregion #1. 

        #region #2. Declaration & Constructor
        public PACK003_043()
        {
            InitializeComponent();
        }
        #endregion #2. Declaration & Constructor

        #region #3. UserControl_Loaded
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                this.txtScan.Clear(); 
                Util.gridClear(dgList);

                CommonCombo.SetDataGridComboItem("DA_BAS_SEL_AREA_COM_CODE_BY_SORT", new string[] { "LANGID", "AREAID", "COM_TYPE_CODE" }, 
                    new string[] { LoginInfo.LANGID, LoginInfo.CFG_AREA_ID, "ILT_MNG_DESC_CODE" }, CommonCombo.ComboStatus.NONE, dgList.Columns["ILT_MNG_DESC_CODE"], "CBO_CODE", "CBO_CODE_NAME");

                txtScan.Focus();
                this.Loaded -= new System.Windows.RoutedEventHandler(this.UserControl_Loaded);
            }
            catch (Exception ex)
            {
                Util.Alert(ex.ToString());
            }
        }
        #endregion #3. UserControl_Loaded

        #region #5. Event  
        /// <summary>
        /// 자재 Box 보류적재 이력 조회
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Util.gridClear(dgList);
            this.SearchProcess();
        }

        private void txtScan_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SearchProcess();
            }
        }

        void checkAllLEFT_Checked(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < dgList.Rows.Count; i++)
            {
                DataTableConverter.SetValue(dgList.Rows[i].DataItem, "CHK", true);
            }
        }

        void checkAllLEFT_Unchecked(object sender, RoutedEventArgs e)
        {
            Util.DataGridCheckAllUnChecked(dgList);
        }
        #endregion #5. Event

        #region Function 조회
        // 조회 - 
        private void SearchProcess()
        {
            string sScan = txtScan.Text.Trim();
            if (string.IsNullOrWhiteSpace(sScan))
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU2060"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                {
                    this.txtScan.Clear();
                    this.txtScan.Focus();
                });
                return;
            }

            this.loadingIndicator.Visibility = Visibility.Visible;
            try
            {
                DataSet dsInput = new DataSet();

                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LANGID", typeof(string));
                INDATA.Columns.Add("AREAID", typeof(string));
                INDATA.Columns.Add("SCAN", typeof(string));

                DataRow dr = INDATA.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["SCAN"] = sScan;
                INDATA.Rows.Add(dr);
                dsInput.Tables.Add(INDATA);

                DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_PARTIAL_ILT_LOT_LIST", "INDATA", "OUT_LOTLIST", dsInput, null);

                Util.GridSetData(this.dgList, dsResult.Tables["OUT_LOTLIST"], FrameOperation, true);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            }
            finally
            {
                this.txtScan.Clear();
                txtScan.Focus();
                this.loadingIndicator.Visibility = Visibility.Collapsed;
            } 
        }
        #endregion

        #region 저장
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            DataTable dtData = DataTableConverter.Convert(dgList.ItemsSource);
            if (dtData.Rows.Count == 0) return;

            var query = from sel in dtData.AsEnumerable()
                        where sel.Field<bool>("CHK") == true
                        select sel;

            if (null == query || query.AsDataView().Count == 0)
            {
                //SFU1651 : 선택된 항목이 없습니다.
                Util.MessageValidation("SFU1651"); 
                return;
            }

            //SFU1925 : 처리하시겠습니까?
            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1925"), null, "Info", MessageBoxButton.OKCancel, MessageBoxIcon.None, (Result) =>
            {
                if (Result == MessageBoxResult.OK)
                {
                    this.loadingIndicator.Visibility = Visibility.Visible;

                    try
                    {
                        DataSet indataSet = new DataSet();

                        DataTable inDataTable = indataSet.Tables.Add("INDATA");
                        inDataTable.Columns.Add("AREAID", typeof(string));
                        inDataTable.Columns.Add("GUBUN", typeof(string));
                        inDataTable.Columns.Add("USERID", typeof(string));

                        DataRow row = null;
                        row = inDataTable.NewRow();                        
                        row["AREAID"] = LoginInfo.CFG_AREA_ID;
                        row["GUBUN"] = "NG";
                        row["USERID"] = LoginInfo.USERID; 
                        inDataTable.Rows.Add(row);

                        //대상 LOT
                        DataTable inLot = indataSet.Tables.Add("INLOT"); 
                        inLot.Columns.Add("LOTID", typeof(string));
                        inLot.Columns.Add("ILT_MNG_DESC_CODE", typeof(string));
                        inLot.Columns.Add("NOTE", typeof(string)); 
 
                        foreach (DataRow item in query)
                        {
                            inLot.Rows.Add(item["LOTID"].ToString(), item["ILT_MNG_DESC_CODE"].ToString(), item["NOTE"].ToString());
                        }
                        
                        new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_UPD_PARTIAL_ILT_NG_RACK", "INDATA,INLOT", "", indataSet);
 
                        ms.AlertInfo("PSS9072");  // 처리가 완료되었습니다.    
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    { 
                        Util.gridClear(dgList);
                        Util.DataGridCheckAllUnChecked(dgList);
                         
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                }
            });
        }
        #endregion

    }
     
}