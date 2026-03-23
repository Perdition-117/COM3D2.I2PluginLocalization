using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using HarmonyLib;
using I2.Loc;

namespace I2PluginLocalization;

public class PluginLocalization {
	private const string LanguageSourceName = "I2Languages";

	private static readonly string[] LanguageCodes = {
		"en",
		"ja",
		"zh-CN",
		"zh-TW",
	};

	private static readonly XmlSerializer Serializer = new(typeof(Localization));
	private static readonly List<PluginLocalization> Plugins = new();

	private readonly Dictionary<string, Localization> _languages = new();
	private readonly string _namespace;

	static PluginLocalization() {
		Harmony.CreateAndPatchAll(typeof(PluginLocalization));
	}

	public PluginLocalization(string @namespace) {
		if (string.IsNullOrEmpty(@namespace)) {
			throw new ArgumentException($"{nameof(@namespace)} must be a non empty string.");
		}

		if (Plugins.Exists(e => e._namespace == @namespace)) {
			throw new ArgumentException($"{@namespace} plugin namespace already exists.");
		}

		_namespace = @namespace;
		Plugins.Add(this);
	}

	public event EventHandler<LanguageChangedEventArgs> SystemLanguageChanged;

	protected virtual void OnSystemLanguageChanged(LanguageChangedEventArgs e) {
		SystemLanguageChanged?.Invoke(this, e);
	}

	public static PluginLocalization Load(string localizationRoot, string @namespace) {
		var pluginLocalization = new PluginLocalization(@namespace);
		pluginLocalization.AddLocalization(localizationRoot);
		return pluginLocalization;
	}

	public void AddLocalization(string localizationRoot, string fileName = null) {
		if (!Directory.Exists(localizationRoot)) {
			throw new ArgumentException($"Localization directory {localizationRoot} does not exist.");
		}

		foreach (var languageCode in LanguageCodes) {
			var localizationPath = Path.Combine(localizationRoot, $"{fileName ?? _namespace}.{languageCode}.xml");
			if (File.Exists(localizationPath)) {
				using var reader = XmlReader.Create(localizationPath);
				var localization = (Localization)Serializer.Deserialize(reader);
				if (_languages.TryGetValue(languageCode, out var language)) {
					language.Terms.AddRange(localization.Terms);
				} else {
					AddLanguage(languageCode, localization);
				}
			}
		}

		if (LocalizationManager.Sources.Exists(e => e.name == LanguageSourceName)) {
			LoadTranslations();
		}
	}

	private void AddLanguage(string languageCode, Localization localization) {
		_languages.Add(languageCode, localization);
	}

	public string GetTermKey(string key) => $"Plugin/{_namespace}/{key}";

	public string GetTermTranslation(string key) => LocalizationManager.GetTranslation(GetTermKey(key));

	private void LoadTranslations() {
		var languageSource = LocalizationManager.Sources.Find(e => e.name == LanguageSourceName);
		foreach (var language in _languages) {
			var languageIndex = languageSource.GetLanguageIndexFromCode(language.Key);
			foreach (var term in language.Value.Terms) {
				var termData = languageSource.AddTerm(GetTermKey(term.Key));
				termData.SetTranslation(languageIndex, term.Translation);
			}
		}
	}

	[HarmonyPostfix]
	[HarmonyPatch(typeof(Product), nameof(Product.systemLanguage), MethodType.Setter)]
	private static void Product_OnSetSystemLanguage() {
		foreach (var plugin in Plugins) {
			plugin.LoadTranslations();
			plugin.OnSystemLanguageChanged(new() {
				Language = LocalizationManager.CurrentLanguage,
				LanguageCode = LocalizationManager.CurrentLanguageCode,
			});
		}
	}
}
