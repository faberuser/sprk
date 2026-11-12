using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OperationData_LoadDataFormatter : IMessagePackFormatter<OperationData.LoadData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OperationData.LoadData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(36);
		writer.Write(value.SkillIndex);
		writer.Write(value.OperationNo);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.Type, options);
		resolver.GetFormatterWithVerify<OperationTargetType>().Serialize(ref writer, value.TargetType, options);
		resolver.GetFormatterWithVerify<OperationTargetSelectType>().Serialize(ref writer, value.TargetSelectType, options);
		writer.Write(value.MaxRangeMm);
		writer.Write(value.AreaOffsetMm);
		writer.Write(value.RadiusMm);
		writer.Write(value.Width);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.AreaDelayMss, options);
		writer.Write(value.Angle);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Value, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ValueFactor, options);
		writer.Write(value.DurationMs);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DurationFactor, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.DelayMs, options);
		writer.Write(value.TickTimeMs);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TickTimeFactor, options);
		writer.Write(value.ReqSkillIndex);
		resolver.GetFormatterWithVerify<OperationConditionType>().Serialize(ref writer, value.ConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue, options);
		writer.Write(value.ConditionReverseCheck);
		resolver.GetFormatterWithVerify<OperationConditionActionType>().Serialize(ref writer, value.ConditionActionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionTrueActionValue, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionFalseActionValue, options);
		writer.Write(value.HitChance);
		writer.Write(value.MaxAccCount);
		writer.Write(value.Parent);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.State, options);
		writer.Write(value.AddStateCount);
		resolver.GetFormatterWithVerify<OperationFlag[]>().Serialize(ref writer, value.Flags, options);
		resolver.GetFormatterWithVerify<StatType[]>().Serialize(ref writer, value.RecalculateValueByStats, options);
		writer.Write(value.Group);
		resolver.GetFormatterWithVerify<OperationGroupConditionType>().Serialize(ref writer, value.GroupConditionType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.GroupConditionValues, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RecalculateValueByStates, options);
	}

	public OperationData.LoadData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OperationData.LoadData loadData = new OperationData.LoadData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				loadData.SkillIndex = reader.ReadInt32();
				break;
			case 1:
				loadData.OperationNo = reader.ReadInt32();
				break;
			case 2:
				loadData.Type = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 3:
				loadData.TargetType = resolver.GetFormatterWithVerify<OperationTargetType>().Deserialize(ref reader, options);
				break;
			case 4:
				loadData.TargetSelectType = resolver.GetFormatterWithVerify<OperationTargetSelectType>().Deserialize(ref reader, options);
				break;
			case 5:
				loadData.MaxRangeMm = reader.ReadInt32();
				break;
			case 6:
				loadData.AreaOffsetMm = reader.ReadInt32();
				break;
			case 7:
				loadData.RadiusMm = reader.ReadInt32();
				break;
			case 8:
				loadData.Width = reader.ReadInt32();
				break;
			case 9:
				loadData.AreaDelayMss = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				loadData.Angle = reader.ReadInt32();
				break;
			case 11:
				loadData.Value = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				loadData.ValueFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 13:
				loadData.DurationMs = reader.ReadInt32();
				break;
			case 14:
				loadData.DurationFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 15:
				loadData.DelayMs = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 16:
				loadData.TickTimeMs = reader.ReadInt32();
				break;
			case 17:
				loadData.TickTimeFactor = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 18:
				loadData.ReqSkillIndex = reader.ReadInt32();
				break;
			case 19:
				loadData.ConditionType = resolver.GetFormatterWithVerify<OperationConditionType>().Deserialize(ref reader, options);
				break;
			case 20:
				loadData.ConditionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 21:
				loadData.ConditionReverseCheck = reader.ReadBoolean();
				break;
			case 22:
				loadData.ConditionActionType = resolver.GetFormatterWithVerify<OperationConditionActionType>().Deserialize(ref reader, options);
				break;
			case 23:
				loadData.ConditionTrueActionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 24:
				loadData.ConditionFalseActionValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 25:
				loadData.HitChance = reader.ReadInt32();
				break;
			case 26:
				loadData.MaxAccCount = reader.ReadInt32();
				break;
			case 27:
				loadData.Parent = reader.ReadInt32();
				break;
			case 28:
				loadData.State = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 29:
				loadData.AddStateCount = reader.ReadInt32();
				break;
			case 30:
				loadData.Flags = resolver.GetFormatterWithVerify<OperationFlag[]>().Deserialize(ref reader, options);
				break;
			case 31:
				loadData.RecalculateValueByStats = resolver.GetFormatterWithVerify<StatType[]>().Deserialize(ref reader, options);
				break;
			case 32:
				loadData.Group = reader.ReadByte();
				break;
			case 33:
				loadData.GroupConditionType = resolver.GetFormatterWithVerify<OperationGroupConditionType>().Deserialize(ref reader, options);
				break;
			case 34:
				loadData.GroupConditionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 35:
				loadData.RecalculateValueByStates = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return loadData;
	}
}
