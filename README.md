# I2PluginLocalization

Allows for implementing localization with automatic language detection and switching (based on game system language) in Unity objects by leveraging the [I2 Localization](http://inter-illusion.com/tools/i2-localization/) plugin.

## Usage

Create an XML file for each of your translated languages in a location of your choosing, using some suitable, unique identifier for the file name (such as your project name), suffixed by the language code.

Each `Term` uses a unique, identifying `Key`, which is shared among all languages.

Supported language codes include:
- `en` (English)
- `ja` (Japanese)
- `zh-CN` (simplified Chinese)
- `zh-TW` (traditional Chinese)

`BepInEx\plugins\MyPlugin\localization\MyPlugin.en.xml`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Localization>
	<Term Key="GreetingPhrase" Translation="Hello" />
	<Term Key="ThanksPhrase" Translation="Thanks" />
</Localization>
```

`BepInEx\plugins\MyPlugin\localization\MyPlugin.ja.xml`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Localization>
	<Term Key="GreetingPhrase" Translation="こんにちは" />
	<Term Key="ThanksPhrase" Translation="ありがとう" />
</Localization>
```

Initialize your localization by creating a new instance of `PluginLocalization`, specifying the path to the directory that holds your localization, as well as your identifier.
```cs
var localization = PluginLocalization.Load(@"BepInEx\plugins\MyPlugin\localization", "MyPlugin");
```
```cs
var localization = new PluginLocalization();
localization.AddLocalization(@"BepInEx\plugins\MyPlugin\localization", "MyPlugin");
```

This creates a pseudo unique term namespace for your plugin in the I2 Localization system and loads your translations from the localization XML files.

By adding a `I2.Loc.Localize` component to any game object that has a `Text` or `UILabel` component and setting its `Term` property using your term key, the `text` property of the `Text`/`UILabel` will automatically be set to the appropriate translation string and updated when the game system language changes.

Use `localization.GetTermKey(termKey)` in order to get the fully qualified term key for your plugin namespace.

```cs
var button = new GameObject();
button.AddComponent<UILabel>();
var localize = button.AddComponent<Localize>();
localize.Term = localization.GetTermKey("GreetingPhrase");
```

### Manual update

In case you do not use `I2.Loc.Localize` components or otherwise need to update strings manually, an event `SystemLanguageChanged` along with a method `localization.GetTermTranslation(termKey)` is provided.

```cs
var button = new GameObject();
var label = button.AddComponent<UILabel>();

localization.SystemLanguageChanged += (o, e) => {
	label.text = localization.GetTermTranslation("GreetingPhrase");
};
```
