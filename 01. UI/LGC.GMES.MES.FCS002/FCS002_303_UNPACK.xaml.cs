/*************************************************************************************
 Created Date : 2020.02.29
      Creator : 이제섭
   Decription : Decription : CNJ 원형 9 ~ 14라인 증설 Pjt - 자동 포장 구성 (원/각형) - inobox 해체 팝업
--------------------------------------------------------------------------------------
 [Change History]
  2020.02.29  이제섭 : 최초생성
  2023.05.24  이병윤 : E20230424-000073_Cell packaging data improved
  2023.07.20  이병윤 : E20230424-000073_if inbox has  outbox, please error and end
  2023.08.30  이병윤 : 자동 포장 구성(원/각형) 박스 해체시 18650모델일 경우 인터락처리
  2024.03.18  이홍주 : 소형용으로 수정, INBOX 방향지시자 포함시 제외처리
  2024.04.18  이홍주 : 조립 Lot 칼럼 추가.
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
using C1.WPF.DataGrid;
using System.Collections;
using System.Configuration;
using C1.WPF.Excel;
using System.IO;

namespace LGC.GMES.MES.FCS002
{

    public partial class FCS002_303_UNPACK : C1Window, IWorkArea
    {
        #region Declaration & Constructor 

        Util _Util = new Util();
        private CheckBoxHeaderType _inBoxHeaderType;

        private string _USERID = string.Empty;
        private string _PGMID = string.Empty;

        public FCS002_303_UNPACK()
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
        private void InitControl()
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            _USERID = Util.NVC(tmps[0]);
            // 부모장 : 자동포장구성(파우치)유무
            if (tmps.Length > 1)
            {
                _PGMID = Util.NVC(tmps[1]);
            }
        }

        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            InitControl();
        }
        #endregion

        #region Event

        private void btnUnPack_Click(object sender, RoutedEventArgs e)
        {
            UnPack_Pallet();
        }

        private void txtBoxID_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {


            if (e.Key == Key.Enter)
            {
                string sBoxID = txtBoxID.Text.ToString().Trim();


                if (sBoxID.Substring(0, 1).ToUpper() == "C")
                {
                    if (sBoxID.ToString().Length == 22) //방향지시자 포함시
                    {
                        sBoxID = sBoxID.ToString().ToUpper().Substring(0, sBoxID.ToString().Length - 1);
                    }
                    else
                    {
                        sBoxID = sBoxID.ToString().ToUpper();
                    }
                    //InBox일경우 InBox해체 CheckBox 숨김.
                    chkUnpackInBox.Visibility = Visibility.Hidden;
                }
                else
                {
                    chkUnpackInBox.Visibility = Visibility.Visible;
                }


                //UnPack_Pallet();
                if (dgBox.Rows.Count == 0)
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("CHK");
                    dt.Columns.Add("ASSY_LOTID");
                    dt.Columns.Add("BOXID");

                    DataRow dr = dt.NewRow();
                    dr["CHK"] = false;
                    dr["ASSY_LOTID"] = GetPkgLot(sBoxID);  //조립 LOTID 찾아오기
                    dr["BOXID"] = sBoxID;
                    dt.Rows.Add(dr);

                    dgBox.ItemsSource = DataTableConverter.Convert(dt);
                }
                else
                {
                    DataTable dt = ((DataView)dgBox.ItemsSource).Table;



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
                    dr["ASSY_LOTID"] = GetPkgLot(sBoxID);  //조립 LOTID 찾아오기
                    dr["BOXID"] = sBoxID;
                    dt.Rows.Add(dr);
                }
                txtBoxID.Text = String.Empty;
            }
        }

        #endregion

        private string GetPkgLot(string pBox)
        {
            string sPkgLot = "";

            // cylinder BOX 여부 조회
            DataTable dtBox = new DataTable();
            dtBox.TableName = "RQSTDT";
            dtBox.Columns.Add("BOXID", typeof(string));

            DataRow drBox = dtBox.NewRow();
            drBox["BOXID"] = pBox;

            dtBox.Rows.Add(drBox);
            DataTable dtRst = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_PKG_LOT", "RQSTDT", "RSLTDT", dtBox);
            if (dtRst.Rows.Count > 0)
            {
                sPkgLot = dtRst.Rows[0]["LOTCODE"].ToString();
            }

            return sPkgLot;

        }


        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }

        private void UnPack_Pallet()
        {
            try
            {
                if (dgBox.Rows.Count == 0)
                {
                    //SFU3413 박스ID를 스캔 또는 입력하세요.10012
                    Util.MessageInfo("SFU3413");
                    return;
                }

                DataSet indataSet = new DataSet();

                DataTable INDATA = indataSet.Tables.Add("INDATA");
                INDATA.Columns.Add("SRCTYPE");
                INDATA.Columns.Add("USERID");
                INDATA.Columns.Add("INBOX_DEL_FLAG");

                DataTable INBOX = indataSet.Tables.Add("INBOX");
                INBOX.Columns.Add("BOXID");

                DataRow dr = INDATA.NewRow();
                dr["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                dr["USERID"] = _USERID;
                dr["INBOX_DEL_FLAG"] = (bool)chkUnpackInBox.IsChecked ? "Y" : "N";

                INDATA.Rows.Add(dr);

                DataTable dt = ((DataView)dgBox.ItemsSource).Table;

                DataRow newrow;

                string boxList = string.Empty; 
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    newrow = INBOX.NewRow();
                    newrow["BOXID"] = dt.Rows[i]["BOXID"].ToString();
                    INBOX.Rows.Add(newrow);
                    boxList += dt.Rows[i]["BOXID"].ToString() + ",";
                }

                /* CSR ID : E20230424-000073
                 * _PGMID : BOX001_231[자동포장구성(파우치)]인경우
                 * If cylinder BOX, ERROR 발생
                 * 박스해체시 cylinder BOX 여부 확인 로직 추가.
                 */
                if (_PGMID.Equals("BOX001_231"))
                {
                    // cylinder BOX 여부 조회
                    DataTable dtPouch = new DataTable();
                    dtPouch.TableName = "RQSTDT";
                    dtPouch.Columns.Add("BOXID", typeof(string));

                    DataRow drPouch = dtPouch.NewRow();
                    drPouch["BOXID"] = boxList;

                    dtPouch.Rows.Add(drPouch);
                    DataTable dtRst = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CELL_TYPE_NJ", "RQSTDT", "RSLTDT", dtPouch);
                    if (dtRst.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtRst.Rows.Count; i++)
                        {
                            if (!dtRst.Rows[i]["CELL_TYPE_CODE"].ToString().Contains("POUCH")
                                && !dtRst.Rows[i]["CELL_TYPE_CODE"].ToString().Contains("FREE FORM"))
                            {
                                //SFU8275 : Please input POUCH BOX
                                Util.MessageValidation("SFU8275", "POUCH BOX");
                                return;
                            }

                            if (dtRst.Rows[i]["BOXTYPE"].ToString().Equals("INBOX"))
                            {
                                if (!string.IsNullOrEmpty(dtRst.Rows[i]["OUTER_BOXID"].ToString()))
                                {
                                    //101070 : LOT [%1]의 포장정보가 존재합니다. (BOXID : [%2])
                                    Util.MessageValidation("101070", dtRst.Rows[i]["BOXID"].ToString(), dtRst.Rows[i]["OUTER_BOXID"].ToString());
                                    return;
                                }
                            }
                        }
                    }
                }
                /* CSR ID : E20230419-000979
                 * _PGMID : FCS002_303[자동 포장 구성(원/각형)]인경우
                 * BOXTYPE='INBOX','T_OUTBOX'인 경우 BOX해체 가능 
                 * BOXTYPE='OUTBOX' : The 18650 outbox’s BOXTYPE is OUTBOX(cannot handle)
                 */
                else if (_PGMID.Equals("FCS002_303"))
                {
                    // cylinder BOX 여부 조회
                    DataTable dtBox = new DataTable();
                    dtBox.TableName = "RQSTDT";
                    dtBox.Columns.Add("BOXID", typeof(string));

                    DataRow drBox = dtBox.NewRow();
                    drBox["BOXID"] = boxList;

                    dtBox.Rows.Add(drBox);
                    DataTable dtRst = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_BOX_CELL_TYPE_NJ", "RQSTDT", "RSLTDT", dtBox);
                    if (dtRst.Rows.Count > 0)
                    {
                        for (int i = 0; i < dtRst.Rows.Count; i++)
                        {
                            if (dtRst.Rows[i]["BOXTYPE"].ToString().Contains("INBOX"))
                            {
                                continue;
                            }
                            else if (dtRst.Rows[i]["BOXTYPE"].ToString().Equals("T_OUTBOX"))
                            {
                                continue;
                            }
                            else if (dtRst.Rows[i]["BOXTYPE"].ToString().Equals("OUTBOX"))
                            {
                                //SFU8576 : The 18650 outbox’s cannot handle - %1
                                Util.MessageValidation("SFU8576", dtRst.Rows[i]["BOXID"].ToString());
                                return;
                            }
                        }
                    }
                }
                /* CSR ID : E20230419-000979
                 * _PGMID : BOX001_202[1차 포장 구성(원/각형)]인경우
                 * 모든 박스해체 가능함.
                 */

                //new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_BOX_FOR_TESLA_NJ", "INDATA,INBOX", null, indataSet);

                //2023.10.23 수정 아래 BIZ 2023.03월이후 수정부분 확인 필요.
                new ClientProxy().ExecuteServiceSync_Multi("BR_PRD_REG_UNPACK_BOX_FOR_TESLA_MB", "INDATA,INBOX", null, indataSet);

                //정상 처리 되었습니다.
                Util.MessageInfo("SFU1275");

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
            finally
            {
                loadingIndicator.Visibility = Visibility.Collapsed;

                txtBoxID.Text = string.Empty;
            }

        }

        private void ExcelUpload_Click(object sender, RoutedEventArgs e)
        {
            GetExcel();
        }

        void GetExcel()
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

        void LoadExcel(Stream excelFileStream, int sheetNo)
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
                        dt.Columns.Add("ASSY_LOTID");
                        dt.Columns.Add("BOXID");

                        DataRow dr = dt.NewRow();
                        dr["CHK"] = false;
                        dr["ASSY_LOTID"] = GetPkgLot(boxList[i].ToString());  //조립 LOTID 찾아오기 ;
                        dr["BOXID"] = boxList[i].ToString();
                        dt.Rows.Add(dr);

                        dgBox.ItemsSource = DataTableConverter.Convert(dt);
                    }
                    else
                    {
                        DataTable dt = ((DataView)dgBox.ItemsSource).Table;
                        DataRow dr = dt.NewRow();
                        dr["CHK"] = false;
                        dr["ASSY_LOTID"] = GetPkgLot(boxList[i].ToString());  //조립 LOTID 찾아오기 ;
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


        private enum CheckBoxHeaderType
        {
            Zero,
            One,
            Two,
            Three
        }


        private void tbCheckHeaderAll_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            C1DataGrid dg = dgBox;
            if (dg?.ItemsSource == null) return;

            foreach (C1.WPF.DataGrid.DataGridRow row in dg.Rows)
            {
                switch (_inBoxHeaderType)
                {
                    case CheckBoxHeaderType.Zero:
                        DataTableConverter.SetValue(row.DataItem, "CHK", true);
                        break;
                    case CheckBoxHeaderType.One:
                        DataTableConverter.SetValue(row.DataItem, "CHK", false);
                        break;
                }
            }

            switch (_inBoxHeaderType)
            {
                case CheckBoxHeaderType.Zero:
                    _inBoxHeaderType = CheckBoxHeaderType.One;
                    break;
                case CheckBoxHeaderType.One:
                    _inBoxHeaderType = CheckBoxHeaderType.Zero;
                    break;
            }

            dg.EndEdit();
            dg.EndEditRow(true);
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
           
            Button btn = sender as Button;
            int iRow = ((C1.WPF.DataGrid.DataGridCellPresenter)btn.Parent).Row.Index;

            DataTable dt = ((DataView)dgBox.ItemsSource).Table;
            dt.Rows[iRow].Delete();

        }


    }
}
