﻿#pragma checksum "..\..\..\..\Views\UpdaterPage.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "7672F7DA8C2EF8BAB933BF1165C513480E750633"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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
using UXModule.Views;


namespace UXModule.Views {
    
    
    /// <summary>
    /// UpdaterPage
    /// </summary>
    public partial class UpdaterPage : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 78 "..\..\..\..\Views\UpdaterPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StartServerButton;
        
        #line default
        #line hidden
        
        
        #line 79 "..\..\..\..\Views\UpdaterPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StopServerButton;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\..\Views\UpdaterPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button ConnectButton;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\..\..\Views\UpdaterPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button DisconnectButton;
        
        #line default
        #line hidden
        
        
        #line 91 "..\..\..\..\Views\UpdaterPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView ToolViewList;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\..\Views\UpdaterPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Primitives.ToggleButton ExpandCollapseButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.11.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/UXModule;component/views/updaterpage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\UpdaterPage.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "8.0.11.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.StartServerButton = ((System.Windows.Controls.Button)(target));
            
            #line 78 "..\..\..\..\Views\UpdaterPage.xaml"
            this.StartServerButton.Click += new System.Windows.RoutedEventHandler(this.StartServerButton_Click);
            
            #line default
            #line hidden
            return;
            case 2:
            this.StopServerButton = ((System.Windows.Controls.Button)(target));
            
            #line 79 "..\..\..\..\Views\UpdaterPage.xaml"
            this.StopServerButton.Click += new System.Windows.RoutedEventHandler(this.StopServerButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ConnectButton = ((System.Windows.Controls.Button)(target));
            
            #line 82 "..\..\..\..\Views\UpdaterPage.xaml"
            this.ConnectButton.Click += new System.Windows.RoutedEventHandler(this.ConnectButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.DisconnectButton = ((System.Windows.Controls.Button)(target));
            
            #line 83 "..\..\..\..\Views\UpdaterPage.xaml"
            this.DisconnectButton.Click += new System.Windows.RoutedEventHandler(this.DisconnectButton_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ToolViewList = ((System.Windows.Controls.ListView)(target));
            return;
            case 6:
            this.ExpandCollapseButton = ((System.Windows.Controls.Primitives.ToggleButton)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

