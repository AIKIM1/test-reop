/*************************************************************************************
 Created Date : 2017.12.09
      Creator : CNS 고현영S
   Decription : 전지 5MEGA-GMES 구축 - 특이작업 - C생산 공정진척 - 작업완료
--------------------------------------------------------------------------------------
 [Change History]
  2017.12.09  CNS 고현영S : 생성
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_021_RUN_CANCEL.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_021_CONFIRM : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _LotID = string.Empty;
        private string _WipSeq = string.Empty;
        private string _WipStat = string.Empty;
        private string sCaldate = string.Empty;

        private string _RetShiftCode = string.Empty;
        private string _RetShiftName = string.Empty;
        private string _RetWrkStrtTime = string.Empty;
        private string _RetWrkEndTime = string.Empty;

        private string _RetUserID = string.Empty;
        private string _RetUserName = string.Empty;

        private BizDataSet _Biz = new BizDataSet();
        private Util _Util = new Util();

        DataTable dtCellRslt = new DataTable();

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

        public ASSY003_021_CONFIRM()
        {
            InitializeComponent();
        }

        #endregion

        #region Event
        private void C1Window_Initialized(object sender, EventArgs e)
        {
            try
            {

            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 12)
                {
                    _LineID = Util.NVC(tmps[0]);
                    _EqptID = Util.NVC(tmps[1]);
                    _LotID = Util.NVC(tmps[2]);
                    _WipSeq = Util.NVC(tmps[3]);
                    _WipStat = Util.NVC(tmps[4]);

                    _RetShiftName = Util.NVC(tmps[5]);
                    _RetShiftCode = Util.NVC(tmps[6]);
                    _RetUserName = Util.NVC(tmps[7]);
                    _RetUserID = Util.NVC(tmps[8]);
                    _RetWrkStrtTime = Util.NVC(tmps[9]);
                    _RetWrkEndTime = Util.NVC(tmps[10]);

                    dtCellRslt = tmps[11] as DataTable;
                }
                else
                {
                    _LineID = "";
                    _EqptID = "";
                    _LotID = "";
                    _WipSeq = "";
                    _WipStat = "";

                    _RetShiftName = "";
                    _RetShiftCode = "";
                    _RetUserName = "";
                    _RetUserID = "";
                    _RetWrkStrtTime = "";
                    _RetWrkEndTime = "";

                    dtCellRslt = null;
                }

                Setting();
               
                GetAllData();

                ApplyPermissions();
            }
            catch ( Exception ex )
            {
                Util.MessageException(ex);
            }
        }

        private void Setting()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("EQSGID");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _EqptID;
                dr["EQSGID"] = _LineID;

                dt.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CPROD_CURR_INFO", "INDATA", "OUTDATA", dt);

                if (dtRslt.Rows.Count > 0)
                {
                    txtEquipmentSegment.Text = Util.NVC(dtRslt.Rows[0]["EQSGNAME"]);
                    txtEquipment.Text = Util.NVC(dtRslt.Rows[0]["EQPTNAME"]);
                }

                HiddenLoadingIndicator();
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

        private void GetAllData()
        {
            GetLotInfo();
            GerWorkResult();
        }

        private void GetLotInfo()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("PROCID");
                dt.Columns.Add("EQSGID");
                dt.Columns.Add("LOTID");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _EqptID;
                dr["PROCID"] = Process.CPROD;
                dr["EQSGID"] = _LineID;
                dr["LOTID"] = _LotID;

                dt.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PRODUCTLOT_CPROD", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgdLotInfo, dtRslt, null);

                HiddenLoadingIndicator();
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

        private void GerWorkResult()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable dt = new DataTable("INDATA");
                dt.Columns.Add("LANGID");
                dt.Columns.Add("EQPTID");
                dt.Columns.Add("PROCID");
                //dt.Columns.Add("EQSGID");
                dt.Columns.Add("LOTID");

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _EqptID;
                dr["PROCID"] = Process.CPROD;
                //dr["EQSGID"] = _LineID;
                dr["LOTID"] = _LotID;

                dt.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_CPROD_PROD_LIST", "INDATA", "OUTDATA", dt);

                Util.GridSetData(dgdWorkResult, dtRslt, null);

                HiddenLoadingIndicator();
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

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataSet indataSet = new DataSet();
                DataTable inTable = indataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("PROD_LOTID", typeof(string));
                inTable.Columns.Add("INPUTQTY", typeof(decimal));
                inTable.Columns.Add("OUTPUTQTY", typeof(decimal));
                inTable.Columns.Add("RESNQTY", typeof(decimal));
                inTable.Columns.Add("SHIFT", typeof(string));
                inTable.Columns.Add("WIPNOTE", typeof(string));
                inTable.Columns.Add("WRK_USERID", typeof(string));
                inTable.Columns.Add("WRK_USER_NAME", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("WIPDTTM_ED", typeof(DateTime));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _LotID;
                newRow["INPUTQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgdWorkResult.Rows[0].DataItem, "INPUT_QTY")));
                newRow["OUTPUTQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgdWorkResult.Rows[0].DataItem, "RECYC_QTY_FOLD")));
                newRow["RESNQTY"] = Convert.ToDouble(Util.NVC(DataTableConverter.GetValue(dgdWorkResult.Rows[0].DataItem, "DFCT_QTY")));
                newRow["SHIFT"] = _RetShiftName;
                newRow["WIPNOTE"] = "";
                newRow["WRK_USERID"] = _RetUserID;
                newRow["WRK_USER_NAME"] = _RetUserName;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["WIPDTTM_ED"] = System.DateTime.Now;

                indataSet.Tables["INDATA"].Rows.Add(newRow);

                DataTable inLot = indataSet.Tables.Add("IN_RWK");
                inLot.Columns.Add("PRODID", typeof(string));
                inLot.Columns.Add("RECYC_QTY", typeof(Decimal));
                inLot.Columns.Add("SCRP_QTY", typeof(Decimal));
                inLot.Columns.Add("ADD_INPUT_QTY", typeof(Decimal));

                DataRow newRow2 = inLot.NewRow();

                for (int i = 0; i < dtCellRslt.Rows.Count ; i++)
                {
                    newRow2["PRODID"] = Util.NVC(dtCellRslt.Rows[i]["PRODID"]);
                    newRow2["RECYC_QTY"] = Util.NVC(dtCellRslt.Rows[i]["RECYC_QTY"]);
                    newRow2["SCRP_QTY"] = "0";
                    newRow2["ADD_INPUT_QTY"] = Util.NVC(dtCellRslt.Rows[i]["ADD_INPUT_QTY"]);

                    indataSet.Tables["IN_RWK"].Rows.Add(newRow2);

                    newRow2 = inLot.NewRow();
                }

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_END_PROD_LOT_CPROD", "INDATA,IN_RWK", null, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1275");	//정상 처리 되었습니다.

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
                }, indataSet
                );
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }     

        #endregion

        #region Method

        #region [Func]

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnConfirm);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void ShowLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Visible;
        }

        private void HiddenLoadingIndicator()
        {
            if ( loadingIndicator != null )
                loadingIndicator.Visibility = Visibility.Collapsed;
        }

        #endregion

        #endregion

    }
}
