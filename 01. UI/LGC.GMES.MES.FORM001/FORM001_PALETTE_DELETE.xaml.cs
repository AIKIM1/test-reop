/*************************************************************************************
 Created Date : 2017.07.25
      Creator : 
   Decription : Pallet 삭제
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/

using System;
using System.Windows;
using System.Data;

using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Class;
using C1.WPF.DataGrid;
using System.Collections.Generic;
using System.Windows.Controls;
using LGC.GMES.MES.CMM001;
using LGC.GMES.MES.CMM001.Extensions;
using Application = System.Windows.Application;
using DataGridLength = C1.WPF.DataGrid.DataGridLength;
using DataGridRow = C1.WPF.DataGrid.DataGridRow;
using System.Linq;

namespace LGC.GMES.MES.FORM001
{
    public partial class FORM001_PALETTE_DELETE : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        CommonCombo _combo = new CMM001.Class.CommonCombo();

        private string _procID = string.Empty;        // 공정코드
        private string _eqptID = string.Empty;        // 설비코드

        private bool _load = true;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Initialize

        public FORM001_PALETTE_DELETE()
        {
            InitializeComponent();
        }
        #endregion

        #region Event

        #region [Form Load]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_load)
            {
                InitializeUserControls();
                SetControl();
                SetControlVisibility();
                SetControlHeader();
                _load = false;
            }

        }
        private void InitializeUserControls()
        {
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _procID = tmps[0] as string;
            _eqptID = tmps[1] as string;

            // Pallete 정보
            DataRow[] prodPallet = tmps[2] as DataRow[];

            if (prodPallet == null)
                return;

            DataTable prodPalletBind = new DataTable();
            prodPalletBind = prodPallet[0].Table.Clone();

            for (int row = 0; row < prodPallet.Length; row++)
            {
                prodPalletBind.ImportRow(prodPallet[row]);
            }

            Util.GridSetData(dgPallet, prodPalletBind, null);

        }

        private void SetControlVisibility()
        {
            if (string.Equals(_procID, Process.CircularGrader) || string.Equals(_procID, Process.SmallGrader))
            {
                dgPallet.Columns["VLTG_GRD_CODE"].Visibility = Visibility.Collapsed;
            }

            // 저항등급
            if (_procID.Equals(Process.CircularCharacteristicGrader) || _procID.Equals(Process.CircularReTubing))
            {
                dgPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Visible;
            }
            else
            {
                dgPallet.Columns["RSST_GRD_CODE"].Visibility = Visibility.Collapsed;
            }
        }

        private void SetControlHeader()
        {
            if (string.Equals(_procID, Process.SmallOcv) || string.Equals(_procID, Process.SmallLeak) || string.Equals(_procID, Process.SmallDoubleTab))
            {
                // 초소형 OCV 검사, 초소형 누액검사, 초소형 더블탭
                this.Header = ObjectDic.Instance.GetObjectName("대차 삭제");
            }
            else
            {
                this.Header = ObjectDic.Instance.GetObjectName("Pallet 삭제");
            }
        }


        #endregion

        #region [삭제]
        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;

            // Pallet를 삭제 하시겠습니까?
            Util.MessageConfirm("SFU4008", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    DeletePallet();
                }
            });
        }
        #endregion

        #region [닫기]
        /// <summary>
        /// 닫기
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }
        #endregion

        #endregion

        #region User Method

        #region [BizCall]
        /// <summary>
        /// Pallet 삭제
        /// </summary>
        private void DeletePallet()
        {
            try
            {
                ShowLoadingIndicator();

                // 원각형 특성/Grading 공정 Pallet 삭제시 Box 테이블의 Pallet도 삭제
                string bizRuleName = _procID.Equals(Process.CircularCharacteristicGrader) ? "BR_PRD_REG_DELETE_PALLET_NEW" : "BR_PRD_REG_DELETE_PALLET";

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("SRCTYPE", typeof(string));
                inTable.Columns.Add("IFMODE", typeof(string));
                inTable.Columns.Add("EQPTID", typeof(string));
                inTable.Columns.Add("USERID", typeof(string));

                DataTable inLot = inDataSet.Tables.Add("INLOT");
                inLot.Columns.Add("PALLETID", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(_eqptID);
                newRow["USERID"] = LoginInfo.USERID;
                inTable.Rows.Add(newRow);

                for (int row = 0; row < dgPallet.Rows.Count; row++)
                {
                    newRow = inLot.NewRow();
                    newRow["PALLETID"] = DataTableConverter.GetValue(dgPallet.Rows[row].DataItem, "PALLETE_ID");
                    inLot.Rows.Add(newRow);
                }


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INLOT", null, (bizResult, bizException) =>
                {
                    try
                    {
                        HiddenLoadingIndicator();

                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        ////Util.AlertInfo("정상 처리 되었습니다.");
                        //Util.MessageInfo("SFU1889");

                        this.DialogResult = MessageBoxResult.OK;
                    }
                    catch (Exception ex)
                    {
                        HiddenLoadingIndicator();
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }
        #endregion

        #region[[Validation]
        private bool ValidateStartRun()
        {
            if (dgPallet.Rows.Count == 0)
            {
                // 삭제할 Pallet가 없습니다.
                Util.MessageValidation("SFU4009");
                return false;
            }

            return true;
        }
        #endregion

        #region [Func]
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
