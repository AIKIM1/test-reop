/*************************************************************************************
 Created Date : 2022.12.13
      Creator : 
   Decription : Lot별 등급 수량
--------------------------------------------------------------------------------------
 [Change History]
  2022.12.13  DEVELOPER : Initial Created.
 
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Windows.Controls;
using System.Windows;
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_073_GRADE_DETAIL : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _sLotID;
        private string _sEqptID;
        private string _sGradeCd;

        public FCS002_073_GRADE_DETAIL()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize    
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _sLotID = tmps[0] as string;
            _sEqptID = tmps[1] as string;
            _sGradeCd = tmps[2] as string;

            txtLot.Text = _sLotID;
            txtGrade.Text = _sGradeCd;

            //조회함수
            GetList();
        }
        
        #endregion

        #region Mehod

        private void GetList()
        {
            try
            {
                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";
                dtRqst.Columns.Add("LOTID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("GRADE_CD", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LOTID"] = _sLotID;
                dr["EQPTID"] = _sEqptID;
                dr["GRADE_CD"] = _sGradeCd;
                dtRqst.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_SEL_LOAD_CELL_DATA_MB", "RQSTDT", "RSLTDT", dtRqst);

                DataTable dt = new DataTable();
                dt.TableName = "DT";
                dt.Columns.Add("CSTSLOT1", typeof(string));
                dt.Columns.Add("GRADE_CNT1", typeof(string));
                dt.Columns.Add("CSTSLOT2", typeof(string));
                dt.Columns.Add("GRADE_CNT2", typeof(string));


                if (dtRslt.Rows.Count > 0)
                {
                    int iROWCNT = dtRslt.Rows.Count / 2;

                    for (int i = 0; i < iROWCNT; i++)
                    {
                        DataRow row = dt.NewRow();
                        row["CSTSLOT1"] = Util.NVC(dtRslt.Rows[i]["CSTSLOT"].ToString());
                        row["GRADE_CNT1"] = Util.NVC(dtRslt.Rows[i]["GRADE_CNT"].ToString());

                        if(i + iROWCNT < dtRslt.Rows.Count)
                        {
                            row["CSTSLOT2"] = Util.NVC(dtRslt.Rows[i + iROWCNT]["CSTSLOT"].ToString());
                            row["GRADE_CNT2"] = Util.NVC(dtRslt.Rows[i + iROWCNT]["GRADE_CNT"].ToString());
                        }

                        dt.Rows.Add(row);
                    }
                }
                
                Util.GridSetData(dgLotGradeQty, dt, FrameOperation, true);
            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
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
        #endregion

    }
}
