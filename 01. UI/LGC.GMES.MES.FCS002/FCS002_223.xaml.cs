/*************************************************************************************
 Created Date : 2023.05.26
      Creator : 최성필
   Decription : Pin 접촉/불량횟수
--------------------------------------------------------------------------------------
 [Change History]
  2023.10.19  DEVELOPER : Initial Created.





 
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.DataGrid.Summaries;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LGC.GMES.MES.CMM001;
using System.Reflection;
using System.Linq;
using System.Windows.Data;

namespace LGC.GMES.MES.FCS002
{
    public partial class FCS002_223 : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        private string[] temp = new string[4];
        private string sEQPKind = string.Empty;

        private string _sNotUseRowLIst;
        private string _sNotUseColLIst;

        public FCS002_223()
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
      

        private void InitCombo()
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            string[] sFilter1 = { "FORM_BOX_GR_MB" };
            _combo.SetCombo(cboEQPGR, CommonCombo_Form_MB.ComboStatus.SELECT, sFilter: sFilter1, sCase: "AREA_COMMON_CODE");


            if (temp[0] != null )
            {
                cboEQPGR.SelectedValue = temp[0];

                if (cboEQPGR.SelectedValue == null)
                    cboEQPGR.SelectedIndex = 0;
            }
        }

        private void SetLaneCombo(C1ComboBox cbo)
        {

            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();
            
            C1ComboBox[] cboLineParent = { cboEQPGR };
            _combo.SetCombo(cboLane, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "LANE_BY_EQGR", cbParent: cboLineParent);

            if (temp[1] != null)
            {
                cboLane.SelectedValue = temp[1];
                if (cboLane.SelectedValue == null)
                    cboLane.SelectedIndex = 0;
            }

            

        }

        private void cboEQPGR_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            // 임시
            switch (cboEQPGR.SelectedValue.ToString())
            {
                case "FOR":
                    sEQPKind = "1";
                    break;
                case "LCI":
                    sEQPKind = "L";
                    break;
                case "OCV":
                    sEQPKind = "8";
                    break;
                case "ORV":
                    sEQPKind = "I";
                    break;
            }

            SetLaneCombo(cboLane);
        }



        private void cboLane_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            CommonCombo_Form_MB _combo = new CommonCombo_Form_MB();

            object[] oParent = { cboLane, sEQPKind, "", "M" };
            _combo.SetComboObjParent(cboEQP, CommonCombo_Form_MB.ComboStatus.SELECT, sCase: "EQPIDBYLANEMB", objParent: oParent);

            if (temp[2] != null)
            {
                cboEQP.SelectedValue = temp[2];
                if (cboEQP.SelectedValue == null)
                    cboEQP.SelectedIndex = 0;
            }

        }

        #endregion

        #region Event


        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 1)
            {
                for (int i = 0; i < tmps.Length; i++)
                    if(tmps[i] != null)
                        temp[i] = tmps[i].ToString();
            }
            

            InitCombo();


            Getlist();
          
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
             Getlist();
        }


        
        private void dgPinCnt_LoadedRowHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridRowEventArgs e)
        {
            if (e.Row.HeaderPresenter == null)
            {
                return;
            }

            e.Row.HeaderPresenter.Content = null;

            TextBlock tb = new TextBlock();

            tb.Text = (e.Row.Index + 1 - dgPinCnt.TopRows.Count).ToString();
            tb.VerticalAlignment = VerticalAlignment.Center;
            tb.HorizontalAlignment = HorizontalAlignment.Center;
            e.Row.HeaderPresenter.Content = tb;
        }


        #endregion

        #region Mehod


        private void Getlist()
        {
           try
            {
                dgPinCnt.Refresh();

                Util.gridClear(dgPinCnt);

                string sBiz = "DA_SEL_BOX_PIN_CONTACT_COUNT_MB";

                DataTable dtRqst = new DataTable();
                dtRqst.TableName = "RQSTDT";

                dtRqst.Columns.Add("AREAID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("FROM_TIME", typeof(string));
                dtRqst.Columns.Add("TO_TIME", typeof(string));
                
                DataRow dr = dtRqst.NewRow();

                dr["AREAID"] = LoginInfo.CFG_AREA_ID; 
                dr["EQPTID"] = Util.GetCondition(cboEQP, sMsg: "FM_ME_0171");  //설비를 선택해주세요.
               

                if (string.IsNullOrEmpty(dr["EQPTID"].ToString())) return;
                dtRqst.Rows.Add(dr);
                
                ShowLoadingIndicator();

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync(sBiz, "RQSTDT", "RSLTDT", dtRqst);


                //dgPinCnt.ItemsSource = DataTableConverter.Convert(dtRslt);

                string CST_CELL_TYPE_CODE = string.Empty;
                if (dtRslt.Rows.Count > 0)
                {
                    txtLastRunTime.Text = dtRslt.Rows[0]["UPDDTTM"].ToString();
                    CST_CELL_TYPE_CODE = dtRslt.Rows[0]["CST_CELL_TYPE_CODE"].ToString();
                }
                else
                {
                    Util.MessageInfo("FM_ME_0232");
                    return;
                }


                InitializeDataGrid(CST_CELL_TYPE_CODE, dgPinCnt);

                int iCellNo = 1;
                int iCnt = 1;

                DataTable dt = DataTableConverter.Convert(dgPinCnt.ItemsSource);

                for (int iCol = 0; iCol < dgPinCnt.Columns.Count; iCol++)
                {
                    for (int iRow = 0; iRow < dgPinCnt.Rows.Count; iRow++)
                    {
                        if (!dt.Rows[iRow][iCol].ToString().Equals("NOT_USE")) //빈자리 띄우기 위해 체크
                        {
                            DataRow[] dr1 = dtRslt.Select("CH_NO = '" + iCellNo.ToString() + "'");

                            if (dr1.Length > 0)
                            {
                                dt.Rows[iRow][iCol] = Util.NVC(dr1[0]["CNT"]);
                                iCnt = Convert.ToInt32(dr1[0]["CNT"].ToString());
                            }
                            iCellNo++;
                        }
                    }
                }
                Util.GridSetData(dgPinCnt, dt, FrameOperation, false);

            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                HiddenLoadingIndicator();
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


        private DataTable GetTestData()
        {
            DataTable dtRslt = new DataTable();

            dtRslt.Columns.Add("EQPTID", typeof(string));
            dtRslt.Columns.Add("CH_NO", typeof(string));
            dtRslt.Columns.Add("CNT", typeof(string));

            DataRow drr = dtRslt.NewRow();
            drr["EQPTID"] = "E9FFOR31101-010";
            drr["CH_NO"] = "1";
            drr["CNT"] = "3";


            DataRow drr1 = dtRslt.NewRow();
            drr1["EQPTID"] = "E9FFOR31101-010";
            drr1["CH_NO"] = "15";
            drr1["CNT"] = "6";

            DataRow drr2 = dtRslt.NewRow();
            drr2["EQPTID"] = "E9FFOR31101-010";
            drr2["CH_NO"] = "81";
            drr2["CNT"] = "3";

            dtRslt.Rows.Add(drr);
            dtRslt.Rows.Add(drr1);
            dtRslt.Rows.Add(drr2);

            return dtRslt;
        }

        #endregion

       
 

        private void InitializeDataGrid(string sComCode, C1DataGrid dg)
        {
            try
            {
                _sNotUseRowLIst = string.Empty;
                _sNotUseColLIst = string.Empty;

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("COM_TYPE_CODE", typeof(string));
                RQSTDT.Columns.Add("COM_CODE", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                dr["COM_TYPE_CODE"] = "CST_CELL_TYPE_CODE";
                dr["COM_CODE"] = sComCode;

                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_TB_MMD_AREA_COM_CODE", "RQSTDT", "RSLTDT", RQSTDT);

                Util.gridClear(dg);

                if (dtRslt.Rows.Count > 0)
                {
                    int iColName = 65;
                    string sRowCnt = dtRslt.Rows[0]["ATTR1"].ToString();
                    string sColCnt = dtRslt.Rows[0]["ATTR2"].ToString();
                    _sNotUseRowLIst = dtRslt.Rows[0]["ATTR3"].ToString();
                    _sNotUseColLIst = dtRslt.Rows[0]["ATTR4"].ToString();

                    #region Grid 초기화
                    int iMaxCol;
                    int iMaxRow;
                    List<string> rowList = new List<string>();

                    int iColCount = dg.Columns.Count;
                    for (int i = 0; i < iColCount; i++)
                    {
                        int index = (iColCount - i) - 1;
                        dg.Columns.RemoveAt(index);
                    }

                    iMaxRow = Convert.ToInt16(sRowCnt);
                    iMaxCol = Convert.ToInt16(sColCnt);

                    List<DataTable> dtList = new List<DataTable>();

                    double AAA = Math.Round((dg.ActualWidth - 70) / (iMaxCol - 1), 1);
                    int iColWidth = int.Parse(Math.Truncate(AAA).ToString());

                    int iSeq = 1;
                    DataTable dt = new DataTable();
                    dt.TableName = "RQSTDT";

                    for (int iCol = 0; iCol < iMaxCol; iCol++)
                    {
                        SetGridHeaderSingleChannel(Convert.ToChar(iColName + iCol).ToString(), dg, iColWidth);
                        dt.Columns.Add(Convert.ToChar(iColName + iCol).ToString(), typeof(string));

                        if (iCol == 0)
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                DataRow row1 = dt.NewRow();

                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString()] = "NOT_USE";
                                }
                                else
                                {
                                    row1[Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                                    iSeq++;
                                }
                                dt.Rows.Add(row1);
                            }
                        }
                        else
                        {
                            for (int iRow = 0; iRow < iMaxRow; iRow++)
                            {
                                string[] NotUseRow = _sNotUseRowLIst.Split(',');
                                string[] NotUseCol = _sNotUseColLIst.Split(',');

                                if (NotUseRow.Contains(iRow.ToString()) && NotUseCol.Contains(iCol.ToString()))
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = "NOT_USE";
                                }
                                else
                                {
                                    dt.Rows[iRow][Convert.ToChar(iColName + iCol).ToString()] = string.Empty;
                                    iSeq++;
                                }
                            }
                        }
                    }

                    dg.ItemsSource = DataTableConverter.Convert(dt);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void SetGridHeaderSingleChannel(string sColName, C1DataGrid dg, double dWidth)
        {
            dg.Columns.Add(new C1.WPF.DataGrid.DataGridTextColumn()
            {
                Header = sColName,
                Binding = new Binding() { Path = new PropertyPath(sColName), Mode = BindingMode.TwoWay },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.NoWrap,
                //CanUserResizeRows = true,
                Width = new C1.WPF.DataGrid.DataGridLength(dWidth, DataGridUnitType.Star),
                IsReadOnly = true,
                CanUserResize = false
            });
        }

        private void rdo_Click(object sender, RoutedEventArgs e)
        {
            Getlist();
        }
    }
}
