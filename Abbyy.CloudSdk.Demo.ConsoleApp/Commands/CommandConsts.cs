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

namespace Abbyy.CloudSdk.Demo.ConsoleApp.Commands
{
	internal static class CommandConsts
	{
		internal static class Root
		{
			public const string CommandName = "process";
		}

		internal static class Process
		{
			internal static class LanguageParam
			{
				public const string Name = "-l|--language <language>";
				public const string Description = "Recognize with specified language.\n\tExamples: '-l English',  '--language=English,German,French'";
			}

			internal static class ExportFormatParam
			{
				public const string Name = "-ef|--exportFormat <export_format>";
				public const string Description = "Create output in specified format: txt, rtf, docx, xlsx, pptx, pdfSearchable, pdfTextAndImages, xml.\n\tExamples: '-ef txt',  '--exportFormat rtf --exportFormat pdfSearchable'";
			}

			internal static class ProfileParam
			{
				public const string Name = "-p|--profile <profile>";
				public const string Description = "Use specific profile: documentConversion, documentArchiving or textExtraction";
			}

			internal static class TargetPathParam
			{
				public const string Name = "-t|--target <target_path>";
				public const string Description = "Target directory path for save results";
			}

			internal static class SourceParamParam
			{
				public const string Name = "-s|--source <source_file>";
				public const string Description = "Path to source file for process";
			}

			internal static class MultipleSourceParamParam
			{
				public const string Name = "-s|--source <source_path>";
				public const string Description = "Path to source directory for process";
			}
			internal static class XmlSettingsPathParamParam
			{
				public const string Name = "-xs|--xml-settings";
				public const string Description = "Processing settings";
			}

			internal static class ProcessImageCommand
			{
				public const string Name = "image";
				public const string Description = "Perform full-text recognition of a single document.\n\t'image [options] -s <source_file> -t <target_dir>'";
			}

			internal static class ProcessDocumentCommand
			{
				public const string Name = "document";
				public const string Description = "Recognize directory treating each subdirectory as a single document.\n\t'document [options] -s <source_path> -t <target_dir>'";
			}

			internal static class ProcessMrzCommand
			{
				public const string Name = "mrz";
				public const string Description = "Recognize and parse Machine-Readable Zone (MRZ) of Passport, ID card, Visa or other official document.\n\t'mrz [options] -s <source_path> -t <target_dir>'";
			}

			internal static class ProcessFieldsCommand
			{
				public const string Name = "fields";
				public const string Description = "Perform recognition via processFields call. Processing settings should be specified in xml file.\n\t'fields [options] -s <source_file> -xs <settings.xml> -t <target_dir>'";
			}

			internal static class ProcessTextFieldCommand
			{
				public const string Name = "textField";
				public const string Description = "Perform recognition via processTextField call.\n\t'textField [options] -s <source_path> -t <target_dir>'";
			}
		}
	}
}
