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

using System.Collections.Generic;
using System.IO;

namespace Abbyy.CloudSdk.Demo.Core.Models
{
	public sealed class Options
	{
		public Mode Mode { get; set; } = Mode.Image;

	    public string OutputFormat { get; set; }

	    public string Profile { get; set; }

	    public string Language { get; set; } = "english";

	    public List<string> OtherArgs { get; set; }

	    public string SourcePath { get; set; }

	    public string TargetPath { get; set; } = Directory.GetCurrentDirectory();

	    public string XmlSettingsPath { get; set; }

	    public string FileName => Path.GetFileNameWithoutExtension(SourcePath);
	}
}
