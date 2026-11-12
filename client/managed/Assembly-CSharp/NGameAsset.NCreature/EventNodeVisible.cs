using System.Collections.Generic;
using JsonBin;
using LitJson;

namespace NGameAsset.NCreature;

public class EventNodeVisible : JsonBin.ILoadJson
{
	public class FrameVisible
	{
		public float Frame;

		public bool IsShow;

		public FrameVisible(float frame, bool isShow)
		{
			Frame = frame;
			IsShow = isShow;
		}
	}

	private Dictionary<string, List<FrameVisible>> _dic = new Dictionary<string, List<FrameVisible>>();

	public bool GetNodeVisible(string weaponName, float frame, out bool isNullNode)
	{
		isNullNode = true;
		List<FrameVisible> vALUE = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, weaponName, null);
		if (vALUE == null)
		{
			return true;
		}
		isNullNode = false;
		bool result = true;
		for (int i = 0; i < vALUE.Count; i++)
		{
			FrameVisible frameVisible = vALUE[i];
			if (!(frameVisible.Frame <= frame))
			{
				break;
			}
			result = frameVisible.IsShow;
		}
		return result;
	}

	public static bool CanLoad(JsonData json)
	{
		if (!json.IsArray())
		{
			return false;
		}
		return true;
	}

	bool JsonBin.ILoadJson.Load(JsonRead json)
	{
		if (!json.IsArray())
		{
			return false;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			if (fromArray["Frame"].TryGetFloat(out var value) && fromArray["Name"].TryGetString(out var value2) && fromArray["IsShow"].TryGetBoolean(out var value3))
			{
				List<FrameVisible> list = Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_dic, value2, null);
				if (list == null)
				{
					list = new List<FrameVisible>();
					_dic.Add(value2, list);
				}
				list.Add(new FrameVisible(value, value3));
			}
		}
		return true;
	}
}
