/*************************************************************************************
 Created Date : 2019.12.04
      Creator : 이제섭
   Decription : Decription : CNJ 원형 9 ~ 14라인 증설 Pjt - 포장출고 - 매핑해제 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2019.12.20  이제섭 : 최초생성
  
**************************************************************************************/

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using C1.WPF;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.Data;
using LGC.GMES.MES.CMM001.Extensions;
using System.Windows.Input;
using System.Collections;
using System.Configuration;
using C1.WPF.Excel;
using System.IO;


namespace LGC.GMES.MES.FCS002
{

    public partial class FCS002_300_TESLA_UNMAP : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();

        private string _USERID = string.Empty;
        
        public FCS002_300_TESLA_UNMAP()
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
        /// 컨트롤 초기값 설정
        /// </summary>
        /// 
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _USERID = Util.NVC(tmps[0]) as string;
        }
        #endregion

        #region Event

        private void btnUnMap_Click(object sender, RoutedEventArgs e)
        {
            UnMap_Pallet();
        }
        #endregion

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void UnMap_Pallet()
        {
            try
            {
                if (dgBox.Rows.Count == 0)
                {
                    //SFU3413 박스ID를 스캔 또는 입력하세요.
                    Util.MessageInfo("SFU3413");
                    return;
                }

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("USERID");
                INDATA.Columns.Add("SHOPID");

                DataTable INBOX = indataSet.Tables.Add("INBOX");
                INBOX.Columns.Add("BOXID");

                DataRow dr = INDATA.NewRow();
                dr["USERID"] = _USERID;
                dr["SHOPID"] = LoginInfo.CFG_SHOP_ID;
                INDATA.Rows.Add(dr);

                DataTable dt = ((DataView)dgBox.ItemsSource).Table;

                DataRow newrow;

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newrow = INBOX.NewRow();
                    newrow["BOXID"] = dt.Rows[i]["BOXID"].ToString();
                    INBOX.Rows.Add(newrow);
                }

                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_TESLA_PLT_LABEL_UNMAP_MB", "INDATA,INBOX", null, indataSet);

                this.DialogResult = MessageBoxResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void txtBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                //UnMap_Pallet();
                if (dgBox.Rows.Count == 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CHK");
                    dt.Columns.Add("BOXID");

                    DataRow dr = dt.NewRow();
                    dr["CHK"] = false;
                    dr["BOXID"] = txtBoxID.Text.ToString();
                    dt.Rows.Add(dr);

                    dgBox.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    DataTable dt = ((DataView)dgBox.ItemsSource).Table;

                    string sBoxID = txtBoxID.Text.ToString();

                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        if (dt.Rows[i]["BOXID"].ToString() == sBoxID)
                        {
                            Util.MessageValidation("SFU8466", sBoxID);  //BOX ID가 이미 존재 합니다.[%1]
                            return;
                        }
                    }

                    DataRow dr = dt.NewRow();
                    dr["CHK"] = false;
                    dr["BOXID"] = sBoxID;
                    dt.Rows.Add(dr);
                }
                txtBoxID.Text = String.Empty;
            }
        }

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        private void GetExcel()
        {
            try
            {
                Microsoft.Win32.OpenFileDialog fd = new Microsoft.Win32.OpenFileDialog();

                if (ConfigurationManager.AppSettings["APP_CONFIG_SBC_MODE"] == "SBC")
                {
                    fd.InitialDirectory = @"\\Client\C$";
                }

                fd.Filter = "Excel Files (.xlsx)|*.xlsx";
                if (fd.ShowDialog() == true)
                {
                    using (Stream stream = fd.OpenFile())
                    {
                        LoadExcel(stream, (int)0);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void LoadExcel(Stream excelFileStream, int sheetNo)
        {
            try
            {
                excelFileStream.Seek(0, SeekOrigin.Begin);
                C1XLBook book = new C1XLBook();
                book.Load(excelFileStream, FileFormat.OpenXml);
                XLSheet sheet = book.Sheets[sheetNo];

                ArrayList boxList = new ArrayList();

                if (sheet == null)
                {
                    //업로드한 엑셀파일의 데이타가 잘못되었습니다. 확인 후 다시 처리하여 주십시오.
                    Util.MessageValidation("9017");
                    return;
                }

                if (sheet.Rows.Count <= 1)
                {
                    Util.MessageValidation("SFU1498");  //데이터가 없습니다.
                    return;
                }

                //헤더(0) 번째는 제외. 데이터는 1번째 부터로 인식한다.
                for (int rowInx = 1; rowInx < sheet.Rows.Count; rowInx++)
                {
                    string sBoxID = Util.NVC(sheet.GetCell(rowInx, 0).Text);

                    if (string.IsNullOrEmpty(sBoxID))
                    {
                        Util.MessageValidation("SFU8465");  //BOX ID가 입력되지 않은 ROW가 있습니다.
                        return;
                    }

                    if (dgBox.GetRowCount() > 0)
                    {
                        for (int inx = 0; inx < dgBox.GetRowCount(); inx++)
                        {
                            if (DataTableConverter.GetValue(dgBox.Rows[inx].DataItem, "BOXID").ToString() == sBoxID)
                            {
                                Util.MessageValidation("SFU8466", sBoxID);  //BOX ID가 이미 존재 합니다.[%1]
                                return;
                            }
                        }
                    }

                    for (int rowInx2 = 1; rowInx2 < sheet.Rows.Count; rowInx2++)
                    {
                        if (rowInx == rowInx2)   //동일한 데이터는 중복 판정 제외
                        {
                            continue;
                        }

                        string sBoxID2 = Util.NVC(sheet.GetCell(rowInx2, 0).Text);
                        if (sBoxID == sBoxID2)
                        {
                            Util.MessageValidation("SFU8467", sBoxID);  //입력할 파일에 동일한 BOX ID가 존재합니다.[%1]
                            return;
                        }
                    }
                    boxList.Add(sBoxID);
                }


                for (int i = 0; i < boxList.Count; i++)
                {
                    if (dgBox.Rows.Count == 0)
                    {
                        DataTable dt = new DataTable();
                        dt.Columns.Add("CHK");
                        dt.Columns.Add("BOXID");

                        DataRow dr = dt.NewRow();
                        dr["CHK"] = false;
                        dr["BOXID"] = boxList[i].ToString();
                        dt.Rows.Add(dr);

                        dgBox.ItemsSource = DataTableConverter.Convert(dt);
                    }
                    else
                    {
                        DataTable dt = ((DataView)dgBox.ItemsSource).Table;
                        DataRow dr = dt.NewRow();
                        dr["CHK"] = false;
                        dr["BOXID"] = boxList[i].ToString();
                        dt.Rows.Add(dr);
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgBox.Rows.Count == 0)
            {
                Util.MessageValidation("SFU3537");  //조회된 데이터가 없습니다.
                return;
            }

            DataTable dt = ((DataView)dgBox.ItemsSource).Table;

            for (int i = (dt.Rows.Count - 1); i >= 0; i--)
                if (Convert.ToBoolean(dt.Rows[i]["CHK"]))
                    dt.Rows[i].Delete();
        }

    }
}
