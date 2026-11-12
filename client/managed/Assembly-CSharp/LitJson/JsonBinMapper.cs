using System;
using NVespa;

namespace LitJson;

public class JsonBinMapper
{
	public static void ToObjectAsync(byte[] bytes, Action<JsonData> onComplete)
	{
		JsonData json = null;
		SingletonUpdater<ThreadManager>.instance.Enqueue((Action)delegate
		{
			json = ToObject(bytes);
		}, (Action)delegate
		{
			Vespa_System_Action_Extension_Method.fire(onComplete, json);
		}, ThreadManager.EEntryType.LitJson);
	}

	public static JsonData ToObject(byte[] bytes)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(bytes))
		{
			return null;
		}
		return JsonBin.Deserialize(bytes);
	}
}
