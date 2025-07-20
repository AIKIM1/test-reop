/*************************************************************************************
 Created Date : 2017.11.16
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - Folding, Packing 공정진척 화면 - 작업 타입 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]

  
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

namespace LGC.GMES.MES.ASSY003
{
    /// <summary>
    /// ASSY003_005_CHANGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY003_005_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProdLotID = string.Empty;
        private string _ProcID = string.Empty;
        private string _OutLotID = string.Empty;
        private decimal _OutQty = 0;
        private string _WORKING_TYPE = string.Empty;

        private BizDataSet _Biz = new BizDataSet();

        public string OUT_LOT_ID
        {
            get { return _OutLotID; }
        } 

        public decimal OUT_QTY
        {
            get { return _OutQty; }
        }
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
        public ASSY003_005_CHANGE()
        {
            InitializeComponent();
        }        
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 5)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ProdLotID = Util.NVC(tmps[2]);
                _WORKING_TYPE = Util.NVC(tmps[3]);
                _ProcID = Util.NVC(tmps[4]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ProdLotID = "";
                _WORKING_TYPE = "";
                _ProcID = "";
            }

            ApplyPermissions();
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            string[] sFilter = { "WORK_TYPE", _ProcID };
            _combo.SetCombo(cboChange, CommonCombo.ComboStatus.NONE, sCase: "AREA_COM_CODE_WORKTYPE_CBO", sFilter: sFilter);
            cboChange.SelectedValue = _WORKING_TYPE;
        }

        private void C1Window_Initialized(object sender, EventArgs e)
        {

        }
        
        private void btnChange_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowLoadingIndicator();

                DataTable inTable = new DataTable();
                inTable.TableName = "RQSTDT";
                inTable.Columns.Add("SRCTYPE", typeof(String));
                inTable.Columns.Add("EQPTID", typeof(String));
                inTable.Columns.Add("PROD_LOTID", typeof(String));
                inTable.Columns.Add("WIP_WRK_TYPE_CODE", typeof(String));
                inTable.Columns.Add("USERID", typeof(String));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["WIP_WRK_TYPE_CODE"] = cboChange.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LOTATTR_WIP_WRK_TYPE_CODE_PRODLOT", "INDATA", null, inTable, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }

                        Util.MessageValidation("SFU1275");  //정상 처리 되었습니다.

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
                });                
            }
            catch (Exception ex)
            {
                HiddenLoadingIndicator();
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
        }

        #endregion

        #region Mehod

        #region [BizCall]


        #endregion

        #region [Func]
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnChange);

            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
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

        #region [Validation]

        #endregion

        #endregion


    }
}
