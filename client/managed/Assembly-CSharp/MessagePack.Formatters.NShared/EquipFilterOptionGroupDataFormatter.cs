using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipFilterOptionGroupDataFormatter : IMessagePackFormatter<EquipFilterOptionGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipFilterOptionGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Key, options);
		writer.Write(value.Step);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
		writer.Write(value.Show);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.FieldTypeName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.FieldName, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DataTypeName, options);
	}

	public EquipFilterOptionGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipFilterOptionGroupData equipFilterOptionGroupData = new EquipFilterOptionGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipFilterOptionGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				equipFilterOptionGroupData.Key = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				equipFilterOptionGroupData.Step = reader.ReadInt32();
				break;
			case 3:
				equipFilterOptionGroupData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				equipFilterOptionGroupData.Show = reader.ReadBoolean();
				break;
			case 5:
				equipFilterOptionGroupData.FieldTypeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				equipFilterOptionGroupData.FieldName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				equipFilterOptionGroupData.DataTypeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipFilterOptionGroupData;
	}
}
