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
using System.ComponentModel;
using System.IO;
using Abbyy.CloudSdk.V2.Client.Models;

namespace Abbyy.CloudSdk.Demo.WindowsApp.Models
{
	public sealed class UserTask : INotifyPropertyChanged
	{
		public UserTask(string filePath)
		{
			SourceFilePath = filePath;
			TaskId = "<unknown>";
			TaskStatus = "<initializing>";
		}

		public UserTask(TaskInfo task)
		{
			SourceFilePath = null;
			TaskId = task.TaskId.ToString();
			TaskStatus = task.Status.ToString();
			FilesCount = task.FilesCount;
			Description = task.Description;
			RegistrationTime = task.RegistrationTime;
			StatusChangeTime = task.StatusChangeTime;
		}

		public string SourceFilePath { get; set; }

		public string SourceFileName => Path.GetFileName(SourceFilePath);

		public string TargetFormat { get; set; }

		public string TaskId
		{
			get => _taskId;
			set
			{
				_taskId = value;
				NotifyPropertyChanged("TaskId");
			}
		}

		public string TaskStatus
		{
			get => _taskStatus;
			set
			{
				_taskStatus = value;
				NotifyPropertyChanged("TaskStatus");
			}
		}


		public Dictionary<string, string> OutputFilePaths
		{
			get => _outputFilePaths;
			set
			{
				_outputFilePaths = value;
				NotifyPropertyChanged("OutputFilePaths");
			}
		}

		public int FilesCount
		{
			get => _filesCount;
			set { _filesCount = value; NotifyPropertyChanged("FilesCount"); }
		}

		public string Description
		{
			get => _description;
			set { _description = value; NotifyPropertyChanged("Description"); }
		}

		public DateTime RegistrationTime
		{
			get => _registrationTime;
			set
			{
				_registrationTime = value;
				NotifyPropertyChanged("RegistrationTime");
			}
		}

		public DateTime StatusChangeTime
		{
			get => _statusChangeTime;
			set
			{
				_statusChangeTime = value;
				NotifyPropertyChanged("StatusChangeTime");
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged(string info)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
		}

		public bool IsFieldLevel { get; set; }

		public string RecognizedText
		{
			get => _recognizedText;
			set { _recognizedText = value; NotifyPropertyChanged("RecognizedText"); }
		}

		public string ErrorMessage
		{
			get => _errorMessage;
			set { _errorMessage = value; NotifyPropertyChanged("ErrorMessage"); }
		}

		private string _taskId;
		private string _taskStatus;
		private Dictionary<string, string> _outputFilePaths;

		private int _filesCount;
		private string _description;

		private DateTime _registrationTime;
		private DateTime _statusChangeTime;

		private string _recognizedText;
		private string _errorMessage;
	}
}
