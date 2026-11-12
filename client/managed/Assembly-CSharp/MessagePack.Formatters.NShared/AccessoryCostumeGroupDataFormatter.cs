using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class AccessoryCostumeGroupDataFormatter : IMessagePackFormatter<AccessoryCostumeGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, AccessoryCostumeGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.CostumeGroupIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CategoryName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath, options);
	}

	public AccessoryCostumeGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		AccessoryCostumeGroupData accessoryCostumeGroupData = new AccessoryCostumeGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				accessoryCostumeGroupData.CostumeGroupIndex = reader.ReadInt32();
				break;
			case 1:
				accessoryCostumeGroupData.CategoryName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				accessoryCostumeGroupData.TexturePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return accessoryCostumeGroupData;
	}
}
