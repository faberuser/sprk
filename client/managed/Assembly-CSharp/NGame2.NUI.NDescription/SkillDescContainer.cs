using System;
using System.Collections;
using LitJson;
using NGame2.NAccount;
using NGameAsset;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUI.NDescription;

public class SkillDescContainer : SingletonAuto<SkillDescContainer>, IBaseDataContainer
{
	public struct DescKey
	{
		public int SkillIndex;

		public int CostumeIndex;

		public DescKey(int skillIndex, int costumeIndex)
		{
			SkillIndex = skillIndex;
			CostumeIndex = costumeIndex;
		}

		public static DescKey CreateFromJson(JsonData json)
		{
			return new DescKey
			{
				SkillIndex = json.GetFromArray(0).GETINT(0),
				CostumeIndex = json.GetFromArray(1).GETINT(0)
			};
		}

		public static JsonData MakeJson(DescKey key)
		{
			JsonData jsonData = new JsonData();
			jsonData.Add(new JsonData(key.SkillIndex));
			jsonData.Add(new JsonData(key.CostumeIndex));
			return jsonData;
		}
	}

	public IncreaseDic<DescKey, SkillDesc> SkillDescDic = new IncreaseDic<DescKey, SkillDesc>();

	public string TableDataPath => "TableData/SkillDescription.json";

	public bool IsLoaded { get; protected set; }

	public string LoadError { get; private set; }

	IEnumerable IBaseDataContainer.GetAllData()
	{
		return SkillDescDic.GetEnumerable();
	}

	object IBaseDataContainer.GetData(object key)
	{
		return null;
	}

	public string GetByHeroIndex(int skillIndex, int heroIndex)
	{
		try
		{
			CachedHeroInfo referenceHero = SceneAutoBehaviour<StatDataCache>.instance.GetReferenceHero(heroIndex);
			return GetByHeroInfo(skillIndex, referenceHero);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public string GetByHeroInfo(int skillIndex, HeroInfo heroInfo)
	{
		if (heroInfo == null)
		{
			return string.Empty;
		}
		try
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(heroInfo.HeroIndex);
			int skillLevelBySkillIndex = heroInfo.GetSkillLevelBySkillIndex(data, skillIndex);
			return GetDesc(stat: (!(heroInfo is CachedHeroInfo)) ? SceneAutoBehaviour<StatDataCache>.instance.GetHeroStat(heroInfo) : SceneAutoBehaviour<StatDataCache>.instance.GetCachedHeroStat(heroInfo as CachedHeroInfo), creatureIndex: heroInfo.HeroIndex, creatureStar: heroInfo.Star, creatureTranscended: heroInfo.Transcended, costumeIndex: heroInfo.CostumeIndex, skillIndex: skillIndex, skillLevel: skillLevelBySkillIndex);
		}
		catch (Exception)
		{
		}
		try
		{
			SkillData data2 = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			return GetSkillName(heroInfo, data2);
		}
		catch (Exception)
		{
			return string.Empty;
		}
	}

	public string GetDesc(int creatureIndex, int creatureStar, int creatureTranscended, int costumeIndex, int skillIndex, int skillLevel, StatData stat)
	{
		SkillDesc skillDesc = null;
		if (costumeIndex != 0)
		{
			skillDesc = SkillDescDic.GETVALUE(new DescKey(skillIndex, costumeIndex), null);
		}
		if (skillDesc == null)
		{
			skillDesc = SkillDescDic.GETVALUE(new DescKey(skillIndex, 0), null);
		}
		if (skillDesc == null)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
			if (data != null)
			{
				return data.Description;
			}
			return $"Empty Desc(skill={skillIndex}, skill level={skillLevel})";
		}
		return skillDesc.GetDescription(stat, creatureIndex, creatureStar, creatureTranscended, skillLevel);
	}

	public static string GetSkillName(SkillData skillData)
	{
		return GetSkillNameInternal(skillData, skillData.NAME_KEY);
	}

