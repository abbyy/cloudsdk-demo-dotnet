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
using System.Text;
using Abbyy.CloudSdk.V2.Client;

namespace Abbyy.CloudSdk.Demo.Core.Extensions
{
	public static class ExceptionExtensions
	{
		public static string ApiErrorToString(this ApiException exception)
		{
			if (exception == null)
			{
				return null;
			}

			var sb = new StringBuilder();
			sb.AppendLine($"OCR SDK Api error occurred: {exception.Message}");
			sb.AppendLine($"\tHttpCode: {exception.StatusCode}");
			if (!string.IsNullOrWhiteSpace(exception.Error?.ErrorData?.Code))
			{
				sb.AppendLine($"\tApiCode: {exception.Error?.ErrorData?.Code}");
			}

			if (!string.IsNullOrWhiteSpace(exception.Error?.ErrorData?.Target))
			{
				sb.AppendLine($"\tTarget: {exception.Error?.ErrorData?.Target}");
			}

			if (!string.IsNullOrWhiteSpace(exception.Error?.ErrorData?.Message))
			{
				sb.AppendLine($"\tMessage: {exception.Error?.ErrorData?.Message}");
			}

			return sb.ToString();
		}

		public static IEnumerable<string> GetMessages(this Exception ex)
		{
			if (ex == null) { yield break; }
			yield return ex.Message;
			foreach (var innerMessage in ex.InnerException.GetMessages())
			{
				yield return innerMessage;
			}
		}

		public static string CombineMessagesToString(this Exception exception)
		{
			if (exception == null)
			{
				return null;
			}
			return string.Join(" ", exception.GetMessages());
		}

	}
}
