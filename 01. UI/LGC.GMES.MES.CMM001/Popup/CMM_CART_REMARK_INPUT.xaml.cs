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
using C1.WPF.DataGrid;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace LGC.GMES.MES.CMM001.Popup
{

    public partial class CMM_CART_REMARK_INPUT : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        public string PRODID
        {
            get;
            set;
        }


        public CMM_CART_REMARK_INPUT()
        {
            InitializeComponent();
        }

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


        /// <summary>
        /// 화면내 버튼 권한 처리
        /// </summary>
        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            //listAuth.Add(btnInReplace);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        /// <summary>
        /// 콤보박스 초기 설정
        /// </summary>
        private void InitCombo()
        {
        }

        /// <summary>
        /// 컨트롤 초기값 설정
        /// </summary>
        private void InitControl()
        {
        }
        #endregion

        #region Event

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitCombo();
            InitControl();
            //Search();
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchCtnr();
        }
        #endregion

        #region Mehod
        /// <summary>
        /// 조회
        /// </summary>
        private void SearchCtnr()
        {
            try
            {

                string sCtnrID = txtCtnrID.Text.ToString();
                string sProdID = txtProdID.Text.ToString();

                DataTable dt = DataTableConverter.Convert(dgCtnr.ItemsSource);

                if(!string.IsNullOrEmpty(sCtnrID))
                {
                    for(int i = 0; i < dt.Rows.Count; i++)
                    {
                        if(dt.Rows[i]["CTNR_ID"].ToString() == sCtnrID)
                        {
                            Util.Alert("SFU3471", sCtnrID);
                            dgCtnr.ScrollIntoView(i, 0);
                            return;
                        }
                    }
                }
                else if(string.IsNullOrEmpty(sProdID))
                {
                    Util.Alert("SFU5018");
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("CTNR_ID", typeof(string));
                RQSTDT.Columns.Add("PRODID", typeof(string));
                RQSTDT.Columns.Add("PJT_NAME", typeof(string));
                RQSTDT.Columns.Add("LOTID_RT", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;                
                dr["CTNR_ID"] = string.IsNullOrEmpty(sCtnrID) ? null : sCtnrID;
                dr["PRODID"] = sProdID;
                dr["PJT_NAME"] = string.Empty;
                dr["LOTID_RT"] = string.Empty;
                dr["AREAID"] = LoginInfo.CFG_AREA_ID;
                
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_POLYMER_CART_WIP_INFO", "RQSTDT", "RSLTDT", RQSTDT);

                if(dtResult.Rows.Count == 0)
                {
                    Util.Alert("SFU1801");
                    return;
                }

                var summarydata = from row in dtResult.AsEnumerable()
                                  group row by new
                                  {
                                      CartID = row.Field<string>("CTNR_ID"),
                                  } into grp
                                  select new
                                  {
                                      CartID = grp.Key.CartID,
                                      CellSum = grp.Sum(r => r.Field<decimal>("CELL_QTY"))
                                  };

                foreach (var row in summarydata)
                {
                    dtResult.Select("CTNR_ID = '" + row.CartID + "'").ToList<DataRow>().ForEach(r => r["CART_CELL_QTY"] = row.CellSum);
                }

                DataTable dtSrc = DataTableConverter.Convert(dgCtnr.ItemsSource);

                dtSrc.Merge(dtResult);

                DataTable distinctTable = dtSrc.DefaultView.ToTable(true, new string[] { "CHK","CTNR_ID", "PRJT_NAME", "PRODID", "CART_CELL_QTY", "REMARKS_CNTT" });


                Util.GridSetData(dgCtnr, distinctTable, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


        #endregion

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DataRow[] dr = DataTableConverter.Convert(dgCtnr.ItemsSource).Select("CHK = 1");

                if(dr.Length == 0)
                {
                    Util.Alert("SFU3538");
                    return;
                }

                // 저장하시겠습니까?
                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SaveCtnrRemarks();
                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void SaveCtnrRemarks()
        {
            try
            {
                const string bizRuleName = "BR_PRD_REG_TB_SFC_CTNR_REMARKS";

                // DATA SET
                DataSet inDataSet = new DataSet();

                DataTable inTable = inDataSet.Tables.Add("INDATA");
                inTable.Columns.Add("USERID", typeof(string));
                inTable.Columns.Add("REMARKS_CNTT", typeof(string));

                // INDATA              
                DataRow newRow = inTable.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["REMARKS_CNTT"] = tbRemark.Text.ToString();

                inTable.Rows.Add(newRow);

                DataRow[] dr = DataTableConverter.Convert(dgCtnr.ItemsSource).Select("CHK = 1");

                DataTable inCtnr = inDataSet.Tables.Add("INCTNR");
                inCtnr.Columns.Add("CTNRID", typeof(string));

                for (int i = 0; i < dr.Length; i++)
                {
                    newRow = inCtnr.NewRow();
                    newRow["CTNRID"] = dr[i]["CTNR_ID"];
                    inCtnr.Rows.Add(newRow);
                }
           
                new ClientProxy().ExecuteService_Multi(bizRuleName, "INDATA,INCTNR", "", (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }

                        //정상 처리 되었습니다
                        Util.MessageInfo("SFU1889");
                        Clear();
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                }, inDataSet);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
       
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }

        private void Clear()
        {
            dgCtnr.ItemsSource = null;
            txtCtnrID.Text = string.Empty;
            txtProdID.Text = string.Empty;
            tbRemark.Text = string.Empty;
        }
    }
}
