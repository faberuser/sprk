using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceEnchantOptionCountDataFormatter : IMessagePackFormatter<ValanceEnchantOptionCountData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceEnchantOptionCountData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		writer.Write(value.Rate);
		writer.Write(value.OptionCount);
	}

	public ValanceEnchantOptionCountData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceEnchantOptionCountData valanceEnchantOptionCountData = new ValanceEnchantOptionCountData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceEnchantOptionCountData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				valanceEnchantOptionCountData.Rate = reader.ReadInt32();
				break;
			case 2:
				valanceEnchantOptionCountData.OptionCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceEnchantOptionCountData;
	}
}
