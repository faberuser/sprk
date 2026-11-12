using System.Collections.Generic;
using LitJson;
using NVespa;
using UnityEngine;

namespace NGame2.NGameSetting.NBattleView;

public class DamageDotData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class DotInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string Name;

		public Color ColorTop = Color.white;

		public Color ColorBottom = Color.white;

		public string SpriteName = string.Empty;

		bool ILoadJson.Load(JsonData json)
		{
			Name = json["Name"].GETSTRING(Name);
			ColorTop = json["ColorTop"].GETCOLOR(ColorTop);
			ColorBottom = json["ColorBottom"].GETCOLOR(ColorBottom);
			SpriteName = json["SpriteName"].GETSTRING(SpriteName);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Name", Name);
			jsonData.AddFieldColor("ColorTop", ColorTop);
			jsonData.AddFieldColor("ColorBottom", ColorBottom);
			jsonData.AddFieldString("SpriteName", SpriteName);
			return jsonData;
		}
	}

	private List<DotInfo> _infoList = new List<DotInfo>();

	private IncreaseDic<string, DotInfo> _dic;

	public IncreaseDic<string, DotInfo> Dic
	{
		get
		{
			if (_dic == null)
			{
				_dic = new IncreaseDic<string, DotInfo>();
				for (int i = 0; i < _infoList.Count; i++)
				{
					DotInfo dotInfo = _infoList[i];
					if (!String_Extension_Method.IsNullOrEmpty(dotInfo.Name) && !_dic.ContainsKey(dotInfo.Name))
					{
						_dic.Add(dotInfo.Name, dotInfo);
					}
				}
			}
			return _dic;
		}
	}

	bool ILoadJson.Load(JsonData json)
	{
		_infoList.LoadFromJson(json["InfoList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldList("InfoList", _infoList);
		return jsonData;
	}
}
