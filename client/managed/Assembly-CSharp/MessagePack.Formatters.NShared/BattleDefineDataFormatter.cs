using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class BattleDefineDataFormatter : IMessagePackFormatter<BattleDefineData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, BattleDefineData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(40);
		resolver.GetFormatterWithVerify<HashString>().Serialize(ref writer, value.Code, options);
		writer.Write(value.MaxTime);
		writer.Write(value.ExtraTime);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LabelPrefix, options);
		writer.Write(value.StartMpRatioM);
		writer.Write(value.AutoPlayStarCount);
		writer.Write(value.FastSpeedStarCount);
		writer.Write(value.AutoRepeatStarCount);
		writer.Write(value.ApplyLevelPenalty);
		writer.Write(value.FastSkillTooltip);
		writer.Write(value.DenyRaidInvite);
		writer.Write(value.DenyDuel);
		writer.Write(value.HPPercent);
		resolver.GetFormatterWithVerify<DamageMeterFlag>().Serialize(ref writer, value.DamageMeterFlag, options);
		writer.Write(value.DisableBossHUD);
		writer.Write(value.HideAutoStartButton);
		resolver.GetFormatterWithVerify<BattleRetryConditionType[]>().Serialize(ref writer, value.RetryCondition, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattleCameraName, options);
		writer.Write(value.IgnoreDropItem);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.ModStatType1, options);
		writer.Write(value.ModStatRatioM1);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.ModStatType2, options);
		writer.Write(value.ModStatRatioM2);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.ModStatType3, options);
		writer.Write(value.ModStatRatioM3);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.ModStatType4, options);
		writer.Write(value.ModStatRatioM4);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.ModStatType5, options);
		writer.Write(value.ModStatRatioM5);
		resolver.GetFormatterWithVerify<EBattleMenu>().Serialize(ref writer, value.BattleMenu, options);
		writer.Write(value.SkillCameraOff);
		writer.Write(value.DisableInventoryInResult);
		writer.Write(value.DisablePartySettingInResult);
		writer.Write(value.DisableHeroListInResult);
		resolver.GetFormatterWithVerify<ExInfoType>().Serialize(ref writer, value.ExInfoTypeOnPartySetting, options);
		writer.Write(value.DisableLastKill);
		writer.Write(value.ShowBossManaGauge);
		writer.Write(value.CameraFarClipPlane);
		writer.Write(value.EnhanceTimeSyncMode);
		writer.Write(value.BetterBossHpUI);
	}

	public BattleDefineData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		BattleDefineData battleDefineData = new BattleDefineData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				battleDefineData.Code = resolver.GetFormatterWithVerify<HashString>().Deserialize(ref reader, options);
				break;
			case 1:
				battleDefineData.MaxTime = reader.ReadInt32();
				break;
			case 2:
				battleDefineData.ExtraTime = reader.ReadInt32();
				break;
			case 3:
				battleDefineData.LabelPrefix = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				battleDefineData.StartMpRatioM = reader.ReadInt32();
				break;
			case 5:
				battleDefineData.AutoPlayStarCount = reader.ReadInt32();
				break;
			case 6:
				battleDefineData.FastSpeedStarCount = reader.ReadInt32();
				break;
			case 7:
				battleDefineData.AutoRepeatStarCount = reader.ReadInt32();
				break;
			case 8:
				battleDefineData.ApplyLevelPenalty = reader.ReadBoolean();
				break;
			case 9:
				battleDefineData.FastSkillTooltip = reader.ReadBoolean();
				break;
			case 10:
				battleDefineData.DenyRaidInvite = reader.ReadBoolean();
				break;
			case 11:
				battleDefineData.DenyDuel = reader.ReadBoolean();
				break;
			case 12:
				battleDefineData.HPPercent = reader.ReadBoolean();
				break;
			case 13:
				battleDefineData.DamageMeterFlag = resolver.GetFormatterWithVerify<DamageMeterFlag>().Deserialize(ref reader, options);
				break;
			case 14:
				battleDefineData.DisableBossHUD = reader.ReadBoolean();
				break;
			case 15:
				battleDefineData.HideAutoStartButton = reader.ReadBoolean();
				break;
			case 16:
				battleDefineData.RetryCondition = resolver.GetFormatterWithVerify<BattleRetryConditionType[]>().Deserialize(ref reader, options);
				break;
			case 17:
				battleDefineData.BattleCameraName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				battleDefineData.IgnoreDropItem = reader.ReadBoolean();
				break;
			case 19:
				battleDefineData.ModStatType1 = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 20:
				battleDefineData.ModStatRatioM1 = reader.ReadInt32();
				break;
			case 21:
				battleDefineData.ModStatType2 = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 22:
				battleDefineData.ModStatRatioM2 = reader.ReadInt32();
				break;
			case 23:
				battleDefineData.ModStatType3 = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 24:
				battleDefineData.ModStatRatioM3 = reader.ReadInt32();
				break;
			case 25:
				battleDefineData.ModStatType4 = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 26:
				battleDefineData.ModStatRatioM4 = reader.ReadInt32();
				break;
			case 27:
				battleDefineData.ModStatType5 = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 28:
				battleDefineData.ModStatRatioM5 = reader.ReadInt32();
				break;
			case 29:
				battleDefineData.BattleMenu = resolver.GetFormatterWithVerify<EBattleMenu>().Deserialize(ref reader, options);
				break;
			case 30:
				battleDefineData.SkillCameraOff = reader.ReadBoolean();
				break;
			case 31:
				battleDefineData.DisableInventoryInResult = reader.ReadBoolean();
				break;
			case 32:
				battleDefineData.DisablePartySettingInResult = reader.ReadBoolean();
				break;
			case 33:
				battleDefineData.DisableHeroListInResult = reader.ReadBoolean();
				break;
			case 34:
				battleDefineData.ExInfoTypeOnPartySetting = resolver.GetFormatterWithVerify<ExInfoType>().Deserialize(ref reader, options);
				break;
			case 35:
				battleDefineData.DisableLastKill = reader.ReadBoolean();
				break;
			case 36:
				battleDefineData.ShowBossManaGauge = reader.ReadBoolean();
				break;
			case 37:
				battleDefineData.CameraFarClipPlane = reader.ReadInt32();
				break;
			case 38:
				battleDefineData.EnhanceTimeSyncMode = reader.ReadBoolean();
				break;
			case 39:
				battleDefineData.BetterBossHpUI = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return battleDefineData;
	}
}
