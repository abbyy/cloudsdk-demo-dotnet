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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Abbyy.CloudSdk.Demo.Core;
using Abbyy.CloudSdk.Demo.Core.EventArgs;
using Abbyy.CloudSdk.Demo.Core.Extensions;
using Abbyy.CloudSdk.Demo.Core.Models;
using Abbyy.CloudSdk.Demo.WindowsApp.Models;
using Abbyy.CloudSdk.Sample.Models;
using Abbyy.CloudSdk.V2.Client;
using Abbyy.CloudSdk.V2.Client.Models;
using Application = System.Windows.Application;
using Image = System.Drawing.Image;

namespace Abbyy.CloudSdk.Demo.WindowsApp
{
	public partial class MainWindow : Window
	{
		public ObservableCollection<UserTask> UserTasks { get; } = new ObservableCollection<UserTask>();

		public ObservableCollection<CompletedUserTaskElement> CompletedTasksElements { get; } = new ObservableCollection<CompletedUserTaskElement>();

		public ObservableCollection<UserTask> ServerTasks { get; } = new ObservableCollection<UserTask>();

		public ObservableCollection<UserTask> FieldLevelTasks { get; } = new ObservableCollection<UserTask>();

		private Processor _processor;
		private readonly SynchronizationContext _sync;
		private readonly List<Task> _processingTasks;

		public MainWindow()
		{
			InitializeComponent();

			if (string.IsNullOrEmpty(Properties.Settings.Default.ServerAddress) ||
				string.IsNullOrEmpty(Properties.Settings.Default.ApplicationId) ||
			    string.IsNullOrEmpty(Properties.Settings.Default.Password))
			{
				ChangeApplicationCredentials();
			}

			InitializeProcessor(Properties.Settings.Default.ServerAddress, Properties.Settings.Default.ApplicationId, Properties.Settings.Default.Password);

			_sync = SynchronizationContext.Current;
			_processingTasks = new List<Task>();

			fieldLevelImage.RegionSelected += FieldSelected;

			UpdateFormatButtons(this, null);
		}

		private void InitializeProcessor(string serverAddress, string applicationId, string password)
		{
			_processor = new Processor(
				new VoidScope(),
				new AuthInfo
				{
					Host = serverAddress,
					ApplicationId = applicationId,
					Password = password,
				});

			_processor.UploadFileCompleted += UploadCompleted;
			_processor.TaskProcessingCompleted += ProcessingCompleted;
			_processor.DownloadFileCompleted += DownloadCompleted;
			_processor.ListTasksCompleted += TaskListObtained;
		}

		private void ActiveTaskList_Drop(object sender, DragEventArgs e)
		{
			var filenames = GetDropFiles(e.Data);

			foreach (var file in filenames)
			{
				if (File.Exists(file))
				{
					_processingTasks.Add(AddFileTaskAsync(file, null));
				}
			}
		}

		private void FieldLevelImage_Drop(object sender, DragEventArgs e)
		{
			fieldLevelImage.Source = GetDropFiles(e.Data).Last();
		}

		private string[] GetDropFiles(IDataObject data)
		{
			if (!data.GetDataPresent(DataFormats.FileDrop, true))
			{
				throw new InvalidOperationException();
			}

			var filenames = (string[])data.GetData(DataFormats.FileDrop, true);

			if (filenames is null)
			{
				throw new InvalidOperationException();
			}

			return filenames;
		}

		private async void FieldSelected(object sender, RegionSelectedEventArgs e)
		{
			var tempFilePath = Path.GetTempFileName();
			e.CroppedImage.Save(tempFilePath, System.Drawing.Imaging.ImageFormat.Tiff);

			await AddFileTaskAsync(tempFilePath, e.CroppedImage);
		}

		private async Task AddFileTaskAsync(string filePath, Image sourceImage)
		{
			var outputDir = GetOutputDir();
			var options = BuildOptions(filePath, outputDir);
			var exportFormats = options.OutputFormat.Split(',');

			if (modeBcr.IsChecked == true)
			{
				var bcrTasks = exportFormats
					.Select(exportFormat => StartBusinessCardProcessing(exportFormat, options));

				await Task.WhenAll(bcrTasks);
			}
			else if (options.Mode.IsSingleFieldLevel())
			{
				await StartSingleFieldProcessing(sourceImage, options)
					.ConfigureAwait(false);
			}
			else
			{
				await StartGeneralProcessing(exportFormats, options)
					.ConfigureAwait(false);
			}
		}

