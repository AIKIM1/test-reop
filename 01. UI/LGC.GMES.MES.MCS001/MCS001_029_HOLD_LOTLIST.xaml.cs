/*************************************************************************************
 Created Date : 2021.05.19
      Creator : 오화백
   Decription : HOLD Lot List [팝업]
--------------------------------------------------------------------------------------
 [Change History]
  2021.05.19  오화백 : Initial Created.    
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
    public partial class MCS001_029_HOLD_LOTLIST : C1Window, IWorkArea
    {

		#region Declaration & Constructor 

        private string _areaCode;
        private string _equipmentSegmentCode;
        private string _projectName;
        private string _type;


		public MCS001_029_HOLD_LOTLIST()
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

            if (parameters != null)
            {
                _areaCode = Util.NVC(parameters[0]);
                _equipmentSegmentCode = Util.NVC(parameters[1]);
                _projectName = Util.NVC(parameters[2]);
                _type = Util.NVC(parameters[3]);
             
                
                SelectHoldLotList();
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        #endregion

        #region Mehod

        private void SelectHoldLotList()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "BR_MCS_SEL_WAREHOUSE_SMM_HOLD_LOT_LIST";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("CFG_AREA_ID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PRJT_NAME", typeof(string));
                inTable.Columns.Add("TYPE", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = _areaCode;
                dr["CFG_AREA_ID"] = LoginInfo.CFG_AREA_ID;
                dr["EQSGID"] = _equipmentSegmentCode;
                dr["PRJT_NAME"] = _projectName;
                dr["TYPE"] = _type;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    DataTable dtBinding = bizResult.Copy();
                    dtBinding.Columns.Add(new DataColumn() { ColumnName = "ROWNUM", DataType = typeof(int) });
                    int rowIndex = 1;
                    foreach (DataRow row in dtBinding.Rows)
                    {
                        row["ROWNUM"] = rowIndex;
                        rowIndex++;
                    }
                    dtBinding.AcceptChanges();

                    Util.GridSetData(dgList, dtBinding, null, true);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        //private static DateTime GetSystemTime()
        //{
        //    DateTime systemDateTime = new DateTime();

        //    const string bizRuleName = "BR_CUS_GET_SYSTIME";
        //    DataTable inDataTable = new DataTable("INDATA");
        //    DataTable dtResult = new ClientProxy().ExecuteServiceSync(bizRuleName, "INDATA", "OUTDATA", inDataTable);

        //    if (CommonVerify.HasTableRow(dtResult))
        //    {
        //        systemDateTime = Convert.ToDateTime(dtResult.Rows[0][0]);
        //    }

        //    return systemDateTime;
        //}

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

    
    }
}