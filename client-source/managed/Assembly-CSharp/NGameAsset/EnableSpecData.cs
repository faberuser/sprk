using JsonBin;
using LitJson;
using NVespa;

namespace NGameAsset;

public class EnableSpecData : JsonBin.ILoadJson, IBuilder, LitJson.ILoadJson, IJsonInterface, IMakeJson
{
	public bool EnableHigh = true;

	public bool EnableMid = true;

	public bool EnableLow = true;

	public bool CheckEnable()
	{
		if (SystemOptionData.Spec == ESpecType.Undefine || SystemOptionData.Spec == ESpecType.Original)
		{
			return EnableHigh;
		}
		if (SystemOptionData.Spec == ESpecType.Mid)
		{
			return EnableMid;
		}
		if (SystemOptionData.Spec == ESpecType.Low)
		{
			return EnableLow;
		}
		return false;
	}

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		EnableHigh = json["EnableHigh"].GETBOOLEAN(EnableHigh);
		EnableMid = json["EnableMid"].GETBOOLEAN(EnableMid);
		EnableLow = json["EnableLow"].GETBOOLEAN(EnableLow);
		return true;
	}

	bool LitJson.ILoadJson.Load(JsonData json)
	{
		EnableHigh = json["EnableHigh"].GETBOOLEAN(EnableHigh);
		EnableMid = json["EnableMid"].GETBOOLEAN(EnableMid);
		EnableLow = json["EnableLow"].GETBOOLEAN(EnableLow);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("EnableHigh", EnableHigh);
		jsonData.AddFieldBool("EnableMid", EnableMid);
		jsonData.AddFieldBool("EnableLow", EnableLow);
		return jsonData;
	}
}
