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

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using Abbyy.CloudSdk.Demo.Core.Models;
using McMaster.Extensions.CommandLineUtils;

namespace Abbyy.CloudSdk.Demo.ConsoleApp.Commands
{
	/// <summary>
	/// Common processing options for process command
	/// </summary>
	internal abstract class BaseProcessCommand : ICommand
	{
		protected string[] ExportFormats = {};
		
		protected readonly IProcessRunnerService ProcessRunnerService;
		protected readonly Mode Mode;
		
		protected BaseProcessCommand(IProcessRunnerService processRunnerService, Mode mode)
		{
			ProcessRunnerService = processRunnerService;
			Mode = mode;
		}
		
		[Option(CommandConsts.Process.TargetPathParam.Name, CommandConsts.Process.TargetPathParam.Description, CommandOptionType.SingleValue)]
		[Required]
		public string TargetPath { get; set; } = Directory.GetCurrentDirectory();

		public virtual async Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
		{
			await ProcessAsync();
			return 1;
		}

		protected async Task ProcessAsync()
		{
			var options = GenerateOptions();
			await ProcessRunnerService.StartProcessAsync(options);
		}

		protected virtual Options GenerateOptions()
		{
			var options = new Options
			{
				Mode = Mode,
				TargetPath = TargetPath,
			};

			return options;
		}
	}
}
