using System;

namespace I2PluginLocalization;

public class LanguageChangedEventArgs : EventArgs {
	public object Language { get; set; }
	public string LanguageCode { get; set; }
}
