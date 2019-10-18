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

using System.Threading.Tasks;
using McMaster.Extensions.CommandLineUtils;

namespace Abbyy.CloudSdk.Demo.ConsoleApp.Commands
{
	[Command(CommandConsts.Root.CommandName)]
	[Subcommand(typeof(ProcessImageCommand),
		typeof(ProcessDocumentCommand),
		typeof(ProcessFieldsCommand),
		typeof(ProcessTextFieldCommand),
		typeof(ProcessMrzCommand))]
	internal sealed class RootCommand : ICommand
	{
		public Task<int> OnExecuteAsync(CommandLineApplication app, IConsole console)
		{
			console.WriteLine("You must specify a command.");
			app.ShowHelp();
			return Task.FromResult(1);
		}
	}
}