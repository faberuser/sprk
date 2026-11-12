using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillDataFormatter : IMessagePackFormatter<SkillData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(48);
		writer.Write(value.Index);
		writer.WriteNil();
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceNameKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.NameParam, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReplaceDescKey, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.DescParam, options);
		writer.Write(value.ReqIndex);
		writer.Write(value.NextIndex);
		writer.Write(value.FollowLevelIndex);
		resolver.GetFormatterWithVerify<SkillTriggerType>().Serialize(ref writer, value.TriggerType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TriggerValue, options);
		writer.Write(value.TriggerRatio);
		writer.Write(value.TriggerDisabled);
		resolver.GetFormatterWithVerify<SkillTriggerFlag[]>().Serialize(ref writer, value.TriggerFlag, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CancelableSkillIndex, options);
		writer.Write(value.Mp);
		writer.Write(value.ReqHpRatio);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ActiveCondition, options);
		writer.Write(value.CoolTimeMs);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.CoolTimeGroup, options);
		writer.Write(value.InitialCoolTimeMs);
		writer.Write(value.CastingTimeMs);
		writer.Write(value.ActingTimeMs);
		writer.Write(value.SoloTimeMs);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DurationTimeMs, options);
		writer.Write(value.CancelOnWaveEnd);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TickTimeValue, options);
		resolver.GetFormatterWithVerify<SkillAttrType>().Serialize(ref writer, value.AttrType, options);
		writer.Write(value.BaseAttack);
		resolver.GetFormatterWithVerify<AttackAttrType>().Serialize(ref writer, value.AttackType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ClearStateType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.IgnoreStateType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.DisabledStateType, options);
		resolver.GetFormatterWithVerify<SkillTargetType>().Serialize(ref writer, value.TargetType, options);
		writer.Write(value.TargetCount);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TargetCountState, options);
		writer.Write(value.MinRangeMm);
		writer.Write(value.MaxRangeMm);
		writer.Write(value.DeadTarget);
		writer.Write(value.IgnoreBlock);
		writer.Write(value.RetargetOnFire);
		writer.Write(value.ViewTargetWhileUse);
		resolver.GetFormatterWithVerify<SkillTargetFlag>().Serialize(ref writer, value.TargetFlag, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TargetStateType, options);
		resolver.GetFormatterWithVerify<SkillCcFlag>().Serialize(ref writer, value.CcFlag, options);
	}

	public SkillData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SkillData skillData = new SkillData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				skillData.Index = reader.ReadInt32();
				break;
			case 3:
				skillData.ReplaceNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				skillData.NameParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				skillData.ReplaceDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				skillData.DescParam = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				skillData.ReqIndex = reader.ReadInt32();
				break;
			case 8:
				skillData.NextIndex = reader.ReadInt32();
				break;
			case 9:
				skillData.FollowLevelIndex = reader.ReadInt32();
				break;
			case 10:
				skillData.TriggerType = resolver.GetFormatterWithVerify<SkillTriggerType>().Deserialize(ref reader, options);
				break;
			case 11:
				skillData.TriggerValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				skillData.TriggerRatio = reader.ReadInt32();
				break;
			case 13:
				skillData.TriggerDisabled = reader.ReadBoolean();
				break;
			case 14:
				skillData.TriggerFlag = resolver.GetFormatterWithVerify<SkillTriggerFlag[]>().Deserialize(ref reader, options);
				break;
			case 15:
				skillData.CancelableSkillIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 16:
				skillData.Mp = reader.ReadInt32();
				break;
			case 17:
				skillData.ReqHpRatio = reader.ReadInt32();
				break;
			case 18:
				skillData.ActiveCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 19:
				skillData.CoolTimeMs = reader.ReadInt32();
				break;
			case 20:
				skillData.CoolTimeGroup = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 21:
				skillData.InitialCoolTimeMs = reader.ReadInt32();
				break;
			case 22:
				skillData.CastingTimeMs = reader.ReadInt32();
				break;
			case 23:
				skillData.ActingTimeMs = reader.ReadInt32();
				break;
			case 24:
				skillData.SoloTimeMs = reader.ReadInt32();
				break;
			case 25:
				skillData.DurationTimeMs = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 26:
				skillData.CancelOnWaveEnd = reader.ReadBoolean();
				break;
			case 27:
				skillData.TickTimeValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 28:
				skillData.AttrType = resolver.GetFormatterWithVerify<SkillAttrType>().Deserialize(ref reader, options);
				break;
			case 29:
				skillData.BaseAttack = reader.ReadBoolean();
				break;
			case 30:
				skillData.AttackType = resolver.GetFormatterWithVerify<AttackAttrType>().Deserialize(ref reader, options);
				break;
			case 31:
				skillData.StateType = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 32:
				skillData.ClearStateType = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 33:
				skillData.IgnoreStateType = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 34:
				skillData.DisabledStateType = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 35:
				skillData.TargetType = resolver.GetFormatterWithVerify<SkillTargetType>().Deserialize(ref reader, options);
				break;
			case 36:
				skillData.TargetCount = reader.ReadInt16();
				break;
			case 37:
				skillData.TargetCountState = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 38:
				skillData.MinRangeMm = reader.ReadInt32();
				break;
			case 39:
				skillData.MaxRangeMm = reader.ReadInt32();
				break;
			case 40:
				skillData.DeadTarget = reader.ReadBoolean();
				break;
			case 41:
				skillData.IgnoreBlock = reader.ReadBoolean();
				break;
			case 42:
				skillData.RetargetOnFire = reader.ReadBoolean();
				break;
			case 43:
				skillData.ViewTargetWhileUse = reader.ReadBoolean();
				break;
			case 44:
				skillData.TargetFlag = resolver.GetFormatterWithVerify<SkillTargetFlag>().Deserialize(ref reader, options);
				break;
			case 45:
				skillData.CreatureTagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 46:
				skillData.TargetStateType = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 47:
				skillData.CcFlag = resolver.GetFormatterWithVerify<SkillCcFlag>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return skillData;
	}
}
