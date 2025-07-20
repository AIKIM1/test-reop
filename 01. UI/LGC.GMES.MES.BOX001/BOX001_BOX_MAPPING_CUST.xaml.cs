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
using LGC.GMES.MES.CMM001.Extensions;

namespace LGC.GMES.MES.BOX001
{

    public partial class BOX001_BOX_MAPPING_CUST : C1Window, IWorkArea
    {
        List<string> pltList;
        string _PRODID;
        string _CUSTPRODID;
        string _CUST_MDLID;
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public BOX001_BOX_MAPPING_CUST()
        {
            InitializeComponent();
        }

        /// <summary>
        ///  Frame과 상호작용하기 위한 객체
        /// </summary>
        /// 
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);
            _PRODID = Util.NVC(tmps[0]);
            _CUSTPRODID = Util.NVC(tmps[1]);
            _CUST_MDLID = Util.NVC(tmps[2]);
            pltList = (List<string>)tmps[3];
        }

        #region Initialize

        #endregion

        #region Event


        #endregion

        private void btnClose_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        #region Mehod

        #endregion

        private void btnMapping_Clicked(object sender, RoutedEventArgs e)
        {
            try
            {
                DataSet indataSet = new DataSet();

                DataTable dtInPallet = indataSet.Tables.Add("INPALLET");
                dtInPallet.Columns.Add("PALLETID", typeof(string));
                dtInPallet.Columns.Add("CUSTPRODID", typeof(string));
                dtInPallet.Columns.Add("CUST_MDLID", typeof(string));

                DataRow drnewrow;

                for (int i = 0; i < pltList.Count; i++)
                {
                    drnewrow = dtInPallet.NewRow();
                    drnewrow["PALLETID"] = pltList[i];
                    dtInPallet.Rows.Add(drnewrow);
                }

                DataTable dtCustBox = indataSet.Tables.Add("INCUSTBOX");
                dtCustBox.Columns.Add("CUST_BOXID", typeof(string));

                drnewrow = dtCustBox.NewRow();
                drnewrow["CUST_BOXID"] = txtCustBoxId.Text.ToString();

                dtCustBox.Rows.Add(drnewrow);

                DataTable dtProd = indataSet.Tables.Add("INPROD");
                dtProd.Columns.Add("PRODID");
                dtProd.Columns.Add("CUSTPRODID");
                dtProd.Columns.Add("CUST_MDLID");

                drnewrow = dtProd.NewRow();
                drnewrow["PRODID"] = _PRODID;
                drnewrow["CUSTPRODID"] = _CUSTPRODID;
                drnewrow["CUST_MDLID"] = _CUST_MDLID;

                dtProd.Rows.Add(drnewrow);


                DataTable dtData = indataSet.Tables.Add("INDATA");
                dtData.Columns.Add("USERID");

                drnewrow = dtData.NewRow();
                drnewrow["USERID"] = LoginInfo.USERID;

                dtData.Rows.Add(drnewrow);

                new ClientProxy().ExecuteService_Multi("BR_PRD_REG_BOXID_MAPPING", "INPALLET,INCUSTBOX,INPROD,INDATA", null , (Result, ex) =>
                {
                    if (ex != null)
                    {
                        Util.MessageException(ex);
                        return;
                    }
                    Util.MessageInfo("SFU1275");    //정상 처리 되었습니다.
                    this.DialogResult = MessageBoxResult.OK;
                    this.Close();

                }, indataSet);



            }
            catch (Exception ex)
            {
                LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
