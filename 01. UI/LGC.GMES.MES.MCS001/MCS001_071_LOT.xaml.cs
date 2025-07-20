/*************************************************************************************
 Created Date : 2021.11.04
      Creator : 공민경
   Decription : VD 설비 SKID 공급 요청 이력 조회 - LOT 조회 팝업
--------------------------------------------------------------------------------------
 [Change History]
    2021.11.04  공민경 선임 : 신규 생성
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace LGC.GMES.MES.MCS001
{
	public partial class MCS001_071_LOT : C1Window, IWorkArea
	{
        private readonly Util _util = new Util();

        private string _REQUEST_ID = string.Empty;  //요청 ID

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public MCS001_071_LOT()
		{
			InitializeComponent();
		}

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] parameters = C1WindowExtension.GetParameters(this);

            if (parameters != null)
            {
                _REQUEST_ID = parameters[0] as string; //요청 ID

                txtRequestID.Text = _REQUEST_ID;
            }

            SelectLotList();
            this.Loaded -= C1Window_Loaded;

        }

        private void SelectLotList()
        {
            try
            {
                ShowLoadingIndicator();

                const string bizRuleName = "DA_MCS_SEL_VD_EQPT_SPLY_REQ_LOT_NJ";

                DataTable inTable = new DataTable("INDATA");
                inTable.Columns.Add("SPLY_REQ_ID", typeof(string));

                DataRow dr = inTable.NewRow();
                dr["SPLY_REQ_ID"] = _REQUEST_ID;
                inTable.Rows.Add(dr);

                new ClientProxy().ExecuteService(bizRuleName, "RQSTDT", "RSLTDT", inTable, (bizResult, bizException) =>
                {
                    HiddenLoadingIndicator();
                    if (bizException != null)
                    {
                        Util.MessageException(bizException);
                        return;
                    }

                    Util.GridSetData(dgList, bizResult, null, true);

                    _util.SetDataGridMergeExtensionCol(dgList, new string[] { "CSTID" }, DataGridMergeMode.VERTICALHIERARCHI);
                });

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
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

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}