using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class OrdealArenaBuffDataFormatter : IMessagePackFormatter<OrdealArenaBuffData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, OrdealArenaBuffData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Name, options);
		writer.Write(value.Grade);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		writer.Write(value.OptionValue);
		writer.Write(value.Rate);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
	}

	public OrdealArenaBuffData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		OrdealArenaBuffData ordealArenaBuffData = new OrdealArenaBuffData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				ordealArenaBuffData.Index = reader.ReadInt32();
				break;
			case 1:
				ordealArenaBuffData.Name = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				ordealArenaBuffData.Grade = reader.ReadInt32();
				break;
			case 3:
				ordealArenaBuffData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 4:
				ordealArenaBuffData.OptionValue = reader.ReadInt32();
				break;
			case 5:
				ordealArenaBuffData.Rate = reader.ReadInt32();
				break;
			case 6:
				ordealArenaBuffData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return ordealArenaBuffData;
	}
}
