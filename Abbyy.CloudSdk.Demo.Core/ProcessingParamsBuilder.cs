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
using System.Linq;
using Abbyy.CloudSdk.Demo.Core.Models;
using Abbyy.CloudSdk.V2.Client.Models.Enums;
using Abbyy.CloudSdk.V2.Client.Models.RequestParams;

namespace Abbyy.CloudSdk.Demo.Core
{
	public static class ProcessingParamsBuilder
    {
	    public static ImageProcessingParams GetImageProcessingParams(Options options)
	    {
		    return new ImageProcessingParams
		    {
			    ExportFormats = ConvertExportFormats(options.OutputFormat),
				Profile = ConvertProfile(options.Profile),
				Language = options.Language,
		    };
	    }

	    public static DocumentProcessingParams GetDocumentProcessingParams(Guid taskId, Options options)
	    {
		    return new DocumentProcessingParams
			{
				TaskId = taskId,
				ExportFormats = ConvertExportFormats(options.OutputFormat),
			    Profile = ConvertProfile(options.Profile),
			    Language = options.Language,
		    };
		}

	    public static FieldsProcessingParams GetFieldsProcessingParams(Guid taskId)
	    {
		    return new FieldsProcessingParams { TaskId = taskId };
	    }

		public static TextFieldProcessingParams GetTextFieldProcessingParams(Options options)
	    {
			return new TextFieldProcessingParams
			{
				Language = options.Language,
				Region = options.Region?.Format(),
			};
		}

	    public static CheckmarkFieldProcessingParams GetCheckmarkFieldProcessingParams(Options options)
	    {
			return new CheckmarkFieldProcessingParams
			{
				CheckmarkType = CheckmarkType.Square,
				Region = options.Region?.Format(),
			};
		}

	    public static BarcodeFieldProcessingParams GetBarcodeFieldProcessingParams(Options options)
	    {
			return new BarcodeFieldProcessingParams
			{
				Region = options.Region?.Format(),
			};
		}

	    public static BusinessCardProcessingParams GetBusinessCardProcessingParams(Options options)
	    {
		    return new BusinessCardProcessingParams
		    {
				Language = options.Language,
				ExportFormats = ConvertBusinessCardExportFormat(options.OutputFormat),
		    };
	    }

	    public static MrzProcessingParams GetMrzProcessingParams()
	    {
			return new MrzProcessingParams();
	    }
				
		public static ExportFormat[] ConvertExportFormats(string outputFormat)
	    {
		    try
		    {
			    return outputFormat
				    .Split(',')
				    .Select(f => (ExportFormat)Enum.Parse(typeof(ExportFormat), f, true))
				    .ToArray();
		    }
		    catch
		    {
			    throw new ArgumentException("Invalid output format", nameof(outputFormat));
		    }
		}

		public static BusinessCardExportFormat ConvertBusinessCardExportFormat(string outputFormat)
		{
			try
			{
				return (BusinessCardExportFormat)Enum.Parse(typeof(BusinessCardExportFormat), outputFormat, true);
			}
			catch
			{
				throw new ArgumentException("Invalid output format", nameof(outputFormat));
			}
		}

		private static ProcessingProfile? ConvertProfile(string profile)
	    {
		    if (string.IsNullOrWhiteSpace(profile)) return null;

		    switch (profile.ToLower())
		    {
			    case "documentconversion":
					return ProcessingProfile.DocumentConversion;
			    case "documentarchiving":
				    return ProcessingProfile.DocumentArchiving;
			    case "textextraction":
				    return ProcessingProfile.TextExtraction;
			    default:
				    throw new ArgumentException("Invalid profile", nameof(profile));
		    }
		}
    }
}
