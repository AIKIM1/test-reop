/*************************************************************************************
 Created Date : 2017.07.06
      Creator : 이대영D
   Decription : Washing 공정진척 Cell 관리
--------------------------------------------------------------------------------------
 [Change History]
   2017.07.06   INS 이대영D : Initial Created.
   2021.10.07   오화백K     : 버튼셀 관련 추가
**************************************************************************************/

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.CMM001.Extensions;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LGC.GMES.MES.CMM001
{
    /// <summary>
    /// CMM_WASHING_CELL_INFO.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class CMM_WASHING_CSH_CELL_INFO : C1Window, IWorkArea
    {
        // DataSet, DataTable
        private DataSet dsResult = null;              // 그리드에 바인딩할 데이터 조회해온 DataTable : GetData() - BR_PRD_GET_TRAY_INFO_WSS 호출
        private DataTable dtCell = null;              // 그리드 Cell Data
        private DataTable dtCellTemp = null;          // 상단 : 0 ~ 32, 좌측 0 ~ 32 Cell 생성
        private DataTable dtSubLotID = null;          // SUBLOTID 정보        
        private DataTable dtOutData = null;           // dsResult 에 담긴 OUTDATA
        private DataTable dtCstPstn = null;           // dsResult 에 담긴 CST_PSTN
        private DataTable dtCstPstnClone = null;      // dtCstPstn의 CSTSLOT 정렬
        private DataTable cstPstnSupport = null;      // 최종적으로 바인딩할 가공된 CstPstn 데이터
        private DataTable dtTrayLocation = null;      // TrayLocation 정보
        private DataTable dtTrayLocationClone = null; // 메인에서 수량이 0인 항목을 선택했을 경우 데이터를 가지고만 있고 화면에 바인딩은 dtTrayLocation을 함
        private DataTable dtSubLotChkCode = null;     // SUBLOT_CHK_CODE 정보

        // Main에서 넘겨받은 파라미터
        private string prodLotID = string.Empty; // LOTID
        private string trayID = string.Empty;    // TRAYID
        private string outLotID = string.Empty;  // OUTLOTID
        private string eqptID = string.Empty;    // 설비코드

        // 전역 변수
        private int iCount = 0;                              // GetData() 에서 조회한 데이터를 바인딩 시점에서 dtCstPstn의 갯수만큼 돌리기위한 루프 변수
        private string sCellId = string.Empty;               // CELLID 텍스트박스에 수정되기전 값
        private string slotID = string.Empty;                // 원래 데이터에서 StartIndex : 4, EndIndex : 10 자른 7자리 LOTID
        private string sSubCellID = string.Empty;            // 원래 데이터에서 StartIndex : 3, EndIndex : 9 자른 7자리 CELLID
        private string sCstSLot = string.Empty;              // Carrier 슬롯번호
        private string subLotId = string.Empty;              // 뒤에6자리 자른 SUBLOTID
        private string subLotIdClone = string.Empty;         // 원본 SUBLOTID
        private string sSelectedTrayLocation = string.Empty; // 선택한 Tray 위치
        private string sTrayTag = string.Empty;              // R: 읽기 / W: 쓰기 모드
        private string completeProd = string.Empty;          // 생산LOT 완료여부
        private bool bSave = false;
        //private bool loadType = true;

        // 그리드 Cell 갯수 : 24 by 24
        private int _nX;
        private int _nY;

        //2021-10-07 오화백  버튼셀 관련 추가
        private string _Product_Lev = string.Empty;

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public CMM_WASHING_CSH_CELL_INFO()
        {
            InitializeComponent();
        }

        private void CMM_WASHING_CSH_CELL_INFO_Loaded(object sender, RoutedEventArgs e)
        {
            object[] tmps = C1WindowExtension.GetParameters(this);

            prodLotID = tmps[0] as string;
            trayID = tmps[1] as string;
            outLotID = tmps[2] as string;
            eqptID = tmps[3] as string;
            sTrayTag = tmps[4] as string;
            completeProd = tmps[5] as string;
            _Product_Lev = tmps[6] as string;

            //읽기 모드
            if (sTrayTag.Equals("R"))
            {
                this.Header = ObjectDic.Instance.GetObjectName("Cell조회");
                btnSave.Visibility = Visibility.Collapsed;
                btnDelete.Visibility = Visibility.Collapsed;
                btnCell.Visibility = Visibility.Collapsed;
            }
            else //쓰기모드
            {
                this.Header = ObjectDic.Instance.GetObjectName("Cell관리");
                btnSave.Visibility = Visibility.Visible;
                btnDelete.Visibility = Visibility.Visible;
                btnCell.Visibility = Visibility.Visible;
            }


            _nX = 24;
            _nY = 24;

            SetGridCell(true); // 최초 로드시 true

            InitControl();
        }

        private void InitControl()
        {
            // 컨트롤 초기화
            txtLotId.Text = string.Empty;         // LOTID
            txtTrayId.Text = string.Empty;        // Cell수량
            txtWipQty.Text = string.Empty;        // Cell수량
            txtCellId.Text = string.Empty;        // CELLID
            txtElPreWeight.Text = string.Empty;   // 주액전 중량
            txtElAfterWeight.Text = string.Empty; // 주액후 중량
            txtElWeight.Text = string.Empty;      // 주액량
            txtIrOcv.Text = string.Empty;         // IR OCV
            txtIrOcvAverage.Text = string.Empty;  // IR OCV 평균
            txtTrayLocation.Text = string.Empty;  // Tray 위치

            if (dtCstPstn == null || dtCstPstn.Rows.Count <= 0)
            {
                //txtElPreWeight.Text = Util.NVC(0);   // 주액전 중량
                //txtElAfterWeight.Text = Util.NVC(0); // 주액후 중량
                //txtElWeight.Text = Util.NVC(0);      // 주액량
                //txtIrOcv.Text = Util.NVC(0);         // IR OCV
                //txtIrOcvAverage.Text = Util.NVC(0);  // IROCV 평균

                txtElPreWeight.Text = string.Empty;   // 주액전 중량
                txtElAfterWeight.Text = string.Empty; // 주액후 중량
                txtElWeight.Text = string.Empty;      // 주액량
                txtIrOcv.Text = string.Empty;         // IR OCV
                txtIrOcvAverage.Text = string.Empty;  // IROCV 평균

                iCount = 1;
                
                dtTrayLocationClone = dtCell.Copy();

                for (int row = 1; row < dtCell.Rows.Count; row++)
                {
                    for (int col = 1; col < dtCell.Columns.Count; col++)
                    {
                        dtTrayLocationClone.Rows[col][row.ToString()] = iCount++;
                    }
                }
            }

            // 텍스트박스 바인딩
            if (dtOutData != null)
            {
                //int sLotIDLength = 0;
                txtLotId.Text = Util.NVC(dtOutData.Rows[0]["PROD_LOTID"]) != null ? Util.NVC(dtOutData.Rows[0]["PROD_LOTID"]) : string.Empty;
                slotID = Util.NVC(txtLotId.Text) != string.Empty ? Util.NVC(txtLotId.Text) : string.Empty;
                //sLotIDLength = Util.NVC_Int(slotID.Length);
                slotID = slotID != string.Empty ? slotID.Substring(3, 7) : string.Empty;
                //txtTrayId.Text = Util.NVC(dtOutData.Rows[0]["TRAYID"]) != null ? Util.NVC(dtOutData.Rows[0]["TRAYID"]) : string.Empty;
                //txtWipQty.Text = Util.NVC(dtOutData.Rows[0]["WIPQTY"]) != null ? Util.NVC(dtOutData.Rows[0]["WIPQTY"]) : Util.NVC(0);
                //txtIrOcvAverage.Text = Util.NVC(dtOutData.Rows[0]["EL_AVG"]) != null ? Util.NVC(dtOutData.Rows[0]["EL_AVG"]) : Util.NVC(0);
                txtTrayId.Text = Util.NVC(dtOutData.Rows[0]["TRAYID"]) != null ? Util.NVC(dtOutData.Rows[0]["TRAYID"]) : string.Empty;
                txtWipQty.Text = Util.NVC(dtOutData.Rows[0]["WIPQTY"]) != null ? Util.NVC(dtOutData.Rows[0]["WIPQTY"]) : string.Empty;
                txtIrOcvAverage.Text = Util.NVC(dtOutData.Rows[0]["EL_AVG"]) != null ? Util.NVC(dtOutData.Rows[0]["EL_AVG"]) : string.Empty;
            }

            // Cell수량 뒤에 소수점 제거와 숫자 표현 형식 (1,000 : 3자리마다 , 출력)
            string sWipQty = Util.NVC(txtWipQty.Text);
            int iWipQty = Util.NVC_Int(txtWipQty.Text);
            sWipQty = iWipQty != 0 ? $"{iWipQty:#,###}" : string.Empty;
            txtWipQty.Text = Util.NVC(sWipQty);

            // CELLID
            sCellId = txtCellId.Text.Trim();
        }

        private void SetGridCell(bool loadType)
        {
            int sum = _nX * _nY;
            int iTopHeader = 0;
            int iLeftHeader = 0;
            int iTopHeaderTemp = 0;
            int iLeftHeaderTemp = 1;

            // Column 생성
            dtCell = new DataTable();

            DataRow dr = dtCell.NewRow();

            // 상단 0 ~ 32 까지 칼럼 생성
            for (int col = 0; col < _nX + 1; col++)
            {
                dtCell.Columns.Add(col.ToString());
                dr[col.ToString()] = iTopHeader++;
            }
            dtCell.Rows.Add(dr);

            // 좌측 0 ~ 32 까지 행 추가
            for (int row = 0; row < _nY + 1; row++)
            {
                dtCell.Rows[row]["0"] = iLeftHeader++;
                if (dtCell.Rows.Count == _nY + 1)
                    break;
                dtCell.Rows.Add();
            }

            // dtCellTemp 생성
            dtCellTemp = new DataTable();

            // 상단 0 ~ 32 까지 칼럼 생성
            dtCellTemp = dtCell.Clone();

            DataRow drTemp = dtCellTemp.NewRow();

            for (int col = 0; col < _nX + 1; col++)
            {
                drTemp[col.ToString()] = iTopHeaderTemp++;
            }

            dtCellTemp.Rows.Add(drTemp);

            // 좌측 0 ~ 32 까지 행 추가
            for (int row = 0; row < _nY + 1; row++)
            {
                if (row == 0)
                    dtCellTemp.Rows[row]["0"] = string.Empty;
                else
                    dtCellTemp.Rows[row]["0"] = iLeftHeaderTemp++;

                if (dtCellTemp.Rows.Count == _nY + 1)
                    break;
                dtCellTemp.Rows.Add();
            }

            GetData(); // 바인딩 Data

            // dtOutData, dtCstPstn
            if (dtCstPstnClone == null || dtCstPstnClone.Rows.Count == 0)
            {
                dtTrayLocation = new DataTable();
                dtSubLotChkCode = new DataTable();
                dtTrayLocation = dtCell.Copy();  // Tray 위치
                dtSubLotChkCode = dtCell.Copy(); // SUBLOT 점검 코드
                dgCellInfo.ItemsSource = DataTableConverter.Convert(dtCell);
            }
            else
            {
                // 그리드 바인딩
                int iDtCstPstnRowCount = 0;
                iDtCstPstnRowCount = dtCstPstn.Rows.Count; // GetData()에서 조회한 데이터 갯수
                int subLotIdLength = 0;
                dtSubLotID = dtCell.Copy();
                dtTrayLocationClone = dtCell.Copy();

                int iTrayLocation = 1;
                string subLotChkCode = string.Empty;

                dtTrayLocation = new DataTable();
                dtSubLotChkCode = new DataTable();
                dtTrayLocation = dtCell.Copy();  // Tray 위치
                dtSubLotChkCode = dtCell.Copy(); // SUBLOT 점검 코드

                if (!loadType) // 삭제 후 Reload 시 iCount 초기화
                    iCount = 0;

                DataRow dRow = null;

                // CSTSLOT 오름차순 정렬
                var cstQuery = (from cstPstn in dtCstPstnClone.AsEnumerable()
                                orderby cstPstn.Field<Int32>("CSTSLOT") ascending
                                select cstPstn).CopyToDataTable();

                // CSTSLOT 그룹핑
                var cstGroup = cstQuery.AsEnumerable().GroupBy(g => g.Field<Int32>("CSTSLOT"))
                                                      .Select(s => s.OrderByDescending(o => o.Field<Int32>("CSTSLOT"))
                                                      .Last()).CopyToDataTable();

                // cstGroup의 데이터가 셀의갯수 576개(그리드 셀의 갯수) 보다 적을 경우 빈 DataRow 생성
                if (cstGroup != null && cstGroup.Rows.Count > 0 && sum != 0 && cstGroup.Rows.Count < sum)
                {
                    for (int i = cstGroup.Rows.Count; i < sum; i++)
                    {
                        dRow = cstGroup.NewRow();
                        cstGroup.Rows.Add(dRow);
                    }
                }

                cstPstnSupport = new DataTable();

                cstPstnSupport = cstGroup.Clone();

                // 576번(그리드 셀의 갯수) 돌면서 CSTSLOT의 위치에 값이 있으면 break, 없으면 루프의 값을 CSTSLOT에 넣고 DataRow 생성
                if (sum != 0)
                {
                    for (int i = 0; i < sum; i++)
                    {
                        if (Util.NVC_Int(cstGroup.Rows[iCount]["CSTSLOT"]) != i + 1)
                        {
                            dRow = cstPstnSupport.NewRow();

                            dRow["CSTSLOT"] = i + 1;
                            dRow["SUBLOTID"] = string.Empty;
                            dRow["SUBLOT_CHK_CODE"] = string.Empty;
                            dRow["EL_PRE_WEIGHT"] = string.Empty;
                            dRow["EL_AFTER_WEIGHT"] = string.Empty;
                            dRow["EL_WEIGHT"] = string.Empty;
                            dRow["IROCV"] = string.Empty;

                            cstPstnSupport.Rows.Add(dRow);
                        }
                        else
                            iCount++;
                    }
                }

                // cstGroup과 dtCstPstnSupport Merge : 최종적으로 가공된 데이터 바인딩을 위한 과정
                IEnumerable<DataRow> enumerCstGroup = cstGroup.AsEnumerable();
                IEnumerable<DataRow> enumerCstPstnSupport = cstPstnSupport.AsEnumerable();
                IEnumerable<DataRow> unionEnumerable = enumerCstGroup.Union(enumerCstPstnSupport);
                DataTable LastBindData = unionEnumerable.CopyToDataTable();

                DataRow lastRow = null;

                // 바인딩 데이터 갯수와 그리드 셀의 갯수를 맞추기 위해 생성된 DataRow 삭제
                for (int i = LastBindData.Rows.Count; i >= 0; i--)
                {
                    if (i == 0)
                        break;

                    if (Util.NVC(LastBindData.Rows[i - 1]["CSTSLOT"]) == string.Empty || LastBindData.Rows[i - 1]["CSTSLOT"] == null)
                    {
                        lastRow = LastBindData.Rows[i - 1];
                        LastBindData.Rows.Remove(lastRow);
                    }
                }

                LastBindData.AcceptChanges();

                iCount = 0;

                // CSTSLOT 오름차순 정렬
                var cstLastQuery = (from cstPstn in LastBindData.AsEnumerable()
                                    orderby cstPstn.Field<Int32>("CSTSLOT") ascending
                                    select cstPstn).CopyToDataTable();

                for (int row = 1; row < dtCell.Rows.Count; row++)
                {
                    for (int col = 1; col < dtCell.Columns.Count; col++)
                    {
                        if (iCount >= cstLastQuery.Rows.Count)
                            break;

                        if (row == 1)
                        {
                            subLotIdLength = Util.NVC_Int(cstLastQuery.Rows[iCount]["SUBLOTID"].ToString().Length) != 0 ? Util.NVC_Int(cstLastQuery.Rows[iCount]["SUBLOTID"].ToString().Length) - 1 : 0;                                                                                                         // SUBLOTID 길이
                            subLotId = subLotIdLength > 0 ? Util.NVC(cstLastQuery.Rows[iCount]["SUBLOTID"]).Substring(subLotIdLength - 5) : string.Empty; // 뒤에6자리 자른 SUBLOTID
                            subLotIdClone = Util.NVC(cstLastQuery.Rows[iCount]["SUBLOTID"]);                                                              // 원본 SUBLOTID
                            dtCell.Rows[col][row.ToString()] = Util.NVC(subLotId);                                                                        // 그리드에 뒤에 6자리 자른                                                                                                                                   SUBLOTID을 보여줌
                            dtSubLotID.Rows[col][row.ToString()] = Util.NVC(subLotIdClone);                                                               // 원본 SUBLOTID
                            dtTrayLocation.Rows[col][row.ToString()] = Util.NVC_Int(iTrayLocation++);                                                     // TrayLocation 정보
                            subLotChkCode = Util.NVC(cstLastQuery.Rows[iCount]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(cstLastQuery.Rows[iCount]["SUBLOT_CHK_CODE"]) : string.Empty;                                                                                                               // SUBLOT_CHK_CODE 정보
                            dtSubLotChkCode.Rows[col][row.ToString()] = Util.NVC(subLotChkCode);                                                          // SUBLOT_CHK_CODE 정보
                            iCount++;
                        }
                        else
                        {
                            subLotIdLength = Util.NVC_Int(cstLastQuery.Rows[iCount]["SUBLOTID"].ToString().Length) != 0 ? Util.NVC_Int(cstLastQuery.Rows[iCount]["SUBLOTID"].ToString().Length) - 1 : 0;
                            subLotId = subLotIdLength > 0 ? Util.NVC(cstLastQuery.Rows[iCount]["SUBLOTID"]).Substring(subLotIdLength - 5) : string.Empty;
                            subLotIdClone = Util.NVC(cstLastQuery.Rows[iCount]["SUBLOTID"]);
                            dtCell.Rows[col][row.ToString()] = Util.NVC(subLotId);
                            dtSubLotID.Rows[col][row.ToString()] = Util.NVC(subLotIdClone);
                            dtTrayLocation.Rows[col][row.ToString()] = Util.NVC_Int(iTrayLocation++);
                            subLotChkCode = Util.NVC(cstLastQuery.Rows[iCount]["SUBLOT_CHK_CODE"]) != string.Empty ? Util.NVC(cstLastQuery.Rows[iCount]["SUBLOT_CHK_CODE"]) : string.Empty;
                            dtSubLotChkCode.Rows[col][row.ToString()] = Util.NVC(subLotChkCode);
                            iCount++;
                        }
                    }
                }

                dgCellInfo.ItemsSource = DataTableConverter.Convert(dtCell);
            }
        }

        private void GetData()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = "BR_PRD_GET_TRAY_INFO_WSS"; // 조회

                DataSet inDataSet = new DataSet();

                DataTable inData = inDataSet.Tables.Add("INDATA");
                inData.Columns.Add("PROD_LOTID", typeof(string));
                inData.Columns.Add("TRAYID", typeof(string));
                inData.Columns.Add("OUT_LOTID", typeof(string));

                DataRow inDataRow = inData.NewRow();
                inDataRow["PROD_LOTID"] = Util.NVC(prodLotID);
                inDataRow["TRAYID"] = Util.NVC(trayID);
                inDataRow["OUT_LOTID"] = Util.NVC(outLotID);
                inData.Rows.Add(inDataRow);

                dsResult = new ClientProxy().ExecuteServiceSync_Multi(sBizName, "INDATA", "OUTDATA,CST_PSTN", inDataSet);

                dtOutData = dsResult.Tables["OUTDATA"];
                dtCstPstn = dsResult.Tables["CST_PSTN"];

                dtCstPstnClone = new DataTable();
                dtCstPstnClone.Columns.Add("CSTSLOT", typeof(Int32));
                dtCstPstnClone.Columns.Add("SUBLOTID", typeof(string));
                dtCstPstnClone.Columns.Add("SUBLOT_CHK_CODE", typeof(string));
                dtCstPstnClone.Columns.Add("EL_PRE_WEIGHT", typeof(string));
                dtCstPstnClone.Columns.Add("EL_AFTER_WEIGHT", typeof(string));
                dtCstPstnClone.Columns.Add("EL_WEIGHT", typeof(string));
                dtCstPstnClone.Columns.Add("IROCV", typeof(string));

                for (int i = 0; i < dtCstPstn.Rows.Count; i++)
                {
                    inDataRow = dtCstPstnClone.NewRow();
                    inDataRow["CSTSLOT"] = dtCstPstn.Rows[i]["CSTSLOT"];
                    inDataRow["SUBLOTID"] = dtCstPstn.Rows[i]["SUBLOTID"];
                    inDataRow["SUBLOT_CHK_CODE"] = dtCstPstn.Rows[i]["SUBLOT_CHK_CODE"];
                    inDataRow["EL_PRE_WEIGHT"] = dtCstPstn.Rows[i]["EL_PRE_WEIGHT"];
                    inDataRow["EL_AFTER_WEIGHT"] = dtCstPstn.Rows[i]["EL_AFTER_WEIGHT"];
                    inDataRow["EL_WEIGHT"] = dtCstPstn.Rows[i]["EL_WEIGHT"];
                    inDataRow["IROCV"] = dtCstPstn.Rows[i]["IROCV"];
                    dtCstPstnClone.Rows.Add(inDataRow);
                }
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

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            SetGridCell(false);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            if (bSave)
                this.DialogResult = MessageBoxResult.OK;
            else
                this.DialogResult = MessageBoxResult.Cancel;
        }

        private void btnCell_Click(object sender, RoutedEventArgs e)
        {
            //Cell 입력
            try
            {
                bSave = true;
                CMM_ASSY_INPUT_CELL _xlsLoad = new CMM_ASSY_INPUT_CELL();
                _xlsLoad.FrameOperation = FrameOperation;
                              
                if (_xlsLoad != null)
                {
                    object[] Parameters = new object[6];
                    Parameters[0] = txtLotId.Text.ToString();
                    Parameters[1] = txtTrayId.Text.ToString();
                    Parameters[2] = eqptID.ToString();
                    Parameters[3] = string.Empty;
                    Parameters[4] = string.Empty;
                    Parameters[5] = completeProd;
                    C1WindowExtension.SetParameters(_xlsLoad, Parameters);

                    _xlsLoad.Closed += new EventHandler(xlsLoad_Closed);
                    _xlsLoad.ShowModal();
                    _xlsLoad.CenterOnScreen();


                }
            

            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void xlsLoad_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_INPUT_CELL runStartWindow = sender as CMM_ASSY_INPUT_CELL;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                SetGridCell(false);
            }

        }
        private void dgCellInfo_LoadedCellPresenter(object sender, C1.WPF.DataGrid.DataGridCellEventArgs e)
        {
            C1.WPF.DataGrid.C1DataGrid dataGrid = sender as C1.WPF.DataGrid.C1DataGrid;

            dataGrid.Dispatcher.BeginInvoke(new Action(() =>
            {
                if (e.Cell.Presenter == null)
                    return;

                //Grid Data Binding 이용한 Background 색 변경
                if (e.Cell.Row.Type == DataGridRowType.Item)
                {
                    if (dtCell != null)
                    {
                        // 가로 Header 색상 적용
                        if (e.Cell.Row.Index == 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E9E9EE"));
                        }
                        // 세로 Header 색상 적용
                        if (e.Cell.Column.Index == 0)
                        {
                            e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#E9E9EE"));
                            //e.Cell.Presenter.Width = 20;
                        }
                        // dtCell의 Cell의 갯수만큼 돌려서 데이터 "0"이 있는지 찾고 "0"인 dtCell의 위치와 e.Cell의 위치를 비교해서 색상적용
                        for (int i = 0; i < dtCell.Rows.Count; i++)
                        {
                            for (int j = 0; j < dtCell.Columns.Count; j++)
                            {
                                if (dtCell.Rows[i][j.ToString()].ToString() == "0")
                                {
                                    if (e.Cell.Row.Index == i && e.Cell.Column.Index == j && e.Cell.Column.Index.ToString() != "0")
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#708090"));
                                    }
                                }                                
                            }
                        }

                        if (dtSubLotChkCode != null)
                        {
                            for (int row = 1; row < dtSubLotChkCode.Rows.Count; row++)
                            {
                                for (int col = 1; col < dtSubLotChkCode.Columns.Count; col++)
                                {
                                    // NR : 읽을 수 없음(No Read)
                                    if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(dtSubLotChkCode.Rows[row][col.ToString()]), "NR"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF0000"));
                                    }
                                    // DL : 자리수 상이 (Different ID Length) 
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(dtSubLotChkCode.Rows[row][col.ToString()]), "DL"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF00EE"));
                                    }
                                    // ID : ID 중복 (ID Duplication)
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(dtSubLotChkCode.Rows[row][col.ToString()]), "ID"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#3AE82C"));
                                    }
                                    // SC : 특수문자 포함 (Include Special Character) 
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(dtSubLotChkCode.Rows[row][col.ToString()]), "SC"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#7300FF"));
                                    }
                                    // PD : Tray Location 중복 (Position Duplication)
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(dtSubLotChkCode.Rows[row][col.ToString()]), "PD"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#FF6C00"));
                                    }
                                    // NI : 주액량 정보 없음 (No Information)
                                    else if (e.Cell.Row.Index == row && e.Cell.Column.Index == col && string.Equals(Util.NVC(dtSubLotChkCode.Rows[row][col.ToString()]), "NI"))
                                    {
                                        e.Cell.Presenter.Background = new SolidColorBrush((Color)System.Windows.Media.ColorConverter.ConvertFromString("#F0FF00"));
                                    }

                                }
                            }
                        }
                    }
                }
                HiddenLoadingIndicator();
            }));
        }

        private void dgCellInfo_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            try
            {
                // 선택한 셀의 위치
                int rowIdx = dgCellInfo.CurrentRow.Index;
                int colIdx = dgCellInfo.CurrentColumn.Index;

                if (dtSubLotID == null)
                {
                    string selectedTrayLocation = Util.NVC(dtTrayLocationClone.Rows[rowIdx][colIdx.ToString()]); // 메인에서 수량 0인 항목을 선택했을 경우 Tray 위치 정보
                    txtTrayLocation.Text = Util.NVC(selectedTrayLocation);

                    sCellId = txtCellId.Text.Trim();
                    sSelectedTrayLocation = txtTrayLocation.Text.Trim();
                }
                else
                {
                    string selectedSubLot = Util.NVC(dtSubLotID.Rows[rowIdx][colIdx.ToString()]); // 선택한 Cell의 SUBLOTID
                    int sCellIDLength = 0;                                                        // CELLID 길이

                    //if (dtCstPstn != null && dtCstPstn.Rows.Count != 0 && dtOutData != null && dtOutData.Rows.Count != 0)
                    if (CommonVerify.HasTableRow(dtCstPstn) && CommonVerify.HasTableRow(dtOutData))
                    {
                        var query = (from cstPstn in dtCstPstn.AsEnumerable()
                                     where cstPstn.Field<string>("SUBLOTID") == selectedSubLot
                                     select new
                                     {
                                         CSTSLOT = cstPstn.Field<string>("CSTSLOT"),                 // Carrier 슬롯번호
                                         CELLID = cstPstn.Field<string>("SUBLOTID"),                 // CELLID
                                         EL_PRE_WEIGHT = cstPstn.Field<string>("EL_PRE_WEIGHT"),     // 주액전 중량
                                         EL_AFTER_WEIGHT = cstPstn.Field<string>("EL_AFTER_WEIGHT"), // 주액후 중량
                                         EL_WEIGHT = cstPstn.Field<string>("EL_WEIGHT"),             // 주액량
                                         IROCV = cstPstn.Field<string>("IROCV")                      // IR OCV
                                     }).FirstOrDefault();
                        if (query != null)
                        {
                            sSubCellID = query.CELLID != null ? query.CELLID : string.Empty;
                            sCellIDLength = Util.NVC_Int(sSubCellID.Length);
                            sSubCellID = sCellIDLength != 0 ? sSubCellID.Substring(2, 7) : string.Empty;                       // 7자리 자른 CELLID

                            //sCstSLot = query.CSTSLOT != null ? query.CSTSLOT : Util.NVC(0);                              // Carrier 슬롯번호
                            //txtCellId.Text = query.CELLID != null ? query.CELLID : string.Empty;                         // CELLID
                            //txtElPreWeight.Text = query.EL_PRE_WEIGHT != null ? query.EL_PRE_WEIGHT : Util.NVC(0);       // 주액전 중량
                            //txtElAfterWeight.Text = query.EL_AFTER_WEIGHT != null ? query.EL_AFTER_WEIGHT : Util.NVC(0); // 주액후 중량
                            //txtElWeight.Text = query.EL_WEIGHT != null ? query.EL_WEIGHT : Util.NVC(0);                  // 주액량
                            //txtIrOcv.Text = query.IROCV != null ? query.IROCV : Util.NVC(0);                             // IR OCV

                            sCstSLot = query.CSTSLOT;                              // Carrier 슬롯번호
                            txtCellId.Text = query.CELLID != null ? query.CELLID : string.Empty;                         // CELLID
                            txtElPreWeight.Text = query.EL_PRE_WEIGHT;       // 주액전 중량
                            txtElAfterWeight.Text = query.EL_AFTER_WEIGHT; // 주액후 중량
                            txtElWeight.Text = query.EL_WEIGHT;   // 주액량
                            txtIrOcv.Text = query.IROCV;        // IR OCV

                            //ControlsLibrary.MessageBox.Show("dgCellInfo.GetCell :" + dgCellInfo.GetCell(rowIdx, colIdx).Presenter.Background.ToString());

                                //CELL ID가 중복일 경우
                            if (Util.NVC(dtSubLotChkCode.Rows[rowIdx][colIdx.ToString()]) == "PD")
                            {
                                DataRow[] dr = null;
                                if (dtCstPstn.Columns.Contains("CSTSLOT"))
                                {
                                    //dr = dtCstPstn.Select("CSTSLOT=" + query.CSTSLOT);
                                    dr = dtCstPstn.Select("CSTSLOT ='" + query.CSTSLOT + "'");
                                }
                                if (dr.Length > 1)
                                {
                                    CMM_ASSY_DUPCELLDELETE _xlsLoad = new CMM_ASSY_DUPCELLDELETE();
                                    _xlsLoad.FrameOperation = FrameOperation;

                                    if (_xlsLoad != null)
                                    {
                                        object[] Parameters = new object[7];
                                        Parameters[0] = dr; //중복된 Cell ID
                                        Parameters[1] = eqptID; //설비정보
                                        Parameters[2] = txtLotId.Text; //생산LOT ID
                                        Parameters[3] = outLotID; //완성LOT ID
                                        Parameters[4] = txtTrayId.Text; //Tray ID
                                        Parameters[5] = sTrayTag; //읽기 : R, 쓰기 : W
                                        Parameters[6] = completeProd; //생산LOT 완료 여부
                                        C1WindowExtension.SetParameters(_xlsLoad, Parameters);

                                        _xlsLoad.Closed += new EventHandler(DupCellLoad_Closed);
                                        _xlsLoad.ShowModal();
                                        _xlsLoad.CenterOnScreen();
                                    }
                                }
                            }
                        }
                        else
                        {
                            txtCellId.Text = string.Empty; // CELLID
                            txtElPreWeight.Text = string.Empty; // 주액전
                            txtElAfterWeight.Text = string.Empty; // 주액후
                            txtElWeight.Text = string.Empty; // 주액량
                            txtIrOcv.Text = string.Empty;
                            sSubCellID = string.Empty;


                        }

                        // 주액전 중량 뒤에 소수점 제거와 숫자 표현 형식 (1,000 : 3자리마다 , 출력)
                        //string sElPreWeight = Util.NVC(txtElPreWeight.Text);
                        ////int iElPreWeight = txtElPreWeight.Text != string.Empty ? Util.NVC_Int(txtElPreWeight.Text) : 0;
                        ////sElPreWeight = iElPreWeight != 0 ? $"{iElPreWeight:0:#,##0.00}" : Util.NVC(0);
                        // txtElPreWeight.Text = Util.NVC(sElPreWeight);

                        //// 주액후 중량 뒤에 소수점 제거와 숫자 표현 형식 (1,000 : 3자리마다 , 출력)
                        //string sElAfterWeight = Util.NVC(txtElAfterWeight.Text);
                        ////int iElAfterWeight = txtElAfterWeight.Text != string.Empty ? Util.NVC_Int(txtElAfterWeight.Text) : 0;
                        ////sElAfterWeight = iElAfterWeight != 0 ? $"{iElAfterWeight:0:#,##0.00}" : Util.NVC(0);
                        //txtElAfterWeight.Text = Util.NVC(sElAfterWeight);

                        //// 주액량 뒤에 소수점 제거와 숫자 표현 형식 (1,000 : 3자리마다 , 출력)
                        //string sElWeight = Util.NVC(txtElWeight.Text);
                        ////int iElWeight = txtElWeight.Text != string.Empty ? Util.NVC_Int(txtElWeight.Text) : 0;
                        ////sElWeight = iElWeight != 0 ? $"{iElWeight:0:#,##0.00}" : Util.NVC(0);
                        //txtElWeight.Text = Util.NVC(sElWeight);
                        txtTrayLocation.Text = Util.NVC(dtTrayLocation.Rows[rowIdx][colIdx.ToString()]); // Tray위치
                    }
                    sCellId = txtCellId.Text.Trim();
                    sSelectedTrayLocation = txtTrayLocation.Text.Trim();
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }
        private void DupCellLoad_Closed(object sender, EventArgs e)
        {
            CMM_ASSY_DUPCELLDELETE runStartWindow = sender as CMM_ASSY_DUPCELLDELETE;
            if (runStartWindow.DialogResult == MessageBoxResult.OK)
            {
                SetGridCell(false);
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



        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
          
            //2021-10-07 오화백 버튼셀 관련 추가

            if(_Product_Lev == "B")
            {
                // 저장 버튼 클릭 시 삭제 BizRule 실행 후 저장 BizRule 실행
                if (!ValidationSaveBtnCell())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        bSave = true;
                        Delete(false); // Cell 삭제
                    }
                });
            }
            else
            {
                // 저장 버튼 클릭 시 삭제 BizRule 실행 후 저장 BizRule 실행
                if (!ValidationSave())
                    return;

                Util.MessageConfirm("SFU1241", (result) =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        bSave = true;
                        Delete(false); // Cell 삭제
                    }
                });
            }
            
          
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateDelete())
                return;

            Util.MessageConfirm("SFU1230", (result) =>
            {
                if (result == MessageBoxResult.OK)
                {
                    bSave = true;
                    Delete(true); // Cell 삭제
                }
            });
        }

        private bool ValidateDelete()
        {
            if (string.IsNullOrEmpty(Util.NVC(txtCellId.Text))) // 삭제할 항목이 없습니다.
            {
                Util.MessageValidation("SFU1597");
                return false;
            }
            if (txtTrayLocation.Text == "0")
            {
                //Util.MessageValidation("SFU3636");
                Util.MessageValidation("SFU3681"); //Tray 정보는 0 이상이어야 합니다.
                return false;
            }
            if (txtTrayLocation.Text.Length == 0)
            {
                //Tray 위치정보를 입력하세요
                Util.MessageValidation("SFU4278"); 
                return false;
            }

            return true;
        }

        private bool ValidationSave()
        {
            if (txtLotId.Text.Length != 12)
            {
                //Util.MessageValidation("SFU3634");
                Util.MessageValidation("SFU3634"); //LOTID는 12자리여야 합니다.
                return false;
            }

            if (txtCellId.Text.Length != 15)
            {
                //Util.MessageValidation("SFU3635");
                Util.MessageValidation("SFU3635"); //CELLID는 15자리여야 합니다.
                return false;
            }

            if (!string.Equals(slotID, txtCellId.Text.Substring(2, 7)))
            {
                //Util.MessageValidation("SFU3636");
                Util.MessageValidation("SFU3636"); //LOTID와 CELLID가 다릅니다.
                return false;
            }
            if (txtTrayLocation.Text == "0")
            {
                //Util.MessageValidation("SFU3683");
                Util.MessageValidation("SFU3683"); //Tray 정보는 0 이상이어야 합니다.
                return false;
            }
            if (txtTrayLocation.Text.Length == 0)
            {
                Util.MessageValidation("SFU4278"); //Tray 위치정보를 입력하세요
                return false;
            }
            return true;
        }

        //2021-10-07 오화백  버튼셀 관련 추가
        private bool ValidationSaveBtnCell()
        {
            if (txtLotId.Text.Length != 12)
            {
                //Util.MessageValidation("SFU3634");
                Util.MessageValidation("SFU3634"); //LOTID는 12자리여야 합니다.
                return false;
            }

            if (txtCellId.Text.Length != 16)
            {
                //Util.MessageValidation("SFU3635");
                Util.MessageValidation("SFU3635"); //CELLID는 15자리여야 합니다.
                return false;
            }

            if (!string.Equals(slotID, txtCellId.Text.Substring(2, 4) + txtCellId.Text.Substring(1, 1) + txtCellId.Text.Substring(6, 2)))
            {
                //Util.MessageValidation("SFU3636");
                Util.MessageValidation("SFU3636"); //LOTID와 CELLID가 다릅니다.
                return false;
            }

            if (txtTrayLocation.Text == "0")
            {
                //Util.MessageValidation("SFU3683");
                Util.MessageValidation("SFU3683"); //Tray 정보는 0 이상이어야 합니다.
                return false;
            }
            if (txtTrayLocation.Text.Length == 0)
            {
                Util.MessageValidation("SFU4278"); //Tray 위치정보를 입력하세요
                return false;
            }
            return true;
        }



        private void Delete(bool deleteType)
        {
            try
            {
                //C20180328_46567 선택 셀 확인
                if (dgCellInfo.Selection.SelectedCells.Count == 0)
                {
                    return;
                }

                ShowLoadingIndicator();
                if (sSubCellID != string.Empty)
                {
                    int iDeletSubLotCount = 0;
                    string sBizName = ""; // 삭제

                    if ("Y".Equals(completeProd))
                    {
                        sBizName = "BR_PRD_REG_DELETE_SUBLOT_WSS_UI";
                    }
                    else
                    {
                        sBizName = "BR_PRD_REG_DELETE_SUBLOT_WSS";
                    }

                    // DATA SET
                    DataSet inDataSet = new DataSet();
                    DataTable inData = inDataSet.Tables.Add("INDATA");
                    inData.Columns.Add("SRCTYPE", typeof(string));
                    inData.Columns.Add("IFMODE", typeof(string));
                    inData.Columns.Add("EQPTID", typeof(string));
                    inData.Columns.Add("PROD_LOTID", typeof(string));
                    inData.Columns.Add("OUT_LOTID", typeof(string));
                    inData.Columns.Add("USERID", typeof(string));

                    DataTable inCST = inDataSet.Tables.Add("IN_CST");
                    inCST.Columns.Add("CSTID", typeof(string));
                    inCST.Columns.Add("SUBLOTID", typeof(string));

                    DataRow newRow = inData.NewRow();
                    newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                    newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                    newRow["EQPTID"] = Util.NVC(eqptID);
                    newRow["PROD_LOTID"] = Util.NVC(prodLotID);
                    newRow["OUT_LOTID"] = Util.NVC(outLotID);
                    newRow["USERID"] = LoginInfo.USERID;
                    inData.Rows.Add(newRow);

                    foreach (C1.WPF.DataGrid.DataGridCell cell in dgCellInfo.Selection.SelectedCells)
                    {
                        if (cell.Presenter != null)
                        {
                            string selectedSubLot = Util.NVC(dtSubLotID.Rows[cell.Row.Index ][cell.Column.Index.ToString()]); // 선택한 Cell의 SUBLOTID
                            if (!selectedSubLot.Trim().Equals(""))
                            {
                                

                                newRow = inCST.NewRow();
                                newRow["CSTID"] = Util.NVC(txtTrayId.Text);
                                newRow["SUBLOTID"] = Util.NVC(selectedSubLot);
                                //newRow["SUBLOTID"] = Util.NVC(txtCellId.Text);
                                //newRow["CSTID"] = Util.NVC(txtTrayId.Text);
                                inCST.Rows.Add(newRow);
                                iDeletSubLotCount++;
                            }
                        }
                    }

                    dgCellInfo.Selection.Clear();
                    if (iDeletSubLotCount == 0 )
                    {
                        Util.MessageValidation("SFU1597");
                        return;
                    }

                    //string sTestXml = inDataSet.GetXml();

                        new ClientProxy().ExecuteService_Multi(sBizName, "INDATA,IN_CST", null, (bizResult, bizException) =>
                    {
                        try
                        {
                            if (bizException != null)
                            {
                                Util.MessageException(bizException);
                                return;
                            }

                            if (deleteType)
                                SetGridCell(false); // Reload
                            else
                            {
                                Save();
                                SetGridCell(false); // Reload
                            }
                        }
                        catch (Exception ex)
                        {
                            Util.MessageException(ex);
                        }
                    }, inDataSet);
                }
                else
                {
                        Save();
                        SetGridCell(false); // Reload
                 }
               

             
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

        private void Save()
        {
            try
            {
                ShowLoadingIndicator();

                string sBizName = ""; // 저장

                //2021-10-07 오화백 버튼셀
                if(_Product_Lev == "B")
                {
                    if ("Y".Equals(completeProd))
                    {
                        sBizName = "BR_PRD_REG_PUT_SUBLOT_IN_CST_WSB_UI";
                    }
                    else
                    {
                        sBizName = "BR_PRD_REG_PUT_SUBLOT_IN_CST_WSB";
                    }
                }
                else
                {
                    if ("Y".Equals(completeProd))
                    {
                        sBizName = "BR_PRD_REG_PUT_SUBLOT_IN_CST_WSS_UI";
                    }
                    else
                    {
                        sBizName = "BR_PRD_REG_PUT_SUBLOT_IN_CST_WSS";
                    }
                }
                

                // DATA SET
                DataSet inDataSet = new DataSet();
                DataTable inEQP = inDataSet.Tables.Add("IN_EQP");
                inEQP.Columns.Add("SRCTYPE", typeof(string));
                inEQP.Columns.Add("IFMODE", typeof(string));
                inEQP.Columns.Add("EQPTID", typeof(string));
                inEQP.Columns.Add("USERID", typeof(string));
                inEQP.Columns.Add("PROD_LOTID", typeof(string));
                inEQP.Columns.Add("EQPT_LOTID", typeof(string));
                inEQP.Columns.Add("OUT_LOTID", typeof(string));
                inEQP.Columns.Add("CSTID", typeof(string));

                DataTable inCST = inDataSet.Tables.Add("IN_CST");
                inCST.Columns.Add("SUBLOTID", typeof(string));
                inCST.Columns.Add("CSTSLOT", typeof(string));
                inCST.Columns.Add("CSTSLOT_F", typeof(string));
                inCST.Columns.Add("IROCV", typeof(string));

                DataRow newRow = inEQP.NewRow();
                newRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                newRow["IFMODE"] = IFMODE.IFMODE_OFF;
                newRow["EQPTID"] = Util.NVC(eqptID);
                newRow["USERID"] = LoginInfo.USERID;
                newRow["PROD_LOTID"] = Util.NVC(txtLotId.Text);
                newRow["EQPT_LOTID"] = string.Empty;
                newRow["OUT_LOTID"] = Util.NVC(outLotID);
                newRow["CSTID"] = Util.NVC(txtTrayId.Text);

                inEQP.Rows.Add(newRow);

                newRow = inCST.NewRow();
                newRow["SUBLOTID"] = Util.NVC(txtCellId.Text);
                // newRow["CSTSLOT"] = Util.NVC(sCstSLot);
                newRow["CSTSLOT"] = Util.NVC(txtTrayLocation.Text);
                newRow["CSTSLOT_F"] = string.Empty;
                newRow["IROCV"] = string.Empty;

                inCST.Rows.Add(newRow);

                string sTestXml = inDataSet.GetXml();

                new ClientProxy().ExecuteService_Multi(sBizName, "IN_EQP,IN_CST", null, (bizResult, bizException) =>
                {
                    try
                    {
                        if (bizException != null)
                        {
                            Util.MessageException(bizException);
                            return;
                        }
                       else
                        {
                            SetGridCell(false); // Reload
                        }

                        //Util.AlertInfo("정상 처리 되었습니다.");
                        Util.MessageInfo("SFU1889");
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
            finally
            {
                HiddenLoadingIndicator();
            }
        }

        private void txtTrayLocation_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                string sTrayLocationRegex = string.Empty;
                sSelectedTrayLocation = txtTrayLocation.Text;

                if (!Util.CheckDecimal(sSelectedTrayLocation, 0)) // 입력된 데이터형 판단
                {
                    txtTrayLocation.Text = Util.NVC(sSelectedTrayLocation);
                    return;
                }

                if (e.Key == Key.Back)
                    return;
                if (e.Key == Key.Delete)
                    return;

                //txtWipQty.Text = Util.NVC(txtChangeQty.Text) != string.Empty ? Util.NVC($"{Util.NVC_Int(txtWipQty.Text):#,###}") : Util.NVC(string.Empty);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
            }
        }


    }
}
