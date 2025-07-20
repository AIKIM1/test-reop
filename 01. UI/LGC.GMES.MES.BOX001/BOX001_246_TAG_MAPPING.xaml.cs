/*************************************************************************************
 Created Date : 2023.05.24
      Creator : 최경아
   Decription : Tag 매핑 이력 조회 (자동차) - tag 매핑 등록 화면
--------------------------------------------------------------------------------------
 [Change History]

**************************************************************************************/
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data;

using C1.WPF;
using C1.WPF.DataGrid;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.ControlsLibrary;
using System.ComponentModel;
using System.Threading;
using System.Linq;



namespace LGC.GMES.MES.BOX001
{
    /// <summary>
    /// BOX001_246_TAG_MAPPING.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class BOX001_246_TAG_MAPPING : C1Window, IWorkArea
    {
        #region Declaration & Constructor 
        Util _Util = new Util();
        string _mapTrgtCode = string.Empty;
        public BOX001_246_TAG_MAPPING()
        {
            InitializeComponent();
        }

        public IFrameOperation FrameOperation
        {
            get;
            set;
        }
        #endregion

        #region Event
        #region [Load 이벤트]
        private void C1Window_Loaded(object sender, RoutedEventArgs e)
        {
            //Util.SetWindowState(this);

            
            //txtbProcess.Text = LoginInfo.CFG_PDA_SHOP_ID + ">" + LoginInfo.CFG_PDA_AREA_NAME;
            txtbUserName.Text = LoginInfo.USERNAME;
            initGridTable();

            object[] tmps = C1WindowExtension.GetParameters(this);
            _mapTrgtCode = tmps[2].ToString();

            if (string.IsNullOrWhiteSpace(_mapTrgtCode))
            {
                //기본 Checked 설정이 RFID기 때문에, 화면 오픈 시 RFID 사용가능하도록 함.
                SetUseRF(true);
            }
            else
            {
                SetUsePallet(true);
               
            }


            this.Loaded -= C1Window_Loaded;
        }
        #endregion

        #region [Pallet ID Scan]
        private void txtPalletID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                //RFID 체크 되어있을 시, Return
                if ((bool)rdoRF.IsChecked)
                {
                    txtPalletID.Text = string.Empty;
                    txtRFID.Focus();
                    txtRFID.SelectAll();
                    return;
                }

                string strPalletId = txtPalletID.Text.Trim();

                if (string.IsNullOrWhiteSpace(strPalletId))
                {
                    //%1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", result =>
                    {
                        SetUseRF(true);

                    }, "Pallet ID");
                    return;
                }

                initGridTable();

                // 출하가능여부 체크
                GetPalletInfo(strPalletId);

