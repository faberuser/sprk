using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PetCommonOptionOrderDataFormatter : IMessagePackFormatter<PetCommonOptionOrderData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PetCommonOptionOrderData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		resolver.GetFormatterWithVerify<OperationType>().Serialize(ref writer, value.OptionType, options);
		writer.Write(value.Priority);
	}

	public PetCommonOptionOrderData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PetCommonOptionOrderData petCommonOptionOrderData = new PetCommonOptionOrderData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				petCommonOptionOrderData.OptionType = resolver.GetFormatterWithVerify<OperationType>().Deserialize(ref reader, options);
				break;
			case 1:
				petCommonOptionOrderData.Priority = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return petCommonOptionOrderData;
	}
}