		private string GetOutputDir()
		{
			var outputDir = Properties.Settings.Default.OutputDirectory;

			if (!Directory.Exists(outputDir))
			{
				Directory.CreateDirectory(outputDir);
			}

			return outputDir;
		}

		private Options BuildOptions(string filePath, string outputDir)
		{
			var mode = Mode.None;
			
			if (FieldLevelTab.IsSelected)
			{
				if (flModeText.IsChecked == true) mode = Mode.TextField;
				else if (flModeBarcode.IsChecked == true) mode = Mode.BarcodeField;
				else if (flModeCheckmark.IsChecked == true) mode = Mode.CheckmarkField;
			}
			else
			{
				if (modeGeneral.IsChecked == true) mode = Mode.Image;
				else if (modeBcr.IsChecked == true) mode = Mode.BusinessCard;
				else if (modeMrz.IsChecked == true) mode = Mode.Mrz;
			}

			if (mode == Mode.None)
			{
				throw new NotSupportedException("Impossible to start task without method specified");
			}

			var options = new Options
			{
				Mode = mode,
				SourcePath = filePath,
				TargetPath = outputDir,
				Language = GetLanguages(),
				OutputFormat = GetOutputFormat(mode),
			};

			return options;
		}

		private Task StartGeneralProcessing(IEnumerable<string> exportFormats, Options options)
		{
			var task = new UserTask(options.SourcePath)
			{
				TaskStatus = "Uploading",
				OutputFilePaths = exportFormats
					.ToDictionary(
						x => x,
						x => Path.Combine(options.TargetPath, $"{options.FileName}.{x.ToExtension()}")),
				TargetFormat = options.OutputFormat,
			};

			UserTasks.Add(task);

			return SafeInvokeProcessorCommands(_processor.ProcessPathAsync(options, task), task);
		}

		private Task StartSingleFieldProcessing(Image sourceImage, Options options)
		{
			var task = new UserTask(options.SourcePath)
			{
				TaskStatus = "Uploading",
				SourceIsTempFile = true,
				IsFieldLevel = true,
				SourceImage = sourceImage,
				OutputFilePaths = new Dictionary<string, string>
				{
					["Xml"] = Path.Combine(options.TargetPath, $"{options.FileName}.xml"),
				},
			};

			UserTasks.Add(task);
			FieldLevelTasks.Add(task);

			return SafeInvokeProcessorCommands(_processor.ProcessPathAsync(options, task), task);
		}

		private Task StartBusinessCardProcessing(string exportFormat, Options options)
		{
			var task = new UserTask(options.SourcePath)
			{
				TaskStatus = "Uploading",
				OutputFilePaths = new Dictionary<string, string>
				{
					[exportFormat] = Path.Combine(options.TargetPath, $"{options.FileName}.{exportFormat.ToExtension()}"),
				},
				TargetFormat = exportFormat,
			};

			UserTasks.Add(task);

			var bcrOptions = new Options
			{
				SourcePath = options.SourcePath,
				TargetPath = options.TargetPath,
				Mode = options.Mode,
				Language = options.Language,
				OutputFormat = exportFormat,
			};

			 return SafeInvokeProcessorCommands(_processor.ProcessPathAsync(bcrOptions, task), task);
		}

		private void MoveTaskToCompleted(UserTask task)
		{
			UserTasks.Remove(task);

			foreach (var outputFilePath in task.OutputFilePaths)
			{
				CompletedTasksElements.Insert(
					0, 
					new CompletedUserTaskElement
					{
						TaskId = task.TaskId,
						Format = outputFilePath.Key,
						Source = task.SourceFileName,
						Target = outputFilePath.Value,
						Error = task.ErrorMessage,
					});
			}
		}

		private void MoveTaskToFailed(UserTask task, string errorMessage)
		{
			CompletedTasksElements.Insert(
				0,
				new CompletedUserTaskElement
				{
					TaskId = task.TaskId,
					Source = task.SourceFileName,
					Error = errorMessage,
				});
			UserTasks.Remove(task);
		}

