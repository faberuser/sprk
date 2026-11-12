using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NLobby.NActivity;

public class OpenUrl : IActivity
{
	private string _url;

	public static IActivity Create(string[] values)
	{
		return new OpenUrl((__Extension_Method_ICollection__.COUNT(values) > 0) ? values[0].ToString() : null);
	}

	public OpenUrl(string url)
	{
		_url = url;
	}

	IEnumerator IActivity.coStart()
	{
		string url = GetUrl();
		if (!String_Extension_Method.IsNullOrEmpty(url))
		{
			Application.OpenURL(url);
		}
		yield break;
	}

	void IActivity.OnRequestStop()
	{
	}

	private string GetUrl()
	{
		string result = _url;
		try
		{
			if (Json.Deserialize(_url) is IDictionary<string, object> urlDictionary)
			{
				result = GetUrlByLanguage(urlDictionary);
			}
		}
		catch (Exception)
		{
		}
		return result;
	}

	private string GetUrlByLanguage(IDictionary<string, object> urlDictionary)
	{
		string result = string.Empty;
		string b = SingletonAuto<LocalizationManager>.instance.Language.ToString();
		foreach (KeyValuePair<string, object> item in urlDictionary)
		{
			string key = item.Key;
			string text = ((item.Value != null) ? item.Value.ToString() : string.Empty);
			if (string.Equals(key, b, StringComparison.InvariantCultureIgnoreCase))
			{
				return text;
			}
			if (string.Equals(key, "English", StringComparison.InvariantCultureIgnoreCase))
			{
				result = text;
			}
		}
		return result;
	}
}
