using System.Collections.Generic;
using LitJson;

namespace NGame2.NUtil.NSoundFx;

public class AnimationFxData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public List<FrameSoundData> FrameSoundList = new List<FrameSoundData>();

	bool ILoadJson.Load(JsonData json)
	{
		FrameSoundList.LoadFromJson(json["FrameSoundList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldList("FrameSoundList", FrameSoundList);
		return jsonData;
	}
}
