/*************************************************************************************
 Created Date : 2018.10.09
      Creator : 
   Decription : 믹서원자재 수동투입(CWA)
--------------------------------------------------------------------------------------
 [Change History]
  2018.10.09  DEVELOPER : Initial Created.
**************************************************************************************/

using C1.WPF;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LGC.GMES.MES.ELEC001
{
    public partial class ELEC001_003_CWA : UserControl, IWorkArea
    {
        #region < Declaration & Constructor >

        Util _Util = new Util();
        CommonCombo _combo = new CommonCombo();
        private string _EQPTID = string.Empty;        
        string _HOPPER = string.Empty;
        string sMTRLID = string.Empty;
        string _WOID = string.Empty;
        string _BTCHORDID = string.Empty;
        string _ProdID = string.Empty;
        string _SUPPLIERID = string.Empty;
        string _PROD_VER_CODE = string.Empty;
        DataSet inDataSet = null;
        DataTable IndataTable = null;
        DataTable RMTRL_LABEL = null;
        DataRowView SaveData = null;
        DataTable Initialize = new DataTable();
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public ELEC001_003_CWA()
        {
            InitializeComponent();
            this.Loaded += UserControl_Loaded;
        }
       
        #endregion < Declaration & Constructor >


        #region < Initialize >

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= UserControl_Loaded;
            ApplyPermissions();
            InitCombo();
        }

        private void InitCombo()
        {
            string[] Filter = new string[] { LoginInfo.CFG_AREA_ID };

            //라인
            C1ComboBox[] cboEquipmentSegmentChild = { cboProcess, cboEquipment };
            _combo.SetCombo(cboEquipmentSegment, CommonCombo.ComboStatus.SELECT, cbChild: cboEquipmentSegmentChild, sFilter: Filter);

            //공정
            C1ComboBox[] cboProcessParent = { cboEquipmentSegment };
            C1ComboBox[] cboProcessChild = { cboEquipment };
            _combo.SetCombo(cboProcess, CommonCombo.ComboStatus.NONE, cbChild: cboProcessChild, cbParent: cboProcessParent, sCase: "ProcessCWA");

            //설비
            C1ComboBox[] cboEquipmentParent = { cboEquipmentSegment, cboProcess };
            _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.NONE, cbParent: cboEquipmentParent);            
        }

        public void SetCombo_Select(DataTable dt, string sValue, string sDisplay)
        {
            if (dt == null || dt.Rows.Count < 1)
            {
                dt.Columns.Add("MTRL_LOTID", typeof(string));
                DataRow dr1 = dt.NewRow();
                dr1["MTRL_LOTID"] = "-SELECT-";
                dt.Rows.InsertAt(dr1, 0);
            }
            cboMtrlLOTID.ItemsSource = DataTableConverter.Convert(Initialize);
            cboMtrlLOTID.SelectedIndex = 0;
            return;
        }

        public void SetCombo_Select_Value(DataTable dt, string sValue, string sDisplay)
        {
            DataRow dr = dt.NewRow();
            dr["MTRL_LOTID"] = "-SELECT-";
            dt.Rows.InsertAt(dr, 0);
            return;
        }
        
        #endregion < Initialize >


        #region < Event >

        private void cboEquipmentSegment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            //if (Util.NVC(cboEquipmentSegment.SelectedValue) != string.Empty)
            //{
            //    String[] sFilter2 = { cboEquipmentSegment.SelectedValue.ToString(), "E0500,E1000", null };//2017-08-14 권병훈C 요청으로 LoginInfo.CFG_PROC_ID -> "E0500,E1000" 수정
            //    _combo.SetCombo(cboEquipment, CommonCombo.ComboStatus.SELECT, sFilter: sFilter2);
            //    cboEquipment.SelectedIndex = 0;
            //}
        }

        private void cboEquipment_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            Util.gridClear(dgList);
            Util.gridClear(dgRequest);
            Util.gridClear(dgMaterial);
            this.txtRequestNo.Clear();
            this.txtEquipmentName.Clear();
            this.txtHopper.Clear();

            if (Util.NVC(cboEquipment.SelectedValue) != string.Empty && Util.NVC(cboEquipment.SelectedValue) != "-SELECT-")
            {
                _EQPTID = Util.NVC(cboEquipment.SelectedValue.ToString().Trim());
            }
        }

        private void cboMtrlLOTID_SelectedValueChanged(object sender, PropertyChangedEventArgs<object> e)
        {
            try
            {
                if (cboMtrlLOTID.Text == "-SELECT-" || cboMtrlLOTID.Text == "")
                    return;
                if (string.IsNullOrEmpty(txtRequestNo.Text))
                {
                    //투입요청서를 입력하세요.
                    Util.MessageInfo("SFU1976", (result) =>
                    {
                        this.txtMtrlID.Clear();
                    });
                    return;
                }
                if (string.IsNullOrEmpty(txtHopper.Text) || sMTRLID == "")
                {
                    //요청자재정보를 선택하세요.
                    Util.MessageInfo("SFU2923", (result) =>
                    {
                        this.txtMtrlID.Clear();
                    });
                    return;
                }
                if (string.IsNullOrEmpty(txtMtrlID.Text) || sMTRLID == "")
                {
                    //자재파렛트를 스캔하세요.
                    Util.MessageInfo("SFU1834", (result) =>
                    {
                        SetCombo_Select(Initialize, "MTRL_LOTID", "MTRL_LOTID");//콤보초기화
                        this.txtMtrlID.Focus();
                    });
                    return;
                }

                DataTable dt = ValidSublot(txtMtrlID.Text);

                //업체정보 체크 Validation
                this._SUPPLIERID = dt.Rows[cboMtrlLOTID.SelectedIndex - 1]["SUPPLIERID"].ToString();
                decimal dINPUT_QTY = (decimal)dt.Rows[cboMtrlLOTID.SelectedIndex - 1]["INPUT_QTY"];

                if (!SupplierProdVerCheck())
                {
                    cboMtrlLOTID.SelectedIndex = 0;
                    cboMtrlLOTID.Focus();
                    return;
                }

                DataTable dtData = new DataTable();
                dtData.Columns.Add("CHK", typeof(string));
                dtData.Columns.Add("RMTRL_LABEL_ID", typeof(string));
                dtData.Columns.Add("MTRLID", typeof(string));
                dtData.Columns.Add("MTRL_LOTID", typeof(string));
                dtData.Columns.Add("PLLT_ID", typeof(string));
                dtData.Columns.Add("INPUT_QTY", typeof(decimal));
                dtData.Columns.Add("HOPPER_ID", typeof(string));
                dtData.AcceptChanges();

                if (dt == null)
                {
                    Util.MessageValidation("SFU1832");  //자재정보가 맞지않습니다.
                    txtMtrlID.Text = string.Empty;
                    txtMtrlID.Focus();
                    return;
                }
                if (dt.Rows.Count > 0)
                {
                    //업제LOT 콤보셋팅
                    if (cboMtrlLOTID == null || cboMtrlLOTID.Text == "")
                    {
                        cboMtrlLOTID.ItemsSource = DataTableConverter.Convert(dt);
                        cboMtrlLOTID.SelectedIndex = 0;
                    }
                    if (dgList.ItemsSource != null && dgList.Rows.Count > 0)
                    {
                        dtData = DataTableConverter.Convert(dgList.ItemsSource);
                    }
                    if (ValidMtrlLotid())
                    {
                        DataRow newRow = null;
                        newRow = dtData.NewRow();
                        newRow["CHK"] = true;
                        newRow["RMTRL_LABEL_ID"] = Util.NVC(cboMtrlLOTID.SelectedValue.ToString());//dt.Rows[0]["RMTRL_LABEL_ID"];
                        newRow["MTRLID"] = sMTRLID;// dt.Rows[cboMtrlLOTID.SelectedIndex-1]["MTRLID"];
                        newRow["MTRL_LOTID"] = Util.NVC(cboMtrlLOTID.Text.Trim()); //dt.Rows[0]["MTRL_LOTID"];
                        newRow["PLLT_ID"] = Util.NVC(txtMtrlID.Text.Trim().ToString());//dt.Rows[0]["PLLT_ID"];
                        newRow["INPUT_QTY"] = dINPUT_QTY;
                        newRow["HOPPER_ID"] = Util.NVC(txtHopper.Tag.ToString());
                        dtData.Rows.Add(newRow);

                        dgList.BeginEdit();
                        //dgList.ItemsSource = DataTableConverter.Convert(dtData);
                        Util.GridSetData(dgList, dtData, null);
                        dgList.EndEdit();
                        RMTRL_LABEL = dtData; //((DataView)dgList.ItemsSource).Table;
                        txtMtrlID.Clear();
                        txtMtrlID.Focus();
                        this.cboMtrlLOTID.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    txtMtrlID.Clear();
                    txtMtrlID.Focus();
                });
                return;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
            if (RMTRL_LABEL != null)
            {
                for (int i = 0; i < RMTRL_LABEL.Rows.Count; i++)
                {
                    DataRow row = RMTRL_LABEL.Rows[i];

                    if (idx == i)
                        RMTRL_LABEL.Rows[i]["CHK"] = true;
                }
            }
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)cb.Parent).Row.Index;
            if (RMTRL_LABEL != null)
            {
                for (int i = 0; i < RMTRL_LABEL.Rows.Count; i++)
                {
                    DataRow row = RMTRL_LABEL.Rows[i];

                    if (idx == i)
                        RMTRL_LABEL.Rows[i]["CHK"] = false;
                }
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Items.Count == 0)
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            if (cboEquipment.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            GetRequest();
        }

        private void btnInput_Click(object sender, RoutedEventArgs e)
        {
            SetInput();
        }

        private void btnInputHist_Click(object sender, RoutedEventArgs e)
        {
            if (cboEquipment.Text.Equals("-SELECT-"))
            {
                Util.MessageValidation("SFU1673");  //설비를 선택하세요
                return;
            }
            ELEC001_003_INPUT_HIST _ReqDetail = new ELEC001_003_INPUT_HIST();
            _ReqDetail.FrameOperation = FrameOperation;

            if (_ReqDetail != null)
            {
                object[] Parameters = new object[2];
                Parameters[0] = cboEquipmentSegment.SelectedValue.ToString();
                Parameters[1] = _EQPTID;
                C1WindowExtension.SetParameters(_ReqDetail, Parameters);

                _ReqDetail.ShowModal();
                _ReqDetail.CenterOnScreen();
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (dgList.Rows.Count < 1)
            {
                //삭제할 항목이 없습니다.
                Util.MessageInfo("SFU1597", (result) =>
                {
                    txtMtrlID.Focus();
                });
                return;
            }
            else
            {
                DataTable dt = ((DataView)dgList.ItemsSource).Table;

                for (int i = dt.Rows.Count; 0 <= i; i--)
                {
                    if (_Util.GetDataGridCheckValue(dgList, "CHK", i))
                    {
                        dt.Rows[i].Delete();
                        dgList.BeginEdit();
                        dgList.ItemsSource = DataTableConverter.Convert(dt);
                        dgList.EndEdit();
                    }
                }
                this.txtMtrlID.Focus();
            }
        }

        private void dgRequest_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;
                
                //DataTable dt = DataTableConverter.Convert(((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid.ItemsSource);

                //if (dt != null)
                //{
                //    for (int i = 0; i < dt.Rows.Count; i++)
                //    {
                //        DataRow row = dt.Rows[i];

                //        if (idx == i)
                //            dt.Rows[i]["CHK"] = true;
                //        else
                //            dt.Rows[i]["CHK"] = false;
                //    }
                //    dgRequest.BeginEdit();
                //    dgRequest.ItemsSource = DataTableConverter.Convert(dt);
                //    dgRequest.EndEdit();
                //}
                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                
                GetRequstList(dgRequest.Rows[idx].DataItem);
                this.txtRequestNo.Text = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[idx].DataItem, "REQ_ID"));
                this.txtEquipmentName.Text = Util.NVC(cboEquipment.Text);
                this._BTCHORDID = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[idx].DataItem, "BTCH_ORD_ID"));
                this._WOID = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[idx].DataItem, "WOID"));
                this._ProdID = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[idx].DataItem, "PRODID"));
                this._PROD_VER_CODE = Util.NVC(DataTableConverter.GetValue(dgRequest.Rows[idx].DataItem, "PROD_VER_CODE"));
                SetCombo_Select(Initialize, "MTRL_LOTID", "MTRL_LOTID");//콤보초기화
                //row 색 바꾸기
                dgRequest.SelectedIndex = idx;
            }
           
        }

        private void dgMaterial_Checked(object sender, RoutedEventArgs e)
        {
            if (sender == null)
                return;

            RadioButton rb = sender as RadioButton;

            if (rb.DataContext == null)
                return;

            if ((bool)rb.IsChecked && (rb.DataContext as DataRowView).Row["CHK"].ToString().Equals("0"))
            {
                int idx = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Row.Index;
                DataRow dtRow = (rb.DataContext as DataRowView).Row;

                C1.WPF.DataGrid.C1DataGrid dg = ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).DataGrid;
                for (int i = 0; i < dg.GetRowCount(); i++)
                {
                    if (idx == i)
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, true);
                    else
                        DataTableConverter.SetValue(dg.Rows[i].DataItem, ((C1.WPF.DataGrid.DataGridCellPresenter)rb.Parent).Column.Name, false);
                }
                this._HOPPER = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "HOPPER_ID"));
                this.sMTRLID = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "MTRLID"));
                this.txtHopper.Text = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "HOPPER_NAME"));
                this.txtHopper.Tag = Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[idx].DataItem, "HOPPER_ID"));
                SetCombo_Select(Initialize, "MTRL_LOTID", "MTRL_LOTID");//콤보초기화
                this.txtMtrlID.Clear();
                this.txtMtrlID.Focus();
            }
        }

        private void txtRequest_KeyDown(object sender, KeyEventArgs e) //사용제외처리
        {
            try
            {
                if (string.IsNullOrEmpty(txtRequestNo.Text)) return;
                if (e.Key == Key.Enter)
                {
                    DataTable dt = ValidRequestNo(txtRequestNo.Text);
                    if (dt == null)
                    {
                        Util.MessageValidation("SFU1911"); //존재하지 않는 작업요청서입니다.
                        txtRequestNo.Clear();
                        txtEquipmentName.Clear();
                        txtRequestNo.Focus();
                        return;
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            txtEquipmentName.Clear();
                            txtEquipmentName.Text = dt.Rows[0]["EQPTNAME"].ToString();
                            _EQPTID = dt.Rows[0]["EQPTID"].ToString();
                            txtMtrlID.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    txtRequestNo.Clear();
                    txtEquipmentName.Clear();
                    txtRequestNo.Focus();
                });
            }
        }

        private void txtHopper_KeyDown(object sender, KeyEventArgs e) //사용안함
        {
            try
            {
                if (string.IsNullOrEmpty(txtHopper.Text)) return;

                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtRequestNo.Text))
                    {
                        //투입요청서를 입력하세요.
                        Util.MessageInfo("SFU1976", (result) =>
                        {
                            this.txtHopper.Clear();
                            this.txtRequestNo.Focus();
                        });
                        return;
                    }
                    if (string.IsNullOrEmpty(txtMtrlID.Text))
                    {
                        //라벨정보를 입력하세요.
                        Util.MessageInfo("SFU1524", (result) =>
                        {
                            this.txtHopper.Clear();
                            this.txtMtrlID.Focus();
                        });
                        return;
                    }
                    DataTable dt = ValidHopper(txtHopper.Text);
                    if (dt.Rows.Count > 0)
                    {
                        if (dgList.ItemsSource != null)
                        {
                            ValueToFind(dgList, txtMtrlID.Text);
                        }
                    }
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("CHK", typeof(string));
                    dtData.Columns.Add("RMTRL_LABEL_ID", typeof(string));
                    dtData.Columns.Add("MTRLID", typeof(string));
                    dtData.Columns.Add("MTRL_LOTID", typeof(string));
                    dtData.Columns.Add("PLLT_ID", typeof(string));
                    dtData.AcceptChanges();

                    if (dt == null)
                    {
                        Util.MessageValidation("SFU2036");  //호퍼정보가 맞지않습니다.
                        txtHopper.Clear();
                        txtHopper.Focus();
                        return;
                    }
                    else
                    {
                        if (dt.Rows.Count > 0)
                        {
                            //if (dgList.ItemsSource != null)
                            //{
                            //    if (ValueToFind(dgList, txtMtrlID.Text))
                            //    {
                            //        dtData = DataTableConverter.Convert(dgList.ItemsSource);
                            //    }
                            //}
                            DataRow newRow = null;
                            newRow = dtData.NewRow();
                            newRow["CHK"] = true;
                            newRow["RMTRL_LABEL_ID"] = dt.Rows[0]["RMTRL_LABEL_ID"];
                            newRow["MTRLID"] = dt.Rows[0]["MTRLID"];
                            newRow["MTRL_LOTID"] = dt.Rows[0]["MTRL_LOTID"];
                            newRow["PLLT_ID"] = dt.Rows[0]["PLLT_ID"];
                            dtData.Rows.Add(newRow);

                            dgList.BeginEdit();
                            dgList.ItemsSource = DataTableConverter.Convert(dtData);
                            dgList.EndEdit();
                            RMTRL_LABEL = dtData; //((DataView)dgList.ItemsSource).Table;
                            txtMtrlID.Clear();
                            txtHopper.Clear();
                            txtMtrlID.Focus();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    txtHopper.Clear();
                    txtHopper.Focus();
                });
            }
        }

        private void txtMtrlID_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(txtMtrlID.Text.ToString())) return;
                if (e.Key == Key.Enter)
                {
                    if (string.IsNullOrEmpty(txtRequestNo.Text))
                    {
                        //투입요청서를 입력하세요.
                        Util.MessageInfo("SFU1976", (result) =>
                        {
                            this.txtMtrlID.Clear();
                        });
                        return;
                    }
                    if (string.IsNullOrEmpty(txtHopper.Text) || sMTRLID == "")
                    {
                        //요청 자재 정보를 선택하세요.
                        Util.MessageInfo("SFU1750", (result) =>
                        {
                            this.txtMtrlID.Clear();
                        });
                        return;
                    }
                    DataTable dt = ValidSublot(txtMtrlID.Text);
                  
                    if (dt == null)
                    {
                        Util.MessageValidation("SFU1832");  //자재정보가 맞지않습니다.
                        txtMtrlID.Text = string.Empty;
                        txtMtrlID.Focus();
                        return;
                    }
                    else
                    {
                        if(dt.Rows.Count == 1)
                        {
                            SetCombo_Select_Value(dt, "MTRL_LOTID", "MTRL_LOTID");
                            cboMtrlLOTID.ItemsSource = DataTableConverter.Convert(dt);
                            cboMtrlLOTID.SelectedIndex = 1;
                            this.cboMtrlLOTID.Focus();
                        }
                        else
                        {
                            SetCombo_Select_Value(dt, "MTRL_LOTID", "MTRL_LOTID");
                            cboMtrlLOTID.ItemsSource = DataTableConverter.Convert(dt);
                            cboMtrlLOTID.SelectedIndex = 0;
                            this.cboMtrlLOTID.Focus();
                        }

                        //if (dt.Rows.Count > 0)
                        //{
                        //    ////팔레트 중복 체크
                        //    //if (dgList.ItemsSource != null && dgList.Rows.Count > 0)
                        //    //{
                        //    //    if (ValueToFind(dgList, txtMtrlID.Text))
                        //    //    {
                        //    //        dtData = DataTableConverter.Convert(dgList.ItemsSource);
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        return;
                        //    //    }
                        //    //}
                        //    //업제LOT 콤보셋팅
                        //    if(cboMtrlLOTID == null || cboMtrlLOTID.Text == "")
                        //    {
                        //        cboMtrlLOTID.ItemsSource = DataTableConverter.Convert(dt);
                        //        cboMtrlLOTID.SelectedIndex = 0;
                        //    }
                        //    if (dgList.ItemsSource != null && dgList.Rows.Count > 0)
                        //    {
                        //        dtData = DataTableConverter.Convert(dgList.ItemsSource);
                        //    }
                        //    if (ValidMtrlLotid())
                        //    {
                        //        DataRow newRow = null;
                        //        newRow = dtData.NewRow();
                        //        newRow["CHK"] = true;
                        //        newRow["RMTRL_LABEL_ID"] = Util.NVC(cboMtrlLOTID.SelectedValue.ToString());//dt.Rows[0]["RMTRL_LABEL_ID"];
                        //        newRow["MTRLID"] = dt.Rows[0]["MTRLID"];
                        //        newRow["MTRL_LOTID"] = Util.NVC(cboMtrlLOTID.Text.Trim()); //dt.Rows[0]["MTRL_LOTID"];
                        //        newRow["PLLT_ID"] = dt.Rows[0]["PLLT_ID"];
                        //        dtData.Rows.Add(newRow);

                        //        dgList.BeginEdit();
                        //        dgList.ItemsSource = DataTableConverter.Convert(dtData);
                        //        dgList.EndEdit();
                        //        RMTRL_LABEL = dtData; //((DataView)dgList.ItemsSource).Table;
                        //        LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("투입하고자 하는 업체LOT이 맞는지 확인해주세요.\n(틀리면 자재 삭제 -> 업체LOT 선택 -> 팔레트 재스캔, 맞으면 그대로 진행)"), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                        //        {
                        //            txtMtrlID.Clear();
                        //            txtMtrlID.Focus();
                        //        });
                        //    }
                        //    else
                        //    {
                        //        txtMtrlID.Clear();
                        //        cboMtrlLOTID.Focus();
                        //    }
                        //}
                    }
                }
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    txtMtrlID.Clear();
                    txtMtrlID.Focus();
                });
            }
        }        
        
        #endregion < Event >


        #region < Mehod >

        private bool SupplierProdVerCheck()
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("AREAID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("BTCH_ORD_ID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("WOID", typeof(string));
                IndataTable.Columns.Add("SUPPLIERID", typeof(string));
                IndataTable.Columns.Add("PRODID", typeof(string));
                IndataTable.Columns.Add("PROD_VER_CODE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["AREAID"] = LoginInfo.CFG_AREA_ID;
                Indata["EQPTID"] = _EQPTID;
                Indata["BTCH_ORD_ID"] = _BTCHORDID;
                Indata["MTRLID"] = sMTRLID;
                Indata["WOID"] = _WOID;
                Indata["SUPPLIERID"] = _SUPPLIERID;
                Indata["PRODID"] = _ProdID;
                Indata["PROD_VER_CODE"] = _PROD_VER_CODE;
                IndataTable.Rows.Add(Indata);
                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RMTRL_INPUT_REQ_FOR_SUPPLIER", "INDATA", "OUTDATA", IndataTable);

                if (dtMain.Rows[0]["CHK_FLAG"].ToString() == "N")
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ApplyPermissions()
        {
            List<Button> listAuth = new List<Button>();
            listAuth.Add(btnInput);
            Util.pageAuth(listAuth, FrameOperation.AUTHORITY);
        }

        private void GetRequest()
        {
            try
            {
                Util.gridClear(dgList);
                Util.gridClear(dgRequest);
                Util.gridClear(dgMaterial);
                this.txtRequestNo.Clear();
                this.txtEquipmentName.Clear();
                this.txtHopper.Clear();
                
                SetCombo_Select(Initialize, "MTRL_LOTID", "MTRL_LOTID");//콤보초기화

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["EQPTID"] = Util.NVC(cboEquipment.SelectedValue.ToString());
                IndataTable.Rows.Add(Indata);                

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_INPUT_REQUEST_CWA", "INDATA", "RSLTDT", IndataTable);                

                Util.GridSetData(dgRequest, dtMain, FrameOperation);
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private void GetRequstList(Object SelectedItem)
        {
            DataRowView rowview = SelectedItem as DataRowView;
            SaveData = SelectedItem as DataRowView;

            if (rowview == null)
            {
                return;
            }

            try
            {
                Util.gridClear(dgMaterial);
                Util.gridClear(dgList);
                this.txtHopper.Clear();

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LANGID", typeof(string));
                IndataTable.Columns.Add("REQ_ID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LANGID"] = LoginInfo.LANGID;
                Indata["REQ_ID"] = rowview["REQ_ID"].ToString();
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MTRL_INFO_BY_REQUEST", "INDATA", "RSLTDT", IndataTable);

                Util.GridSetData(dgMaterial, dtMain, null);                
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }
       
        private void SetInput()
        {
            try
            {
                if (dgList.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1979");  //투입요청한 자재가 없습니다.
                    return;
                }

                inDataSet = new DataSet();
                DataTable inDataTable = inDataSet.Tables.Add("INDATA");
                inDataTable.Columns.Add("REQ_ID", typeof(string));                
                inDataTable.Columns.Add("EQPTID", typeof(string));
                inDataTable.Columns.Add("AREAID", typeof(string));
                inDataTable.Columns.Add("USERID", typeof(string));
                inDataTable.Columns.Add("SRCTYPE", typeof(string));

                DataRow inDataRow = null;

                inDataRow = inDataTable.NewRow();
                inDataRow["REQ_ID"] = txtRequestNo.Text.Trim();                
                inDataRow["EQPTID"] = _EQPTID;
                inDataRow["AREAID"] = LoginInfo.CFG_AREA_ID;
                inDataRow["USERID"] = LoginInfo.USERID;
                inDataRow["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                inDataTable.Rows.Add(inDataRow);

                DataTable inMtrlid = _Mtrlid();
                
                if (IndataTable.Rows.Count <= 0)
                {
                    Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                    return;
                }
                int idx = _Util.GetDataGridCheckFirstRowIndex(dgList, "CHK");
                if (idx < 0)
                {
                    Util.MessageValidation("SFU1662");  //선택한 자재가 없습니다.
                    return;
                }
                //투입수량 확인
                DataTable dtTop2 = DataTableConverter.Convert(dgList.ItemsSource);
                foreach (DataRow _iRow in dtTop2.Rows)
                {
                    if (_iRow["INPUT_QTY"].ToString() == "" || Double.Parse(Util.NVC(_iRow["INPUT_QTY"])) <= 0)
                    {
                        Util.MessageValidation("SFU1953");  //투입수량을 입력해주세요.
                        return;
                    }
                }

                //BR_PRD_CHK_MIX_MTRL_VALIDATION
                //투입처리 하시겠습니까?
                Util.MessageConfirm("SFU1987", (sresult) =>
                {
                    if (sresult == MessageBoxResult.OK)
                    {
                        new ClientProxy().ExecuteService_Multi("BR_PRD_REG_RMTRL_INPUT_REQ", "INDATA,INLABEL", null, (result, ex) =>
                        {
                            if (ex != null)
                            {
                                //LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1737" + ex.Message), null, "Error", MessageBoxButton.OK, MessageBoxIcon.None);
                                //Util.AlertByBiz("BR_PRD_REG_RMTRL_INPUT_REQ", ex.Message, ex.ToString());
                                Util.MessageException(ex);
                                return;
                            }
                            else
                            {
                                Util.AlertInfo("SFU1973");  //투입완료되었습니다.
                                Util.gridClear(dgList);
                                this.GetRequstList(SaveData);
                                this.txtHopper.Clear();
                                this.txtMtrlID.Clear();
                                this.ValidateMtrlInputFlag();
                                SetCombo_Select(Initialize, "MTRL_LOTID", "MTRL_LOTID");//콤보초기화
                            }
                        }, inDataSet);

                    }
                });
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private DataTable _Mtrlid()
        {
            IndataTable = inDataSet.Tables.Add("INLABEL");
            IndataTable.Columns.Add("RMTRL_LABEL_ID", typeof(string));
            IndataTable.Columns.Add("MTRL_LOTID", typeof(string));
            IndataTable.Columns.Add("PLLT_ID", typeof(string));
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("INPUT_QTY", typeof(string));
            IndataTable.Columns.Add("HOPPER_ID", typeof(string));

            dgList.EndEdit();
            DataTable dtTop = RMTRL_LABEL; //((DataView)dgList.ItemsSource).Table;
            DataTable dtTop2 = DataTableConverter.Convert(dgList.ItemsSource);

            foreach (DataRow _iRow in dtTop2.Rows)
            {
                if (_iRow["CHK"].Equals("True"))
                {
                    _iRow["RMTRL_LABEL_ID"] = _iRow["RMTRL_LABEL_ID"];
                    _iRow["MTRL_LOTID"] = _iRow["MTRL_LOTID"];
                    _iRow["PLLT_ID"] = _iRow["PLLT_ID"];
                    _iRow["MTRLID"] = _iRow["MTRLID"];
                    _iRow["INPUT_QTY"] = _iRow["INPUT_QTY"];
                    _iRow["HOPPER_ID"] = _iRow["HOPPER_ID"];
                    IndataTable.ImportRow(_iRow);
                }
            }
            return IndataTable;
        }

        private DataTable GetRequestEqpt(string ReqNo)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("REQ_ID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["REQ_ID"] = ReqNo;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("DA_PRD_SEL_MIXMTRL_REQUEST_EQPT_CBO", "INDATA", "RSLTDT", IndataTable);

            if (dtMain.Rows.Count == 0)
            {
                return null;
            }
            return dtMain;

        }

        private DataTable ValidRequestNo(string ReqNo)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("LANGID", typeof(string));
            IndataTable.Columns.Add("REQ_ID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["LANGID"] = LoginInfo.LANGID;
            Indata["REQ_ID"] = ReqNo;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RMTRL_INPUT_REQ_FOR_INPUT_REQ", "INDATA", "RSLTDT", IndataTable);

            if (dtMain.Rows.Count == 0)
            {
                return null;
            }
            return dtMain;
        }

        private DataTable ValidHopper(string HopperId)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQ_ID", typeof(string));
                IndataTable.Columns.Add("EQPTID", typeof(string));
                IndataTable.Columns.Add("MTRLID", typeof(string));
                IndataTable.Columns.Add("HOPPER_ID", typeof(string));
                IndataTable.Columns.Add("PLLT_ID", typeof(string));
                IndataTable.Columns.Add("SRCTYPE", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["REQ_ID"] = txtRequestNo.Text.ToString().Trim(); 
                Indata["EQPTID"] = _EQPTID;
                Indata["MTRLID"] = sMTRLID;
                Indata["HOPPER_ID"] = HopperId;
                Indata["PLLT_ID"] = txtMtrlID.Text.ToString();
                Indata["SRCTYPE"] = SRCTYPE.SRCTYPE_UI;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RMTRL_INPUT_FOR_HOPPER", "INDATA", "OUTDATA", IndataTable);
                if (dtMain == null || dtMain.Rows.Count < 1)
                {
                    return null;
                }
                return dtMain;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private DataTable ValidSublot(string sublotid)
        {
            DataTable IndataTable = new DataTable();
            IndataTable.Columns.Add("REQ_ID", typeof(string));
            IndataTable.Columns.Add("PLLT_ID", typeof(string));
            IndataTable.Columns.Add("EQPTID", typeof(string));
            IndataTable.Columns.Add("MTRLID", typeof(string));
            IndataTable.Columns.Add("HOPPER_ID", typeof(string));

            DataRow Indata = IndataTable.NewRow();
            Indata["REQ_ID"] = Util.NVC(txtRequestNo.Text.Trim());
            Indata["PLLT_ID"] = sublotid;
            Indata["EQPTID"] = _EQPTID;
            Indata["MTRLID"] = sMTRLID;
            Indata["HOPPER_ID"] = txtHopper.Tag;
            IndataTable.Rows.Add(Indata);

            DataTable dtMain = new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RMTRL_INPUT_REQ_FOR_LABEL_UI", "INDATA", "OUTDATA", IndataTable);
            if (dtMain.Rows.Count == 0)
            {
                return null;
            }
            return dtMain;
        }

        private bool ValueToFind(C1.WPF.DataGrid.C1DataGrid _grid, string sublotid)
        {
            if(dgList.Rows.Count > 0)
            {
                DataTable dt = DataTableConverter.Convert(_grid.ItemsSource);

                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgList.Rows[iRow].DataItem, "PLLT_ID")).ToString() == sublotid) 
                    {
                        //동일한 라벨이 스캔되었습니다.
                        Util.MessageInfo("SFU1505", (result) =>
                        {
                            txtMtrlID.Clear();
                            txtMtrlID.Focus();
                        });
                        return false;
                    }
                }
            }
            return true;
        }

        private bool LotIDCheck(string lotid)
        {
            try
            {
                Util.gridClear(dgRequest);

                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("REQNO", typeof(string));
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["REQNO"] = txtRequestNo.Text;
                Indata["LOTID"] = lotid;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("MM_INPUTVALIDATION", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return false;
            }
        }

        private void ValidateMtrlInputFlag()
        {
            // 자재투입여부가 모두 Y면 팝업 발생
            int i = 0;
            if (dgMaterial.Rows.Count > 0)
            {
                DataTable dt = DataTableConverter.Convert(dgMaterial.ItemsSource);

                for (int iRow = 0; iRow < dt.Rows.Count; iRow++)
                {
                    if (Util.NVC(DataTableConverter.GetValue(dgMaterial.Rows[iRow].DataItem, "INPUT_CMPL_FLAG")).ToString() == "N")
                    {
                        i++;
                    }
                }
                if (i == 0)
                {
                    //자재가 전부 투입처리 되었습니다. \r\n 투입요청서 리스트를 다시 조회합니다.
                    Util.MessageConfirm("SFU1825", (result) =>
                    {
                        if (result == MessageBoxResult.OK)
                        {
                            this.btnSearch.RaiseEvent(new RoutedEventArgs(Button.ClickEvent, btnSearch));
                        }
                    });
                }
            }
        }

        private void ValidateLotID(string lotid)
        {
            try
            {
                DataTable IndataTable = new DataTable();
                IndataTable.Columns.Add("LOTID", typeof(string));

                DataRow Indata = IndataTable.NewRow();
                Indata["LOTID"] = lotid;
                IndataTable.Rows.Add(Indata);

                DataTable dtMain = new ClientProxy().ExecuteServiceSync("MM_PDA_SUBLOTLIST", "INDATA", "RSLTDT", IndataTable);

                if (dtMain.Rows.Count == 0)
                {
                    return ;
                }
                else
                {
                    DataTable dt = ((DataView)dgList.ItemsSource).Table;

                    DataRow dr = dt.NewRow();
                    dr["INPUT_LOTID"] = DataTableConverter.GetValue(dtMain, "INPUT_LOTID").ToString();
                    dr["MTRLNAME"] = DataTableConverter.GetValue(dtMain, "MTRLNAME").ToString();
                    dr["INPUT_QTY"] = DataTableConverter.GetValue(dtMain, "INPUT_QTY").ToString();
                    dt.Rows.Add(dr);
                }
            }
            catch (Exception ex)
            {
                Util.MessageException(ex);
                return;
            }
        }

        private bool ValidMtrlLotid()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(cboMtrlLOTID.Text))
                {
                    //자재LOT선택은 필수입니다.
                    Util.MessageInfo("SFU1725", (result) =>
                    {
                        txtMtrlID.Clear();
                        txtMtrlID.Focus();
                    });
                    return false;
                }

                //이미 스캔한 자재인지 확인
                if (dgList.ItemsSource != null || dgList.Rows.Count > 0)
                {
                    DataTable dtData = new DataTable();
                    dtData.Columns.Add("RMTRL_LABEL_ID", typeof(string));
                    dtData.AcceptChanges();

                    dtData = DataTableConverter.Convert(dgList.ItemsSource);

                    for (int icnt = 0; icnt < dtData.Rows.Count; icnt++)
                    {
                        if (dtData.Rows[icnt]["RMTRL_LABEL_ID"].ToString() == Util.NVC(cboMtrlLOTID.SelectedValue.ToString()) && dtData.Rows[icnt]["HOPPER_ID"].ToString() == Util.NVC(txtHopper.Tag.ToString()))
                        {
                            //{0}은 이미 스캔한 파렛트입니다. \r\n자재LOT을 확인 후 다시 스캔해주세요.
                            LGC.GMES.MES.ControlsLibrary.MessageBox.Show(MessageDic.Instance.GetMessage("SFU1757", new object[] { Util.NVC(txtMtrlID.Text) }), null, "Info", MessageBoxButton.OK, MessageBoxIcon.None, (result) =>
                            {
                                this.txtMtrlID.Clear();
                                this.txtMtrlID.Focus();
                                this.cboMtrlLOTID.SelectedIndex = 0;
                            });
                            return false;
                        }
                    }
                }
                DataTable RQSTDT = new DataTable();
                RQSTDT.TableName = "RQSTDT";
                RQSTDT.Columns.Add("REQ_ID", typeof(string));
                RQSTDT.Columns.Add("EQPTID", typeof(string));
                RQSTDT.Columns.Add("MTRL_LOTID", typeof(string));
                RQSTDT.Columns.Add("RMTRL_LABEL_ID", typeof(string));
                RQSTDT.Columns.Add("HOPPER_ID", typeof(string));

                DataRow dr = RQSTDT.NewRow();
                dr["REQ_ID"] = txtRequestNo.Text.Trim();
                dr["EQPTID"] = _EQPTID;
                dr["MTRL_LOTID"] = Util.NVC(cboMtrlLOTID.Text.Trim());
                dr["RMTRL_LABEL_ID"] = Util.NVC(cboMtrlLOTID.SelectedValue.ToString());
                dr["HOPPER_ID"] = Util.NVC(txtHopper.Tag.ToString());
                RQSTDT.Rows.Add(dr);

                new ClientProxy().ExecuteServiceSync("BR_PRD_CHK_RMTRL_INPUT_REQ_FOR_LABEL_MTRL_LOTID", "INDATA", null, RQSTDT);
                return true;
            }
            catch (Exception ex)
            {
                Util.MessageInfo(ex.Data["CODE"].ToString(), (result) =>
                {
                    txtMtrlID.Clear();
                    txtMtrlID.Focus();
                    SetCombo_Select(Initialize, "MTRL_LOTID", "MTRL_LOTID");//콤보초기화
                });
                return false;
            }
        }

        #endregion < Method >        
    }
}
