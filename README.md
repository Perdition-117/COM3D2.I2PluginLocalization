# COM3D2.I2PluginLocalization

Allows for implementing localization with automatic language detection and switching (based on game system language) in Unity objects by leveraging the [I2 Localization](http://inter-illusion.com/tools/i2-localization/) plugin.

## Usage

Create an XML file containing your translations in `BepInEx\config\localization`, using some suitable identifier for the file name. (such as your assembly name)

Translations are grouped by language. Each `Term` uses a unique, identifying `Key`, which is shared among all languages.

`BepInEx\config\localization\MyPlugin.xml`
```xml
<?xml version="1.0" encoding="utf-8"?>
<Localization>
	<Language Code="en">
		<Term Key="GreetingPhrase" Translation="Hello" />
		<Term Key="ThanksPhrase" Translation="Thanks" />
	</Language>
	<Language Code="ja">
		<Term Key="GreetingPhrase" Translation="こんにちは" />
		<Term Key="ThanksPhrase" Translation="ありがとう" />
	</Language>
	<Language Code="zh-CN">
		<Term Key="GreetingPhrase" Translation="你好" />
		<Term Key="ThanksPhrase" Translation="谢谢" />
	</Language>
	<Language Code="zh-TW">
		<Term Key="GreetingPhrase" Translation="你好" />
		<Term Key="ThanksPhrase" Translation="謝謝" />
	</Language>
</Localization>
```

Initialize your localization by creating a new instance of `PluginLocalization` using your identifier.
```cs
var localization = new PluginLocalization("MyPlugin");
```

This creates a pseudo unique term namespace for your plugin in the I2 Localization system and loads your translations from the localization XML file.

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