                //RFID 미스캔 시, TextBox Focus
                if (dgPalletInfo.GetRowCount() > 0 && string.IsNullOrWhiteSpace(txtRFID.Text.Trim()))
                {
                    SetUseRF(true);
                }
            }
        }
        #endregion

        #region [RFID Scan]
        private void txtRFID_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                //PalletID 체크 되어있을 시, Return 
                if ((bool)rdoPallet.IsChecked)
                {
                    txtRFID.Text = string.Empty;
                    txtPalletID.Focus();
                    txtPalletID.SelectAll();
                    return;
                }

                string strCarrierId = txtRFID.Text.Trim();

                if (string.IsNullOrWhiteSpace(strCarrierId))
                {
                    //%1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", result =>
                    {
                        SetUseRF(true);
                    }, "RFID");
                    return;
                }

                if (ChkUseRF(strCarrierId))
                {
                    if (dgPalletInfo.GetRowCount() > 0)
                    {

                    }
                    else
                    {
                        SetUsePallet(true);
                    }
                }
                //else
                //{
                //	SetUseRF(true);
                //}
            }
        }
        #endregion

        #region [RFID 라디오버튼 체크 이벤트]
        private void rdoRF_Ischecked(object sender, RoutedEventArgs e)
        {
            SetUseRF();
        }
        #endregion

        #region [Pallet 라디오버튼 체크 이벤트]
        private void rdoPallet_Ischecked(object sender, RoutedEventArgs e)
        {
            SetUsePallet(true);
        }
        #endregion

        #region [초기화 버튼 클릭 이벤트]
        private void btnInitialize_Click(object sender, RoutedEventArgs e)
        {
            DataClear();
        }
        #endregion

        #region [매핑 버튼 클릭 이벤트]
        private void btnMap_Click(object sender, RoutedEventArgs e)
        {
            string strCarrierId = txtRFID.Text.Trim();

            if(ChkUseRF(strCarrierId))
             {
                MappingTag();
            }

        }
        #endregion

        #region [닫기 버튼 클릭 이벤트]
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = MessageBoxResult.Cancel;
            this.Close();
        }
        #endregion
        #endregion

        #region Mehod
        #region [Pallet RFID 매핑 가능여부 체크]
        private void GetPalletInfo(string PalletID)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(PalletID))
                {
                    ////PALLETID를 입력해주세요
                    Util.MessageValidation("SFU1411", result =>
                    {
                        SetUsePallet(true);
                    });
                    return;
                }

                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("LANGID", typeof(String));
                RQSTDT.Columns.Add("BOXID", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["LANGID"] = LoginInfo.LANGID;
                dr["BOXID"] = PalletID;

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_GET_PALLET_INFO_FOR_MAPPING_RFID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult.Rows.Count == 0)
                {
                    //Util.MessageValidation("SFU1905");   //조회된 Data가 없습니다.
                    //SetUsePallet(true);

                    //조회된 Data가 없습니다.
                    Util.MessageValidation("SFU1905", result =>
                    {
                        SetUsePallet(true);
                    });
                    return;
                }

                Util.GridSetData(dgPalletInfo, SearchResult, FrameOperation);
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                //SetUsePallet(true);
                Util.MessageException(ex, result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetUsePallet(true);
                    }
                });

                return;
            }
        }
        #endregion

        #region [RFID 사용 가능여부 체크]
        private bool ChkUseRF(string sRFID)
        {
            try
            {
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "INDATA";
                RQSTDT.Columns.Add("TAGID", typeof(String));
                RQSTDT.Columns.Add("MODE", typeof(String));

                DataRow dr = RQSTDT.NewRow();
                dr["TAGID"] = sRFID;
                dr["MODE"] = "M";       // Mapping모드

                RQSTDT.Rows.Add(dr);

                DataTable SearchResult = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_USING_CSTID", "INDATA", "OUTDATA", RQSTDT);

                if (SearchResult.Rows.Count > 0)
                {
                    return true;
                }
                else
                {
                    SetUseRF(true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                //Util.MessageException(ex);
                //SetUseRF(true);

                Util.MessageException(ex, result =>
                {
                    if (result == MessageBoxResult.OK)
                    {
                        SetUseRF(true);
                    }
                });

                return false;
            }
        }
        #endregion

        #region [초기화 함수]
        private void DataClear()
        {
            dgPalletInfo.ItemsSource = null;
            txtPalletID.Text = string.Empty;
            txtRFID.Text = string.Empty;
            _mapTrgtCode = string.Empty;

            SetUseRF(true);

            //txtbProcess.Text = LoginInfo.CFG_PDA_AREA_ID + ">" + LoginInfo.CFG_PDA_PROC_NAME;

            initGridTable();
        }
        #endregion

        #region [Grid 테이블 생성]
        private void initGridTable()
        {
            DataTable dt = new DataTable();
            foreach (C1.WPF.DataGrid.DataGridColumn col in dgPalletInfo.Columns)
            {
                dt.Columns.Add(Convert.ToString(col.Name));
            }
            dgPalletInfo.BeginEdit();
            dgPalletInfo.ItemsSource = DataTableConverter.Convert(dt);
            dgPalletInfo.EndEdit();
        }
        #endregion

        #region [Pallet - RFID 매핑 처리]
        private void MappingTag()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtRFID.Text))
                {
                    //%1이 입력되지 않았습니다.
                    Util.MessageValidation("SFU1299", result =>
                    {
                        SetUseRF(true);

                    }, "RFID");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtPalletID.Text) || dgPalletInfo.Rows.Count == 0)
                {
                    ////PALLETID를 입력해주세요
                    Util.MessageValidation("SFU1411", result =>
                    {
                        SetUsePallet(true);
                    });
                    return;
                }

                DataTable dt = DataTableConverter.Convert(dgPalletInfo.ItemsSource);

                if (dt.Rows.Count == 0)
                {
                    /*
					InputManager.Current.ProcessInput(
						new KeyEventArgs(Keyboard.PrimaryDevice,
							Keyboard.PrimaryDevice.ActiveSource,
							0, Key.Tab)
						{
							RoutedEvent = Keyboard.KeyDownEvent
						}
					);
					*/
                    KeyEventArgs arg = new KeyEventArgs(Keyboard.PrimaryDevice, Keyboard.PrimaryDevice.ActiveSource, 0, Key.Enter);
                    arg.RoutedEvent = Keyboard.KeyDownEvent;
                    this.txtPalletID_KeyDown(null, arg);

                    dt = DataTableConverter.Convert(dgPalletInfo.ItemsSource);

                    if (dt.Rows.Count == 0)
                    {
                        return;
                    }
                }

                DataTable RQSTDT = new DataTable("INDATA");
                RQSTDT.Columns.Add("USERID");
                RQSTDT.Columns.Add("BOXID");
                RQSTDT.Columns.Add("TAGID");
                RQSTDT.Columns.Add("SRCTYPE");

                DataRow newRow = RQSTDT.NewRow();
                newRow["USERID"] = LoginInfo.USERID;
                newRow["BOXID"] = dt.Rows[0]["BOXID"].ToString().Trim();
                newRow["TAGID"] = txtRFID.Text.Trim();
                newRow["SRCTYPE"] = "UI";
                RQSTDT.Rows.Add(newRow);

                new ClientProxy().ExecuteService("BR_FORM_REG_MAPPING_CSTID_PLTID_UI", "INDATA", null, RQSTDT, (RSLTDT, ex) =>
                {
                    try
                    {
                        if (ex != null)
                        {
                            Util.MessageException(ex);
                            return;
                        }
                        DataClear();
                    }
                    catch (Exception ex1)
                    {
                        Util.MessageException(ex1);
                        return;
                    }
                    Util.MessageValidation("SFU1275", result =>
                    {
                        //SetUseRF(true);
                        this.DialogResult = MessageBoxResult.OK;
                        this.Close();

                    });
                    return;
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
        #endregion

        #region [RFID 스캔 시 Set]
        private void SetUseRF(bool bClear = false)
        {
            if (txtRFID != null)
            {
                if (!(bool)rdoRF.IsChecked)
                {
                    rdoRF.IsChecked = true;
                }

                if (bClear)
                {
                    txtRFID.Text = string.Empty;
                }

                txtRFID.Focus();
                txtRFID.SelectAll();
            }
        }
        #endregion

        #region [PalletID 스캔 시 Set]
        private void SetUsePallet(bool bClear = false)
        {
            if (!(bool)rdoPallet.IsChecked)
            {
                rdoPallet.IsChecked = true;
            }

            if (bClear)
            {
                txtPalletID.Text = string.Empty;
            }

            if (string.IsNullOrWhiteSpace(_mapTrgtCode))
            {
                txtPalletID.Text = string.Empty;
                initGridTable();
                txtPalletID.Focus();
                txtPalletID.SelectAll();
            }
            else
            {
                txtPalletID.Text = _mapTrgtCode;
                GetPalletInfo(_mapTrgtCode);
            }

        }
        #endregion
        #endregion

    }
}
