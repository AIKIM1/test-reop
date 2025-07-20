
using C1.WPF;
using C1.WPF.DataGrid;
using C1.WPF.Excel;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Media;
using System.Threading;
using System.Threading.Tasks;





namespace LGC.GMES.MES.COM001
{
    /// <summary>
    /// Interaction logic for STK_CARRIER_PRINT.xaml
    /// </summary>
    public partial class COM001_371 : UserControl, IWorkArea
    {

        
        Util _Util = new Util();

        private const string _COMPLIANCE_IDICATOR1 = "_5B";
        private const string _COMPLIANCE_IDICATOR2 = "_29";
        private const string _COMPLIANCE_IDICATOR3 = "_3E";
        private const string _RECORD_SEPARATOR = "_1E";
        private const string _DATA_FORMAT = "06";
        private const string _GROUP_SEPARATOR = "_1D";
        private const string _END_OF_TRAILER = "_04";
        private const string _CR = "_0A";
        private const string _LF = "_0D";




        string sPrt = string.Empty;
        string sRes = string.Empty;
        string sCopy = string.Empty;
        string sXpos = string.Empty;
        string sYpos = string.Empty;
        string sDark = string.Empty;
        DataRow drPrtInfo = null;



        DataTable RQSTDT;



        //프린터
        private string _equipmentCode;







        public COM001_371()
        {
            InitializeComponent();

            InitCombo();

            

        }
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }


        private void InitCombo()
        {
            //combobox setting
            CommonCombo _combo = new CommonCombo();

            _combo.SetCombo(cboArea, CommonCombo.ComboStatus.SELECT, sCase: "cboArea");
            cboArea.SelectedValue = LoginInfo.CFG_AREA_ID;
            //동 세팅

       

            //C1ComboBox[] cboCSTTypeChild = { cboUseMaterial };
            //_combo.SetCombo(cboCSTType,CommonCombo.ComboStatus.SELECT, sCase : "COMMCODE",sFilter :   new String[] { "CSTTYPE",null} );
            //CST유형 세팅


            SetCSTTYPE(cboCSTType);

            SetCSTPROD(cboUseMaterial);
            //자재

            _combo.SetCombo(cboPolarity, CommonCombo.ComboStatus.ALL, sCase: "POLARITY", sFilter: new String[] { "ELTR_POLAR_CODE",null });
            //극성 세팅

            getCstLabelCodeList();
            //Carrier Label Code 세팅




            cboPrintQty.Items.Add("1");//되나
            cboPrintQty.Items.Add("2");
            cboPrintQty.Items.Add("3");
            cboPrintQty.Items.Add("4");
            cboPrintQty.SelectedItem = "1";



            txtCSTIDMIN.Visibility = Visibility.Visible;
            between_minmax.Visibility = Visibility.Collapsed;
            txtCSTIDMAX.Visibility = Visibility.Collapsed;


        }

        private void CHK_ALL_CST_CHECK(object sender, EventArgs e)
        {


            for (int i = 0; i < dgCSTList.GetRowCount(); i++)
            {

                dgCSTList.Rows[i].DataItem.SetValue("CHK", true);
            }
            //dgCSTList.Refresh();

        }

        private void CHK_ALL_CST_UNCHECK(object sender, EventArgs e)
        {


            for (int i = 0; i < dgCSTList.GetRowCount(); i++)
            {

                dgCSTList.Rows[i].DataItem.SetValue("CHK", false);
            }
            //dgCSTList.Refresh();

        }


        private void CHK_Search_chk(object sender, EventArgs e)
        {
            txtCSTIDMIN.Visibility = Visibility.Visible;
            between_minmax.Visibility = Visibility.Visible;
            txtCSTIDMAX.Visibility = Visibility.Visible;
        }

       
        private void CHK_Search_unchk(object sender, EventArgs e)
        {
            txtCSTIDMIN.Visibility = Visibility.Visible;
            between_minmax.Visibility = Visibility.Collapsed;
            txtCSTIDMAX.Visibility = Visibility.Collapsed;
        }


