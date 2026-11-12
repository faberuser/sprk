using System.Collections.Generic;
using JsonBin;
using NGameAsset.NNgui.NComponent;

namespace NGameAsset.NNgui;

public class NguiComponentData : ILoadJson
{
	public List<WidgetData> WidgetList = new List<WidgetData>();

	bool ILoadJson.Load(JsonRead json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		WidgetList.LoadListFromJson(json["WidgetList"]);
		return true;
	}
}
