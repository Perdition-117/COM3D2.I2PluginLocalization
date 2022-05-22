using System;

namespace COM3D2.I2PluginLocalization;

public class LanguageChangedEventArgs : EventArgs {
	public object Language { get; set; }
	public string LanguageCode { get; set; }
}
