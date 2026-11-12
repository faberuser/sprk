using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NBattleFieldMap2;

public class Builder
{
	public string Code = string.Empty;

	public MapBaseData MapBase = new MapBaseData();

	public bool UseCustomFieldCamera;

	public FieldCameraData CustomFieldCamera = new FieldCameraData();

	public ModelData Model;

	public EffectFxGroupData EffectFxGroup = new EffectFxGroupData();

	public List<EffectFxGroupData> PrefixEffectFxGroupList = new List<EffectFxGroupData>();

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

	public OpenData CreateOpenData(BattleFieldMapStock stock)
	{
		OpenData openData = new OpenData();
		openData.Open(this, stock);
		return openData;
	}

	public AssetDataAsync<GameObject, ModelData> CreateModelAsync()
	{
		return Model.CreateAssetAsync();
	}

	private bool LoadFromJson(JsonRead json)
	{
		string sTRING = json["BundleTypeName"].GETSTRING("(empty)");
		string sTRING2 = json["VersionCode"].GETSTRING("(empty)");
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING, "[BattleFieldMap2]"))
		{
			return false;
		}
		if (!Vespa_Common_Class_Extension_Method.EQUALS(sTRING2, "20151207"))
		{
			return false;
		}
		Model = new ModelData();
		MapBase.LOADFROMJSON(json["MapBase"]);
		UseCustomFieldCamera = json["UseCustomFieldCamera"].GETBOOLEAN(UseCustomFieldCamera);
		CustomFieldCamera.LOADFROMJSON(json["CustomFieldCamera"]);
		Model.LOADFROMJSON(json["Model"]);
		EffectFxGroup.LOADFROMJSON(json["EffectFxGroup"]);
		PrefixEffectFxGroupList.LoadListFromJson(json["PrefixEffectFxGroupList"]);
		if (json.Contains("EffectFxList"))
		{
			EffectFxGroup.EffectFxList.LoadListFromJson(json["EffectFxList"]);
		}
		return true;
	}
}
