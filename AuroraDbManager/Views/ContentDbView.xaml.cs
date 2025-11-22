// 
// 	ContentDbView.xaml.cs
// 	AuroraDbManager
// 
// 	Created by Swizzy on 23/05/2015
// 	Copyright (c) 2015 Swizzy. All rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using AuroraDbManager.Classes;
using AuroraDbManager.Database;
using Microsoft.Win32;

namespace AuroraDbManager.Views {
    /// <summary>
    ///     Interaction logic for ContentDbView.xaml
    /// </summary>
    public partial class ContentDbView : UserControl {
        public ContentDbView() { InitializeComponent(); }

        public void OpenDb(string filename = null) {
            try {
                if(string.IsNullOrWhiteSpace(filename)) {
                    var ofd = new OpenFileDialog();
                    if(ofd.ShowDialog() != true)
                        return;
                    filename = ofd.FileName;
                }
                SendStatusChanged("Loading {0}...", filename);
                App.DbManager.ConnectToContent(filename);
                Dispatcher.Invoke(new Action(() => ContentDbViewBox.ItemsSource = App.DbManager.GetContentItems()));
                Dispatcher.Invoke(new Action(() => TitleUpdatesDbViewBox.ItemsSource = App.DbManager.GetTitleUpdateItems()));
                SendStatusChanged("Finished loading Content DB...");
            }
            catch(Exception ex) {
                App.SaveException(ex);
                SendStatusChanged("Failed loading Content DB... Check error.log for more information...");
            }
        }

        private void DbViewChanged(object sender, SelectionChangedEventArgs e) {
            //TODO: Open Editor
        }

        private void SendStatusChanged(string msg, params object[] param) {
            var handler = App.StatusChanged;
            if(handler != null)
                handler(this, new StatusEventArgs(string.Format(msg, param)));
        }
    }
}