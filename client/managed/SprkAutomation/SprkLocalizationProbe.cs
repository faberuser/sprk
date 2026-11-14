using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NClientData;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

public static class SprkLocalizationProbe
{
	private static SystemLanguage? original;

	public static string Run(string language)
	{
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "Login" ||
			!(Environment.GetEnvironmentVariable("SPRK_AUTOMATION_PROFILE") ?? "").StartsWith("localization-"))
			throw new InvalidOperationException("Localization fixture requires isolated Login and localization profile");
		var loader = SingletonAuto<LocalizationLoader>.instance;
		var manager = SceneAutoBehaviour<LoginManagement>.instance;
		var window = manager.GetInstance<GameOptionWindow>();
		if (window != null && window.IsOpen()) window.Close();
		if (!original.HasValue) original = loader.Language;
		var target = language == "restore" ? original.Value : (SystemLanguage)Enum.Parse(typeof(SystemLanguage), language);
		loader.SetEnableLoadFromPatch(true);
		loader.ChangeLanguage(target);
		// The normal language picker reloads the login scene. This fixture keeps
		// the scene alive to test repeated changes, so refresh its native labels.
		foreach (var localizer in Resources.FindObjectsOfTypeAll<NVespa.NNGUIExtension.UILocalizeEx>())
			if (localizer.enabled && localizer.gameObject.scene.IsValid()) localizer.OnReloadLocalization();
		if (loader.Language != target) throw new Exception("Language selection failed: " + language);
		if (language == "restore") return "Restored language";
		var catalog = (Dictionary<string, string[]>)typeof(SprkText).GetField("Translations", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
		int checkedStrings = 0;
		foreach (string key in catalog.Keys)
		{
			string value = SprkText.Get(key);
			if (value != SprkText.Get(key, language)) throw new Exception("Wrong language: " + key);
			string.Format(value, new object[] { "123", "456", "789", "101" });
			checkedStrings++;
		}
		if (SprkText.Get("Uncatalogued diagnostic") != "Uncatalogued diagnostic") throw new Exception("Fallback changed");
		if (SprkText.Format("{0} wants to retry this battle.\nDo you want to retry?", "Player {0}").IndexOf("Player {0}", StringComparison.Ordinal) < 0)
			throw new Exception("Player name was interpreted as a template");
		string server = SprkText.ServerMessage("Inventory expanded by 10 slots. 20 unused ticket slots converted into 30 Rubies.");
		if (server != SprkText.Format("Inventory expanded by {0} slots. {1} unused ticket slots converted into {2} Rubies.", "10", "20", "30"))
			throw new Exception("Server result not localized");
		var login = manager.GetInstance<LoginAccountInfo>();
		SprkLoginSettings.Configure(login);
		if (login.LanguageButtonRoot.GetComponentsInChildren<UILabel>(true).Any(label => label.text != SprkText.Get("Settings")))
			throw new Exception("Login settings caption not localized");
		login.GetComponent<SprkLoginSettings>().OpenSettings();
		window = manager.GetInstance<GameOptionWindow>();
		// Reinitialize native pooled rows, normally rebuilt by the scene reload.
		foreach (var row in window.GetComponentsInChildren<GameOptionBaseComponent>(true))
			if (row.gameOptionData != null) row.Refresh();
		SprkGraphicsOptions.Setup(window);
		var rows = window.GetComponentsInChildren<GameOptionBaseComponent>(true);
		foreach (string title in new[] { "Anti-Aliasing", "Render Scale", "Particle Density", "Global Filter" })
		{
			var row = rows.Single(r => r.name == "SPRK " + title);
			if (!row.GetComponentsInChildren<UILabel>(true).Any(label => label.text == SprkText.Get(title)))
				throw new Exception("Graphics caption not localized: " + title);
		}
		return "PASS " + language + ": " + checkedStrings + " formatted catalog entries, runtime language, fallback, player names, server result, login and graphics captions";
	}
}
