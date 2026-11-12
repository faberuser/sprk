using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassExpDataFormatter : IMessagePackFormatter<KingsPassExpData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassExpData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.ExpIndex);
		resolver.GetFormatterWithVerify<StaminaType>().Serialize(ref writer, value.StaminaType, options);
		writer.Write(value.Multiple);
	}

	public KingsPassExpData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		KingsPassExpData kingsPassExpData = new KingsPassExpData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				kingsPassExpData.ExpIndex = reader.ReadInt32();
				break;
			case 1:
				kingsPassExpData.StaminaType = resolver.GetFormatterWithVerify<StaminaType>().Deserialize(ref reader, options);
				break;
			case 2:
				kingsPassExpData.Multiple = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return kingsPassExpData;
	}
}
