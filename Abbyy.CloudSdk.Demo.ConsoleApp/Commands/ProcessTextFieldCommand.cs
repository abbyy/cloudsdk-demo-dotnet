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


using Abbyy.CloudSdk.Demo.Core.Models;
using McMaster.Extensions.CommandLineUtils;

namespace Abbyy.CloudSdk.Demo.ConsoleApp.Commands
{
	[Command(CommandConsts.Process.ProcessTextFieldCommand.Name, Description = CommandConsts.Process.ProcessTextFieldCommand.Description)]
	internal sealed class ProcessTextFieldCommand : MultipleSourceFileCommand
	{

		private string[] ExportFormat => new[] { "xml" };

		[Option(CommandConsts.Process.LanguageParam.Name, CommandConsts.Process.LanguageParam.Description, CommandOptionType.SingleValue)]
		public string Language { get; set; } = "english";

		public ProcessTextFieldCommand(IProcessRunnerService processRunnerService) : base(processRunnerService, Mode.TextField)
		{
		}
		protected override Options GenerateOptions()
		{
			var options = base.GenerateOptions();
			options.Language = Language;
			options.OutputFormat = string.Join(",", ExportFormat);
			return options;
		}
	}
}
