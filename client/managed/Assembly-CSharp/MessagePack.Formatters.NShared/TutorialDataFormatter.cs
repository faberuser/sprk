using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class TutorialDataFormatter : IMessagePackFormatter<TutorialData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, TutorialData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		writer.Write(value.Index);
		writer.Write(value.Sequence);
		writer.Write(value.ReqIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Command, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Argument, options);
		writer.Write(value.RewardGold);
		writer.Write(value.RewardGem);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RewardAction, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RewardDungeonIndex, options);
		writer.Write(value.HideRewardPopup);
		writer.Write(value.Repeatable);
		writer.Write(value.SendAdjustEventIndex);
	}

	public TutorialData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		TutorialData tutorialData = new TutorialData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				tutorialData.Index = reader.ReadInt32();
				break;
			case 1:
				tutorialData.Sequence = reader.ReadInt32();
				break;
			case 2:
				tutorialData.ReqIndex = reader.ReadInt32();
				break;
			case 3:
				tutorialData.Command = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				tutorialData.Argument = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				tutorialData.RewardGold = reader.ReadInt32();
				break;
			case 6:
				tutorialData.RewardGem = reader.ReadInt32();
				break;
			case 7:
				tutorialData.RewardIndex = reader.ReadInt32();
				break;
			case 8:
				tutorialData.RewardAction = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 9:
				tutorialData.RewardDungeonIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 10:
				tutorialData.HideRewardPopup = reader.ReadBoolean();
				break;
			case 11:
				tutorialData.Repeatable = reader.ReadBoolean();
				break;
			case 12:
				tutorialData.SendAdjustEventIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return tutorialData;
	}
}
