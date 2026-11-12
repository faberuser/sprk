using System;
using LitJson;
using UnityEngine;

namespace NGame2.NCutScene.NModifier;

[Serializable]
public class SceneModifier : IBuilder, ILoadJson, IJsonInterface, IMakeJson
{
	public bool PauseMainScene = true;

	public BackgroundController.StockData BackgroundStock = new BackgroundController.StockData();

	public float ZoomScale = 1f;

	public Vector3 Offset = Vector3.zero;

	bool ILoadJson.Load(JsonData json)
	{
		PauseMainScene = json["PauseMainScene"].GETBOOLEAN(PauseMainScene);
		if (json.Contains("BackgroundStock"))
		{
			BackgroundStock.LOADFROMJSON(json["BackgroundStock"]);
		}
		else
		{
			LoadOld(json);
		}
		ZoomScale = json["ZoomScale"].GETFLOAT(ZoomScale);
		Offset = JsonTools.ToVector3(json["Offset"], Offset);
		return true;
	}

	private void LoadOld(JsonData json)
	{
		if (json.Contains("UseBackgroundFilter"))
		{
			bool bOOLEAN = json["UseBackgroundFilter"].GETBOOLEAN(i_default: false);
			BackgroundStock.Type = ((!bOOLEAN) ? BackgroundController.EType.BlackScreen : BackgroundController.EType.Filter);
		}
		else
		{
			BackgroundStock.Type = json["BackgroundType"].GETENUM(BackgroundStock.Type);
		}
		BackgroundStock.Filter.LOADFROMJSON(json["BackgroundFilter"]);
		BackgroundStock.EntryMenuCode = json["EntryMenuCode"].GETSTRING(BackgroundStock.EntryMenuCode);
	}

	JsonData IMakeJson.MakeJson()
	{
		JsonData jsonData = new JsonData();
		jsonData.AddFieldBool("PauseMainScene", PauseMainScene);
		jsonData.AddFieldJsonMake("BackgroundStock", BackgroundStock);
		jsonData.AddFieldDouble("ZoomScale", ZoomScale);
		jsonData.AddField("Offset", JsonTools.FromVector3(Offset));
		return jsonData;
	}
}
