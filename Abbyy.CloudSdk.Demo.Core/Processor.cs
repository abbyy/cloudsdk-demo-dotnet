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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abbyy.CloudSdk.Demo.Core.EventArgs;
using Abbyy.CloudSdk.Demo.Core.Extensions;
using Abbyy.CloudSdk.Demo.Core.Models;
using Abbyy.CloudSdk.Sample.Models;
using Abbyy.CloudSdk.V2.Client;
using Abbyy.CloudSdk.V2.Client.Models;
using Abbyy.CloudSdk.V2.Client.Models.RequestParams;
using TaskStatus = Abbyy.CloudSdk.V2.Client.Models.Enums.TaskStatus;

namespace Abbyy.CloudSdk.Demo.Core
{
	public sealed class Processor
	{
		private readonly IOcrClient _ocrClient;
		private readonly IScope _scope;

		public event EventHandler<UploadCompletedEventArgs> UploadFileCompleted;
		public event EventHandler<TaskEventArgs> TaskProcessingCompleted;
		public event EventHandler<TaskEventArgs> DownloadFileCompleted;
		public event EventHandler<ListTaskEventArgs> ListTasksCompleted;

		public Processor(IScope scope, AuthInfo authInfo)
		{
			_ocrClient = new OcrClient(authInfo);
			_scope = scope;
		}

