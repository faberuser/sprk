using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaEventDataFormatter : IMessagePackFormatter<OrdealArenaEventData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaEventData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EOrdealEventType>().Serialize(ref writer, value.EventType, options);
		writer.Write(value.Condition);
		writer.Write(value.Rate);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.DescKeys, options);
		writer.Write(value.Volume);
	}

	public OrdealArenaEventData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OrdealArenaEventData ordealArenaEventData = new OrdealArenaEventData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ordealArenaEventData.Index = reader.ReadInt32();
				break;
			case 1:
				ordealArenaEventData.EventType = resolver.GetFormatterWithVerify<EOrdealEventType>().Deserialize(ref reader, options);
				break;
			case 2:
				ordealArenaEventData.Condition = reader.ReadInt32();
				break;
			case 3:
				ordealArenaEventData.Rate = reader.ReadInt32();
				break;
			case 4:
				ordealArenaEventData.TexturePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				ordealArenaEventData.DescKeys = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 6:
				ordealArenaEventData.Volume = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return ordealArenaEventData;
	}
}
