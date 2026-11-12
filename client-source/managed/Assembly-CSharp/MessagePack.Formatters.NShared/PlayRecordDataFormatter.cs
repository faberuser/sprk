using NShared;
using NShared.NUtil;

namespace MessagePack.Formatters.NShared;

public sealed class PlayRecordDataFormatter : IMessagePackFormatter<PlayRecordData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PlayRecordData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(24);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<PlayRecordCategory>().Serialize(ref writer, value.Category, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubCategoryTitle, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Title, options);
		resolver.GetFormatterWithVerify<QuestActionType>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.StateValue, options);
		resolver.GetFormatterWithVerify<QuestActionType>().Serialize(ref writer, value.ActionType, options);
		resolver.GetFormatterWithVerify<PlayRecordActionCalculateType>().Serialize(ref writer, value.ActionCalculateType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ActionValueAttribute, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionKey1, options);
		resolver.GetFormatterWithVerify<CheckType>().Serialize(ref writer, value.ConditionCompare1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionKey2, options);
		resolver.GetFormatterWithVerify<CheckType>().Serialize(ref writer, value.ConditionCompare2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConditionKey3, options);
		resolver.GetFormatterWithVerify<CheckType>().Serialize(ref writer, value.ConditionCompare3, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.ConditionValue3, options);
		resolver.GetFormatterWithVerify<PlayRecordDisplayType>().Serialize(ref writer, value.DisplayType, options);
		resolver.GetFormatterWithVerify<PlayRecordDisplayValueType>().Serialize(ref writer, value.DisplayValueType, options);
		writer.Write(value.AppendComma);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ButtonLabelText, options);
	}

	public PlayRecordData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PlayRecordData playRecordData = new PlayRecordData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				playRecordData.Index = reader.ReadInt32();
				break;
			case 1:
				playRecordData.Category = resolver.GetFormatterWithVerify<PlayRecordCategory>().Deserialize(ref reader, options);
				break;
			case 2:
				playRecordData.SubCategoryTitle = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				playRecordData.Title = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				playRecordData.StateType = resolver.GetFormatterWithVerify<QuestActionType>().Deserialize(ref reader, options);
				break;
			case 5:
				playRecordData.StateValue = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				playRecordData.ActionType = resolver.GetFormatterWithVerify<QuestActionType>().Deserialize(ref reader, options);
				break;
			case 7:
				playRecordData.ActionCalculateType = resolver.GetFormatterWithVerify<PlayRecordActionCalculateType>().Deserialize(ref reader, options);
				break;
			case 8:
				playRecordData.ActionValueAttribute = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				playRecordData.ConditionKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				playRecordData.ConditionCompare1 = resolver.GetFormatterWithVerify<CheckType>().Deserialize(ref reader, options);
				break;
			case 11:
				playRecordData.ConditionValue1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				playRecordData.ConditionKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				playRecordData.ConditionCompare2 = resolver.GetFormatterWithVerify<CheckType>().Deserialize(ref reader, options);
				break;
			case 14:
				playRecordData.ConditionValue2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 15:
				playRecordData.ConditionKey3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				playRecordData.ConditionCompare3 = resolver.GetFormatterWithVerify<CheckType>().Deserialize(ref reader, options);
				break;
			case 17:
				playRecordData.ConditionValue3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 18:
				playRecordData.DisplayType = resolver.GetFormatterWithVerify<PlayRecordDisplayType>().Deserialize(ref reader, options);
				break;
			case 19:
				playRecordData.DisplayValueType = resolver.GetFormatterWithVerify<PlayRecordDisplayValueType>().Deserialize(ref reader, options);
				break;
			case 20:
				playRecordData.AppendComma = reader.ReadBoolean();
				break;
			case 21:
				playRecordData.MoveTaskType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				playRecordData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 23:
				playRecordData.ButtonLabelText = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return playRecordData;
	}
}
