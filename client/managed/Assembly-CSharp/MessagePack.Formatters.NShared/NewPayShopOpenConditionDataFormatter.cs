using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class NewPayShopOpenConditionDataFormatter : IMessagePackFormatter<NewPayShopOpenConditionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, NewPayShopOpenConditionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		resolver.GetFormatterWithVerify<PayShopUserGroup>().Serialize(ref writer, value.Group, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LinkCategory, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.LinkProductIndices, options);
	}

	public NewPayShopOpenConditionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		NewPayShopOpenConditionData newPayShopOpenConditionData = new NewPayShopOpenConditionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				newPayShopOpenConditionData.Group = resolver.GetFormatterWithVerify<PayShopUserGroup>().Deserialize(ref reader, options);
				break;
			case 1:
				newPayShopOpenConditionData.Condition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 2:
				newPayShopOpenConditionData.LinkCategory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				newPayShopOpenConditionData.LinkProductIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return newPayShopOpenConditionData;
	}
}
