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
using Abbyy.CloudSdk.Demo.ConsoleApp.Commands;
using Abbyy.CloudSdk.Demo.Core.Extensions;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using IConsole = McMaster.Extensions.CommandLineUtils.IConsole;

namespace Abbyy.CloudSdk.Demo.ConsoleApp
{
	internal sealed class Program
	{
		public static int Main(string[] args)
		{
			string errorMessage;
			try
			{
				var services = new ServiceCollection()
					.AddSingleton<IProcessRunnerService, ProcessRunnerService>()
					.AddSingleton<IConsole>(PhysicalConsole.Singleton)
					.BuildServiceProvider();

				var app = new CommandLineApplication<RootCommand>();
				app.HelpOption();
				app.Conventions
					.UseDefaultConventions()
					.UseConstructorInjection(services);
				return app.Execute(args);
			}
			catch (UnrecognizedCommandParsingException ex)
			{
				Console.WriteLine(string.Join(" ", args));
				errorMessage = ex.Message;
			}
			catch (CommandParsingException ex)
			{
				Console.WriteLine(string.Join(" ", args));
				errorMessage = ex.Message;
			}
			catch (Exception ex)
			{
				errorMessage = ex.CombineMessagesToString();
			}

			Console.WriteLine(errorMessage);
			return -1;

		}
	}
}

