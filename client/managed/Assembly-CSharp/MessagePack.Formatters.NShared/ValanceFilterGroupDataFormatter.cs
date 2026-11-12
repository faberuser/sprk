using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceFilterGroupDataFormatter : IMessagePackFormatter<ValanceFilterGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceFilterGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(5);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<EValanceFilterGroupType>().Serialize(ref writer, value.GroupType, options);
		writer.Write(value.SortingOrder);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
		writer.Write(value.IsLongComponent);
	}

	public ValanceFilterGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceFilterGroupData valanceFilterGroupData = new ValanceFilterGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceFilterGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				valanceFilterGroupData.GroupType = resolver.GetFormatterWithVerify<EValanceFilterGroupType>().Deserialize(ref reader, options);
				break;
			case 2:
				valanceFilterGroupData.SortingOrder = reader.ReadInt32();
				break;
			case 3:
				valanceFilterGroupData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				valanceFilterGroupData.IsLongComponent = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceFilterGroupData;
	}
}
