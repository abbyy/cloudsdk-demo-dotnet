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
	[Command(CommandConsts.Process.ProcessMrzCommand.Name, Description = CommandConsts.Process.ProcessMrzCommand.Description )]
	internal sealed class ProcessMrzCommand : MultipleSourceFileCommand
	{
		private string[] ExportFormat => new[] { "xml" };

		public ProcessMrzCommand(IProcessRunnerService processRunnerService) : base(processRunnerService, Mode.Mrz)
		{
		}

		protected override Options GenerateOptions()
		{
			var options = base.GenerateOptions();
			options.OutputFormat = string.Join(",", ExportFormat);
			return options;
		}
	}
}
