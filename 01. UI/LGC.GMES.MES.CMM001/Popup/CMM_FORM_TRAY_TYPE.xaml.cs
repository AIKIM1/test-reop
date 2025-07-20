/*************************************************************************************
 Created Date : 2020.06.03
      Creator : INS 김동일K
   Decription : 조립 공정진척 화면 - 활성화 트레이 조회
--------------------------------------------------------------------------------------
 [Change History]
  2020.06.03  INS 김동일K : Initial Created. [C20200602-000207]
  
**************************************************************************************/

using System;
using System.Windows;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using System.Data;
using LGC.GMES.MES.ControlsLibrary;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LGC.GMES.MES.CMM001.Popup
{
    public partial class CMM_FORM_TRAY_TYPE : C1Window, IWorkArea
    {
        private string _eqptId;
        private Dictionary<string, string> COMBO_TYPE = null;

        public CMM_FORM_TRAY_TYPE()
        {
            InitializeComponent();
            COMBO_TYPE = new Dictionary<string, string>()
            {
                { "SELECT", "-SELECT-" }
            };
            _eqptId = string.Empty;
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            Logger.Instance.WriteLine("[C1Window_Loaded]", LogCategory.UI, new object[] { "Before InitWindow()" });
            InitWindow();
            Logger.Instance.WriteLine("[C1Window_Loaded]", LogCategory.UI, new object[] { "After InitWindow()" });
            this.Loaded -= C1Window_Loaded;
        }

        private void InitWindow()
        {
            object[] parameters = C1WindowExtension.GetParameters(this);
            if (parameters == null || parameters.Length == 0)
            {
                return;
            }

            _eqptId = Util.NVC(parameters[0]);
            if (string.IsNullOrEmpty(_eqptId))
            {
                return;
            }
            SetTexts();
        }

        private void SetTexts()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _eqptId;
                dt.Rows.Add(dr);

                Logger.Instance.WriteLine("[SetTexts]", LogCategory.UI, new object[] { "Before Call BR_PRD_SEL_TRAY_SUPPLY_INFO_BY_EQPTID" });
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_TRAY_SUPPLY_INFO_BY_EQPTID", "INDATA", "OUTDATA", dt);
                Logger.Instance.WriteLine("[SetTexts]", LogCategory.UI, new object[] { "After Call BR_PRD_SEL_TRAY_SUPPLY_INFO_BY_EQPTID" });

                if (dtResult == null || dtResult.Rows.Count == 0)
                {
                    return;
                }

                txtEqptName.Text = Util.NVC(dtResult.DefaultView[0]["EQPTNAME"]);
                txtMdlLot.Text = Util.NVC(dtResult.DefaultView[0]["MDLLOT"]);
                txtSupplyStatus.Text = Util.NVC(dtResult.DefaultView[0]["SUPPLY_YN"]);
                string currTrayType = Util.NVC(dtResult.DefaultView[0]["TRAY_TYPE_CD"]);

                if (Util.NVC(dtResult.DefaultView[0]["SUPPLY_YN"]).Equals("Y"))
                {
                    btnStopSupply.IsEnabled = true;
                    SetCbo(currTrayType);
                }
                else
                {
                    btnStopSupply.IsEnabled = false;
                    SetCbo();
                }

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCbo(string currTrayType)
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("LANGID", typeof(string));
                dt.Columns.Add("EQPTID", typeof(string));

                DataRow dr = dt.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["EQPTID"] = _eqptId;
                dt.Rows.Add(dr);

                Logger.Instance.WriteLine("[SetCbo]", LogCategory.UI, new object[] { "Before Call DA_INF_PFC3_SEL_TRAY_TYPE_EQPTID_CBO" });
                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_INF_PFC3_SEL_TRAY_TYPE_EQPTID_CBO", "INDATA", "OUTDATA", dt);
                Logger.Instance.WriteLine("[SetCbo]", LogCategory.UI, new object[] { "After Call DA_INF_PFC3_SEL_TRAY_TYPE_EQPTID_CBO" });

                if (dtResult == null)
                {
                    return;
                }

                DataRow tmpDr = dtResult.NewRow();
                tmpDr["TRAY_TYPE_CD"] = COMBO_TYPE["SELECT"];
                dtResult.Rows.Add(tmpDr);

                if (dtResult.Rows.Count == 1 && !string.IsNullOrEmpty(currTrayType))
                {
                    tmpDr = dtResult.NewRow();
                    tmpDr["TRAY_TYPE_CD"] = currTrayType;
                    dtResult.Rows.Add(tmpDr);
                }

                cboTrayType.ItemsSource = dtResult.DefaultView;
                cboTrayType.SelectedValue = string.IsNullOrEmpty(currTrayType) ? COMBO_TYPE["SELECT"] : currTrayType;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SetCbo()
        {
            try
            {
                DataTable tmpDt = new DataTable();
                tmpDt.Columns.Add("TRAY_TYPE_CD", typeof(string));

                DataRow tmpDr = tmpDt.NewRow();
                tmpDr["TRAY_TYPE_CD"] = COMBO_TYPE["SELECT"];
                tmpDt.Rows.Add(tmpDr);

                cboTrayType.ItemsSource = tmpDt.DefaultView;
                cboTrayType.SelectedIndex = 0;
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void StopSupply()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = dt.NewRow();
                dr["EQPTID"] = _eqptId;
                dr["UPDUSER"] = LoginInfo.USERID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_INF_PFC3_UPD_EW_FORM_ASSY_SUPPLY_STOP", "INDATA", "OUTDATA", dt);
                SetTexts();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void ChangeTrayType()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.TableName = "INDATA";
                dt.Columns.Add("EQPTID", typeof(string));
                dt.Columns.Add("TRAY_TYPE_CD", typeof(string));
                dt.Columns.Add("UPDUSER", typeof(string));

                DataRow dr = dt.NewRow();
                dr["EQPTID"] = _eqptId;
                dr["TRAY_TYPE_CD"] = Util.NVC(cboTrayType.SelectedValue);
                dr["UPDUSER"] = LoginInfo.USERID;
                dt.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_REG_CHG_TRAY_TYPE_FOR_PKG", "INDATA", "OUTDATA", dt);
                SetTexts();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnStopSupply_Click(object sender, RoutedEventArgs e)
        {
            StopSupply();
        }

        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Util.NVC(cboTrayType.SelectedValue)) || Util.NVC(cboTrayType.SelectedValue).Equals(COMBO_TYPE["SELECT"]))
            {
                //Type을 선택하세요
                Util.MessageValidation("SFU5054");
                return;
            }
            ChangeTrayType();
        }
    }
}
