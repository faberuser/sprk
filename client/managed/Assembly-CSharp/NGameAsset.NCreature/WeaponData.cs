using System.Collections.Generic;
using JsonBin;
using NGameAsset.NAsyncLoad;
using UnityEngine;

namespace NGameAsset.NCreature;

public class WeaponData : ILoadJson, IWeaponData
{
	public List<ModelData> ModelList = new List<ModelData>();

	public List<AnimationData> AnimationList = new List<AnimationData>();

	public string Name { get; set; }

	public string AttachPivot { get; set; }

	public bool IsShow { get; set; }

	public bool AnimationFollowBodySelection { get; set; }

	public AnimationKeyData DefaultAnimationKey { get; set; }

	public WeaponData()
	{
		Name = string.Empty;
		AttachPivot = string.Empty;
		IsShow = true;
		AnimationFollowBodySelection = false;
	}

	public WeaponInstance.OpenData CreateOpenData(CreatureStock stock)
	{
		stock = SprkMariaLegendStock(stock);
		ModelData modelData = ModelData.SelectDataFromList(stock.GetWeaponStarSafe(), stock.WeaponStock.WeaponPrefix, ModelList);
		if (modelData == null)
		{
			return null;
		}
		return new WeaponInstance.OpenData
		{
			Data = this,
			ModelAsync = modelData.CreateAssetAsync(),
			WeaponEffectDataList = GetWeaponEffectList(stock, modelData),
			AnimationLoadList = CreateAnimationLoadList(stock, stock.LoadLevel)
		};
	}

	public List<AnimationLoadData> CreateAnimationLoadList(CreatureStock stock, CreatureStock.ELoadLevel loadLevel)
	{
		stock = SprkMariaLegendStock(stock);
		string prefix = (AnimationFollowBodySelection ? stock.BodyPrefix : stock.WeaponStock.WeaponPrefix);
		ModelData modelData = ModelData.SelectDataFromList(stock.GetWeaponStarSafe(), prefix, ModelList);
		List<AnimationData> animationDataList = AnimationData.GetAnimationDataList(AnimationList, modelData, loadLevel);
		animationDataList.Sort(delegate(AnimationData lhs, AnimationData rhs)
		{
			if (lhs.Key.MotionType < rhs.Key.MotionType)
			{
				return -1;
			}
			return (lhs.Key.MotionType > rhs.Key.MotionType) ? 1 : lhs.Key.Name.CompareTo(rhs.Key.Name);
		});
		List<AnimationLoadData> list = new List<AnimationLoadData>();
		for (int num = 0; num < animationDataList.Count; num++)
		{
			AnimationData animationData = animationDataList[num];
			if (DefineCreature.NeedLoadAnimation(loadLevel, animationData.Key.MotionType))
			{
				bool preload = stock.NeedPreload(animationData);
				AnimationLoadData item = new AnimationLoadData(animationData, preload);
				list.Add(item);
			}
		}
		return list;
	}

	public List<AssetDataAsync<AnimationClip, AnimationData>> CreateAnimationAsyncList(CreatureStock stock, CreatureStock.ELoadLevel loadLevel)
	{
		stock = SprkMariaLegendStock(stock);
		string prefix = (AnimationFollowBodySelection ? stock.BodyPrefix : stock.WeaponStock.WeaponPrefix);
		ModelData modelData = ModelData.SelectDataFromList(stock.GetWeaponStarSafe(), prefix, ModelList);
		List<AnimationData> animationList = AnimationList;
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(modelData.AnimationList))
		{
			animationList = modelData.AnimationList;
		}
		List<AssetDataAsync<AnimationClip, AnimationData>> list = new List<AssetDataAsync<AnimationClip, AnimationData>>();
		for (int i = 0; i < animationList.Count; i++)
		{
			AnimationData animationData = animationList[i];
			if (DefineCreature.NeedLoadAnimation(loadLevel, animationData.Key.MotionType))
			{
				list.Add(animationData.CreateAssetAsync());
			}
		}
		return list;
	}

	public List<IWeaponEffectData> GetWeaponEffectList(CreatureStock stock, ModelData model)
	{
		int num = __Extension_Method_ICollection__.COUNT(model.WeaponEffectList);
		if (num == 0)
		{
			return null;
		}
		List<IWeaponEffectData> list = null;
		List<WeaponEffectData> weaponEffectList = model.WeaponEffectList;
		for (int i = 0; i < num; i++)
		{
			WeaponEffectData weaponEffectData = weaponEffectList[i];
			if (weaponEffectData.EnableEffect(stock))
			{
				if (list == null)
				{
					list = new List<IWeaponEffectData>();
				}
				list.Add(weaponEffectData);
			}
		}
		return list;
	}

	bool ILoadJson.Load(JsonRead json)
	{
		Name = json["Name"].GETSTRING(Name);
		AttachPivot = json["AttachPivot"].GETSTRING(AttachPivot);
		string key = "IsShow";
		if (json.Contains(key))
		{
			IsShow = json[key].GETBOOLEAN(iDefault: true);
		}
		AnimationFollowBodySelection = json["AnimationFollowBodySelection"].GETBOOLEAN(iDefault: false);
		DefaultAnimationKey = AnimationKeyData.CreateFromJson(json["DefaultAnimationKey"]);
		ModelData.LoadList(json["ModelList"], ModelList);
		AnimationData.LoadList(json["AnimationList"], AnimationList);
		return true;
	}

	public static void LoadList(JsonRead json, List<WeaponData> weaponList)
	{
		weaponList.Clear();
		if (!json.IsArray())
		{
			return;
		}
		for (int i = 0; i < json.Count; i++)
		{
			JsonRead fromArray = json.GetFromArray(i);
			WeaponData weaponData = new WeaponData();
			if (weaponData.LOADFROMJSON(fromArray))
			{
				weaponList.Add(weaponData);
			}
		}
	}

	public CreatureStock SprkMariaLegendStock(CreatureStock stock)
	{
		if (stock.Code == "Hero.Maria" && Name == "Weapon1" && (stock.BodyPrefix == "Cos21Legend" || stock.BodyPrefix == "Cos21Legend1") && (string.IsNullOrEmpty(stock.WeaponStock.WeaponPrefix) || stock.WeaponStock.WeaponPrefix == "Unique01" || stock.WeaponStock.WeaponPrefix == "Trans" || stock.WeaponStock.WeaponPrefix == "Trial" || stock.WeaponStock.WeaponPrefix == "Cos21Legend" || stock.WeaponStock.WeaponPrefix == "Cos21Legend1"))
		{
			stock.WeaponStock.WeaponPrefix = stock.BodyPrefix;
		}
		return stock;
	}
}
