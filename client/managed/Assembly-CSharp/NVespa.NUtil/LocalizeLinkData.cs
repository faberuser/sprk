using System;
using LitJson;
using NVespa.NGlobal;

namespace NVespa.NUtil;

[Serializable]
public class LocalizeLinkData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public string Key = string.Empty;

	public bool IsValid()
	{
		return LocalizationManager.IsValidKey(Key);
	}

	public string GetLocalize()
	{
		return LocalizationManager.GetStringDirect(Key);
	}

	public string GetLocalize(object arg1)
	{
		return LocalizationManager.GetStringDirect(Key, arg1);
	}

	public string GetLocalize(object arg1, object arg2)
	{
		return LocalizationManager.GetStringDirect(Key, arg1, arg2);
	}

	public string GetLocalize(object arg1, object arg2, object arg3)
	{
		return LocalizationManager.GetStringDirect(Key, arg1, arg2, arg3);
	}

	public string GetLocalize(object arg1, object arg2, object arg3, object arg4)
	{
		return LocalizationManager.GetStringDirect(Key, arg1, arg2, arg3, arg4);
	}

	public string GetLocalize(params object[] args)
	{
		return LocalizationManager.GetStringDirect(Key, args);
	}

	bool ILoadJson.Load(JsonData json)
	{
		Key = json["Key"].GETSTRING(Key);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldString("Key", Key);
		return jsonData;
	}
}
