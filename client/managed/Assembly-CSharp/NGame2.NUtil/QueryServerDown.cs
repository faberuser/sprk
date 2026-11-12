using System;
using System.Collections;
using System.Collections.Generic;
using NVespa;
using NVespa.NGlobal;
using UnityEngine;

namespace NGame2.NUtil;

public class QueryServerDown
{
	public bool IsDone { get; private set; }

	public string ResultMessage { get; private set; }

	public void Query(string url, Action onComplete)
	{
		IsDone = false;
		VespaCoroutine.Start(coQuery(url, onComplete));
	}

	private IEnumerator coQuery(string url, Action onComplete)
	{
		if (string.IsNullOrEmpty(url))
		{
			IsDone = true;
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		if (GuiManager.IsShowDebugButton())
		{
			IsDone = true;
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		SystemLanguage myLanguage = (SingletonAuto<LocalizationManager>.isValidInstance ? SingletonAuto<LocalizationManager>.instance.Language : SystemLanguage.English);
		string url2 = Vespa_String_Extension_Method.AppendRandomNumber(url, "?");
		WwwText www = new WwwText(url2);
		DateTime utcNow = DateTime.UtcNow;
		float num = 60f;
		DateTime expiredTime = utcNow.AddSeconds(num);
		while (www != null && !www.isDone && DateTime.UtcNow < expiredTime)
		{
			yield return null;
		}
		string text = string.Empty;
		string text2 = string.Empty;
		try
		{
			if (www != null && www.isDone)
			{
				if (String_Extension_Method.IsNullOrEmpty(www.error))
				{
					text2 = www.text;
				}
				else
				{
					text = www.error;
				}
			}
			else
			{
				text = "Timeout";
			}
		}
		catch (Exception ex)
		{
			text = ex.ToString();
		}
		if (!string.IsNullOrEmpty(text))
		{
			VespaDebug.LogError(text);
			IsDone = true;
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		string text3 = null;
		try
		{
			int num2 = text2.IndexOf('{');
			if (num2 == -1)
			{
				throw new Exception("Wrong server status json. text=" + text2);
			}
			if (!(MiniJson.Deserialize(text2.Substring(num2)) is Dictionary<string, object> dictionary))
			{
				throw new Exception("Failed to parse server status. text=" + text2);
			}
			if (!(bool)dictionary["Open"] && dictionary["Message"] is Dictionary<string, object> dictionary2)
			{
				string text4 = null;
				foreach (KeyValuePair<string, object> item in dictionary2)
				{
					string key = item.Key;
					string text5 = ((item.Value != null) ? item.Value.ToString() : null);
					text4 = text4 ?? text5;
					if (!string.IsNullOrEmpty(text5) && string.Equals(key, myLanguage.ToString(), StringComparison.InvariantCultureIgnoreCase))
					{
						text3 = text5.ToString();
						break;
					}
				}
				if (string.IsNullOrEmpty(text3))
				{
					text3 = text4;
				}
			}
		}
		catch (Exception obj)
		{
			VespaDebug.LogError(obj);
			IsDone = true;
			Vespa_System_Action_Extension_Method.fire(onComplete);
			yield break;
		}
		IsDone = true;
		ResultMessage = text3?.Replace("\\n", "\n");
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	public bool Verify()
	{
		return String_Extension_Method.IsNullOrEmpty(ResultMessage);
	}
}
