/*************************************************************************************
Created Date : 2019.11.08
      Creator : 염규범
   Decription : 출하처 인쇄 항목 체크 리스트
--------------------------------------------------------------------------------------
 [Change History]
  2019.11.08 염규범A : Initial Created.

**************************************************************************************/


using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Collections.Generic;
using System.Windows.Threading;

namespace LGC.GMES.MES.PACK001
{
    /// <summary>
    /// Window1.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class PACK001_000_LABEL_CHECK_POPUP : C1Window, IWorkArea
    {
        #region Declaration & Constructor
        private string strLabelCode = string.Empty;
        private string strEqsgID = string.Empty;
        private string strProcID = string.Empty;
        private string strEqptId = string.Empty;
        private string strProdId = string.Empty;
        private string strWoId = string.Empty;
        private string strLastView = string.Empty;
        private string strDPI = string.Empty;

        private int iChkItemCnt;

        public delegate void EventFormClosed();
        public event EventFormClosed FormClosed;

        
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
        public PACK001_000_LABEL_CHECK_POPUP()
        {
            InitializeComponent();
            

        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                object[] tmps = C1WindowExtension.GetParameters(this);

                if (tmps != null && tmps.Length >= 1)
                {
                    strLabelCode = Util.NVC(tmps[0]);
                    strEqsgID = Util.NVC(tmps[1]);
                    //strProcID = Util.NVC(tmps[2]);
                    //strEqptId = Util.NVC(tmps[3]);
                    strProdId = Util.NVC(tmps[2]);
                    strWoId = Util.NVC(tmps[3]);
                    strLastView = Util.NVC(tmps[4]);
                    //strProcID = Util.NVC(tmps[5]);    

                }
                else
                {
                    Util.MessageConfirm("정상적인 접근이 아닙니다.", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.Close();
                        }
                    });
                }

                DataTable dtContext = dgLablePrintSettingContext(strEqsgID, strProcID, strLabelCode, strProdId);

                if (string.IsNullOrEmpty(dtContext.Rows[0]["PRTR_DPI"].ToString()))
                {
                    this.Close();
                }
                else
                {
                    strDPI = dtContext.Rows[0]["PRTR_DPI"].ToString();
                }

                if (strLastView.Equals("Y"))
                {
                    

                    if (chkOverLap(dtContext))
                    {
                        LastestLableItemSampleValue(strLabelCode, strEqsgID, strProcID, strProdId);
                        CHK.Visibility = Visibility.Collapsed;
                        btnSave.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        this.Close();
                    }
                }
                else if (strLastView.Equals("N"))
                {

                    if (chkOverLap(dtContext))
                    {
                        LableItemSampleValue(strLabelCode, strEqsgID, strProcID, strEqptId, strProdId);
                        USERNAME.Visibility = Visibility.Collapsed;
                        USERNAME.Width = new C1.WPF.DataGrid.DataGridLength(0);
                        UPDUSER.Visibility = Visibility.Collapsed;
                        UPDUSER.Width = new C1.WPF.DataGrid.DataGridLength(0);
                        UPDDTTM.Visibility = Visibility.Collapsed;
                        UPDDTTM.Width = new C1.WPF.DataGrid.DataGridLength(0);
                        DoEvents();
                        LablePreView();
                    }
                    else
                    {
                        this.Close();
                    }
                }
                else
                {

                    if (chkOverLap(dtContext))
                    {
                        LableItemSampleValue(strLabelCode, strEqsgID, strProcID, strEqptId, strProdId);
                        CHK.Visibility = Visibility.Collapsed;
                        btnSave.Visibility = Visibility.Collapsed;
                        USERNAME.Visibility = Visibility.Collapsed;
                        USERNAME.Width = new C1.WPF.DataGrid.DataGridLength(0);
                        UPDUSER.Visibility = Visibility.Collapsed;
                        UPDUSER.Width = new C1.WPF.DataGrid.DataGridLength(0);
                        UPDDTTM.Visibility = Visibility.Collapsed;
                        UPDDTTM.Width = new C1.WPF.DataGrid.DataGridLength(0);
                    }
                    else
                    {
                        this.Close();
                    }
                }

