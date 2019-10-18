using System.Configuration;

namespace Abbyy.CloudSdk.Demo.WindowsApp.CustomConfigSections
{
	[ConfigurationCollection(typeof(OcrSdkServiceElement), AddItemName = "OcrSdkService")]
	public class OcrSdkServiceCollection : ConfigurationElementCollection
	{
		public OcrSdkServiceElement this[int idx] => (OcrSdkServiceElement) BaseGet(idx);

		protected override ConfigurationElement CreateNewElement()
		{
			return new OcrSdkServiceElement();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((OcrSdkServiceElement) element).Address;
		}
	}
}
