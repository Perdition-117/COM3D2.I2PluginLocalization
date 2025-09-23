using System.Xml.Serialization;

namespace COM3D2.I2PluginLocalization;

public class Localization {
	[XmlElement("Term")]
	public Term[] Terms { get; set; }

	public class Term {
		[XmlAttribute]
		public string Key { get; set; }
		[XmlAttribute]
		public string Translation { get; set; }
	}
}
