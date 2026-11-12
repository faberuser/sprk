using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameGuideDataFormatter : IMessagePackFormatter<GameGuideData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameGuideData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(23);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.GuideType);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GuideImage, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MoveTaskType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.MoveTaskValue, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SummaryKEY, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer3, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer4, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question5, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer5, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question6, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer6, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question7, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer7, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Question8, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Answer8, options);
	}

	public GameGuideData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GameGuideData gameGuideData = new GameGuideData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gameGuideData.Index = reader.ReadInt32();
				break;
			case 1:
				gameGuideData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				gameGuideData.GuideType = reader.ReadInt32();
				break;
			case 3:
				gameGuideData.GuideImage = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				gameGuideData.MoveTaskType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				gameGuideData.MoveTaskValue = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				gameGuideData.SummaryKEY = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				gameGuideData.Question1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				gameGuideData.Answer1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				gameGuideData.Question2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				gameGuideData.Answer2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				gameGuideData.Question3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				gameGuideData.Answer3 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				gameGuideData.Question4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				gameGuideData.Answer4 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 15:
				gameGuideData.Question5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				gameGuideData.Answer5 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				gameGuideData.Question6 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				gameGuideData.Answer6 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 19:
				gameGuideData.Question7 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 20:
				gameGuideData.Answer7 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 21:
				gameGuideData.Question8 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 22:
				gameGuideData.Answer8 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gameGuideData;
	}
}
