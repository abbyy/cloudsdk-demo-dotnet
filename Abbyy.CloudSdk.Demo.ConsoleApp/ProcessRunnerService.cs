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
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Abbyy.CloudSdk.Demo.Core;
using Abbyy.CloudSdk.Demo.Core.Extensions;
using Abbyy.CloudSdk.Demo.Core.Models;
using Abbyy.CloudSdk.V2.Client;
using Abbyy.CloudSdk.V2.Client.Models;
using McMaster.Extensions.CommandLineUtils;

namespace Abbyy.CloudSdk.Demo.ConsoleApp
{
	internal class ProcessRunnerService : IProcessRunnerService
	{
		private readonly IConsole _console;

		public ProcessRunnerService(IConsole console)
		{
			_console = console;
		}

		public async Task StartProcessAsync(Options options)
		{
			try
			{
				if (!Directory.Exists(options.TargetPath))
				{
					Directory.CreateDirectory(options.TargetPath);
				}

				var host = ConfigurationManager.AppSettings["Host"];
				var applicationId = ConfigurationManager.AppSettings["ApplicationId"];
				var password = ConfigurationManager.AppSettings["Password"];

				_console.WriteLine($"OCR SDK host: '{host}', ApplicationId: '{applicationId}'");

				var processor = new Processor(
					new ConsoleScope(),
					new AuthInfo
					{
						Host = host,
						ApplicationId = applicationId,
						Password = password,
					});

				await processor.ProcessPathAsync(options);
			}
			catch (ApiException e)
			{
				Console.WriteLine(e.ApiErrorToString());
			}
			catch (Exception e)
			{
				_console.WriteLine("Unexpected error occured:");
				_console.WriteLine(e.CombineMessagesToString());
			}
		}
	}
}
