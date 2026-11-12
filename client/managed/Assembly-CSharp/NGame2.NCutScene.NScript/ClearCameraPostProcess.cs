using System;
using System.Collections.Generic;
using System.Text;
using LitJson;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NCutScene.NScript;

public class ClearCameraPostProcess : IScript, IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public Dictionary<string, bool> SettingNameRemoveExceptPair = new Dictionary<string, bool>();

	private Dictionary<string, bool> _settingNameRmoveExceptTemporaryDics = new Dictionary<string, bool>();

	private Dictionary<string, Type> _settingNameAndTypeMapper = new Dictionary<string, Type>();

	bool IScript.IsEnd => true;

	public ClearCameraPostProcess()
	{
		Dictionary<Type, Func<UnityEngine.Rendering.PostProcessing.PostProcessEffectSettings>>.Enumerator enumerator = SceneAutoBehaviour<CutSceneManager>.instance.PostProcessManager.SettingCreatorPair.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string name = enumerator.Current.Key.Name;
			SettingNameRemoveExceptPair[name] = false;
			_settingNameAndTypeMapper[name] = enumerator.Current.Key;
		}
	}

	void IScript.Visit(IVisitor visitor)
	{
		visitor.TryVisit(this);
	}

	void IScript.Begin(ILocalTimer localTimer, bool isSkip)
	{
		Dictionary<string, bool>.Enumerator enumerator = SettingNameRemoveExceptPair.GetEnumerator();
		while (enumerator.MoveNext())
		{
			string key = enumerator.Current.Key;
			if (!enumerator.Current.Value)
			{
				Type setting = _settingNameAndTypeMapper[key];
				RemoveSetting(setting);
			}
		}
	}

	private bool RemoveSetting(Type setting)
	{
		return SceneAutoBehaviour<CutSceneManager>.instance.PostProcessManager.RemoveSetting(setting);
	}

	void IScript.Update()
	{
	}

	void IScript.End()
	{
	}

	void IScript.FinalizeScript(FinalizeType finalizeType)
	{
	}

	void IScript.Reset()
	{
	}

	bool ILoadJson.Load(JsonData json)
	{
		string sTRING = json["ExceptSettingNames"].GETSTRING(string.Empty);
		HashSet<string> hashSet = ToSet(sTRING);
		if (hashSet != null)
		{
			foreach (string item in hashSet)
			{
				if (SettingNameRemoveExceptPair.ContainsKey(item))
				{
					SettingNameRemoveExceptPair[item] = true;
				}
			}
		}
		return true;
	}

	private HashSet<string> ToSet(string exceptString)
	{
		if (String_Extension_Method.IsNullOrEmpty(exceptString))
		{
			return null;
		}
		return new HashSet<string>(exceptString.Split('_'));
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("ExceptSettingNames", ToString(GetExceptedSettings()));
		return jsonData;
	}

	private HashSet<string> GetExceptedSettings()
	{
		Dictionary<string, bool>.Enumerator enumerator = SettingNameRemoveExceptPair.GetEnumerator();
		HashSet<string> hashSet = new HashSet<string>();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value)
			{
				hashSet.Add(enumerator.Current.Key);
			}
		}
		return hashSet;
	}

	private string ToString(HashSet<string> container)
	{
		if (__Extension_Method_ICollection__.COUNT(container) == 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		HashSet<string>.Enumerator enumerator = container.GetEnumerator();
		while (enumerator.MoveNext())
		{
			stringBuilder.Append(enumerator.Current);
			stringBuilder.Append("_");
		}
		return stringBuilder.ToString();
	}
}
