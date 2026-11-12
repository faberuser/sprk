using LitJson;
using NVespa.NGameSharedSetting.NNgui;

namespace NVespa.NGameSharedSetting;

public class NguiData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public GridData Grid = new GridData();

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		Grid.LOADFROMJSON(json["Grid"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldJsonMake("Grid", Grid);
		return jsonData;
	}
}
