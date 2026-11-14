using System;
using System.Globalization;
using System.Text.RegularExpressions;
using NVespa;
using NVespa.NGlobal;

/// <summary>SPRK UI text, resolved at presentation time so language changes are respected.</summary>
public static partial class SprkText
{
	private static readonly string[] ServerTemplates =
	{
		"Inventory expanded by {0} slots. {1} unused ticket slots converted into {2} Rubies.",
		"Server gold gains increased by {0}%. Active Honor token bonus: {1}%. Applies to all players until the next Honor rewards calculation ({2})."
	};
	private static readonly Regex[] ServerPatterns =
	{
		new Regex(@"\AInventory expanded by (\d+) slots\. (\d+) unused ticket slots converted into (\d+) Rubies\.\z"),
		new Regex(@"\AServer gold gains increased by (\d+)%\. Active Honor token bonus: (\d+)%\. Applies to all players until the next Honor rewards calculation \(([^\r\n]+)\)\.\z")
	};

	// Recognize only these server-owned messages; never rewrite arbitrary player text.
	public static string ServerMessage(string text)
	{
		if (string.IsNullOrEmpty(text)) return text;
		for (int i = 0; i < ServerPatterns.Length; i++)
		{
			Match match = ServerPatterns[i].Match(text);
			if (match.Success)
				return Format(ServerTemplates[i], match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
		}
		return Get(text);
	}

	public static string Get(string english)
	{
		return Get(english, SingletonAuto<LocalizationManager>.instance.Language.ToString());
	}

	public static string Get(string english, string language)
	{
		if (string.IsNullOrEmpty(english)) return english;
		int index = Array.IndexOf(Languages, language);
		return index >= 0 && Translations.TryGetValue(english, out var values) ? values[index] : english;
	}

	public static string Format(string english, params object[] args)
	{
		return string.Format(CultureInfo.CurrentCulture, Get(english), args);
	}
}
