﻿#pragma checksum "..\..\..\Controls\UcPartialILT_Rack.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "9BB340FA0D48F9A82E58814E5DDCC81041558A40"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace LGC.GMES.MES.PACK001.Controls {
    
    
    /// <summary>
    /// UcPartialILT_Rack
    /// </summary>
    public partial class UcPartialILT_Rack : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 12 "..\..\..\Controls\UcPartialILT_Rack.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grTitle;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\Controls\UcPartialILT_Rack.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lbRackName;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\Controls\UcPartialILT_Rack.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbRackName;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Controls\UcPartialILT_Rack.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid grLotCnt;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\Controls\UcPartialILT_Rack.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lbLotCnt;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\Controls\UcPartialILT_Rack.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock tbLotCnt;
        
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
            System.Uri resourceLocater = new System.Uri("/LGC.GMES.MES.PACK001;component/controls/ucpartialilt_rack.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Controls\UcPartialILT_Rack.xaml"
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
            this.grTitle = ((System.Windows.Controls.Grid)(target));
            
            #line 12 "..\..\..\Controls\UcPartialILT_Rack.xaml"
            this.grTitle.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.grTitle_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 2:
            this.lbRackName = ((System.Windows.Controls.Label)(target));
            return;
            case 3:
            this.tbRackName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 4:
            this.grLotCnt = ((System.Windows.Controls.Grid)(target));
            
            #line 24 "..\..\..\Controls\UcPartialILT_Rack.xaml"
            this.grLotCnt.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.grTitle_MouseLeftButtonDown);
            
            #line default
            #line hidden
            return;
            case 5:
            this.lbLotCnt = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.tbLotCnt = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

