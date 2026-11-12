using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipOptionGroupDataFormatter : IMessagePackFormatter<EquipOptionGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipOptionGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OptionIndex, options);
	}

	public EquipOptionGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipOptionGroupData equipOptionGroupData = new EquipOptionGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipOptionGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				equipOptionGroupData.OptionIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipOptionGroupData;
	}
}
