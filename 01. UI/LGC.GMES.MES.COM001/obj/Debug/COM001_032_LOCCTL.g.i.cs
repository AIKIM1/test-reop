﻿#pragma checksum "..\..\COM001_032_LOCCTL.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2AF612DF13A269617005E2EF06EB850964DC2AC8"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

using LGC.GMES.MES.ControlsLibrary;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace LGC.GMES.MES.COM001 {
    
    
    /// <summary>
    /// COM001_032_LOCCTL
    /// </summary>
    public partial class COM001_032_LOCCTL : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\COM001_032_LOCCTL.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grid1;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\COM001_032_LOCCTL.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblZoneID;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\COM001_032_LOCCTL.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblZoneNM;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\COM001_032_LOCCTL.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblCartCnt;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/LGC.GMES.MES.COM001;component/com001_032_locctl.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\COM001_032_LOCCTL.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 9 "..\..\COM001_032_LOCCTL.xaml"
            ((LGC.GMES.MES.COM001.COM001_032_LOCCTL)(target)).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.LocCtl_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 9 "..\..\COM001_032_LOCCTL.xaml"
            ((LGC.GMES.MES.COM001.COM001_032_LOCCTL)(target)).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.LocCtl_MouseLeftButtonUp);
            
            #line default
            #line hidden
            
            #line 9 "..\..\COM001_032_LOCCTL.xaml"
            ((LGC.GMES.MES.COM001.COM001_032_LOCCTL)(target)).MouseMove += new System.Windows.Input.MouseEventHandler(this.LocCtl_MouseMove);
            
            #line default
            #line hidden
            return;
            case 2:
            this.grid1 = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.lblZoneID = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.lblZoneNM = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.lblCartCnt = ((System.Windows.Controls.Label)(target));
            
            #line 20 "..\..\COM001_032_LOCCTL.xaml"
            this.lblCartCnt.MouseDown += new System.Windows.Input.MouseButtonEventHandler(this.lblCartCnt_MouseDown);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

