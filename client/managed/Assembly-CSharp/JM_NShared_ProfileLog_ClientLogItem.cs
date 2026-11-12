using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using NShared;
using NShared.ProfileLog;

public static class JM_NShared_ProfileLog_ClientLogItem
{
	public static ClientLogItem Parse(string s)
	{
		return Parse(s, new ClientLogItem());
	}

	public static ClientLogItem Parse(string s, ClientLogItem o)
	{
		return Parse(Json.Deserialize(s) as IDictionary, o);
	}

	public static ClientLogItem Parse(IDictionary d)
	{
		return Parse(d, new ClientLogItem());
	}

	public static ClientLogItem Parse(IDictionary d, ClientLogItem o)
	{
		o.TableName = BaseJsonMarshaler.ParseValueString(d, "TableName");
		o.AccountId = BaseJsonMarshaler.ParseValueLong(d, "AccountId");
		o.Version = BaseJsonMarshaler.ParseValueString(d, "Version");
		o.DeviceModel = BaseJsonMarshaler.ParseValueString(d, "DeviceModel");
		o.CpuArch = BaseJsonMarshaler.ParseValueString(d, "CpuArch");
		o.MemorySpec = BaseJsonMarshaler.ParseValueString(d, "MemorySpec");
		o.TextureSpec = BaseJsonMarshaler.ParseValueString(d, "TextureSpec");
		o.ShaderLod = BaseJsonMarshaler.ParseValueInt(d, "ShaderLod");
		o.ShadowType = BaseJsonMarshaler.ParseValueString(d, "ShadowType");
		o.SceneName = BaseJsonMarshaler.ParseValueString(d, "SceneName");
		o.TopWindow = BaseJsonMarshaler.ParseValueString(d, "TopWindow");
		o.ChapterIndice = BaseJsonMarshaler.ParseValueString(d, "ChapterIndice");
		o.PeakMemoryMb = BaseJsonMarshaler.ParseValueLong(d, "PeakMemoryMb");
		o.TimeRange = BaseJsonMarshaler.ParseValueFloat(d, "TimeRange");
		o.FrameCount = BaseJsonMarshaler.ParseValueInt(d, "FrameCount");
		o.SceneFps = BaseJsonMarshaler.ParseValueFloat(d, "SceneFps");
		o.BadSceneFrameRatio = BaseJsonMarshaler.ParseValueFloat(d, "BadSceneFrameRatio");
		o.BattleFps = BaseJsonMarshaler.ParseValueFloat(d, "BattleFps");
		o.BadBattleFrameRatio = BaseJsonMarshaler.ParseValueFloat(d, "BadBattleFrameRatio");
		o.SuperSlowBattleFrames = BaseJsonMarshaler.ParseValueInt(d, "SuperSlowBattleFrames");
		o.RttAvgMs = BaseJsonMarshaler.ParseValueFloat(d, "RttAvgMs");
		o.RttSd = BaseJsonMarshaler.ParseValueFloat(d, "RttSd");
		o.BattleLogUpdateTime = BaseJsonMarshaler.ParseValueFloat(d, "BattleLogUpdateTime");
		o.SuperSlowBattleLogs = BaseJsonMarshaler.ParseValueInt(d, "SuperSlowBattleLogs");
		o.HttpRttAvgMs = BaseJsonMarshaler.ParseValueFloat(d, "HttpRttAvgMs");
		o.HttpRttMax = BaseJsonMarshaler.ParseValueFloat(d, "HttpRttMax");
		o.HttpRttMaxName = BaseJsonMarshaler.ParseValueString(d, "HttpRttMaxName");
		return o;
	}

	public static string EncodeJsonString(ClientLogItem o)
	{
		return Json.Serialize(EncodeJsonObject(o));
	}

	public static Dictionary<string, object> EncodeJsonObject(ClientLogItem o)
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		if (o == null)
		{
			return dictionary;
		}
		if (o.TableName != null)
		{
			dictionary.Add("TableName", o.TableName);
		}
		dictionary.Add("AccountId", o.AccountId.ToString());
		if (o.Version != null)
		{
			dictionary.Add("Version", o.Version);
		}
		if (o.DeviceModel != null)
		{
			dictionary.Add("DeviceModel", o.DeviceModel);
		}
		if (o.CpuArch != null)
		{
			dictionary.Add("CpuArch", o.CpuArch);
		}
		if (o.MemorySpec != null)
		{
			dictionary.Add("MemorySpec", o.MemorySpec);
		}
		if (o.TextureSpec != null)
		{
			dictionary.Add("TextureSpec", o.TextureSpec);
		}
		dictionary.Add("ShaderLod", o.ShaderLod.ToString());
		if (o.ShadowType != null)
		{
			dictionary.Add("ShadowType", o.ShadowType);
		}
		if (o.SceneName != null)
		{
			dictionary.Add("SceneName", o.SceneName);
		}
		if (o.TopWindow != null)
		{
			dictionary.Add("TopWindow", o.TopWindow);
		}
		if (o.ChapterIndice != null)
		{
			dictionary.Add("ChapterIndice", o.ChapterIndice);
		}
		dictionary.Add("PeakMemoryMb", o.PeakMemoryMb.ToString());
		dictionary.Add("TimeRange", o.TimeRange.ToString());
		dictionary.Add("FrameCount", o.FrameCount.ToString());
		dictionary.Add("SceneFps", o.SceneFps.ToString());
		dictionary.Add("BadSceneFrameRatio", o.BadSceneFrameRatio.ToString());
		dictionary.Add("BattleFps", o.BattleFps.ToString());
		dictionary.Add("BadBattleFrameRatio", o.BadBattleFrameRatio.ToString());
		dictionary.Add("SuperSlowBattleFrames", o.SuperSlowBattleFrames.ToString());
		dictionary.Add("RttAvgMs", o.RttAvgMs.ToString());
		dictionary.Add("RttSd", o.RttSd.ToString());
		dictionary.Add("BattleLogUpdateTime", o.BattleLogUpdateTime.ToString());
		dictionary.Add("SuperSlowBattleLogs", o.SuperSlowBattleLogs.ToString());
		dictionary.Add("HttpRttAvgMs", o.HttpRttAvgMs.ToString());
		dictionary.Add("HttpRttMax", o.HttpRttMax.ToString());
		if (o.HttpRttMaxName != null)
		{
			dictionary.Add("HttpRttMaxName", o.HttpRttMaxName);
		}
		return dictionary;
	}
}
