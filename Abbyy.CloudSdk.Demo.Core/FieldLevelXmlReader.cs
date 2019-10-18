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
using System.Text;
using System.Xml.Linq;

namespace Abbyy.CloudSdk.Demo.Core
{
	public static class FieldLevelXmlReader
    {
		public static string ReadText(string filePath)
		{
			var xDoc = XDocument.Load(filePath);

			if (xDoc.Root is null) return null;

			var ns = xDoc.Root.GetDefaultNamespace();
			var field = xDoc.Root.Element(ns + "field");
			var value = field?.Element(ns + "value");

			if (value is null) return null;

			var result = value.Value;

			var xEncoding = value.Attribute("encoding");
			if (xEncoding != null && xEncoding.Value.ToLower() == "base64")
			{
				var bytes = Convert.FromBase64String(result);

				result = Encoding.Unicode.GetString(bytes);
			}

			return result;
		}
	}
}
