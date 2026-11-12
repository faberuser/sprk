using System.Collections.Generic;
using JsonBin;

namespace NGameAsset.NWorldMap;

public class WorldMapData
{
	public string Code = string.Empty;

	public List<MapImageData> MapImageList = new List<MapImageData>();

	public ScaleData ScaleData;

	public bool Open(string code, JsonRead json)
	{
		Code = code;
		if (!json.IsObject())
		{
			return false;
		}
		if (!LoadFromJson(json))
		{
			return false;
		}
		return true;
	}

	private bool LoadFromJson(JsonRead json)
	{
		string sTRING = json["BundleTypeName"].GETSTRING("(empty)");
		string sTRING2 = json["VersionCode"].GETSTRING("(empty)");
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING, "[WorldMap]"))
		{
			return false;
		}
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING2, "20150717"))
		{
			return false;
		}
		MapImageList.LoadListFromJson(json["MapImageList"]);
		LoadScaleData(json["ScaleData"]);
		return true;
	}

	private void LoadScaleData(JsonRead json)
	{
		if (json.IsObject())
		{
			ScaleData = new ScaleData();
			ScaleData.LOADFROMJSON(json);
		}
		else
		{
			ScaleData = null;
		}
	}
}
