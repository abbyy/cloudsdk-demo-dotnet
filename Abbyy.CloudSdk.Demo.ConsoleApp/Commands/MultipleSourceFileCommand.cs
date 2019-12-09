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
using Abbyy.CloudSdk.Demo.Core.Models;
using McMaster.Extensions.CommandLineUtils;

namespace Abbyy.CloudSdk.Demo.ConsoleApp.Commands
{
	internal abstract class MultipleSourceFileCommand : BaseProcessCommand
	{
		protected MultipleSourceFileCommand(IProcessRunnerService processRunnerService, Mode mode) :
			base(processRunnerService, mode)
		{
		}

		[Required]
		[Option(CommandConsts.Process.MultipleSourceParamParam.Name, CommandConsts.Process.MultipleSourceParamParam.Description, CommandOptionType.SingleValue)]
		[DirectoryExists]
		public string SourcePath { get; set; }

		protected override Options GenerateOptions()
		{
			var options = base.GenerateOptions();
			options.SourcePath = SourcePath;
			return options;
		}
	}
}