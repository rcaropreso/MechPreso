﻿#pragma checksum "..\..\..\View\Maneuver.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "E2D65A02BDF21DD0B52C91B334AD66B5EAB00832595266973A0DF31DB539E5D2"
//------------------------------------------------------------------------------
// <auto-generated>
//     O código foi gerado por uma ferramenta.
//     Versão de Tempo de Execução:4.0.30319.42000
//
//     As alterações ao arquivo poderão causar comportamento incorreto e serão perdidas se
//     o código for gerado novamente.
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
using WpfApp1.View;


namespace WpfApp1.View {
    
    
    /// <summary>
    /// Maneuver
    /// </summary>
    public partial class Maneuver : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCircularize;
        
        #line default
        #line hidden
        
        
        #line 12 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnExecuteManeuver;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblNodeTimeTo;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblRemainingDeltaV;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Label lblStartBurn;
        
        #line default
        #line hidden
        
        
        #line 16 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtNodeTimeTo;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtRemainingDeltaV;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtStartBurn;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton rdReduceOrbit;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\View\Maneuver.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.RadioButton rdEnlargeOrbit;
        
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
            System.Uri resourceLocater = new System.Uri("/WpfApp1;component/view/maneuver.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\View\Maneuver.xaml"
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
            this.btnCircularize = ((System.Windows.Controls.Button)(target));
            return;
            case 2:
            this.btnExecuteManeuver = ((System.Windows.Controls.Button)(target));
            return;
            case 3:
            this.lblNodeTimeTo = ((System.Windows.Controls.Label)(target));
            return;
            case 4:
            this.lblRemainingDeltaV = ((System.Windows.Controls.Label)(target));
            return;
            case 5:
            this.lblStartBurn = ((System.Windows.Controls.Label)(target));
            return;
            case 6:
            this.txtNodeTimeTo = ((System.Windows.Controls.TextBox)(target));
            return;
            case 7:
            this.txtRemainingDeltaV = ((System.Windows.Controls.TextBox)(target));
            return;
            case 8:
            this.txtStartBurn = ((System.Windows.Controls.TextBox)(target));
            return;
            case 9:
            this.rdReduceOrbit = ((System.Windows.Controls.RadioButton)(target));
            return;
            case 10:
            this.rdEnlargeOrbit = ((System.Windows.Controls.RadioButton)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
