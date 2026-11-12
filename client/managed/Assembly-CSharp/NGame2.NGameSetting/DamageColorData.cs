using System.Collections.Generic;
using LitJson;
using UnityEngine;

namespace NGame2.NGameSetting;

public class DamageColorData : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public class CustomGradient : IBuilder, ILoadJson, IJsonInterface, IMakeJson
	{
		public string Name = string.Empty;

		public Gradient ColorGradient = new Gradient();

		bool ILoadJson.Load(JsonData json)
		{
			Name = json["Name"].GETSTRING(Name);
			ColorGradient = json["ColorGradient"].GetGradient(ColorGradient);
			return true;
		}

		JsonData IMakeJson.MakeJson()
		{
			JsonData jsonData = new JsonData();
			jsonData.AddFieldString("Name", Name);
			jsonData.AddFieldGradient("ColorGradient", ColorGradient);
			return jsonData;
		}
	}

	public float TimeLength = 0.3f;

	public List<CustomGradient> ColorGradientList = new List<CustomGradient>();

	public List<CustomGradient> CustomGradientList = new List<CustomGradient>();

	private Dictionary<string, Gradient> _gradientDic;

	private Dictionary<string, Gradient> _customDic;

	public Gradient GetGradient(string name)
	{
		if (_gradientDic == null)
		{
			_gradientDic = new Dictionary<string, Gradient>();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ColorGradientList); i++)
			{
				CustomGradient customGradient = ColorGradientList[i];
				_gradientDic.Add(customGradient.Name, customGradient.ColorGradient);
			}
		}
		if (String_Extension_Method.IsNullOrEmpty(name))
		{
			return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_gradientDic, GameOptionData.Render_SoloTimeEffectOption.ToString(), _gradientDic["High"]);
		}
		if (_customDic == null)
		{
			_customDic = new Dictionary<string, Gradient>();
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(CustomGradientList); j++)
			{
				CustomGradient customGradient2 = CustomGradientList[j];
				_customDic.Add(customGradient2.Name, customGradient2.ColorGradient);
			}
		}
		return Vespa_Generic_IDictionary_Extension_Method.GETVALUE(_customDic, name, _gradientDic["High"]);
	}

	bool ILoadJson.Load(JsonData json)
	{
		if (!json.IsObject())
		{
			return false;
		}
		TimeLength = json.GetJsonAny("TimeLength", "time").GETFLOAT(TimeLength);
		ColorGradientList.LoadFromJson(json["ColorGradientList"]);
		CustomGradientList.LoadFromJson(json["CustomGradientList"]);
		return true;
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldDouble("TimeLength", TimeLength);
		jsonData.AddFieldList("ColorGradientList", ColorGradientList);
		jsonData.AddFieldList("CustomGradientList", CustomGradientList);
		return jsonData;
	}
}