                //사용자권한별로 메뉴 숨기기
                string strAuthority = string.Empty;
                if (!LoginInfo.USERTYPE.Equals("G"))
                {
                    strAuthority = "R";
                }

                //사용자권한별로 메뉴 숨기기
                List<Button> listAuth = new List<Button>();
                listAuth.Add(btnSave);
                Util.pageAuth(listAuth, strAuthority);
                Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
                

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

            if (!preViewLable(strLabelCode, strProdId, strDPI))
            {
                btnSave.Visibility = Visibility.Collapsed;
                CHK.Visibility = Visibility.Collapsed;
                zplBrowser.Visibility = Visibility.Collapsed;
                labelAttachFile.Visibility = Visibility.Collapsed;
            }

        }
        #endregion

        #region [ EVENT ]

        #region [ 버튼 이벤트  ]

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int iRowCnt = dgLableItemDetail.Rows.Count();

            DataTable dtLableItemDetail = new DataTable();
            dtLableItemDetail = DataTableConverter.Convert(dgLableItemDetail.ItemsSource);

            dgLableItemDetail.Rows.Count();

            iChkItemCnt = 0;

            for (int i=0; i < iRowCnt; i++)
            {
                if (dtLableItemDetail.Rows[i]["CHK"].ToString().Equals("True")){
                    iChkItemCnt++;
                }
            }
            

            if (iRowCnt == iChkItemCnt)
            {
                Util.MessageConfirm("SFU1241", (result) =>// 저장 하시겠습니까?
                {
                    if (result == MessageBoxResult.OK)
                    {
                        LabelDataSave(dtLableItemDetail);

                        this.DataContext = strLabelCode;
                        this.DialogResult = MessageBoxResult.OK;
                        
                    }
                });
            }
            else
            {
                Util.MessageInfo("총 "+ iRowCnt +"개중 : " + iChkItemCnt + "확인 ");
            }
        }
        #endregion
        
        #endregion

        #region [ Method ]

        #region [ 라벨 샘플 값 ]
        private void LableItemSampleValue(string strLabelCode, string strEqsgID, string strProcID, string strEqptId, string strProdId)
        { 
            try
            {
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                //INDATA.Columns.Add("EQPTID", typeof(string));
                //INDATA.Columns.Add("PROCID", typeof(string));


                DataRow dr = INDATA.NewRow();

                dr["PRODID"] = strProdId;
                dr["EQSGID"] = strEqsgID;
                dr["LABEL_CODE"] = strLabelCode;
                dr["LABEL_TYPE"] = "PACK";
                //dr["EQPTID"] = strEqptId;
                //dr["PROCID"] = strProcID;

                INDATA.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LBL_ITEM_SAMPLE_VALUE", "INDATA", "OUTDATA", INDATA);

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgLableItemDetail, dtResult, FrameOperation, true);
                    txtLableId.Text = dtResult.Rows[0]["LABEL_CODE"].ToString() + "  ( " + strDPI + " Dpi )";
                    txtLableName.Text = dtResult.Rows[0]["LABEL_NAME"].ToString();
                }
                else
                {
                    Util.MessageInfo("데이터가 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        #endregion

        #region [ 마지막 라벨 샘플 값 ]
        private void LastestLableItemSampleValue(string strLabelCode, string strEqsgID, string strProcID, string strProdId)
        {
            try
            {
                DataTable INDATA = new DataTable();

                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("WOID", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("EQSGID", typeof(string));
                //INDATA.Columns.Add("PROCID", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));


                DataRow dr = INDATA.NewRow();

                dr["WOID"] = strWoId;
                dr["LABEL_CODE"] = strLabelCode;
                dr["EQSGID"] = strEqsgID;
                //dr["PROCID"] = strProcID;
                dr["PRODID"] = strProdId;

                INDATA.Rows.Add(dr);

                DataTable dtResult = null;
                dtResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_LBL_ITEM_BY_PRODID_LASTEST", "INDATA", "OUTDATA", INDATA);

                if (dtResult.Rows.Count > 0)
                {
                    Util.GridSetData(dgLableItemDetail, dtResult, FrameOperation, true);
                    txtLableId.Text = dtResult.Rows[0]["LABEL_CODE"].ToString() + "  ( " + strDPI + " Dpi )";
                    txtLableName.Text = dtResult.Rows[0]["LABEL_NAME"].ToString();
                }
                else
                {
                    Util.MessageInfo("데이터가 없습니다.");
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        #endregion

        #region [ 라벨 데이터 저장 ]
        private void LabelDataSave(DataTable dtLableItemDetail)
        {
            int index = dtLableItemDetail.Rows.Count;


            DataTable INDATA = new DataTable();

            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("WOID", typeof(string));
            INDATA.Columns.Add("LABEL_CODE", typeof(string));
            INDATA.Columns.Add("SHIPTO_ID", typeof(string));
            INDATA.Columns.Add("MNGT_ITEM_CODE", typeof(string));
            INDATA.Columns.Add("EQSGID", typeof(string));
            INDATA.Columns.Add("PRODID", typeof(string));
            INDATA.Columns.Add("PROCID", typeof(string));
            INDATA.Columns.Add("EQPTID", typeof(string));
            INDATA.Columns.Add("MNGT_ITEM_VALUE", typeof(string));
            INDATA.Columns.Add("SMPL_VALUE", typeof(string));
            INDATA.Columns.Add("INSUSER", typeof(string));
            INDATA.Columns.Add("UPDUSER", typeof(string));

            for (int i = 0; i < index; i++)
            {

                DataRow dr = INDATA.NewRow();

                dr["WOID"] = strWoId;
                dr["LABEL_CODE"] = dtLableItemDetail.Rows[i]["LABEL_CODE"].ToString();
                dr["SHIPTO_ID"] = dtLableItemDetail.Rows[i]["SHIPTO_ID"].ToString();
                dr["MNGT_ITEM_CODE"] = dtLableItemDetail.Rows[i]["ITEM_CODE"].ToString();
                dr["EQSGID"] = strEqsgID;
                dr["PRODID"] = strProdId;
                dr["PROCID"] = "";
                //dr["EQPTID"] = strEqptId;
                dr["EQPTID"] = "";
                dr["MNGT_ITEM_VALUE"] = dtLableItemDetail.Rows[i]["MNGT_ITEM_NAME"].ToString();
                dr["SMPL_VALUE"] = dtLableItemDetail.Rows[i]["SMAPLE_VALUE"].ToString();
                dr["INSUSER"] = LoginInfo.USERID;
                dr["UPDUSER"] = LoginInfo.USERID;

                INDATA.Rows.Add(dr);
            }

            new ClientProxy().ExecuteService("BR_PRD_REG_WO_LABEL_ITEM", "INDATA", null, INDATA, (result, searchException) =>
            {
                try
                {
                    if (searchException != null)
                    {
                        Util.MessageException(searchException);
                        return;
                    }

                    this.DialogResult = MessageBoxResult.OK;
                }
                catch (Exception ex)
                {
                    Util.MessageException(ex);
                }
                finally
                {
                    //저장 표시?
                }
            });
        }

        private DataTable dgLablePrintSettingContext(string strEqsgId, string strProcId, string strLabelCode, string strProdId)
        {

            try
            {
          
            DataTable INDATA = new DataTable();

            INDATA.TableName = "INDATA";
            INDATA.Columns.Add("LANGID",        typeof(string));
            INDATA.Columns.Add("EQSGID",        typeof(string));
            //INDATA.Columns.Add("PROCID",        typeof(string));
            INDATA.Columns.Add("LABEL_CODE", typeof(string));
            INDATA.Columns.Add("USE_FLAG",    typeof(string));
            INDATA.Columns.Add("PRODID",        typeof(string));


            DataRow dr = INDATA.NewRow();

            dr["LANGID"] = LoginInfo.LANGID;
            dr["EQSGID"] = strEqsgId;
            // dr["PROCID"] = strProcId;
            dr["USE_FLAG"] = "Y";
            dr["LABEL_CODE"] = strLabelCode;
            dr["PRODID"] = strProdId;

            INDATA.Rows.Add(dr);

            DataTable dtResult = null;
            dtResult = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_TB_SFC_EQPT_LABEL", "INDATA", "OUTDATA", INDATA);
                return dtResult;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return null;
            }
        }

        private Boolean chkOverLap(DataTable dtChkOverLap)
        {
            try
            {
                //int iRowCnt = dtChkOverLap.Rows.Count;
                DataTable distinctTable = dtChkOverLap.DefaultView.ToTable(true, "PRTR_DPI");

                if (!(distinctTable.Rows.Count > 1))
                {
                    return true;
                }
                else
                {
                    Util.MessageInfo("DPI가 다른 라벨이 프린트 세팅되어 있습니다. DPI 확인이 필요합니다.");
                    return false;
                }
                
            }
            catch(Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private Boolean preViewLable(string strLableCode, string strProdId, string strDpi)
        {
            try
            {
                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("DPI", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LABEL_TYPE"]  = "PACK";
                drINDATA["LABEL_CODE"]   = strLabelCode;
                drINDATA["SAMPLE_FLAG"] = "Y";
                drINDATA["PRODID"]      = strProdId;
                drINDATA["DPI"]         = strDpi;
                drINDATA["USERID"]      = LoginInfo.USERID;

                INDATA.Rows.Add(drINDATA);

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                DataSet dsResult;
                string url = string.Empty;
                string zplText = string.Empty;
                string dpmm;

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPL_API", "INDATA", "OUTDATA,IMAGE", dsIndata, null);
                dpmm = ((dsResult.Tables["OUTDATA"].Rows[0])).ItemArray[3].ToString();
                url = (dsResult.Tables["IMAGE"].Rows[0]).ItemArray[1].ToString();
                zplText = ((dsResult.Tables["OUTDATA"].Rows[0])).ItemArray[0].ToString();

                if (((dsResult.Tables["IMAGE"].Rows[0])).ItemArray[0].Equals("OK"))
                {
                    Byte[] temp;
                    temp = (Byte[])(dsResult.Tables["IMAGE"].Rows[0]).ItemArray[2];

                    var bi = new BitmapImage();

                    bi.BeginInit();
                    if (temp != null) bi.StreamSource = new MemoryStream(temp);
                    bi.EndInit();

                    labelAttachFile.Source = bi;

                    zplBrowser.Visibility = Visibility.Collapsed;
                    labelAttachFile.Visibility = Visibility.Visible;
                }
                else
                {
                    byte[] zpl = System.Text.Encoding.UTF8.GetBytes(zplText);
                    byte[] IMG = null;

                    var request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url);
                    request.Method = "POST";
                    request.ContentType = "application/x-www-form-urlencoded";
                    request.ContentLength = zpl.Length;

                    var requestStream = request.GetRequestStream();
                    requestStream.Write(zpl, 0, zpl.Length);
                    requestStream.Close();

                    var response = (System.Net.HttpWebResponse)request.GetResponse();
                    using (var responseStream = response.GetResponseStream())
                    {
                        MemoryStream stream_Buffer = new MemoryStream();
                        responseStream.CopyTo(stream_Buffer);

                        IMG = stream_Buffer.ToArray();

                        responseStream.Close();
                        stream_Buffer.Close();

                        var bi = new BitmapImage();

                        bi.BeginInit();
                        if (IMG != null) bi.StreamSource = new MemoryStream(IMG);
                        bi.EndInit();

                        labelAttachFile.Source = bi;

                        zplBrowser.Visibility = Visibility.Collapsed;
                        labelAttachFile.Visibility = Visibility.Visible;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        #endregion

        #endregion

        #region [ Validation ] 

        #endregion

        #region [ 체크 박스 처리 - 미사용 ] 

        void checkAll_Checked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgLableItemDetail.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = true;
            }
            dgLableItemDetail.ItemsSource = DataTableConverter.Convert(dt);
        }

        //2018.12.12
        void checkAll_Unchecked(object sender, RoutedEventArgs e)
        {
            DataTable dt = DataTableConverter.Convert(dgLableItemDetail.ItemsSource);
            foreach (DataRow dr in dt.Rows)
            {
                dr["CHK"] = false;
            }
            dgLableItemDetail.ItemsSource = DataTableConverter.Convert(dt);
        }

        private void dgLableItemDetail_LoadedColumnHeaderPresenter(object sender, C1.WPF.DataGrid.DataGridColumnEventArgs e)
        {
            /*
                C1.WPF.DataGrid.DataGridRowHeaderPresenter pre = new C1.WPF.DataGrid.DataGridRowHeaderPresenter();

                CheckBox chkAll = new CheckBox()
                {
                    IsChecked = false,
                    Background = new SolidColorBrush(Colors.Transparent),
                    VerticalAlignment = System.Windows.VerticalAlignment.Center,
                    HorizontalAlignment = System.Windows.HorizontalAlignment.Center
                };
            if (strLastView.Equals("N"))
            {
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (string.IsNullOrEmpty(e.Column.Name) == false)
                    {
                        if (e.Column.Name.Equals("CHK"))
                        {
                            pre.Content = chkAll;
                            //e.Column.HeaderPresenter.Content = pre;

                            chkAll.Checked -= new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked -= new RoutedEventHandler(checkAll_Unchecked);
                            chkAll.Checked += new RoutedEventHandler(checkAll_Checked);
                            chkAll.Unchecked += new RoutedEventHandler(checkAll_Unchecked);
                        }
                    }
                }));
            }
            */
        }
        #endregion

        #region [ 라벨 미리보기 팝업 ]
        private void LablePreView()
        {
            try
            {
                string url = string.Empty;

                string printResolution = strDPI;
                string dpmm;
                switch (printResolution)
                {
                    case "203":
                        dpmm = "8dpmm";
                        break;
                    case "300":
                        dpmm = "12dpmm";
                        break;
                    case "600":
                        dpmm = "24dpmm";
                        break;
                    default:
                        dpmm = "24dpmm";
                        break;
                }

                url = "http://api.labelary.com/v1/printers/" + dpmm + "/labels/4x6/0/";


                DataTable INDATA = new DataTable();
                INDATA.TableName = "INDATA";
                INDATA.Columns.Add("LABEL_TYPE", typeof(string));
                INDATA.Columns.Add("LABEL_CODE", typeof(string));
                INDATA.Columns.Add("SAMPLE_FLAG", typeof(string));
                INDATA.Columns.Add("PRODID", typeof(string));
                INDATA.Columns.Add("DPI", typeof(string));
                INDATA.Columns.Add("USERID", typeof(string));

                DataRow drINDATA = INDATA.NewRow();
                drINDATA["LABEL_TYPE"] = "PACK";
                drINDATA["LABEL_CODE"] = strLabelCode;
                drINDATA["SAMPLE_FLAG"] = "Y";
                drINDATA["PRODID"] = strProdId;
                drINDATA["DPI"] = strDPI;
                drINDATA["USERID"] = LoginInfo.USERID;

                INDATA.Rows.Add(drINDATA);

                DataSet dsIndata = new DataSet();
                dsIndata.Tables.Add(INDATA);
                DataSet dsResult;
                string zplText = string.Empty;

                dsResult = new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_GET_ZPL_API", "INDATA", "OUTDATA,IMAGE", dsIndata, null);
                zplText = ((dsResult.Tables["OUTDATA"].Rows[0])).ItemArray[0].ToString();


                PACK001_000_LABEL_PREVIEW pop = new PACK001_000_LABEL_PREVIEW(url, zplText);
                pop.Show();
                pop.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                pop.ShowInTaskbar = false;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }

        }
        #endregion

        private void DoEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Send, new System.Threading.ThreadStart(delegate { }));
        }


    }

}

