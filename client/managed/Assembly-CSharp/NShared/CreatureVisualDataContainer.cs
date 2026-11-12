using System;
using NShared.NJit;

namespace NShared;

public class CreatureVisualDataContainer : JitDataContainerDefault<string, CreatureVisualData>
{
	public const string TABLENAME = "CreatureVisualTable";

	public override TablePathDefine PathDefine => new TablePathDefine("CreatureVisualTable");

	public void GetCreatureName(CreatureData creatureData, string visualInfo, Func<string, string> localizer, out string creatureName, out string creatureNamePrefix)
	{
		creatureName = creatureData.Name;
		creatureNamePrefix = string.Empty;
		if (string.IsNullOrEmpty(visualInfo))
		{
			return;
		}
		CreatureVisualData data = GetData(visualInfo);
		if (data != null)
		{
			if (!string.IsNullOrEmpty(data.NameCode))
			{
				creatureName = localizer(data.NameCode);
			}
			if (!string.IsNullOrEmpty(data.NamePrefixCode))
			{
				creatureNamePrefix = localizer(data.NamePrefixCode);
			}
		}
	}

	public void GetCreatureNameByBossNameTag(CreatureData creatureData, string visualInfo, Func<string, string> localizer, out string creatureName, out string creatureNamePrefix)
	{
		creatureName = creatureData.Name;
		creatureNamePrefix = string.Empty;
		if (string.IsNullOrEmpty(visualInfo))
		{
			return;
		}
		CreatureVisualData data = GetData(visualInfo);
		if (data != null)
		{
			if (!string.IsNullOrEmpty(data.BossIntroReplaceNameCode))
			{
				creatureName = localizer(data.BossIntroReplaceNameCode);
			}
			else if (!string.IsNullOrEmpty(data.NameCode))
			{
				creatureName = localizer(data.NameCode);
			}
			if (!string.IsNullOrEmpty(data.BossIntroReplaceNamePrefixCode))
			{
				creatureNamePrefix = localizer(data.BossIntroReplaceNamePrefixCode);
			}
			else if (!string.IsNullOrEmpty(data.NamePrefixCode))
			{
				creatureNamePrefix = localizer(data.NamePrefixCode);
			}
		}
	}

	public void GetCreatureDesc(CreatureData creatureData, string visualInfo, Func<string, string> localizer, out string creatureDesc)
	{
		creatureDesc = creatureData.Description;
		if (!string.IsNullOrEmpty(visualInfo))
		{
			CreatureVisualData data = GetData(visualInfo);
			if (data != null && !string.IsNullOrEmpty(data.DescriptionCode))
			{
				creatureDesc = localizer(data.DescriptionCode);
			}
		}
	}
}
