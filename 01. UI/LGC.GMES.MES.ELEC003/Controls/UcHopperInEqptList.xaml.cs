/*************************************************************************************
 Created Date : 2020.12.03
      Creator : 조영대
   Decription : 투입요청서 - Hopper 정보 사용자 컨트롤
--------------------------------------------------------------------------------------
 [Change History]
   2020.12.03  조영대 : Initial Created.
**************************************************************************************/

using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using LGC.GMES.MES.CMM001.Class;
using LGC.GMES.MES.Common;
using System.ComponentModel;
using System.Windows.Media.Animation;
using System;

namespace LGC.GMES.MES.ELEC003.Controls
{


    /// <summary>
    /// UcHopperInEqptList.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcHopperInEqptList : UserControl
    {
        #region Declaration
        
        public IFrameOperation FrameOperation
        {
            get;
            set;
        }

        public delegate void HopperDoubleClickEventHandler(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId);
        public event HopperDoubleClickEventHandler HopperDoubleClick;
       

        private static Color fillOn = Color.FromArgb(255, 103, 224, 156);
        private static Color fillOff = Color.FromArgb(255, 255, 153, 153);

        LinearGradientBrush fillGradientOnBrush = new LinearGradientBrush(fillOn, Color.FromArgb(100, fillOn.R, fillOn.G, fillOn.B), 40);
        LinearGradientBrush fillGradientOffBrush = new LinearGradientBrush(fillOff, Color.FromArgb(100, fillOff.R, fillOff.G, fillOff.B), 40);

        SolidColorBrush fillSolidOnBrush = new SolidColorBrush(fillOn);
        SolidColorBrush fillSolidOffBrush = new SolidColorBrush(fillOff);

        [DefaultValue(false)]
        public bool UseGradient { get; set; }

        [DefaultValue(false)]
        public bool UseAlarm { get; set; }

        private bool showEqptInfo = true;        
        [Description("설비정보 보기 여부"), DefaultValue(true)]
        public bool ShowEqptInfo
        {
            get { return showEqptInfo; }
            set
            {
                showEqptInfo = value;
                if (showEqptInfo)
                {
                    gdMain.ColumnDefinitions[0].Width = new GridLength(150);
                    gdMain.ColumnDefinitions[1].Width = new GridLength(120);
                    bdrHopperList.BorderThickness = new Thickness(1);
                    bdrHopperList.Margin = new Thickness(1);
                    gdMain.Margin = new Thickness(3);
                }
                else
                {
                    gdMain.ColumnDefinitions[0].Width = new GridLength(0);
                    gdMain.ColumnDefinitions[1].Width = new GridLength(0);

                    bdrHopperList.BorderThickness = new Thickness(0);
                    bdrHopperList.Margin = new Thickness(0);
                    gdMain.Margin = new Thickness(0);
                }
                
                this.InvalidateVisual();
            }
        }

        private string eqptId = string.Empty;
        [Browsable(false)]
        public string EqptId
        {
            get { return eqptId; }
            set { eqptId = value; }
        }

        private string eqptName = string.Empty;
        [Browsable(false)]
        public string EqptName
        {
            get { return eqptName; }
            set
            {
                txtEqptName.Text = eqptName = value;
            }
        }

        private bool eqptOnOff = false;
        [Browsable(false)]
        public bool EqptOnOff
        {
            get { return eqptOnOff; }
            set
            {
                eqptOnOff = value;

                if (eqptOnOff)
                {
                    if (UseGradient)
                    {
                        bdrEqpt.Background = fillGradientOnBrush;
                    }
                    else
                    {
                        bdrEqpt.Background = fillSolidOnBrush;
                    }
                }
                else
                {
                    if (UseGradient)
                    {
                        bdrEqpt.Background = fillGradientOffBrush;
                    }
                    else
                    {
                        bdrEqpt.Background = fillSolidOffBrush;
                    }
                }
            }
        }

        private string projectName = string.Empty;
        [Browsable(false)]
        public string ProjectName
        {
            get { return projectName; }
            set
            {
                txtProjectName.Text= projectName = value;                
            }
        }

        private string version = string.Empty;
        [Browsable(false)]
        public string Version
        {
            get { return version; }
            set
            {
                txtVersion.Text = version = value;
            }
        }

        private string workOrderId = string.Empty;
        [Browsable(false)]
        public string WorkOrderId
        {
            get { return workOrderId; }
            set
            {
                txtWorkOrderId.Text = workOrderId = value;
            }
        }

        private string productId = string.Empty;
        [Browsable(false)]
        public string ProductId
        {
            get { return productId; }
            set
            {
                txtProductId.Text = productId = value;
            }
        }

        public UcHopperInEqptList()
        {
            InitializeComponent();

            InitializeControls();
        }

        #endregion

        #region Initialize

        public void InitializeControls()
        {
            txtEqptName.Text = string.Empty;
            txtProjectName.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtWorkOrderId.Text = string.Empty;
            txtProductId.Text = string.Empty;

            stkHopperList.Children.Clear();
        }


        #endregion

        #region Override

        #endregion

        #region Event

        #endregion

        #region Mehod
        public void ClearHopperData()
        {
            stkHopperList.Children.Clear();
        }

        public void SetHopperData(DataRow[] drHopper)
        {
            stkHopperList.Children.Clear();

            if (drHopper.Length == 0) return;

            foreach (DataRow dr in drHopper)
            {
                UcHopper newHopper = new UcHopper();
                newHopper.Name = Util.NVC(dr["HOPPER_ID"]);

                newHopper.UseGradient = UseGradient;
                newHopper.UseAlarm = UseAlarm;

                if (Util.NVC(dr["HOPPER_USE_YN"]).Equals("Y"))
                {
                    if (Util.NVC(dr["HOPPER_REQ_YN"]).Equals("Y"))
                    {
                        newHopper.Mode = UcHopper.ViewMode.INPUT_REQUEST;
                    }
                    else
                    {
                        newHopper.Mode = UcHopper.ViewMode.NORMAL_OPERATION;
                    }
                }
                else
                {
                    newHopper.Mode = UcHopper.ViewMode.UNUSED_HOPPER;
                }

                newHopper.EquipmentSegment = Util.NVC(dr["EQSGID"]);
                newHopper.Process = Util.NVC(dr["PROCID"]);
                newHopper.Equipment = Util.NVC(dr["EQPTID"]);

                newHopper.HopperId = Util.NVC(dr["HOPPER_ID"]);
                newHopper.HopperName = Util.NVC(dr["HOPPER_NAME"]);
                newHopper.HopperWeight = Util.NVC(dr["HOPPER_WEIGHT"]);
                newHopper.MaterialId = Util.NVC(dr["HOPPER_MATERIAL"]);
                newHopper.HopperMaterial = Util.NVC(dr["HOPPER_MATERIAL_NAME"]);

                if (Util.NVC(dr["PRP_WEIGHT_YN"]).Equals("Y"))
                {
                    newHopper.IsProperWeight = true;
                }
                else
                {
                    newHopper.IsProperWeight = false;
                }

                newHopper.HopperDoubleClick += NewHopper_HopperDoubleClick;
                stkHopperList.Children.Add(newHopper);
            }
        }

        private void NewHopper_HopperDoubleClick(object sender, string equipmentSegment, string process, string equipment, string materialId, string hopperId)
        {
            HopperDoubleClick?.Invoke(sender, equipmentSegment, process, equipment, materialId, hopperId);
        }


        #endregion


    }
}
