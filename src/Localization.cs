using System.Collections.Generic;
using System.Xml.Serialization;

namespace I2PluginLocalization;

public class Localization {
	[XmlElement("Term")]
	public List<Term> Terms { get; set; }

	public class Term {
		[XmlAttribute]
		public string Key { get; set; }
		[XmlAttribute]
		public string Translation { get; set; }
	}
}
