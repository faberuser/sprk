using System.Collections.Generic;
using LitJson;
using NGame2.NBattleView.NDamageOn3d;
using NVespa;
using UnityEngine;

namespace NGame2.NGameSetting.NBattleView;

public class DamageFloatData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class DamageFloatInfo : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public EDamageFloatType FloatType;

		public Color ColorTop = Color.white;

		public Color ColorBottom = Color.white;

		public float NumberScale = 1f;

		public float IconScale = 1f;

		public float TextScale = 1f;

		bool ILoadJson.Load(JsonData json)
		{
			FloatType = json["FloatType"].GETENUM(FloatType);
			NumberScale = json["NumberScale"].GETFLOAT(NumberScale);
			IconScale = json["IconScale"].GETFLOAT(IconScale);
			TextScale = json["TextScale"].GETFLOAT(TextScale);
			ColorTop = json["ColorTop"].GETCOLOR(ColorTop);
			ColorBottom = json["ColorBottom"].GETCOLOR(ColorBottom);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldEnum("FloatType", FloatType);
			jsonData.AddFieldDouble("NumberScale", NumberScale);
			jsonData.AddFieldDouble("IconScale", IconScale);
			jsonData.AddFieldDouble("TextScale", TextScale);
			jsonData.AddFieldColor("ColorTop", ColorTop);
			jsonData.AddFieldColor("ColorBottom", ColorBottom);
			return jsonData;
		}
	}

	private List<DamageFloatInfo> _infoList = new List<DamageFloatInfo>();

	private IncreaseDic<EDamageFloatType, DamageFloatInfo> _dic;

	public IncreaseDic<EDamageFloatType, DamageFloatInfo> Dic
	{
		get
		{
			if (_dic == null)
			{
				_dic = new IncreaseDic<EDamageFloatType, DamageFloatInfo>();
				for (int i = 0; i < _infoList.Count; i++)
				{
					DamageFloatInfo damageFloatInfo = _infoList[i];
					if (!_dic.ContainsKey(damageFloatInfo.FloatType))
					{
						_dic.Add(damageFloatInfo.FloatType, damageFloatInfo);
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
