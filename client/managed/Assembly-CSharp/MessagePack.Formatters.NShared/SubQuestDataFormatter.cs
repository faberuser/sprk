using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SubQuestDataFormatter : IMessagePackFormatter<SubQuestData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SubQuestData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(21);
		writer.Write(value.QuestIndex);
		writer.Write(value.Group);
		writer.Write(value.Step);
		writer.Write(value.OldIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Title, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Goal, options);
		resolver.GetFormatterWithVerify<QuestActionType>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.StateValue, options);
		writer.Write(value.ClientOnly);
		writer.Write(value.ReqProgress);
		writer.Write(value.AutoComplete);
		resolver.GetFormatterWithVerify<ClientActivityType>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable2, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable3, options);
		writer.Write(value.RewardIndex);
		writer.Write(value.TutorialIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.OpenConditionLocalKey, options);
		writer.Write(value.VisualRewardIndex);
	}

	public SubQuestData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SubQuestData subQuestData = new SubQuestData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				subQuestData.QuestIndex = reader.ReadInt32();
				break;
			case 1:
				subQuestData.Group = reader.ReadInt32();
				break;
			case 2:
				subQuestData.Step = reader.ReadInt32();
				break;
			case 3:
				subQuestData.OldIndex = reader.ReadInt32();
				break;
			case 4:
				subQuestData.Title = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				subQuestData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				subQuestData.Goal = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				subQuestData.StateType = resolver.GetFormatterWithVerify<QuestActionType>().Deserialize(ref reader, options);
				break;
			case 8:
				subQuestData.StateValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				subQuestData.ClientOnly = reader.ReadBoolean();
				break;
			case 10:
				subQuestData.ReqProgress = reader.ReadInt32();
				break;
			case 11:
				subQuestData.AutoComplete = reader.ReadBoolean();
				break;
			case 12:
				subQuestData.MoveTaskType = resolver.GetFormatterWithVerify<ClientActivityType>().Deserialize(ref reader, options);
				break;
			case 13:
				subQuestData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				subQuestData.Variable1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 15:
				subQuestData.Variable2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				subQuestData.Variable3 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 17:
				subQuestData.RewardIndex = reader.ReadInt32();
				break;
			case 18:
				subQuestData.TutorialIndex = reader.ReadInt32();
				break;
			case 19:
				subQuestData.OpenConditionLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				subQuestData.VisualRewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return subQuestData;
	}
}