	public static string GetSkillName(HeroInfo heroInfo, SkillData skillData)
	{
		if (skillData == null)
		{
			return string.Empty;
		}
		if (heroInfo == null)
		{
			return GetSkillNameInternal(skillData, skillData.NAME_KEY);
		}
		string localKey = skillData.NAME_KEY;
		LCSkillNameData data = GameRoot.Instance.LCSkillNameDataContainer.GetData(heroInfo.CostumeIndex);
		if (data != null)
		{
			int index = skillData.Index;
			if (data.SkillIndex1 == index)
			{
				localKey = data.SkillNameKey1;
			}
			else if (data.SkillIndex2 == index)
			{
				localKey = data.SkillNameKey2;
			}
			else if (data.SkillIndex3 == index)
			{
				localKey = data.SkillNameKey3;
			}
			else if (data.SkillIndex4 == index)
			{
				localKey = data.SkillNameKey4;
			}
			else if (data.SkillIndex5 == index)
			{
				localKey = data.SkillNameKey5;
			}
			else if (data.SkillIndex6 == index)
			{
				localKey = data.SkillNameKey6;
			}
			else if (data.SkillIndex7 == index)
			{
				localKey = data.SkillNameKey7;
			}
			else if (data.SkillIndex8 == index)
			{
				localKey = data.SkillNameKey8;
			}
			else if (data.SkillIndex9 == index)
			{
				localKey = data.SkillNameKey9;
			}
			else if (data.SkillIndex10 == index)
			{
				localKey = data.SkillNameKey10;
			}
			else if (data.SkillIndex11 == index)
			{
				localKey = data.SkillNameKey11;
			}
			else if (data.SkillIndex12 == index)
			{
				localKey = data.SkillNameKey12;
			}
			else if (data.SkillIndex13 == index)
			{
				localKey = data.SkillNameKey13;
			}
			else if (data.SkillIndex14 == index)
			{
				localKey = data.SkillNameKey14;
			}
			else if (data.SkillIndex15 == index)
			{
				localKey = data.SkillNameKey15;
			}
		}
		return GetSkillNameInternal(skillData, localKey);
	}

	public static string GetSkillName(int skillIndex)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		return GetSkillNameInternal(data, data.NAME_KEY);
	}

	private static string GetSkillNameInternal(SkillData skillData, string localKey)
	{
		return GameRoot.LocalizeProxy.GetCache_or_Build(skillData, "SkillDesc_" + localKey, delegate
		{
			ILocalizeProxy localizeProxy = GameRoot.LocalizeProxy;
			string origKey = localKey;
			string replaceNameKey = skillData.ReplaceNameKey;
			object[] nameParam = skillData.NameParam;
			return localizeProxy.GetStringWithReplaceKey(origKey, replaceNameKey, nameParam);
		});
	}

	public IBaseDataContainer Load(LitJsonAssetAsyncLoader jsonLoader)
	{
		LoadReal(jsonLoader);
		return this;
	}

	private void LoadReal(LitJsonAssetAsyncLoader jsonLoader)
	{
		if (jsonLoader == null)
		{
			VespaDebug.LogError("JsonLoader is null : TableDataPath=" + TableDataPath);
			IsLoaded = true;
		}
		else
		{
			jsonLoader.Load(PlainAssetHelper.EPathType.Table, TableDataPath, OnCompleteJsonLoad);
		}
	}

	private void OnCompleteJsonLoad(JsonData json)
	{
		if (json.isNone())
		{
			LoadError = "Failed to load SkillDescContainer.";
			IsLoaded = true;
		}
		else
		{
			OnCompleteJson(json);
		}
	}

	private void OnCompleteJson(JsonData json)
	{
		if (!json.IsObject())
		{
			IsLoaded = true;
		}
		else if (json.Contains("SkillDescDic"))
		{
			IncreaseDic<int, SkillDesc> increaseDic = new IncreaseDic<int, SkillDesc>();
			increaseDic.LoadFromJson(json["SkillDescDic"]);
			IsLoaded = true;
			for (int i = 0; i < increaseDic.Count; i++)
			{
				int keyByIndex = increaseDic.GetKeyByIndex(i);
				SkillDesc valueByIndex = increaseDic.GetValueByIndex(i);
				SkillDescDic.Add(new DescKey(keyByIndex, 0), valueByIndex);
			}
		}
		else
		{
			SkillDescDic.LoadFromJson(json["SkillDescArray"]);
			IsLoaded = true;
		}
	}
}
