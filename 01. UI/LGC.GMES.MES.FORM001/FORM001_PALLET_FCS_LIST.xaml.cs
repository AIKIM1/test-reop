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
    public partial class FORM001_PALLET_FCS_LIST : C1Window, IWorkArea
    {
        #region Declaration

        Util _Util = new Util();
        private string _areaID = string.Empty;        // 공정코드
        private string _sFCSPalletId = string.Empty;

        private bool _load = true;

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public string FCSPALLETID
        {
            get { return _sFCSPalletId; }
        }

        #endregion

        #region Initialize

        public FORM001_PALLET_FCS_LIST()
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
                FcsPalletInfoSelect();
                _load = false;
            }

        }
        private void InitializeUserControls()
        {
            //
        }

        private void SetControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _areaID = tmps[0] as string;


            if (_areaID == null)
                return;
        }

        private void SetControlVisibility()
        {
            //
        }

        private void SetControlHeader()
        {
            //this.Header = ObjectDic.Instance.GetObjectName("대차 삭제");
        }


        #endregion

        #region [선택]
        /// <summary>
        /// 삭제
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateStartRun())
                return;

            // 선택된 FCS Pallet id 를 사용하시겠습니까 ?
            Util.MessageConfirm("SUF4965", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    this.DialogResult = MessageBoxResult.OK;
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
        private void FcsPalletInfoSelect()
        {
            try
            {
                ShowLoadingIndicator();

                // 원각형 특성/Grading 공정 Pallet 삭제시 Box 테이블의 Pallet도 삭제
                string bizRuleName = "BR_INF_SEL_TC_AUTOBOXING_BOX_PALLET_LIST";

                // DATA Table
                DataSet inDataSet = new DataSet();
                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("AREAID", typeof(string));
                

                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = _areaID;
                inTable.Rows.Add(newRow);


                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA", "OUTDATA", (bizResult, bizException) =>
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

                        //this.DialogResult = MessageBoxResult.OK;
                        DataTable result = bizResult.Tables["OUTDATA"];
                        Util.GridSetData(dgPallet, result, FrameOperation, true);
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
            DataTable dt = DataTableConverter.Convert(dgPallet.ItemsSource);

            DataRow[] dr = dt.Select("CHK = 'True'");

            if (dr.Length == 0)
            {
                Util.MessageValidation("SFU1636");
                return false;
            }
            else
            {
                _sFCSPalletId = dr[0]["PALLET_ID"].ToString();
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
