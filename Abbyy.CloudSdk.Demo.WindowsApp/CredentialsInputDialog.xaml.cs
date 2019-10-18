// Copyright © 2019 ABBYY Production LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     https://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections.Generic;
using  System.Linq;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using Abbyy.CloudSdk.Demo.WindowsApp.CustomConfigSections;

namespace Abbyy.CloudSdk.Demo.WindowsApp
{
	public partial class CredentialsInputDialog : Window
	{
		public  ObservableCollection<string> Addresses { get; set; }
		public  ObservableCollection<ListBoxItem> AddressesComboBoxItems { get; set; }
		public string SelectedServiceUrl { get; set; }

		public CredentialsInputDialog()
		{
			InitializeComponent();
			DataContext = this;

			InitializeOcrServiceAddressList();

			SelectedServiceUrl = Properties.Settings.Default.ServerAddress;
			ApplicationId.Text = Properties.Settings.Default.ApplicationId;
			Password.Text = Properties.Settings.Default.Password;
		}

		private void InitializeOcrServiceAddressList()
		{
			Addresses = new ObservableCollection<string>();
			OcrSdkServicesSection section = (OcrSdkServicesSection) ConfigurationManager.GetSection("OcrSdkServicesSection") ?? new OcrSdkServicesSection();
			foreach (OcrSdkServiceElement ocrSdkServiceElement in section.Services)
			{
				Addresses.Add(ocrSdkServiceElement.Address);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(ApplicationId.Text) || string.IsNullOrEmpty(Password.Text))
			{
				return;
			}

			DialogResult = true;
		}

		private void Button_Click_1(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
		}
	}
}
