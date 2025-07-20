/*************************************************************************************
 Created Date : 2019.01.21
      Creator : 
   Decription : GMES 고도화 - 설비 Trouble SMS 전송 이력 조회
--------------------------------------------------------------------------------------
 [Change History]
  2019.01.21  INS 김동일K : Initial Created.
  2019.02.01  INS 김동일K : 조회 항목 정렬 변경 및 설비 콤보 Unit 까지 나오도록 변경
**************************************************************************************/


using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_132.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_132 : UserControl, IWorkArea
    {
        public COM001_132()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSearch);

                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);

                //dtpDateFrom.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;
                //dtpDateTo.SelectedDataTimeChanged += dtpDate_SelectedDataTimeChanged;

                this.Loaded -= UserControl_Loaded;

                InitCombo();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                loadingIndicator.Visibility = Visibility.Visible;

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LANGID", typeof(string));
                dtRqst.Columns.Add("EQSGID", typeof(string));
                dtRqst.Columns.Add("EQPTID", typeof(string));
                dtRqst.Columns.Add("PROCID", typeof(string));
                dtRqst.Columns.Add("FRDTTM", typeof(string));
                dtRqst.Columns.Add("TODTTM", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                
                dr["EQSGID"] = Util.GetCondition(cboEquipmentSegment, MessageDic.Instance.GetMessage("SFU1223"));

                string sEquipment = Util.GetCondition(cboEquipment);
                dr["EQPTID"] = string.IsNullOrWhiteSpace(sEquipment) ? null : sEquipment;

                string sProcess = Util.GetCondition(cboProcess);
                dr["PROCID"] = string.IsNullOrWhiteSpace(sProcess) ? null : sProcess;
                dr["FRDTTM"] = Util.GetCondition(dtpDateFrom);
                dr["TODTTM"] = Util.GetCondition(dtpDateTo);

                dtRqst.Rows.Add(dr);

                new ClientProxy().ExecuteService("DA_PRD_SEL_EQPT_TRBL_ALARM_HIST", "INDATA", "OUTDATA", dtRqst, (result, bizEx) =>
                {
                    try
                    {
                        if (bizEx != null)
                        {
                            Util.MessageException(bizEx);
                            return;
                        }

                        Util.GridSetData(dgList, result, FrameOperation, true);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private void InitCombo()
        {
            //동,라인,공정,설비 셋팅
            CommonCombo _combo = new CommonCombo();

            //라인
            String[] sFilter1 = { LoginInfo.CFG_AREA_ID };
            C1ComboBox[] cboEqsgChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEqsgChild, sFilter: sFilter1);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.ALL, cbChild: cboProcChild, cbParent: cboProcessParent);

            //if (cboProcess.Items.Count < 1)
            //    SetProcess();

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.ALL, cbParent: cboEquipmentParent, sCase: "EQUIPMENT_BY_EQPTID");
            
            //cboEquipmentSegment.SelectedItemChanged += cboEquipmentSegment_SelectedItemChanged;
            //cboProcess.SelectedItemChanged += CboProcess_SelectedItemChanged;
            
        }
    }
}
