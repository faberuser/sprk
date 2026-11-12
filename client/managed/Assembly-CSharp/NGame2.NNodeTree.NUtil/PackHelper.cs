using System;
using LitJson;

namespace NGame2.NNodeTree.NUtil;

public static class PackHelper
{
	public const string SCRIPT_FOLDER = "NodeTree/";

	public static string GetPathNameFromCode(string codeName)
	{
		return codeName.Replace('.', '/').Replace('\\', '/');
	}

	public static bool FixPathName(ref string pathName)
	{
		string obj = pathName;
		pathName = pathName.Replace('.', '/').Replace('\\', '/');
		return obj != pathName;
	}

	public static PackData CreateFromText(string pathName, string jsonText)
	{
		JsonData jsonData = JsonMapper.ToObject(jsonText);
		if (!jsonData.IsObject())
		{
			return null;
		}
		PackData packData = new PackData();
		if (!packData.LOADFROMJSON(jsonData))
		{
			return null;
		}
		packData.PathName = pathName;
		return packData;
	}

	public static void CreateFromTextAsync(string pathName, string jsonText, Action<PackData> onComplete)
	{
		JsonMapper.ToObjectAsync(jsonText, delegate(JsonData json)
		{
			if (!json.IsObject())
			{
				Vespa_System_Action_Extension_Method.fire(onComplete, null);
			}
			else
			{
				PackData packData = new PackData();
				if (!packData.LOADFROMJSON(json))
				{
					Vespa_System_Action_Extension_Method.fire(onComplete, null);
				}
				else
				{
					packData.PathName = pathName;
					Vespa_System_Action_Extension_Method.fire(onComplete, packData);
				}
			}
		});
	}
}
