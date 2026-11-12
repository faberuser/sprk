using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameTitleSceneIntroduceDataFormatter : IMessagePackFormatter<GameTitleSceneIntroduceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameTitleSceneIntroduceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(14);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<GameIntroduceType>().Serialize(ref writer, value.Type, options);
		writer.Write(value.Show);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroTitleStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroNameStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroDescStringKey1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroDescStringKey2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPopupImage1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.HeroPopupImage2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldTitleStringKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldDescStringKey1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldDescStringKey2, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldPopupImage1, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WorldPopupImage2, options);
	}

	public GameTitleSceneIntroduceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GameTitleSceneIntroduceData gameTitleSceneIntroduceData = new GameTitleSceneIntroduceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gameTitleSceneIntroduceData.Index = reader.ReadInt32();
				break;
			case 1:
				gameTitleSceneIntroduceData.Type = resolver.GetFormatterWithVerify<GameIntroduceType>().Deserialize(ref reader, options);
				break;
			case 2:
				gameTitleSceneIntroduceData.Show = reader.ReadBoolean();
				break;
			case 3:
				gameTitleSceneIntroduceData.HeroTitleStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				gameTitleSceneIntroduceData.HeroNameStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				gameTitleSceneIntroduceData.HeroDescStringKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				gameTitleSceneIntroduceData.HeroDescStringKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				gameTitleSceneIntroduceData.HeroPopupImage1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				gameTitleSceneIntroduceData.HeroPopupImage2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				gameTitleSceneIntroduceData.WorldTitleStringKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				gameTitleSceneIntroduceData.WorldDescStringKey1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				gameTitleSceneIntroduceData.WorldDescStringKey2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				gameTitleSceneIntroduceData.WorldPopupImage1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				gameTitleSceneIntroduceData.WorldPopupImage2 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gameTitleSceneIntroduceData;
	}
}
