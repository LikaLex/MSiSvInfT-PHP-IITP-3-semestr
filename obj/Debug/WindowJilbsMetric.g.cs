﻿#pragma checksum "..\..\WindowJilbsMetric.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "A621F51927C9A3FBC4E44D441CD39948"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using MSiSvInfT;
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


namespace MSiSvInfT {
    
    
    /// <summary>
    /// WindowJilbsMetric
    /// </summary>
    public partial class WindowJilbsMetric : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 8 "..\..\WindowJilbsMetric.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MSiSvInfT.UCLabelWithTextBox AbsoluteDifficulty;
        
        #line default
        #line hidden
        
        
        #line 9 "..\..\WindowJilbsMetric.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MSiSvInfT.UCLabelWithTextBox RelativeDifficulty;
        
        #line default
        #line hidden
        
        
        #line 10 "..\..\WindowJilbsMetric.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MSiSvInfT.UCLabelWithTextBox MaxNestingLevel;
        
        #line default
        #line hidden
        
        
        #line 11 "..\..\WindowJilbsMetric.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Expander GridExpander;
        
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
            System.Uri resourceLocater = new System.Uri("/MSiSvInfT;component/windowjilbsmetric.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\WindowJilbsMetric.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
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
            this.AbsoluteDifficulty = ((MSiSvInfT.UCLabelWithTextBox)(target));
            return;
            case 2:
            this.RelativeDifficulty = ((MSiSvInfT.UCLabelWithTextBox)(target));
            return;
            case 3:
            this.MaxNestingLevel = ((MSiSvInfT.UCLabelWithTextBox)(target));
            return;
            case 4:
            this.GridExpander = ((System.Windows.Controls.Expander)(target));
            
            #line 11 "..\..\WindowJilbsMetric.xaml"
            this.GridExpander.Expanded += new System.Windows.RoutedEventHandler(this.GridExpander_Expanded);
            
            #line default
            #line hidden
            
            #line 11 "..\..\WindowJilbsMetric.xaml"
            this.GridExpander.Collapsed += new System.Windows.RoutedEventHandler(this.GridExpander_Collapsed);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
