using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GameMiniGuideDataFormatter : IMessagePackFormatter<GameMiniGuideData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GameMiniGuideData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.GameGuideIndex, options);
	}

	public GameMiniGuideData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GameMiniGuideData gameMiniGuideData = new GameMiniGuideData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gameMiniGuideData.Index = reader.ReadInt32();
				break;
			case 1:
				gameMiniGuideData.GameGuideIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gameMiniGuideData;
	}
}