		private async Task UpdateServerTasksListAsync()
		{
			await SafeInvokeProcessorCommands(_processor.GetAllTasksAsync());
		}
		
		private void UploadCompleted(object sender, UploadCompletedEventArgs e)
		{
			_sync.Post(x =>
			{
				var task = (UserTask)e.UserState;
				task.TaskStatus = "Processing";
				task.TaskId = e.TaskInfo.TaskId.ToString();
			}, null);
		}

		private void ProcessingCompleted(object sender, TaskEventArgs e)
		{
			_sync.Post(x =>
			{
				var task = (UserTask)e.UserState;

				if (task.SourceIsTempFile)
				{
					File.Delete(task.SourceFilePath);
				}

				if (e.Error != null)
				{
					MoveTaskToCompleted(task);
					task = HandleError(task, e.Result.Status.ToString(), e.Error);
				}
			}, null);
		}

		private void DownloadCompleted(object sender, TaskEventArgs e)
		{
			_sync.Post(x =>
			{
				var task = (UserTask)e.UserState;

				if (e.Error != null)
				{
					task = HandleError(task, e.Result.Status.ToString(), e.Error);
				}
				else
				{
					task.TaskStatus = "Ready";
				}

				if (task.IsFieldLevel)
				{
					task.RecognizedText = FieldLevelXmlReader.ReadText(task.OutputFilePaths.Single().Value);
				}

				MoveTaskToCompleted(task);
			}, null);
		}

		private void TaskListObtained(object sender, ListTaskEventArgs e)
		{
			_sync.Post(x =>
			{
				if (e.Error == null)
				{
					var serverTasks = e.Result;

					ServerTasks.Clear();
					foreach (var task in serverTasks.OrderByDescending(t => t.RegistrationTime))
					{
						var userTask = new UserTask(task);

						ServerTasks.Add(userTask);
					}

					return;
				}
			}, null);
		}

		private UserTask HandleError(UserTask task, string status, Exception e)
		{
			task.TaskStatus = status;
			task.ErrorMessage = e.Message;
			task.RecognizedText = $"<{e.Message}>";
			task.OutputFilePaths = null;

			MessageBox.Show(task.ErrorMessage, task.TaskStatus, MessageBoxButton.OK, MessageBoxImage.Exclamation);

			return task;
		}

		private string GetLanguages()
		{
			var result = new List<string>();

			if (langEn.IsChecked == true) result.Add("english");
			if (langFr.IsChecked == true) result.Add("french");
			if (langIt.IsChecked == true) result.Add("italian");
			if (langDe.IsChecked == true) result.Add("german");
			if (langEs.IsChecked == true) result.Add("spanish");
			if (langRu.IsChecked == true) result.Add("russian");
			if (langZh.IsChecked == true) result.Add("chinesePRC");
			if (langJa.IsChecked == true) result.Add("japanese");
			if (langKo.IsChecked == true) result.Add("korean");
			if (result.Count == 0) return "english";

			return string.Join(",", result);
		}

		private string GetOutputFormat(Mode mode)
		{
			var result = new List<string>();

			switch (mode)
			{
				case Mode.Image:
					if (formatPdfSearchable.IsChecked == true) result.Add("PdfSearchable");
					if (formatPdfText.IsChecked == true) result.Add("PdfTextAndImages");
					if (formatTxt.IsChecked == true) result.Add("Txt");
					if (formatDocx.IsChecked == true) result.Add("Docx");
					if (formatPptx.IsChecked == true) result.Add("Pptx");
					if (formatRtf.IsChecked == true) result.Add("Rtf");
					if (formatXlsx.IsChecked == true) result.Add("Xlsx");
					if (formatXml.IsChecked == true) result.Add("Xml");
					if (result.Count == 0) return "Txt";
					break;
				case Mode.TextField:
				case Mode.BarcodeField:
				case Mode.CheckmarkField:
				case Mode.Mrz:
					return "Xml";
				case Mode.Document:
				case Mode.Fields:
					throw new NotSupportedException($"Mode '{Mode.Fields}' is not supported");
				case Mode.BusinessCard:
					if (formatXml.IsChecked == true) result.Add("Xml");
					if (formatVCard.IsChecked == true) result.Add("VCard");
					if (formatCsv.IsChecked == true) result.Add("Csv");
					if (result.Count == 0) return "Xml";
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}

			return string.Join(",", result);
		}

