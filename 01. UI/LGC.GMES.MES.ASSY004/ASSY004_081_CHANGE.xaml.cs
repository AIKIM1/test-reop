/*************************************************************************************
 Created Date : 2023.10.24
      Creator : 
   Decription : 전지 5MEGA-GMES 구축 - CT 공정진척 화면 - 작업 타입 변경 팝업
--------------------------------------------------------------------------------------
 [Change History]
 2023.10.26 안유수 E20231019-001145 최초생성
  
**************************************************************************************/


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;

namespace LGC.GMES.MES.ASSY004
{
    /// <summary>
    /// ASSY001_005_CHANGE.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ASSY004_081_CHANGE : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        private string _LineID = string.Empty;
        private string _EqptID = string.Empty;
        private string _ProdLotID = string.Empty;
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
        public ASSY004_081_CHANGE()
        {
            InitializeComponent();
        }        
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            if (tmps != null && tmps.Length >= 4)
            {
                _LineID = Util.NVC(tmps[0]);
                _EqptID = Util.NVC(tmps[1]);
                _ProdLotID = Util.NVC(tmps[2]);
                //_WORKING_TYPE = Util.NVC(tmps[3]);
            }
            else
            {
                _LineID = "";
                _EqptID = "";
                _ProdLotID = "";
                //_WORKING_TYPE = "";
            }

            ApplyPermissions();
            InitCombo();
        }

        private void InitCombo()
        {
            CommonCombo _combo = new CommonCombo();

            String[] sFilter = { "PROD_LOT_OPER_MODE" };
            _combo.SetCombo(cboLotMode, CommonCombo.ComboStatus.NONE, sFilter: sFilter, sCase: "COMMCODE");

            if (cboLotMode.Items.Count > 0)
                cboLotMode.SelectedValue = "L"; // UI 는 비정규 모드.
           
            String[] sFilter2 = { "IRREGL_PROD_LOT_TYPE_CODE" };
            _combo.SetCombo(cboChange, CommonCombo.ComboStatus.NONE, sFilter: sFilter2, sCase: "COMMCODE");

            if (cboChange.Items.Count > 0)
            {
                for (int i = 0; cboChange.Items.Count > i; i++)
                {
                    if (!(((DataRowView)cboChange.Items[i]).Row.ItemArray[0].ToString().Equals("R") || ((DataRowView)cboChange.Items[i]).Row.ItemArray[0].ToString().Equals("N")))
                    {
                        ((DataRowView)cboChange.Items[i]).Row.Delete();
                        i--;
                    }
                }
            }
            //string[] sFilter = { "WORKING_TYPE" , Process.CT_INSP };
            //_combo.SetCombo(cboChange, CommonCombo.ComboStatus.NONE, sCase: "WORKTYPE_COMMCODE_CBO", sFilter: sFilter);
            //cboChange.SelectedValue = _WORKING_TYPE;


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
                inTable.Columns.Add("IRREGL_PROD_LOT_TYPE_CODE", typeof(String));
                inTable.Columns.Add("USERID", typeof(String));
                inTable.Columns.Add("LOT_MODE", typeof(String));

                DataRow newRow = inTable.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["EQPTID"] = _EqptID;
                newRow["PROD_LOTID"] = _ProdLotID;
                newRow["IRREGL_PROD_LOT_TYPE_CODE"] = cboChange.SelectedValue;
                newRow["USERID"] = LoginInfo.USERID;
                newRow["LOT_MODE"] = cboLotMode.SelectedValue;

                inTable.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_PRD_REG_LOTATTR_WIP_WRK_TYPE_CODE_PRODLOT_FOR_CTINSP", "INDATA", null, inTable, (searchResult, searchException) =>
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
