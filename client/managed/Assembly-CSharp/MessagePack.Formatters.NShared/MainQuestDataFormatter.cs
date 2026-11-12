using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MainQuestDataFormatter : IMessagePackFormatter<MainQuestData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MainQuestData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.QuestIndex);
		writer.Write(value.Step);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Title, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Desc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BackgroundPath, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable1, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Variable2, options);
		resolver.GetFormatterWithVerify<QuestActionType>().Serialize(ref writer, value.StateType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.StateValue, options);
		writer.Write(value.ClientOnly);
		writer.Write(value.ReqProgress);
		writer.Write(value.AutoComplete);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		writer.Write(value.RewardIndex);
	}

	public MainQuestData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MainQuestData mainQuestData = new MainQuestData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mainQuestData.QuestIndex = reader.ReadInt32();
				break;
			case 1:
				mainQuestData.Step = reader.ReadInt32();
				break;
			case 2:
				mainQuestData.Title = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				mainQuestData.Desc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				mainQuestData.BackgroundPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				mainQuestData.Variable1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				mainQuestData.Variable2 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 7:
				mainQuestData.StateType = resolver.GetFormatterWithVerify<QuestActionType>().Deserialize(ref reader, options);
				break;
			case 8:
				mainQuestData.StateValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				mainQuestData.ClientOnly = reader.ReadBoolean();
				break;
			case 10:
				mainQuestData.ReqProgress = reader.ReadInt32();
				break;
			case 11:
				mainQuestData.AutoComplete = reader.ReadBoolean();
				break;
			case 12:
				mainQuestData.MoveTaskType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				mainQuestData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 14:
				mainQuestData.RewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return mainQuestData;
	}
}
