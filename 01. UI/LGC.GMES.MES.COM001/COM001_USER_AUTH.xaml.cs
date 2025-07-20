using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// COM001_USER_AUTH.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class COM001_USER_AUTH : UserControl, IWorkArea
    {
        private Util _Util = new Util();

        public COM001_USER_AUTH()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation { get; set; }

        private void COM001_USER_AUTH_Loaded(object sender, RoutedEventArgs e)
        {
            SetInitConnection("DA_BAS_SEL_PERSON_CONN", dgPerson);                          // 유저 정보
            SetInitConnection("DA_BAS_SEL_TB_MMD_USER_SHOP_AREA_CONN", dgUserShopArea);     // 유저 등록 플랜트/동
            SetInitConnection("DA_BAS_SEL_TB_MMD_USER_AUTH_CONN", dgUserAuth);              // 유정 등록 권한
        }

        private void SetInitConnection(string sBizName, C1DataGrid grid)
        {
            try
            {
                DataTable RQSTDT = new DataTable("RQSTDT");
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync(sBizName, "RQSTDT", "RSLTDT", RQSTDT);

                Util.GridSetData(grid, dtResult, FrameOperation, false);
            }
            catch (Exception ex) { Util.MessageException(ex); }
        }

        private void btnRequest_Click(object sender, RoutedEventArgs e)
        {
            if (dgUserShopArea.GetRowCount() == 0 || _Util.GetDataGridRowCountByCheck(dgUserShopArea, "CHK", true) == 0)
            {
                Util.MessageValidation("SFU5153");  // 선택된 Plant/동 정보가 없습니다.
                return;
            }

            if (dgUserAuth.GetRowCount() == 0 || _Util.GetDataGridRowCountByCheck(dgUserAuth, "CHK", true) == 0)
            {
                Util.MessageValidation("SFU5154"); // 선택된 권한 정보가 없습니다.
                return;
            }

            Util.MessageConfirm("SFU5155", (sresult) => // 권한 신청을 진행하시겠습니까?
            {
                if (sresult == MessageBoxResult.OK)
                {
                    try
                    {
                        // INDATA
                        DataSet inDataSet = new DataSet();
                        DataTable inData = inDataSet.Tables.Add("INDATA");
                        inData.Columns.Add("LANGID", typeof(string));
                        inData.Columns.Add("SRCTYPE", typeof(string));
                        inData.Columns.Add("USERID", typeof(string));

                        DataRow row = inData.NewRow();
                        row["LANGID"] = LoginInfo.LANGID;
                        row["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                        row["USERID"] = LoginInfo.USERID;
                        inDataSet.Tables["INDATA"].Rows.Add(row);

                        // INSHOP
                        DataTable InShop = inDataSet.Tables.Add("INSHOP");
                        InShop.Columns.Add("SHOPID", typeof(string));
                        InShop.Columns.Add("AREAID", typeof(string));

                        for (int i = 0; i < dgUserShopArea.GetRowCount(); i++)
                        {
                            if (Convert.ToBoolean(DataTableConverter.GetValue(dgUserShopArea.Rows[i].DataItem, "CHK")) == true)
                            {
                                // 이미 기 처리 된 건 PASS
                                if (!string.Equals(DataTableConverter.GetValue(dgUserShopArea.Rows[i].DataItem, "FIRST_CHK"), "Y"))
                                {
                                    row = InShop.NewRow();
                                    row["SHOPID"] = Util.NVC(DataTableConverter.GetValue(dgUserShopArea.Rows[i].DataItem, "SHOPID"));
                                    row["AREAID"] = Util.NVC(DataTableConverter.GetValue(dgUserShopArea.Rows[i].DataItem, "AREAID"));
                                    inDataSet.Tables["INSHOP"].Rows.Add(row);
                                }
                            }
                        }

                        // INAUTH
                        DataTable InAuth = inDataSet.Tables.Add("INAUTH");
                        InAuth.Columns.Add("AUTHID", typeof(string));

                        for (int i = 0; i < dgUserAuth.GetRowCount(); i++)
                        {
                            if (Convert.ToBoolean(DataTableConverter.GetValue(dgUserAuth.Rows[i].DataItem, "CHK")) == true)
                            {
                                // 이미 기 처리 된 건 PASS
                                if (!string.Equals(DataTableConverter.GetValue(dgUserAuth.Rows[i].DataItem, "CHK"), "Y"))
                                {
                                    row = InAuth.NewRow();
                                    row["AUTHID"] = Util.NVC(DataTableConverter.GetValue(dgUserAuth.Rows[i].DataItem, "AUTHID"));
                                    inDataSet.Tables["INAUTH"].Rows.Add(row);
                                }
                            }
                        }

                        if (inDataSet.Tables[1].Rows.Count == 0 && inDataSet.Tables[2].Rows.Count == 0)
                        {
                            Util.MessageValidation("SFU1566"); // 변경된 데이터가 없습니다.
                            return;
                        }

                        new ClientProxy().ExecuteService_Multi("BR_BAS_REG_TB_MMD_USER_AUTH_CONN", "INDATA,INSHOP,INAUTH", null, (result, searchException) =>
                        {
                            if (searchException != null)
                            {
                                Util.MessageException(searchException);
                                return;
                            }

                            Util.MessageInfo("SFU5156", (sresultEnd) =>
                            {
                                //System.Diagnostics.Process.GetCurrentProcess().Kill();
                                Application.Current.MainWindow.Close();
                            });                        

                        }, inDataSet);
                    }
                    catch (Exception ex) { Util.MessageException(ex); }
                }
            });
        }
    }
}
