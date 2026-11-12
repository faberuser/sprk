using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceCreatureFilterDataFormatter : IMessagePackFormatter<ValanceCreatureFilterData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceCreatureFilterData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.SortingOrder);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
		resolver.GetFormatterWithVerify<SkillAttrType>().Serialize(ref writer, value.AttrType, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.CreatureTagType, options);
	}

	public ValanceCreatureFilterData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ValanceCreatureFilterData valanceCreatureFilterData = new ValanceCreatureFilterData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceCreatureFilterData.Index = reader.ReadInt32();
				break;
			case 1:
				valanceCreatureFilterData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				valanceCreatureFilterData.SortingOrder = reader.ReadInt32();
				break;
			case 3:
				valanceCreatureFilterData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				valanceCreatureFilterData.AttrType = resolver.GetFormatterWithVerify<SkillAttrType>().Deserialize(ref reader, options);
				break;
			case 5:
				valanceCreatureFilterData.CreatureTagType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceCreatureFilterData;
	}
}
