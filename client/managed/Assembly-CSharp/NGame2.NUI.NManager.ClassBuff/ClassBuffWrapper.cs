using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NComponent2;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NSingleton;

namespace NGame2.NUI.NManager.ClassBuff;

public class ClassBuffWrapper
{
	private ClassBuffManagement _management;

	private static LocalAccountBool _firstOpenStorage = new LocalAccountBool(i_default: false, "_ClassBuff_Contents_FirstOpen");

	public static bool IsFirstOpenStorage
	{
		get
		{
			return _firstOpenStorage.Value;
		}
		set
		{
			_firstOpenStorage.Value = value;
		}
	}

	public string LocalizeClassBuffPointCenterMessage => "CLASS_BUFF_POINT_SENTER_MESSAGE";

	public string LocalizeClassBuffResetPopupDesc => "CLASS_BUFF_RESET_POPUP_DESC";

	public string LocalizeClassBuffResetBtn => "CLASS_BUFF_RESET_BTN";

	public string LocalizeClassBuffPointRemain => "CLASS_BUFF_POINT_REMAIN";

	public string LocalizeClassBuffNeedSkillLevelUp => "CLASS_BUFF_NEED_SKILL_LEVEL_UP";

	public string LocalizeClassBuffNeedHeroCount => "CLASS_BUFF_NEED_CLASS_HERO";

	public string LocalizeClassBuffNeedPointLevelUp => "CLASS_BUFF_NEED_POINT_LEVEL_UP";

	public string LocalizeClassBuffLevelUpPopupTitle => "CLASS_BUFF_LEVEL_UP_POPUP_TITLE";

	public string LocalizeClassBuffLevelUpDesc => "CLASS_BUFF_LEVEL_UP_DESC";

	public string LocalizeContentsClassBuff => LocalizationManager.GetStringDirect("CONTENT_CLASSBUFF");

	private string LocalizeKeyClassBuffMaxLevel => "CLASS_BUFF_LEVEL_MAX_MESSASGE";

	public string LocalizClassBuffMaxLevel => LocalizationManager.GetStringDirect(LocalizeKeyClassBuffMaxLevel);

	public void Init(ClassBuffManagement management)
	{
		_management = management;
	}

	public void SetClassBuffPointDIc(List<CreatureTagType> list = null)
	{
		GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager?.ApplyByCreatureList(list);
	}

