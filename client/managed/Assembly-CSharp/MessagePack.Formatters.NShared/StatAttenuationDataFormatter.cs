using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class StatAttenuationDataFormatter : IMessagePackFormatter<StatAttenuationData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, StatAttenuationData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		resolver.GetFormatterWithVerify<StatType>().Serialize(ref writer, value.StatType, options);
		writer.Write(value.MaxK);
		writer.Write(value.X1);
		writer.Write(value.A1);
		writer.Write(value.B1);
		writer.Write(value.X2);
		writer.Write(value.A2);
		writer.Write(value.B2);
		writer.Write(value.MinK);
		writer.Write(value.X3);
		writer.Write(value.A3);
		writer.Write(value.B3);
		writer.Write(value.X4);
		writer.Write(value.A4);
		writer.Write(value.B4);
	}

	public StatAttenuationData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		StatAttenuationData statAttenuationData = new StatAttenuationData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				statAttenuationData.StatType = resolver.GetFormatterWithVerify<StatType>().Deserialize(ref reader, options);
				break;
			case 1:
				statAttenuationData.MaxK = reader.ReadInt32();
				break;
			case 2:
				statAttenuationData.X1 = reader.ReadInt32();
				break;
			case 3:
				statAttenuationData.A1 = reader.ReadInt32();
				break;
			case 4:
				statAttenuationData.B1 = reader.ReadInt32();
				break;
			case 5:
				statAttenuationData.X2 = reader.ReadInt32();
				break;
			case 6:
				statAttenuationData.A2 = reader.ReadInt32();
				break;
			case 7:
				statAttenuationData.B2 = reader.ReadInt32();
				break;
			case 8:
				statAttenuationData.MinK = reader.ReadInt32();
				break;
			case 9:
				statAttenuationData.X3 = reader.ReadInt32();
				break;
			case 10:
				statAttenuationData.A3 = reader.ReadInt32();
				break;
			case 11:
				statAttenuationData.B3 = reader.ReadInt32();
				break;
			case 12:
				statAttenuationData.X4 = reader.ReadInt32();
				break;
			case 13:
				statAttenuationData.A4 = reader.ReadInt32();
				break;
			case 14:
				statAttenuationData.B4 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return statAttenuationData;
	}
}
