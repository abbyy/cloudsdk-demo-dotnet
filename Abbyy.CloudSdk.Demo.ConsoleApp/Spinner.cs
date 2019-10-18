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
using System.Threading;
using System.Threading.Tasks;

namespace Abbyy.CloudSdk.Demo.ConsoleApp
{
	internal sealed class Spinner : IDisposable
	{
		private Task SpinnerTask { get; set; }

		private CancellationTokenSource Cancellation { get; set; }

		public static Spinner Start(string message)
		{
			Console.Write(message);
			Console.Write(' ');

			var cancellation = new CancellationTokenSource();

			var spinner = Task.Run(() =>
			{
				var counter = 0;

				Console.CursorVisible = false;

				while (true)
				{
					counter++;

					switch (counter % 10)
					{
						case 0:
							Console.Write("[o     ]");
							break;
						case 1:
						case 9:
							Console.Write("[ o    ]");
							break;
						case 2:
						case 8:
							Console.Write("[  o   ]");
							break;
						case 3:
						case 7:
							Console.Write("[   o  ]");
							break;
						case 4:
						case 6:
							Console.Write("[    o ]");
							break;
						case 5:
							Console.Write("[     o]");
							break;
					}

					Console.SetCursorPosition(Console.CursorLeft - 8, Console.CursorTop);

					if (cancellation.IsCancellationRequested)
					{
						SetSpinnerDone();
						break;
					}

					Thread.Sleep(50);
				}

				Console.CursorVisible = true;
			});

			return new Spinner
			{
				Cancellation = cancellation,
				SpinnerTask = spinner,
			};
		}

		private void Stop()
		{
			Cancellation.Cancel();
			SpinnerTask.Wait();
		}

		private static void SetSpinnerDone()
		{
			var left = Console.CursorLeft;
			var top = Console.CursorTop;

			Console.SetCursorPosition(left, top);
			Console.Write(new string(' ', Console.WindowWidth));
			Console.SetCursorPosition(left, top);

			Console.WriteLine("[ Done ]");
		}

		public void Dispose()
		{
			Stop();

			SpinnerTask?.Dispose();
			Cancellation?.Dispose();
		}
	}
}
