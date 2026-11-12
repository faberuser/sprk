using System;
using NGame2.NContents;
using NGame2.NUI.NManager.ClassBuff;
using NShared;
using NVespa;
using NVespa.NSingleton;

namespace NGame2.NUtil.NConditionChecker;

public class CheckClassBuff : ConditionChecker.ConditionBase
{
	public override bool Check(string[] values)
	{
		if (__Extension_Method_ICollection__.COUNT(values) < 2)
		{
			return false;
		}
		if (!SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.ClassBuff))
		{
			return false;
		}
		CreatureTagType type = CreatureTagType.None;
		string text = values[1];
		if (text != "All" && !BanRuleData.GetCreatureTagType(text, out type))
		{
			return false;
		}
		int result = 0;
		if (!int.TryParse(values[2], out result))
		{
			return false;
		}
		if (text == "All")
		{
			foreach (CreatureTagType value in Enum.GetValues(typeof(CreatureTagType)))
			{
				if (value != CreatureTagType.None && value != CreatureTagType.End && CheckPossibleLevelUp(value, result))
				{
					return true;
				}
			}
		}
		else if (CheckPossibleLevelUp(type, result))
		{
			return true;
		}
		return false;
	}

	private bool CheckPossibleLevelUp(CreatureTagType creatureType, int level)
	{
		ClassBuffWrapper classBuffWrapper = SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper;
		if (classBuffWrapper == null)
		{
			return false;
		}
		foreach (ClassBuffData currentClassBuffData in classBuffWrapper.GetCurrentClassBuffDataList(creatureType))
		{
			if (currentClassBuffData != null && CheckPossibleLevelUpByClassBuffData(currentClassBuffData, level))
			{
				return true;
			}
		}
		return false;
	}

	private bool CheckPossibleLevelUpByClassBuffData(ClassBuffData data, int level)
	{
		if (data == null || level <= 0)
		{
			return false;
		}
		ClassBuffData classBuffData = null;
		for (int i = 1; i <= level; i++)
		{
			classBuffData = GameRoot.Instance.ClassBuffDataContainer.GetClassBuffData(data.TagType, data.ClassBuffIndex, data.ClassBuffLevel + i);
			if (classBuffData == null)
			{
				return false;
			}
			if (!SceneAutoBehaviour<ClassBuffManagement>.instance.ClassBuffWrapper.CheckPossibleLevelUp(classBuffData))
			{
				return false;
			}
		}
		return true;
	}
}
