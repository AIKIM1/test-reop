/*************************************************************************************
 Created Date : 2018.09.12
      Creator : 
   Decription : 활성화 불량, 양품 Cell 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2018.09.12  JMK : Initial Created.

**************************************************************************************/

using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Popup;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using C1.WPF.DataGrid;
using C1.WPF;
using System.Linq;
using C1.WPF.DataGrid.Summaries;

namespace LGC.GMES.MES.COM001
{
    public partial class COM001_254 : UserControl, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        BizDataSet _Biz = new BizDataSet();

        #endregion

        #region Initialize
        /// <summary>
        ///  
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public COM001_254()
        {
            InitializeComponent();
        }

        #endregion

        #region Event

        #region [Form Load]

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeUserControls();
            SetCombo();

            this.Loaded -= UserControl_Loaded;
        }

        private void InitializeUserControls()
        {
            ////사용자 권한별로 버튼 숨기기
            //List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnSearch);

            //Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

            dtpDateFrom.SelectedDateTime = DateTime.Now.AddDays(-7);
            dtpDateTo.SelectedDateTime = DateTime.Now;
        }

        private void SetCombo()
        {
            CommonCombo _combo = new CommonCombo();

            //동
            C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, cbChild: cboAreaChild);

            //라인
            C1ComboBox[] cboEquipmentSegmentParent = { cboArea };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentSegmentParent);
            cboEquipmentSegment.SelectedValueChanged += cboEquipmentSegment_SelectedValueChanged;

            // 설비
            SetEquipmentCombo(cboEquipment);

            // 재공품질유형(양불구분)
            string[] sFilter1 = { "WIP_QLTY_TYPE_CODE" };
            _combo.SetCombo(cboQltyTyprCode, CommonCombo.ComboStatus.ALL, sCase: "COMMCODE", sFilter: sFilter1);
        }

        #endregion

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (cboEquipmentSegment.SelectedValue == null || cboEquipmentSegment.SelectedValue.ToString().Equals("SELECT"))
            //    return;

            SetEquipmentCombo(cboEquipment);
        }

        /// <summary>
        /// 조회
        /// </summary>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search();
        }

        #endregion


        #region Method

        /// <summary>
        /// 설비 콤보
        /// </summary>
        private void SetEquipmentCombo(C1ComboBox cbo)
        {
            const string bizRuleName = "DA_BAS_SEL_EQUIPMENT_AREA_CBO";
            string[] arrColumn = { "LANGID", "AREAID", "EQSGID" };
            string[] arrCondition = { LoginInfo.LANGID, Util.NVC(cboArea.SelectedValue), Util.NVC(cboEquipmentSegment.SelectedValue) };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, LoginInfo.CFG_EQPT_ID);
        }

        /// <summary>
        /// 불량, 양품 Cell 이력 조회
        /// </summary>
        private void Search()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("FROMDATE", typeof(string));
                inTable.Columns.Add("TODATE", typeof(string));
                inTable.Columns.Add("ACTID", typeof(string));
                inTable.Columns.Add("SUBLOTID", typeof(string));

                // INDATA SET
                DataRow newRow = inTable.NewRow();
                newRow["LANGID"] = LoginInfo.LANGID;
                newRow["FROMDATE"] = Util.GetCondition(dtpDateFrom);
                newRow["TODATE"] = Util.GetCondition(dtpDateTo);

                if (string.IsNullOrWhiteSpace(txtSublotID.Text))
                {
                    // 동을 선택하세요.
                    newRow["AREAID"] = Util.GetCondition(cboArea, MessageDic.Instance.GetMessage("SFU1499"));
                    if (newRow["AREAID"].Equals("")) return;

                    newRow["EQSGID"] = Util.GetCondition(cboEquipmentSegment, bAllNull: true);
                    newRow["EQPTID"] = cboEquipment.SelectedValue == null ? null : cboEquipment.SelectedValue.ToString();

                    string actID = Util.GetCondition(cboQltyTyprCode, bAllNull: true);
                    if (string.IsNullOrWhiteSpace(actID))
                        actID = null;
                    else if (actID.Equals("G"))
                        actID = "GOOD_SUBLOT";
                    else if (actID.Equals("N"))
                        actID = "DEFECT_SUBLOT";

                    newRow["ACTID"] = actID;
                }
                else
                {
                    newRow["SUBLOTID"] = txtSublotID.Text;
                }

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("DA_PRD_SEL_SUBLOT_RSN_CLCT", "INDATA", "OUTDATA", inTable, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        Util.GridSetData(dgList, bizResult, FrameOperation, true);
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
        }
        #endregion


        #region Funct
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