        private void cboCSTType_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {


            SetCSTPROD(cboUseMaterial);

        }
        private void SetCSTPROD(C1ComboBox cbo)
        {
            string selectedCstType = string.IsNullOrEmpty(cboCSTType.SelectedValue.GetString()) ? null : cboCSTType.SelectedValue.GetString();

            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1"};
            string[] arrCondition = { LoginInfo.LANGID, "CSTPROD", selectedCstType };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText,string.Empty);
        }


        private void SetCSTTYPE(C1ComboBox cbo)
        {

            const string bizRuleName = "DA_BAS_SEL_COMMCODE_ATTR_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE"};
            string[] arrCondition = { LoginInfo.LANGID, "CARRIER_LABEL_PRINT_CSTTYPE"};
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.SELECT, selectedValueText, displayMemberText, string.Empty);
        }

        // 2024,01.23 남재현 : 공통코드 사용 Carrier Label Code List 리스트 조회
        private bool getCstLabelCodeList()
        {
            bool bFlag = false;

            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CMCDTYPE", typeof(string));
                RQSTDT.Columns.Add("LANGID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["CMCDTYPE"] = "CARRIER_LABEL_CODE";
                dr["LANGID"] = LoginInfo.LANGID;
                RQSTDT.Rows.Add(dr);

                DataTable dtResult = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_COMMONCODE_TBL", "RQSTDT", "RSLTDT", RQSTDT);

                if (dtResult.Rows.Count > 0)
                {
                    cboLabelCode.DisplayMemberPath = "CMCDNAME";
                    cboLabelCode.SelectedValuePath = "CMCODE";
                    cboLabelCode.ItemsSource = dtResult.Copy().AsDataView();
                    cboLabelCode.SelectedIndex = 0;


                    bFlag = true;
                }

                return bFlag;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }






        //cboUseMaterial.Clear();






        /*private static void SetCarrierProdCombo(C1ComboBox cbo, C1ComboBox cboParent)
        {

            string CSTTYPE = string.IsNullOrEmpty(cboUseMaterial.SelectedValue.GetString()) ? null : cboUseMaterial.SelectedValue.GetString();
            


            const string bizRuleName = "DA_BAS_SEL_COMMCODE_CBO";
            string[] arrColumn = { "LANGID", "CMCDTYPE", "ATTRIBUTE1" };
            string[] arrCondition = { LoginInfo.LANGID, "CSTPROD", cboParent.SelectedValue.ToString() };
            string selectedValueText = cbo.SelectedValuePath;
            string displayMemberText = cbo.DisplayMemberPath;

            CommonCombo.CommonBaseCombo(bizRuleName, cbo, arrColumn, arrCondition, CommonCombo.ComboStatus.ALL, selectedValueText, displayMemberText, string.Empty);
        }
        */





        //search버튼누르면

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
  
            if (CHKsearchmethod.IsChecked == true)
            {
                SetList();
            }
            else if (CHKsearchmethod.IsChecked == false)
            {
                SetList_Search();
            }
        }

        private void SetList()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CHK", typeof(Boolean));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CSTTYPE", typeof(string));
                RQSTDT.Columns.Add("CSTUSEMATERIAL", typeof(string));


                RQSTDT.Columns.Add("POLARITY", typeof(string));

                RQSTDT.Columns.Add("CSTIDMIN", typeof(string));
                RQSTDT.Columns.Add("CSTIDMAX", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;




                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["CSTTYPE"] = Util.GetCondition(cboCSTType, "SFU8509"); //carrier 유형을 입력해주세요.
                if (dr["CSTTYPE"].Equals("")) return;

                if(txtCSTIDMIN.Text.Length != 10 || txtCSTIDMAX.Text.Length !=10 )
                {
                     Util.Alert("SFU8513"); return;//범위 검색을 위해서는 양쪽 CSTID를 10자리씩 입력해주세요.

                }

                if (!(txtCSTIDMAX.Text.Substring(0,6).Equals(txtCSTIDMIN.Text.Substring(0, 6))))
                {
                    Util.Alert("SFU8514"); return;//범위 검색을 위해서는 앞자리 6자리가 서로 같아야합니다.

                }

                //all일경우 STRING "" 로 값 들어오는것 변환
                if (!string.IsNullOrEmpty(cboPolarity.SelectedValue.ToString()))
                {
                    dr["POLARITY"] = cboPolarity.SelectedValue;
                }
                else
                    dr["POLARITY"] = null;

                //all일경우 STRING "" 로 값 들어오는것 변환
                if (!string.IsNullOrEmpty(cboCSTType.SelectedValue.ToString()))
                {
                    dr["CSTTYPE"] = cboCSTType.SelectedValue;
                }
                else
                    dr["CSTTYPE"] = null;
                //all일경우 STRING ""또는 NULL로 값 들어오는것 변환
                if (cboUseMaterial.SelectedValue != null)
                {
                    if (cboUseMaterial.SelectedValue.ToString() == "")
                    {
                        dr["CSTUSEMATERIAL"] = null;
                    }
                    else
                        dr["CSTUSEMATERIAL"] = cboUseMaterial.SelectedValue;
                }
                else
                    dr["CSTUSEMATERIAL"] = null;

                //a STRING "" 로 값 들어오는것 null로 변환
                if (!string.IsNullOrEmpty(txtCSTIDMIN.Text) && !string.IsNullOrEmpty(txtCSTIDMIN.Text))
                {

                    dr["CSTIDMIN"] = txtCSTIDMIN.Text;
                    dr["CSTIDMAX"] = txtCSTIDMAX.Text;

                }
                else
                {
                    dr["CSTIDMIN"] = null;
                    dr["CSTIDMAX"] = null;
                }



                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WITH_RANGE", "RQSTDT", "RSLTDT", RQSTDT);//
                if(dtRslt.Rows.Count > 500)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8508"), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);//500 이상 조회불가
                }
                else if(dtRslt.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("101471"), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);//조회된 결과가 없습니다
                }
                else
                {
                    Util.GridSetData(dgCSTList, dtRslt, FrameOperation);
                }
                

                /*new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER_WITH_RANGE", "INDATA", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgCSTList, searchResult, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
                */



            }


            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void SetList_Search()
        {
            try
            {
                ShowLoadingIndicator();

                DataTable RQSTDT = new DataTable();

                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("CHK", typeof(Boolean));
                RQSTDT.Columns.Add("LANGID", typeof(string));
                RQSTDT.Columns.Add("AREAID", typeof(string));
                RQSTDT.Columns.Add("CSTTYPE", typeof(string));
                RQSTDT.Columns.Add("CSTUSEMATERIAL", typeof(string));


                RQSTDT.Columns.Add("POLARITY", typeof(string));

                RQSTDT.Columns.Add("CSTIDMIN", typeof(string));
                RQSTDT.Columns.Add("CSTIDMAX", typeof(string));


                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;




                dr["AREAID"] = Util.GetCondition(cboArea, "SFU3203"); //동은필수입니다.
                if (dr["AREAID"].Equals("")) return;

                dr["CSTTYPE"] = Util.GetCondition(cboCSTType, "SFU8509"); //carrier 유형을 입력해주세요.
                if (dr["CSTTYPE"].Equals("")) return;

                //all일경우 STRING "" 로 값 들어오는것 변환
                if (!string.IsNullOrEmpty(cboPolarity.SelectedValue.ToString()))
                {
                    dr["POLARITY"] = cboPolarity.SelectedValue;
                }
                else
                    dr["POLARITY"] = null;

                //all일경우 STRING "" 로 값 들어오는것 변환
                if (!string.IsNullOrEmpty(cboCSTType.SelectedValue.ToString()))
                {
                    dr["CSTTYPE"] = cboCSTType.SelectedValue;
                }
                else
                    dr["CSTTYPE"] = null;
                //all일경우 STRING ""또는 NULL로 값 들어오는것 변환
                if (cboUseMaterial.SelectedValue != null)
                {
                    if (cboUseMaterial.SelectedValue.ToString() == "")
                    {
                        dr["CSTUSEMATERIAL"] = null;
                    }
                    else
                        dr["CSTUSEMATERIAL"] = cboUseMaterial.SelectedValue;
                }
                else
                    dr["CSTUSEMATERIAL"] = null;

                //a STRING "" 로 값 들어오는것 null로 변환
                if (!string.IsNullOrEmpty(txtCSTIDMIN.Text) && !string.IsNullOrEmpty(txtCSTIDMIN.Text))
                {

                    dr["CSTIDMIN"] = txtCSTIDMIN.Text;
                    dr["CSTIDMAX"] = txtCSTIDMAX.Text;

                }
                else
                {
                    dr["CSTIDMIN"] = null;
                    dr["CSTIDMAX"] = null;
                }



                RQSTDT.Rows.Add(dr);

                DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_BAS_SEL_CARRIER_WITH_SEARCH", "RQSTDT", "RSLTDT", RQSTDT);//
                if (dtRslt.Rows.Count > 500)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU8508"), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);//500 이상 조회불가
                }
                else if (dtRslt.Rows.Count == 0)
                {
                    LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("101471"), "", "Info", MessageBoxButton.OK, MessageBoxIcon.None);//조회된 결과가 없습니다
                }
                else
                {
                    Util.GridSetData(dgCSTList, dtRslt, FrameOperation);
                }


                /*new ClientProxy().ExecuteService("DA_BAS_SEL_CARRIER_WITH_RANGE", "INDATA", "RSLTDT", RQSTDT, (searchResult, searchException) =>
                {
                    try
                    {
                        if (searchException != null)
                        {
                            Util.MessageException(searchException);
                            return;
                        }
                        Util.GridSetData(dgCSTList, searchResult, FrameOperation, false);
                    }
                    catch (Exception ex)
                    {
                        Util.MessageException(ex);
                    }
                    finally
                    {
                        HiddenLoadingIndicator();
                    }
                }
                );
                */









            }


            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                HiddenLoadingIndicator();
            }
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











        //print버튼 누르면


        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {

            bool CarrierExist = false;

            for (int i = 0; i < dgCSTList.GetRowCount(); i++)
            {
                if (Util.NVC(DataTableConverter.GetValue(dgCSTList.Rows[i].DataItem, "CHK")) == "1")
                {
                    CarrierExist = true;
                    break;
                }
            }

            if (!CarrierExist)
            {
                //Util.Alert("선택된 Carrier가 존재 하지 않습니다.");
                Util.MessageValidation("SFU10013");
                return;
                

            }

            _Util.GetConfigPrintInfo(out sPrt, out sRes, out sCopy, out sXpos, out sYpos, out sDark, out drPrtInfo);


            Print_Label( sPrt,  sRes,  sXpos,  sYpos);

           
            //DataTable dtprint = Util.
            //PrintLabel();//zplcode, drPrtInfo 필요


        }


        private void Print_Label(string sPrt, string sRes, string sXpos, string sYpos)
        {
            try
            {
                DataTable dtLog = GetLogTable();

                DataTable dtRqst = new DataTable();
                dtRqst.Columns.Add("LBCD", typeof(string));
                dtRqst.Columns.Add("PRMK", typeof(string));
                dtRqst.Columns.Add("RESO", typeof(string));
                dtRqst.Columns.Add("PRCN", typeof(string));
                dtRqst.Columns.Add("MARH", typeof(string));
                dtRqst.Columns.Add("MARV", typeof(string));
                dtRqst.Columns.Add("ATTVAL001", typeof(string));
                dtRqst.Columns.Add("ATTVAL002", typeof(string));

                DataRow dr = dtRqst.NewRow();
                dtRqst.Rows.Add(dr);

                string ZPL_list = "";

                for (int i = 0; i < dgCSTList.GetRowCount(); i++)
                {
                    if(Util.NVC(DataTableConverter.GetValue(dgCSTList.Rows[i].DataItem, "CHK")) == "1")
                    {
                        // 2024,01.23 남재현 : 공통코드 사용 Carrier Label Code (하드 코딩 제거)
                        if (cboLabelCode.SelectedValue == null)
                        {
                            //Util.Alert("라벨 유형을 선택하세요.");
                            Util.MessageValidation("SFU10011");
                            return;
                        }

                        dtRqst.Rows[0]["LBCD"] = cboLabelCode.SelectedValue.ToString(); // 공통코드 내 라벨유형 선택.
                        dtRqst.Rows[0]["PRMK"] = sPrt;
                        dtRqst.Rows[0]["RESO"] = sRes;
                        dtRqst.Rows[0]["PRCN"] = cboPrintQty.Text;
                        dtRqst.Rows[0]["MARH"] = sXpos;
                        dtRqst.Rows[0]["MARV"] = sYpos;
                        dtRqst.Rows[0]["ATTVAL001"] = Util.NVC(DataTableConverter.GetValue(dgCSTList.Rows[i].DataItem, "CSTID"));
                        dtRqst.Rows[0]["ATTVAL002"] = Util.NVC(DataTableConverter.GetValue(dgCSTList.Rows[i].DataItem, "CSTID"));

                        DataTable dtRslt = new ClientProxy().ExecuteServiceSync("DA_COM_SEL_LABEL_DESIGN_CODE_ITEM_COMMON", "INDATA", "OUTDATA", dtRqst);

                        ZPL_list += dtRslt.Rows[0]["LABELCD"].ToString();
                    } 



                }
               
                PrintZPL(ZPL_list, drPrtInfo);



            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                //  LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage(ex), ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxIcon.Warning);
            }
        }


   



















        private DataTable GetLogTable()
        {
            DataTable dtLog = new DataTable();
            dtLog.Columns.Add("PALLETID", typeof(string));
            dtLog.Columns.Add("LABEL_CODE", typeof(string));
            dtLog.Columns.Add("LABEL_ZPL_CNTT", typeof(string));
            dtLog.Columns.Add("LABEL_PRT_COUNT", typeof(string));
            dtLog.Columns.Add("PRT_ITEM01", typeof(string));
            dtLog.Columns.Add("PRT_ITEM02", typeof(string));
            dtLog.Columns.Add("PRT_ITEM03", typeof(string));
            dtLog.Columns.Add("PRT_ITEM04", typeof(string));
            dtLog.Columns.Add("PRT_ITEM05", typeof(string));
            dtLog.Columns.Add("PRT_ITEM06", typeof(string));
            dtLog.Columns.Add("PRT_ITEM07", typeof(string));
            dtLog.Columns.Add("PRT_ITEM08", typeof(string));
            dtLog.Columns.Add("PRT_ITEM09", typeof(string));
            dtLog.Columns.Add("PRT_ITEM10", typeof(string));
            dtLog.Columns.Add("PRT_ITEM11", typeof(string));
            dtLog.Columns.Add("PRT_ITEM12", typeof(string));
            dtLog.Columns.Add("PRT_ITEM13", typeof(string));
            dtLog.Columns.Add("PRT_ITEM14", typeof(string));
            dtLog.Columns.Add("PRT_ITEM15", typeof(string));
            dtLog.Columns.Add("PRT_ITEM16", typeof(string));
            dtLog.Columns.Add("PRT_ITEM17", typeof(string));
            dtLog.Columns.Add("PRT_ITEM18", typeof(string));
            dtLog.Columns.Add("PRT_ITEM19", typeof(string));
            dtLog.Columns.Add("PRT_ITEM20", typeof(string));
            dtLog.Columns.Add("PRT_ITEM21", typeof(string));
            dtLog.Columns.Add("PRT_ITEM22", typeof(string));
            dtLog.Columns.Add("PRT_ITEM23", typeof(string));
            dtLog.Columns.Add("PRT_ITEM24", typeof(string));
            dtLog.Columns.Add("PRT_ITEM25", typeof(string));
            dtLog.Columns.Add("PRT_ITEM26", typeof(string));
            dtLog.Columns.Add("PRT_ITEM27", typeof(string));
            dtLog.Columns.Add("PRT_ITEM28", typeof(string));
            dtLog.Columns.Add("PRT_ITEM29", typeof(string));
            dtLog.Columns.Add("PRT_ITEM30", typeof(string));
            dtLog.Columns.Add("INSUSER", typeof(string));

            return dtLog;
        }

        //프린트 함수
        private bool PrintZPL(string sZPL, DataRow drPrtInfo)
        {
            if (drPrtInfo?.Table == null)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3030"));

                return false;
            }

            bool brtndefault = false;
            if (drPrtInfo.Table.Columns.Contains("PORTNAME") && drPrtInfo["PORTNAME"].ToString().Trim().Length > 0)
            {
                if (drPrtInfo["PORTNAME"].GetString().ToUpper().Equals("USB"))
                {
                    brtndefault = FrameOperation.Barcode_ZPL_USB_Print(sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else if (drPrtInfo["PORTNAME"].ToString().IndexOf("LPT", StringComparison.Ordinal) >= 0)
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_LPT_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
                else
                {
                    FrameOperation.PrintFrameMessage(string.Empty);
                    brtndefault = FrameOperation.Barcode_ZPL_Print(drPrtInfo, sZPL);
                    if (brtndefault == false)
                    {
                        loadingIndicator.Visibility = Visibility.Collapsed;
                        FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU1309"));
                    }
                }
            }
            else
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                FrameOperation.PrintFrameMessage(MessageDic.Instance.GetMessage("SFU3031"));
                Util.MessageValidation("SFU3031");
            }

            return brtndefault;
        }

        private void txtCSTIDMIN_TextChanged(object sender, TextChangedEventArgs e)
        {
     
        }



        //프린트

        //







    }





}