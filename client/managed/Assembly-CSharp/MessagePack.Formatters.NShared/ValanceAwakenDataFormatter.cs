using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceAwakenDataFormatter : IMessagePackFormatter<ValanceAwakenData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceAwakenData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
		writer.Write(value.Star);
		writer.Write(value.ReqGold);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode1, options);
		writer.Write(value.ItemCount1);
	}

	public ValanceAwakenData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceAwakenData valanceAwakenData = new ValanceAwakenData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceAwakenData.Tier = reader.ReadByte();
				break;
			case 1:
				valanceAwakenData.DetailIndex = reader.ReadInt32();
				break;
			case 2:
				valanceAwakenData.CreatureTagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			case 3:
				valanceAwakenData.Star = reader.ReadInt32();
				break;
			case 4:
				valanceAwakenData.ReqGold = reader.ReadInt32();
				break;
			case 5:
				valanceAwakenData.ItemCode1 = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				valanceAwakenData.ItemCount1 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceAwakenData;
	}
}
