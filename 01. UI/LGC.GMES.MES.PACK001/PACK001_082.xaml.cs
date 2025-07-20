using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Input;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// PACK001_082.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_082 : UserControl
    {

        DataTable dtMain = new DataTable();

        public PACK001_082()
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
            setComboBox();
            
        }


        private void setComboBox()
        {
            try
            {

                CommonCombo _combo = new CommonCombo();


                //동
                String[] sFilter = { LoginInfo.CFG_SHOP_ID, Area_Type.PACK };
                C1ComboBox[] cboAreaChild = { cboEquipmentSegment };
                _combo.SetCombo(cboAreaByAreaType, CommonCombo.ComboStatus.ALL, cbChild: cboAreaChild, sFilter: sFilter);

                //라인
                C1ComboBox[] cboLineParent = { cboAreaByAreaType };
                C1ComboBox[] cboLineChild = { cboProduct };
                //C1ComboBox[] cboLineChild = { cboProcess, cboProductModel };
                //_combo.SetCombo(cboSearchEQSGID, CommonCombo.ComboStatus.SELECT, cbChild: cboLineChild, cbParent: cboLineParent);
                _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.ALL, cbParent: cboLineParent, cbChild: cboLineChild);



                C1ComboBox cboSHOPID = new C1ComboBox();
                cboSHOPID.SelectedValue = LoginInfo.CFG_SHOP_ID;
                //C1ComboBox cboPrdtClass = new C1ComboBox();
                //cboPrdtClass.SelectedValue = "";
                //C1ComboBox cboProductModel = new C1ComboBox();
                //cboPrdtClass.SelectedValue = "";
                // 제품
                C1ComboBox[] cboProductParent = { cboSHOPID, cboAreaByAreaType, cboEquipmentSegment};
                _combo.SetCombo(cboProduct, CommonCombo.ComboStatus.ALL, cbParent: cboProductParent, sCase: "PRJ_PRODUCT");

                SetVerifFlag();

            }
            catch (Exception ex)
            {

                throw ex;
            }
               
        }


        private void SetVerifFlag()
        {
            try
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("CBO_NAME", typeof(string));
                dt.Columns.Add("CBO_CODE", typeof(string));

                DataRow dr_ = dt.NewRow();
                dr_["CBO_NAME"] = "ALL";
                dr_["CBO_CODE"] = "ALL";
                dt.Rows.Add(dr_);

                DataRow dr = dt.NewRow();
                dr["CBO_NAME"] = "Y";
                dr["CBO_CODE"] = "Y";
                dt.Rows.Add(dr);

                DataRow dr1 = dt.NewRow();
                dr1["CBO_NAME"] = "N";
                dr1["CBO_CODE"] = "N";
                dt.Rows.Add(dr1);

                dt.AcceptChanges();

                cboSearchRESULT.ItemsSource = DataTableConverter.Convert(dt);
                cboSearchRESULT.SelectedIndex = 0; //default Y              
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }





        //조회 버튼 클릭 (pallet 정보 조회)
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {

            getPalletInfo(); // 팔레트 정보 조회 로직 DA1
            //getPalletLotInfo(); // LOTID CHECK 조회 로직 DA3

            txtLotID.Focus();



        }

        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.Key == Key.Enter)
            {
                getPalletInfo(); // 팔레트 정보 조회 로직 DA1

            }


        }



        // 조회 버튼 클릭 (pallet 검증 이력 조회)
        private void btnSearch1_Click(object sender, RoutedEventArgs e)
        {
            string[] sfilter = new string[] {
                cboAreaByAreaType.SelectedValue.ToString(),
                cboEquipmentSegment.SelectedValue.ToString(),              
                cboProduct.SelectedValue.ToString(),
                cboSearchRESULT.SelectedValue.ToString(),
                dtpDateFrom.SelectedDateTime.ToShortDateString(),
                dtpDateTo.SelectedDateTime.ToShortDateString()
            };

            getPalletList(sfilter);
            //Util.GridSetData(dgSearchResultList, dtMain, FrameOperation);





        }

        private void getPalletList(string[] filter)
        {

            DataTable result = new DataTable();
            try
            {
                DataTable inTable = new DataTable();

                inTable.Columns.Add("AREAID", typeof(string));
                inTable.Columns.Add("EQSGID", typeof(string));
                inTable.Columns.Add("PRODID", typeof(string));
                inTable.Columns.Add("STDTTM", typeof(string));
                inTable.Columns.Add("EDDTTM", typeof(string));
                inTable.Columns.Add("LANGID", typeof(string));
                inTable.Columns.Add("VERIF_FLAG", typeof(string));


                DataRow newRow = inTable.NewRow();
                newRow["AREAID"] = String.IsNullOrEmpty(filter[0]) || filter[0].Equals("ALL") ? null : filter[0];
                newRow["EQSGID"] = String.IsNullOrEmpty(filter[1]) || filter[1].Equals("ALL") ? null : filter[1];
                newRow["PRODID"] = String.IsNullOrEmpty(filter[2]) || filter[2].Equals("ALL") ? null : filter[2];
                newRow["VERIF_FLAG"] = String.IsNullOrEmpty(filter[3]) || filter[3].Equals("ALL") ? null : filter[3];
                newRow["STDTTM"] = dtpDateFrom.SelectedDateTime.ToShortDateString();
                newRow["EDDTTM"] = dtpDateTo.SelectedDateTime.ToShortDateString();
                newRow["LANGID"] = LoginInfo.LANGID;



                inTable.Rows.Add(newRow);

                result = new ClientProxy().ExecuteServiceSync("DA_SEL_PALLET_VERIF_HIST", "RQSTDT", "RSLTDT", inTable);
                Util.GridSetData(dgSearchResultList, result, FrameOperation, true);

                tbSearchListCount.Text = "[ " + result.Rows.Count.ToString() + " 건 ]";


            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }


        }



        // 바코드 리딩
        private void txtLotID_KeyDown(object sender, KeyEventArgs e)
        {
            

            if (e.Key == Key.Enter)
            {
                // 찍힌 lotid가 전산 상에 존재하는 lotid랑 같은지 비교

                if(chkInputLot(txtLotID.Text))
                {


                    // LOT 의 검증 FLAG 업데이트

                    if(UPD_LOT_CHK_FLAG())
                    {
                        //reload
                        getPalletLotInfo(txtTagetPalletID.Text);

                    }


                    
                }
                txtLotID.Text = String.Empty;
                txtLotID.Focus();

            }

        }

        // 바코드 리딩 된 LOT의 2ND_OCV 통과 여부 체크 및 검증 FLAG 업데이트
        private bool UPD_LOT_CHK_FLAG()
        {

            DataTable result = new DataTable();

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("LOTID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();

                dr["LOTID"] = txtLotID.Text;
                dr["PALLETID"] = txtTagetPalletID.Text;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                result = new ClientProxy().ExecuteServiceSync("BR_PRD_REG_2ND_OCV_LOT_VERIF", "INDATA", null, RQSTDT);
                txtLotID.Text = String.Empty;
                return true;
            }
            catch (Exception ex)
            {
               
               Util.MessageException(ex);
                txtLotID.Text = String.Empty;

                return false;

                
            }



        

        }


        // 찍힌 lotid가 전산 상에 존재하는 lotid랑 같은지 비교 => 바코드 리딩
        private bool chkInputLot(string text)
        {

            DataTable dt = DataTableConverter.Convert(dgTargetList.ItemsSource);

            if(dt.Rows.Count == 0)
            {
                return false;
            }
            else
            {

                for (int i = 0; i < dt.Rows.Count; i++)
                {

                    if (text == dt.Rows[i]["LOTID"].ToString())
                    {
                        if (string.IsNullOrWhiteSpace(dt.Rows[i]["CHECKED"].ToString()))
                        {
                            return true;

                        }
                        else
                        {
                            Util.MessageInfo("SFU1914");  // 중복 스캔되었습니다.
                            return false;
                        }
                        
                    }
                }


            }

            return false;
        }



        //검증 완료 버튼 클릭  : 포장수량 = 검증수량 => 2nd ocv 통과 수량 = 검증수량
        private void btnComfirm_Click(object sender, RoutedEventArgs e)
        {

            if (String.IsNullOrWhiteSpace(txtTargetBoxCnt.Text) == false && String.IsNullOrWhiteSpace(txtTargetChkCnt.Text) == false)
            {
                if ((Convert.ToInt32(float.Parse(txtTargetBoxCnt.Text)) == Convert.ToInt32(txtTargetChkCnt.Text)))
                {

                    DataTable RQSTDT1 = new DataTable();
                    RQSTDT1.TableName = "RQSTDT";
                    RQSTDT1.Columns.Add("LANGID", typeof(string));
                    RQSTDT1.Columns.Add("PALLETID", typeof(string));

                    DataRow dr1 = RQSTDT1.NewRow();
                    dr1["LANGID"] = LoginInfo.LANGID;
                    dr1["PALLETID"] = txtTagetPalletID.Text;

                    RQSTDT1.Rows.Add(dr1);


                    DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLLT_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

                    string flag = "OK";

                    if (flag.Equals(dtResult1.Rows[0]["VERIF_FLAG"]))
                    {
                        Util.MessageValidation("SFU8365");// 이미 검증된 PALLET 입니다.

                    }
                    else
                    {
                        UPD_BOX_CHK_FLAG();
                        Util.MessageInfo("SFU1275"); // 정상처리 되었습니다.
                    }

                }
                else
                {
                    Util.MessageValidation("SFU8364"); // 검증 수량이 부족합니다.
                }


            }



        }




        private void UPD_BOX_CHK_FLAG()
        {
            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";
            RQSTDT.Columns.Add("PALLETID", typeof(string));
            RQSTDT.Columns.Add("USERID", typeof(string));

            DataRow dr = RQSTDT.NewRow();

            dr["PALLETID"] = txtTagetPalletID.Text;
            dr["USERID"] = LoginInfo.USERID;

            RQSTDT.Rows.Add(dr);

            new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_BOX_VERIF_FLAG", "RQSTDT", null, RQSTDT);
            
        }



        // 2nd ocv 검사 완료 체크 DA5

        /*
        private bool CHK_2ND_OCV_INFO()
        {

            DataTable RQSTDT = new DataTable();
            RQSTDT.TableName = "RQSTDT";              
            RQSTDT.Columns.Add("PALLETID", typeof(string));

            DataRow dr = RQSTDT.NewRow();
              
            dr["PALLETID"] = txtPalletID.Text;

            RQSTDT.Rows.Add(dr);

            DataTable dtResult = new ClientProxy().ExecuteServiceSync("추가", "RQSTDT", "RSLTDT", RQSTDT);


            string chkCNT = Util.NVC(dtResult.Rows[0]["CHKCNT"]);

            if ( String.IsNullOrWhiteSpace(txtTargetBoxCnt.Text) || (Convert.ToInt32(chkCNT) > 0)) // null 조건 추가
            {
                return false;
            }

            return true;


        }
        */


        //조회 로직 DA1
        private void getPalletInfo()
        {
            try
            {
                    DataSet dsIndata = new DataSet();
               
                    DataTable dtIndata = new DataTable();
                    dtIndata.TableName = "INDATA";
                    dtIndata.Columns.Add("LANGID", typeof(string));
                    dtIndata.Columns.Add("PALLETID", typeof(string));

                    DataRow dr = dtIndata.NewRow();
                    dr["LANGID"] = LoginInfo.LANGID;
                    dr["PALLETID"] = txtPalletID.Text;

                    dtIndata.Rows.Add(dr);
                    dsIndata.Tables.Add(dtIndata);
                    string box = "BOX";
                
                    //DataTable dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_SEL_GET_PLT_VERIFY_INFO", "INDATE", "RSLTDT", RQSTDT);
                    DataSet dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_SEL_GET_PLT_VERIFY_INFO", "INDATA", "OUTDATA,OUT_LOT,OUT_CNT", dsIndata, null);

                    if (dsResult.Tables["OUTDATA"].Rows.Count == 0)
                    {
                        txtTagetPalletID.Text = string.Empty;
                        txtTargetPackDttm.Text = string.Empty;
                        txtTargetProdID.Text = string.Empty;
                        txtTargetLine.Text = string.Empty;
                        txtTargetBoxCnt.Text = string.Empty;
                        txtTargetChkCnt.Text = string.Empty;
                        Util.gridClear(dgTargetList);
                        Util.MessageValidation("SFU4245");  
                    }
                    else if (dsResult.Tables["OUTDATA"].Rows[0]["BOXTYPE"].Equals(box))
                        {
                            txtTagetPalletID.Text = string.Empty;
                            txtTargetPackDttm.Text = string.Empty;
                            txtTargetProdID.Text = string.Empty;
                            txtTargetLine.Text = string.Empty;
                            txtTargetBoxCnt.Text = string.Empty;
                            txtTargetChkCnt.Text = string.Empty;

                            Util.gridClear(dgTargetList);
                            Util.MessageValidation("SFU8366"); //  BOX 타입은 조회할 수 없습니다.
                    
                        }
                    else
                    {
                        txtTagetPalletID.Text = dsResult.Tables["OUTDATA"].Rows[0]["BOXID"].ToString();
                        txtTargetPackDttm.Text = dsResult.Tables["OUTDATA"].Rows[0]["PACKDTTM"].ToString();
                        txtTargetProdID.Text = dsResult.Tables["OUTDATA"].Rows[0]["PRODID"].ToString();
                        txtTargetLine.Text = dsResult.Tables["OUTDATA"].Rows[0]["EQSGNAME"].ToString();
                        txtTargetBoxCnt.Text = dsResult.Tables["OUTDATA"].Rows[0]["LOTQTY"].ToString();
                        Util.GridSetData(dgTargetList, dsResult.Tables["OUT_LOT"], FrameOperation, true);
                        txtTargetChkCnt.Text = Util.NVC(dsResult.Tables["OUT_CNT"].Rows[0]["CHKCNT"]);
                        txtLotID.Focus();
                }
                }
            catch (Exception ex)
            {
                txtPalletID.Clear();
                txtPalletID.Focus();
                Util.MessageException(ex);
            }
        }

        //조회 로직 DA3
        private void getPalletLotInfo(string Pallet)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = Pallet;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLLT_LOT_CHK", "RQSTDT", "RSLTDT", RQSTDT);
                DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLLT_VERIF_CNT", "RQSTDT", "RSLTDT", RQSTDT);


                //dgSearchResultList.ItemsSource = DataTableConverter.Convert(dtResult);

                if (dtResult.Rows.Count == 0)
                {
                    Util.gridClear(dgTargetList);
                    txtTargetChkCnt.Text = String.Empty;
                }
                else
                {
                    Util.GridSetData(dgTargetList, dtResult, FrameOperation, true);
                    txtTargetChkCnt.Text = Util.NVC(dtResult1.Rows[0]["CHKCNT"]);
                }

                //Util.SetTextBlockText_DataGridRowCount(tbSearchListCount, Util.NVC(dtResult.Rows.Count));

            }
            catch (Exception ex)
            {
                //Util.AlertByBiz("DA_PRD_SEL_RECEIVE_PRODUCT_PACK", ex.Message, ex.ToString());
                Util.MessageException(ex);
            }
        }





        // 해당 PALLET 내 LOT 검증 초기화
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {

            if(string.IsNullOrWhiteSpace(txtTagetPalletID.Text) == false)
            {
                Util.MessageConfirm("SFU3440", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        Reset_VERIF_FLAG();
                        //Util.MessageInfo("SFU3377");
                        btnSearch_Click(null, null);
                    }
                });
            }
            

            


        }

        private void Reset_VERIF_FLAG()
        {




            DataTable RQSTDT1 = new DataTable();
            RQSTDT1.TableName = "RQSTDT";
            RQSTDT1.Columns.Add("LANGID", typeof(string));
            RQSTDT1.Columns.Add("PALLETID", typeof(string));

            DataRow dr1 = RQSTDT1.NewRow();
            dr1["LANGID"] = LoginInfo.LANGID;
            dr1["PALLETID"] = txtTagetPalletID.Text;

            RQSTDT1.Rows.Add(dr1);


            DataTable dtResult1 = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_PLLT_INFO", "RQSTDT", "RSLTDT", RQSTDT1);

            string flag = "OK";

            if (flag.Equals(dtResult1.Rows[0]["VERIF_FLAG"]))
            {
                Util.MessageValidation("SFU8365");// 이미 검증된 PALLET 는 초기화 할 수 없습니다.

            }
            else
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                //RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("PALLETID", typeof(string));
                RQSTDT.Columns.Add("USERID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                //dr["LANGID"] = LoginInfo.LANGID;
                dr["PALLETID"] = txtTagetPalletID.Text;
                dr["USERID"] = LoginInfo.USERID;
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("DA_PRD_UPD_RESET_PLLT_VERIF_FLAG", "RQSTDT", null, RQSTDT);             
            }





            






        }

        private void dgTargetList_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            try
            {
                C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

                dataGrid.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (e.Cell.Presenter == null)
                    {
                        return;
                    }
                    if (e.Cell.Column.Name == "CHECKED")
                    {

                        //e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        //e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#72166B"));
                        e.Cell.Presenter.Foreground = new SolidColorBrush(Colors.Blue);
                        e.Cell.Presenter.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        e.Cell.Presenter.Foreground = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF333333"));
                        e.Cell.Presenter.FontWeight = FontWeights.Normal;
                    }
                }));


            }
            catch (Exception ex)
            {

                Util.Alert(ex.ToString());
            }
        }

        
    }
}