	public List<CreatureTagType> GetCreatureTypeListByGetClassBuffPoint()
	{
		List<CreatureTagType> list = new List<CreatureTagType>();
		list.Clear();
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return list;
		}
		Dictionary<CreatureTagType, long> classBuffPointDic = classBuffManager.ClassBuffPointDic;
		if (classBuffPointDic == null)
		{
			return list;
		}
		foreach (KeyValuePair<CreatureTagType, long> item in classBuffPointDic)
		{
			if (!classBuffManager.PrevClassBuffPointDic.ContainsKey(item.Key))
			{
				if (item.Value > 0)
				{
					list.Add(item.Key);
				}
			}
			else if (item.Value != classBuffManager.PrevClassBuffPointDic[item.Key])
			{
				list.Add(item.Key);
			}
		}
		return list;
	}

	public List<ClassBuffData> GetClassBuffDataListByClassBuffInfo()
	{
		List<ClassBuffData> list = new List<ClassBuffData>();
		list.Clear();
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return list;
		}
		Dictionary<CreatureTagType, Dictionary<int, ClassBuffInfo>> classBuffInfoDic = classBuffManager.ClassBuffInfoDic;
		if (classBuffInfoDic == null)
		{
			return list;
		}
		ClassBuffData classBuffData = null;
		CreatureTagType creatureTagType = CreatureTagType.None;
		foreach (Dictionary<int, ClassBuffInfo> value in classBuffInfoDic.Values)
		{
			if (__Extension_Method_ICollection__.IsNullOrEmpty(value))
			{
				continue;
			}
			foreach (ClassBuffInfo value2 in value.Values)
			{
				if (value2 == null)
				{
					continue;
				}
				creatureTagType = CreatureTagType.None;
				if (BanRuleData.GetCreatureTagType(value2.TagType.ToString(), out creatureTagType))
				{
					classBuffData = GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(creatureTagType, value2.ClassBuffIndex, value2.ClassBuffLevel);
					if (classBuffData != null)
					{
						classBuffData.CreatureType = value2.TagType;
						list.Add(classBuffData);
					}
				}
			}
		}
		return list;
	}

	public void SetClassBuffIcon(ref UIWidget widget, ref IconProxy iconProxy, CreatureTagType type)
	{
		if (!(widget == null))
		{
			if (iconProxy == null)
			{
				iconProxy = IconProxy.Create(widget);
			}
			if (iconProxy != null)
			{
				iconProxy.SetClassBuffIcon(type.ToString());
			}
		}
	}

	public SkillData GetSkillData(int skillIndex)
	{
		SkillData data = GameRoot.Instance.SkillDataContainer.GetData(skillIndex);
		if (data == null)
		{
			return null;
		}
		return data;
	}

	public List<CreatureTagType> GetCreatureTypeListByItemData(ItemData itemData)
	{
		if (!CheckPossibleGetClassBuffPointByItemData(itemData))
		{
			return null;
		}
		List<CreatureTagType> creatureTypeListByGetClassBuffPoint = GetCreatureTypeListByGetClassBuffPoint();
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(itemData.Code);
		List<CreatureTagType> list = new List<CreatureTagType>();
		list.Clear();
		CreatureTagType[] tagType = data.TagType;
		foreach (CreatureTagType item in tagType)
		{
			if (creatureTypeListByGetClassBuffPoint.Contains(item))
			{
				list.Add(item);
			}
		}
		return list;
	}

	public List<ClassBuffData> GetCurrentClassBuffDataList(CreatureTagType creatureType)
	{
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		Dictionary<int, List<ClassBuffData>> classBuffDataDicByCreatureType = GameRoot.Instance.ClassBuffDataContainer.GetClassBuffDataDicByCreatureType(creatureType);
		List<ClassBuffData> list = new List<ClassBuffData>();
		list.Clear();
		if (classBuffManager == null || __Extension_Method_ICollection__.COUNT(classBuffDataDicByCreatureType) == 0)
		{
			return list;
		}
		ClassBuffInfo classBuffInfo = null;
		ClassBuffData classBuffData = null;
		foreach (List<ClassBuffData> value in classBuffDataDicByCreatureType.Values)
		{
			foreach (ClassBuffData item in value)
			{
				if (item == null)
				{
					continue;
				}
				classBuffInfo = classBuffManager.GetClassBuffInfo(creatureType, item.ClassBuffIndex);
				if (classBuffInfo == null)
				{
					classBuffData = GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(creatureType, item.ClassBuffIndex, 0);
				}
				else
				{
					if (item.ClassBuffLevel != classBuffInfo.ClassBuffLevel)
					{
						continue;
					}
					classBuffData = item;
				}
				if (classBuffData != null)
				{
					list.Add(classBuffData);
				}
				break;
			}
		}
		return list;
	}

	public long GetOpenNeedClassBuffPoint()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("ClassBuffContentsOpenPoint", 100);
	}

	public int GetNeedGemResetClassBuff()
	{
		return GameRoot.Instance.ConstantDataContainer.GetValueInt("ResetClassBuff", 500);
	}

	public bool CheckIsInit(CreatureTagType creatureType)
	{
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return false;
		}
		return __Extension_Method_ICollection__.IsNullOrEmpty(classBuffManager.GetClassBuffInfoDicByCreatureType(creatureType));
	}

	public bool CheckPossibleLevelUp(ClassBuffData data)
	{
		if (data == null)
		{
			return false;
		}
		if (!CheckNeedClassBuff(data))
		{
			return false;
		}
		if (!CheckNeedHeroCount(data.TagType, data.NeedHeroCount))
		{
			return false;
		}
		if (!CheckNeedPoint(data))
		{
			return false;
		}
		return true;
	}

	public bool CheckNeedClassBuff(ClassBuffData data)
	{
		if (data == null)
		{
			return false;
		}
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return false;
		}
		if (data.NeedClassBuffIndex > 0)
		{
			ClassBuffInfo classBuffInfo = classBuffManager.GetClassBuffInfo(data.TagType, data.NeedClassBuffIndex);
			if (classBuffInfo == null)
			{
				return false;
			}
			if (data.NeedClassBuffLevel > classBuffInfo.ClassBuffLevel)
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckNeedHeroCount(CreatureTagType creatureType, int needHeroCount)
	{
		if (creatureType == CreatureTagType.None)
		{
			return false;
		}
		if (needHeroCount > 0)
		{
			int count = GlobalAutoBehaviour<AccountManager>.instance.HeroManager.GetHeroesByClass(creatureType).Count;
			if (needHeroCount > count)
			{
				return false;
			}
		}
		return true;
	}

	public bool CheckNeedPoint(ClassBuffData data)
	{
		if (data == null)
		{
			return false;
		}
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (classBuffManager == null)
		{
			return false;
		}
		long totalNeedPoint = GetTotalNeedPoint(data);
		long classBuffPointByCreatureType = classBuffManager.GetClassBuffPointByCreatureType(data.TagType);
		if (totalNeedPoint > classBuffPointByCreatureType)
		{
			return false;
		}
		return true;
	}

	public long GetTotalNeedPoint(ClassBuffData data)
	{
		NGame2.NAccount.UserClassBuffManager classBuffManager = GlobalAutoBehaviour<AccountManager>.instance.ClassBuffManager;
		if (data == null || classBuffManager == null)
		{
			return 9999999999999999L;
		}
		int num = 0;
		long num2 = 0L;
		ClassBuffInfo classBuffInfo = classBuffManager.GetClassBuffInfo(data.TagType, data.ClassBuffIndex);
		if (classBuffInfo != null)
		{
			num = classBuffInfo.ClassBuffLevel;
		}
		ClassBuffData classBuffData = null;
		for (int i = num + 1; i <= data.ClassBuffLevel; i++)
		{
			classBuffData = GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(data.TagType, data.ClassBuffIndex, i);
			if (classBuffData != null)
			{
				num2 += classBuffData.NeedPoint;
			}
		}
		return num2;
	}

	public bool CheckPossibleGetClassBuffPointByItemData(ItemData itemData)
	{
		if (itemData == null)
		{
			return false;
		}
		if (itemData.Type != ItemType.Equip)
		{
			return false;
		}
		EquipItemData data = GameRoot.Instance.EquipItemDataContainer.GetData(itemData.Code);
		if (data == null)
		{
			return false;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(data.CreatureIndex))
		{
			return false;
		}
		return true;
	}

	private string LocalizeKeyClassBuffName(CreatureTagType type)
	{
		return "CLASS_BUFF_" + type.ToString().ToUpper() + "_NAME";
	}

	public string LocalizeClassBuffName(CreatureTagType type)
	{
		return LocalizationManager.GetStringDirect(LocalizeKeyClassBuffName(type));
	}

	public string LocalizeTotalDesc(CreatureTagType type)
	{
		return LocalizationManager.GetStringDirect("CLASS_BUFF_TOTAL_DESC", LocalizationManager.GetString(type));
	}

	private string LocalizeKeyClassBuffPointName(CreatureTagType type)
	{
		return "CLASS_POINT_" + type.ToString().ToUpper() + "_NAME";
	}

	public string LocalizeClassBuffPointName(CreatureTagType type)
	{
		return LocalizationManager.GetStringDirect(LocalizeKeyClassBuffPointName(type));
	}
}
