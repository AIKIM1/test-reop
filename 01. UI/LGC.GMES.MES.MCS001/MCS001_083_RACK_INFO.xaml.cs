/*************************************************************************************
 Created Date : 2023.05.25
      Creator : 오화백
   Decription : STACKING 완성 창고 RACK LIST [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2023.05.25  오화백 : Initial Created.    
**************************************************************************************/
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
using System.Linq;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.UserControls;
using System.Windows.Data;
using System.Windows.Media.Animation;
namespace LGC.GMES.MES.MCS001
{
    public partial class MCS001_083_RACK_INFO : C1Window, IWorkArea
    {

        #region Declaration & Constructor 
    
        private string _areaCode;
        private string _eqgrid;
        private string _cfgAreaCode;
        private string _eqptid;
        private string _rackid;

        public MCS001_083_RACK_INFO()
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
            ApplyPermissions();

            object[] parameters = C1WindowExtension.GetParameters( this );

            //팝업 Parameters 셋팅
            if (parameters != null)
            {
              _areaCode = Util.NVC(parameters[0]);
              _eqgrid = Util.NVC(parameters[1]);
              _cfgAreaCode = Util.NVC(parameters[2]);
              _eqptid = Util.NVC(parameters[3]);
              _rackid = Util.NVC(parameters[4]);
              SelectRackInfo();
            }

        }

        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        #endregion

        #region Event
        #region 닫기 : btnClose_Click()
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region 그룹 Carrier 기준으로 머지 : dgList_MergingCells()
        /// <summary>
        /// 그룹 Carrier 기준으로 머지
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dgList_MergingCells(object sender, DataGridMergingCellsEventArgs e)
        {
            try
            {
                int idxS = 0;
                int idxE = 0;
                bool bStrt = false;
                string sTmpLvCd = string.Empty;
                string sTmpTOTALQTY = string.Empty;

                for (int i = dgList.TopRows.Count; i < dgList.Rows.Count; i++)
                {

                    if (dgList.Rows[i].DataItem.GetType() == typeof(System.Data.DataRowView))
                    {

                        if (!bStrt)
                        {
                            bStrt = true;
                            sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SKID_ID"));
                            idxS = i;

                            if (sTmpLvCd.Equals(""))
                                bStrt = false;
                        }
                        else
                        {
                            if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SKID_ID")).Equals(sTmpLvCd))
                            {
                                idxE = i;
                                //마지막 Row 일경우
                                if (i == dgList.Rows.Count - 1)
                                {
                                    if (idxS > idxE)
                                    {
                                        idxE = idxS;
                                    }
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["ROW_NUM"].Index), dgList.GetCell(idxE, dgList.Columns["ROW_NUM"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["RACK_ID"].Index), dgList.GetCell(idxE, dgList.Columns["RACK_ID"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["RACK_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["RACK_NAME"].Index)));
                                    e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["SKID_ID"].Index), dgList.GetCell(idxE, dgList.Columns["SKID_ID"].Index)));
                                }
                            }
                            else
                            {
                                if (idxS > idxE)
                                {
                                    idxE = idxS;
                                }
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["ROW_NUM"].Index), dgList.GetCell(idxE, dgList.Columns["ROW_NUM"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["RACK_ID"].Index), dgList.GetCell(idxE, dgList.Columns["RACK_ID"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["RACK_NAME"].Index), dgList.GetCell(idxE, dgList.Columns["RACK_NAME"].Index)));
                                e.Merge(new DataGridCellsRange(dgList.GetCell(idxS, dgList.Columns["SKID_ID"].Index), dgList.GetCell(idxE, dgList.Columns["SKID_ID"].Index)));
                                bStrt = true;
                                sTmpLvCd = Util.NVC(DataTableConverter.GetValue(dgList.Rows[i].DataItem, "SKID_ID"));
                                idxS = i;

                                if (sTmpLvCd.Equals(""))
                                    bStrt = false;
                            }
                        }
                    }

                }


            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
            }
        }


        #endregion

        #endregion

        #region Mehod

        #region 리스트 조회 : SelectRackInfo()
        /// <summary>
        /// 리스트 조회
        /// </summary>

        private void SelectRackInfo()
        {
            const string bizRuleName = "BR_MHS_SEL_STO_INVENT_LIST_NORM_USING_CST";
            try
            {
                ShowLoadingIndicator();
                DataTable inTable = new DataTable("RQSTDT");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQGRID", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE_LOT", typeof(string));
                inTable.Columns.Add("ELTR_TYPE_CODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("WIPHOLD", typeof(string));
                inTable.Columns.Add("QMS_HOLD_FLAG", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("LOTID", typeof(string));
                inTable.Columns.Add("RACK_ID", typeof(string));

                DataRow dr = inTable.NewRow();

                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaCode;
                dr["EQGRID"] = _eqgrid;
                dr["CFG_AREA_ID"] = _cfgAreaCode;
                dr["EQPTID"] = _eqptid;
                dr["RACK_ID"] = _rackid;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }


                    if (bizResult.Rows.Count > 0)
                    {
                        DataTable GrTray = bizResult.Clone();
                        List<string> sIdList = bizResult.AsEnumerable().Select(c => c.Field<string>("SKID_ID")).Distinct().ToList();

                        Int32 _Rownum = 1;
                        foreach (string id in sIdList)
                        {
                            DataRow drIndata = GrTray.NewRow();
                            drIndata["ROW_NUM"] = _Rownum;
                            drIndata["SKID_ID"] = id;
                            GrTray.Rows.Add(drIndata);
                            _Rownum = _Rownum + 1;
                        }

                        for (int i = 0; i < GrTray.Rows.Count; i++)
                        {
                            for (int j = 0; j < bizResult.Rows.Count; j++)
                            {
                                if (GrTray.Rows[i]["SKID_ID"].ToString() == bizResult.Rows[j]["SKID_ID"].ToString())
                                {
                                    bizResult.Rows[j]["ROW_NUM"] = GrTray.Rows[i]["ROW_NUM"];
                                }
                            }
                        }
                    }

                    Util.GridSetData(dgList, bizResult, null, true);

                    dgList.MergingCells -= dgList_MergingCells;
                    dgList.MergingCells += dgList_MergingCells;

                });
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }


        #endregion

        #region 상태바  : ShowLoadingIndicator(), HiddenLoadingIndicator()
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