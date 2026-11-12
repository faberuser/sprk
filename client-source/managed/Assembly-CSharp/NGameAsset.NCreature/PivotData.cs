using System.Collections.Generic;
using JsonBin;

namespace NGameAsset.NCreature;

public class PivotData : ILoadJson, IPivotData
{
	public string Name { get; set; }

	public string From { get; set; }

	public string Path { get; set; }

	public bool CanHide { get; set; }

	public PivotData()
	{
		Name = string.Empty;
		From = string.Empty;
		Path = string.Empty;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		From = json["From"].GETSTRING(From);
		Path = json["Path"].GETSTRING(Path);
		CanHide = json["CanHide"].GETBOOLEAN(CanHide);
		return true;
	}

	public static void LoadList(JsonRead json, List<PivotData> pivotList)
	{
		pivotList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			PivotData pivotData = new PivotData();
			if (pivotData.LOADFROMJSON(fromArray))
			{
				pivotList.Add(pivotData);
			}
		}
	}
}