		public Task ProcessPathAsync(Options options, object state = null)
		{
			var files = GetListFilesAtGivenPath(options.SourcePath);

			try
			{
				switch (options.Mode)
				{
					case Mode.Image:
					case Mode.Document when files.Count == 1:
						return ProcessFileAsync(files.Single(), options, state);
					case Mode.Document:
						return ProcessFilesAsync(files, options, state);
					case Mode.Mrz:
						return ProcessMrzAsync(files, options, state);
					case Mode.BusinessCard:
						return ProcessBusinessCardsAsync(files, options, state);
					case Mode.TextField:
					case Mode.BarcodeField:
					case Mode.CheckmarkField:
						return ProcessFieldAsync(files, options, state);
					case Mode.Fields:
						return ProcessFieldsAsync(files, options, state);
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			catch (TaskFailedException e)
			{
				return Task.CompletedTask;
			}
		}

		private static List<string> GetListFilesAtGivenPath(string sourcePath)
		{
			var files = new List<string>();

			if (Directory.Exists(sourcePath))
			{
				files.AddRange(Directory.GetFiles(sourcePath));
				files.Sort();
			}
			else if (File.Exists(sourcePath))
			{
				files.Add(sourcePath);
			}

			return files;
		}

		public async Task<List<TaskInfo>> GetAllTasksAsync()
		{
			var list = default(TaskList);

			try
			{
				list = await _ocrClient.ListTasksAsync(new TasksListingParams()).ConfigureAwait(false);
			}
			catch (Exception e)
			{
				ListTasksCompleted?.Invoke(this, new ListTaskEventArgs(null, e, null));
				throw;
			}

			ListTasksCompleted?.Invoke(this, new ListTaskEventArgs(list.Tasks.ToArray(), null, null));

			return list.Tasks;
		}

		private async Task ProcessFileAsync(string filePath, Options options, object state)
		{
			Console.WriteLine($"Start single file processing: {filePath}");

			TaskInfo task;

			var parameters = ProcessingParamsBuilder.GetImageProcessingParams(options);

			using (_scope.Start("Uploading"))
			using (var fileStream = File.OpenRead(filePath))
			{
				task = await _ocrClient.ProcessImageAsync(parameters, fileStream, options.FileName).ConfigureAwait(false);
			}

			UploadFileCompleted?.Invoke(this, new UploadCompletedEventArgs(task, state));
			
			task = await WaitTaskAsync(task.TaskId, state).ConfigureAwait(false);

			await DownloadResultFilesAsync(task, options, state).ConfigureAwait(false);

			Console.WriteLine("Processing has completed");
		}

		private async Task ProcessFilesAsync(
			IReadOnlyList<string> files,
			Options options,
			object state)
		{
			Console.WriteLine($"Starting the processing of {files.Count} files");

			if (files.Count == 0) return;

			var task = await SubmitImagesAsync(files, state).ConfigureAwait(false);

			if (task is null) throw new InvalidOperationException();

			var parameters = ProcessingParamsBuilder.GetDocumentProcessingParams(task.TaskId, options);

			task = await _ocrClient.ProcessDocumentAsync(parameters).ConfigureAwait(false);
			task = await WaitTaskAsync(task.TaskId, state).ConfigureAwait(false);

			await DownloadResultFilesAsync(task, options, state).ConfigureAwait(false);

			Console.WriteLine("Processing has completed");
		}

		private async Task ProcessBusinessCardsAsync(
			IReadOnlyList<string> files,
			Options options,
			object state)
		{
			Console.WriteLine($"Starting BusinessCards processing of {files.Count} files");

			foreach (var file in files)
			{
				Console.WriteLine($"Starting processing of file: {file}");

				TaskInfo task;

				var parameters = ProcessingParamsBuilder.GetBusinessCardProcessingParams(options);

				using (_scope.Start("Uploading"))
				using (var fileStream = File.OpenRead(file))
				{
					task = await _ocrClient.ProcessBusinessCardAsync(parameters, fileStream, options.FileName).ConfigureAwait(false);
				}

				UploadFileCompleted?.Invoke(this, new UploadCompletedEventArgs(task, state));

				task = await WaitTaskAsync(task.TaskId, state).ConfigureAwait(false);

				await DownloadResultFilesAsync(task, options, state).ConfigureAwait(false);
			}

			Console.WriteLine("Processing has completed");
		}

		private async Task ProcessFieldAsync(
			IReadOnlyList<string> files,
			Options options,
			object state)
		{
			Console.WriteLine($"Starting {options.Mode} processing of {files.Count} files");

			foreach (var file in files)
			{
				Console.WriteLine($"Starting processing of file: {file}");
				TaskInfo task;
				options.SourcePath = file;

				using (_scope.Start("Uploading"))
				using (var fileStream = File.OpenRead(file))
				{
					task = await StartFieldProcessingAsync(fileStream, options).ConfigureAwait(false);
				}

				UploadFileCompleted?.Invoke(this, new UploadCompletedEventArgs(task, state));

				task = await WaitTaskAsync(task.TaskId, state).ConfigureAwait(false);

				await DownloadResultFilesAsync(task, options, state).ConfigureAwait(false);
			}

			Console.WriteLine("Processing has completed");
		}

		private Task<TaskInfo> StartFieldProcessingAsync(
			Stream fileStream,
			Options options)
		{
			switch (options.Mode)
			{
				case Mode.TextField:
					var textParams = ProcessingParamsBuilder.GetTextFieldProcessingParams(options);
					return _ocrClient.ProcessTextFieldAsync(textParams, fileStream, options.FileName);
				case Mode.BarcodeField:
					var barcodeParams = ProcessingParamsBuilder.GetBarcodeFieldProcessingParams();
					return _ocrClient.ProcessBarcodeFieldAsync(barcodeParams, fileStream, options.FileName);
				case Mode.CheckmarkField:
					var checkmarkParams = ProcessingParamsBuilder.GetCheckmarkFieldProcessingParams();
					return _ocrClient.ProcessCheckmarkFieldAsync(checkmarkParams, fileStream, options.FileName);
				default:
					throw new InvalidOperationException("Wrong operation");
			}
		}

		private async Task ProcessFieldsAsync(
			IReadOnlyList<string> files,
			Options options,
			object state)
		{
			Console.WriteLine($"Starting Fields processing of {files.Count} files");

			if (files.Count == 0) return;

			var task = await SubmitImagesAsync(files, state).ConfigureAwait(false);
			var parameters = ProcessingParamsBuilder.GetFieldsProcessingParams(task.TaskId);

			if (task is null) throw new InvalidOperationException();

			using (var fileStream = File.OpenRead(options.XmlSettingsPath))
			{
				task = await _ocrClient.ProcessFieldsAsync(parameters, fileStream, "fieldResult").ConfigureAwait(false);
			}

			task = await WaitTaskAsync(task.TaskId, state).ConfigureAwait(false);

			await DownloadResultFilesAsync(task, options, state).ConfigureAwait(false);

			Console.WriteLine("Processing has completed");
		}

		private async Task ProcessMrzAsync(
			IReadOnlyList<string> files,
			Options options,
			object state)
		{
			Console.WriteLine($"Starting MRZ processing of {files.Count} files");

			foreach (var file in files)
			{
				Console.WriteLine($"Starting processing of file: {file}");
				TaskInfo task;
				options.SourcePath = file;
				var parameters = ProcessingParamsBuilder.GetMrzProcessingParams();

				using (_scope.Start("Uploading"))
				using (var fileStream = File.OpenRead(file))
				{
					task = await _ocrClient.ProcessMrzAsync(parameters, fileStream, options.FileName).ConfigureAwait(false);
				}

				UploadFileCompleted?.Invoke(this, new UploadCompletedEventArgs(task, state));

				task = await WaitTaskAsync(task.TaskId, state).ConfigureAwait(false);

				await DownloadResultFilesAsync(task, options, state).ConfigureAwait(false);
			}

			Console.WriteLine("Processing has completed");
		}

		private async Task<TaskInfo> SubmitImagesAsync(IReadOnlyList<string> files, object state)
		{
			var task = default(TaskInfo);

			for (var i = 0; i < files.Count; i++)
			{
				using (_scope.Start($"Uploading {i + 1}/{files.Count} file: {files[i]}"))
				using (var fileStream = File.OpenRead(files[i]))
				{
					task = await _ocrClient.SubmitImageAsync(
							new ImageSubmittingParams { TaskId = task?.TaskId },
							fileStream,
							Path.GetFileNameWithoutExtension(files[i]))
						.ConfigureAwait(false);
				}
			}

			UploadFileCompleted?.Invoke(this, new UploadCompletedEventArgs(task, state));

			return task;
		}

		private List<ResultUrl> GetResultUrls(
			IReadOnlyList<string> urls,
			IReadOnlyList<string> formats)
		{
			if (urls.Count != formats.Count)
			{
				throw new InvalidOperationException("Urls and ExportFormats count mismatch");
			}

			return urls
				.Select((url, i) => new ResultUrl
				{
					Url = url,
					Format = formats[i],
				})
				.ToList();
		}

		private async Task<TaskInfo> WaitTaskAsync(Guid taskId, object state)
		{
			TaskInfo task = null;

			using (_scope.Start("Processing"))
			{
				do
				{
					task = await _ocrClient
						.GetTaskStatusAsync(taskId)
						.ConfigureAwait(false);

					await Task
						.Delay(TimeSpan.FromMilliseconds(task.RequestStatusDelay))
						.ConfigureAwait(false);
				} while (task.Status == TaskStatus.Queued || task.Status == TaskStatus.InProgress);

				var result = task.Status != TaskStatus.NotEnoughCredits && task.Status != TaskStatus.ProcessingFailed;

				var eventArgs = new TaskEventArgs(
					task,
					result ? null : new Exception($"Task failed. Status={task.Status}, Error={task.Error}"),
					state);

				TaskProcessingCompleted?.Invoke(this, eventArgs); 
			}

			if (!HasTaskSucceeded(task)) throw new TaskFailedException(task.Error);

			return task;
		}

		private bool HasTaskSucceeded(TaskInfo task)
		{
			switch (task.Status)
			{
				case TaskStatus.NotEnoughCredits:
					Console.WriteLine("Not enough credits to process the file. Please add more pages to your application balance");
					return false;
				case TaskStatus.ProcessingFailed:
					Console.WriteLine($"Task ended up with error: '{task.Error}'");
					return false;
				default:
					return true;
			}
		}

		private async Task DownloadResultFilesAsync(
			TaskInfo task,
			Options options,
			object state)
		{
			try
			{
				using (_scope.Start("Downloading Results"))
				using (var httpClient = new HttpClient())
				{
					var urls = GetResultUrls(task.ResultUrls, options.OutputFormat.Split(','));
					foreach (var url in urls)
					{
						if (url.Url is null)
						{
							throw new InvalidOperationException();
						}

						var path = Path.Combine(options.TargetPath, $"{options.FileName ?? "document"}.{url.Format.ToExtension()}");

						using (var fileStream = File.Open(path, FileMode.Create, FileAccess.Write))
						{
							var resultStream = await httpClient
								.GetStreamAsync(url.Url)
								.ConfigureAwait(false);

							resultStream.CopyTo(fileStream);
						}
					}
				}

				DownloadFileCompleted?.Invoke(this, new TaskEventArgs(task, null, state));
			}
			catch (Exception e)
			{
				DownloadFileCompleted?.Invoke(this, new TaskEventArgs(task, e, state));
				throw;
			}
		}
	}
}
