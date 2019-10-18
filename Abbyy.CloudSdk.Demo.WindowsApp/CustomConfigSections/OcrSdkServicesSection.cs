using System.Configuration;

namespace Abbyy.CloudSdk.Demo.WindowsApp.CustomConfigSections
{
	public class OcrSdkServicesSection : ConfigurationSection
	{
		[ConfigurationProperty("OcrSdkServices", IsDefaultCollection = false)]
		[ConfigurationCollection(typeof(OcrSdkServiceCollection))]
		public OcrSdkServiceCollection Services => (OcrSdkServiceCollection) base["OcrSdkServices"];
	}
}
