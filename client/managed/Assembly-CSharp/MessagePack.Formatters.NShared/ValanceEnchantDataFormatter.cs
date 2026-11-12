using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceEnchantDataFormatter : IMessagePackFormatter<ValanceEnchantData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceEnchantData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PartType, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SubType, options);
		writer.Write(value.ReqGold);
		writer.Write(value.OptionGroupIndex);
	}

	public ValanceEnchantData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceEnchantData valanceEnchantData = new ValanceEnchantData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceEnchantData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				valanceEnchantData.PartType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				valanceEnchantData.SubType = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				valanceEnchantData.ReqGold = reader.ReadInt32();
				break;
			case 4:
				valanceEnchantData.OptionGroupIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceEnchantData;
	}
}
