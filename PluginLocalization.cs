using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using BepInEx;
using HarmonyLib;
using I2.Loc;

namespace COM3D2.I2PluginLocalization;

public class PluginLocalization {
	private const string LanguageSourceName = "I2Languages";

	private static readonly string _localizationDirectory = Path.Combine(Paths.ConfigPath, "localization");
	private static readonly XmlSerializer serializer = new(typeof(Localization));
	private static readonly List<PluginLocalization> _plugins = new();

	private readonly string _pluginName;
	private readonly Localization _localization;

	static PluginLocalization() {
		Harmony.CreateAndPatchAll(typeof(PluginLocalization));
	}

	public PluginLocalization(string pluginName) {
		if (string.IsNullOrEmpty(pluginName)) {
			throw new ArgumentException($"{nameof(pluginName)} must be a non empty string.");
		}

		if (_plugins.Exists(e => e._pluginName == pluginName)) {
			throw new ArgumentException($"{pluginName} plugin localization already exists.");
		}

		_pluginName = pluginName;

		var localizationPath = Path.Combine(_localizationDirectory, $"{_pluginName}.xml");
		using var reader = XmlReader.Create(localizationPath);
		_localization = (Localization)serializer.Deserialize(reader);

		_plugins.Add(this);

		if (LocalizationManager.Sources.Exists(e => e.name == LanguageSourceName)) {
			LoadTranslations();
		}
	}

	public event EventHandler<LanguageChangedEventArgs> SystemLanguageChanged;

	protected virtual void OnSystemLanguageChanged(LanguageChangedEventArgs e) {
		SystemLanguageChanged?.Invoke(this, e);
	}

	public string GetTermKey(string key) => $"Plugin/{_pluginName}/{key}";

	public string GetTermTranslation(string key) => LocalizationManager.GetTranslation(GetTermKey(key));

	private void LoadTranslations() {
		var i2LangSource = LocalizationManager.Sources.Find(e => e.name == LanguageSourceName);
		foreach (var language in _localization.Languages) {
			var langIndex = i2LangSource.GetLanguageIndexFromCode(language.Code);
			foreach (var term in language.Terms) {
				var termData = i2LangSource.AddTerm(GetTermKey(term.Key));
				termData.SetTranslation(langIndex, term.Translation);
			}
		}
	}

	[HarmonyPatch(typeof(Product), nameof(Product.systemLanguage), MethodType.Setter)]
	[HarmonyPostfix]
	private static void Product_OnSetSystemLanguage() {
		foreach (var plugin in _plugins) {
			plugin.LoadTranslations();
			plugin.OnSystemLanguageChanged(new() {
				Language = LocalizationManager.CurrentLanguage,
				LanguageCode = LocalizationManager.CurrentLanguageCode,
			});
		}
	}
}