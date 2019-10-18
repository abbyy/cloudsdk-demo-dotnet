using System.Configuration;

namespace Abbyy.CloudSdk.Demo.WindowsApp.CustomConfigSections
{
	public class OcrSdkServiceElement : ConfigurationElement
	{
		[ConfigurationProperty("Address", DefaultValue = "", IsKey = true, IsRequired = true)]
		public string Address
		{
			get => (string) base["Address"];
			set => base["Address"] = value;
		}

		[ConfigurationProperty("Description", DefaultValue = "", IsKey = false, IsRequired = false)]
		public string Description
		{
			get => (string) base["Description"];
			set => base["Description"] = value;
		}
	}
}
