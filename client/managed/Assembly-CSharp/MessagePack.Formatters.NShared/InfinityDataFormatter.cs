using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class InfinityDataFormatter : IMessagePackFormatter<InfinityData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, InfinityData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.Index);
		writer.Write(value.StartValue);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.IncreaseValues, options);
		writer.Write(value.Multiplier);
	}

	public InfinityData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		InfinityData infinityData = new InfinityData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				infinityData.Index = reader.ReadInt32();
				break;
			case 1:
				infinityData.StartValue = reader.ReadInt32();
				break;
			case 2:
				infinityData.IncreaseValues = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 3:
				infinityData.Multiplier = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return infinityData;
	}
}