		private void CompletedTaskList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			if (completedTaskList.SelectedItem == null) return;

			var activeTaskItem = (CompletedUserTaskElement)completedTaskList.SelectedItem;

			if (File.Exists(activeTaskItem.Target))
			{
				System.Diagnostics.Process.Start(activeTaskItem.Target);
			}
		}

		private async void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (!(e.Source is TabControl) || !serverTasksTab.IsSelected) return;

			await UpdateServerTasksListAsync();
		}

		private void SettingsMenu_Click(object sender, RoutedEventArgs e)
		{
			ChangeApplicationCredentials();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void ChangeApplicationCredentials()
		{
			var dialog = new CredentialsInputDialog();

			if (IsActive)	
			{
				dialog.Owner = this;
			}
			else
			{
				dialog.ShowInTaskbar = true;
			}

			dialog.ShowDialog();

			if (dialog.DialogResult != true) return;

			Properties.Settings.Default.ServerAddress = dialog.SelectedServiceUrl;
			Properties.Settings.Default.ApplicationId = dialog.ApplicationId.Text;
			Properties.Settings.Default.Password = dialog.Password.Text;
			Properties.Settings.Default.Save();

			InitializeProcessor(Properties.Settings.Default.ServerAddress, Properties.Settings.Default.ApplicationId, Properties.Settings.Default.Password);
		}

		private void UpdateFormatButtons(object sender, RoutedEventArgs routedEventArgs)
		{
			if (modeBcr.IsChecked == true || modeMrz.IsChecked == true)
			{
				formatDocx.Visibility = Visibility.Collapsed;
				formatPdfSearchable.Visibility = Visibility.Collapsed;
				formatPdfText.Visibility = Visibility.Collapsed;
				formatPptx.Visibility = Visibility.Collapsed;
				formatRtf.Visibility = Visibility.Collapsed;
				formatTxt.Visibility = Visibility.Collapsed;
				formatVCard.Visibility = Visibility.Collapsed;
				formatXlsx.Visibility = Visibility.Collapsed;

				if (modeBcr.IsChecked == true)
				{
					formatVCard.Visibility = Visibility.Visible;
					formatCsv.Visibility = Visibility.Visible;
				}
				else
				{
					formatVCard.Visibility = Visibility.Collapsed;
					formatCsv.Visibility = Visibility.Collapsed;
				}

				formatXml.IsChecked = true;
			}
			else
			{
				formatDocx.Visibility = Visibility.Visible;
				formatPdfSearchable.Visibility = Visibility.Visible;
				formatPdfText.Visibility = Visibility.Visible;
				formatPptx.Visibility = Visibility.Visible;
				formatRtf.Visibility = Visibility.Visible;
				formatTxt.Visibility = Visibility.Visible;
				formatVCard.Visibility = Visibility.Visible;
				formatXlsx.Visibility = Visibility.Visible;

				formatVCard.Visibility = Visibility.Collapsed;
				formatCsv.Visibility = Visibility.Collapsed;

				formatTxt.IsChecked = true;
			}
		}
		
		private async Task SafeInvokeProcessorCommands(Task task, UserTask userTask = null)
		{
			string errorMsg = null;
			try
			{
				await task.ConfigureAwait(false);
			}
			catch (ApiException e)
			{
				errorMsg = e.ApiErrorToString();
			}
			catch (Exception e)
			{
				errorMsg = e.CombineMessagesToString();
			}
			if (!string.IsNullOrWhiteSpace(errorMsg))
			{
				if (userTask != null)
				{
					_sync.Post(x => { MoveTaskToFailed(userTask, errorMsg); }, null);
				}
				else
				{
					_sync.Post(x => { MessageBox.Show(errorMsg, "Error", MessageBoxButton.OK, MessageBoxImage.Error); }, null);
				}
			}
		}
	}
}
