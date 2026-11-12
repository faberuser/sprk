using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CostumeGroupDataFormatter : IMessagePackFormatter<CostumeGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CostumeGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.CostumeGroupIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.GroupName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TexturePath, options);
		writer.WriteNil();
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.SelectItemGroup, options);
		resolver.GetFormatterWithVerify<EHeroCostumeType>().Serialize(ref writer, value.CostumeType, options);
	}

	public CostumeGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		CostumeGroupData costumeGroupData = new CostumeGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				costumeGroupData.CostumeGroupIndex = reader.ReadInt32();
				break;
			case 1:
				costumeGroupData.GroupName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				costumeGroupData.TexturePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				costumeGroupData.SelectItemGroup = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				costumeGroupData.CostumeType = resolver.GetFormatterWithVerify<EHeroCostumeType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return costumeGroupData;
	}
}
