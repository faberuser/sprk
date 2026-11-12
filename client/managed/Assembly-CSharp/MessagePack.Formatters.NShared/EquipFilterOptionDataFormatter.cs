using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipFilterOptionDataFormatter : IMessagePackFormatter<EquipFilterOptionData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipFilterOptionData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(10);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Key, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.Group, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReferenceValue, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.RedirectionValues, options);
		writer.Write(value.IsDefault);
		writer.Write(value.ChangeOnOpenInHeroEquip);
		writer.Write(value.Step);
		writer.Write(value.Show);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalKey, options);
	}

	public EquipFilterOptionData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipFilterOptionData equipFilterOptionData = new EquipFilterOptionData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipFilterOptionData.Index = reader.ReadInt32();
				break;
			case 1:
				equipFilterOptionData.Key = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				equipFilterOptionData.Group = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				equipFilterOptionData.ReferenceValue = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				equipFilterOptionData.RedirectionValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				equipFilterOptionData.IsDefault = reader.ReadBoolean();
				break;
			case 6:
				equipFilterOptionData.ChangeOnOpenInHeroEquip = reader.ReadBoolean();
				break;
			case 7:
				equipFilterOptionData.Step = reader.ReadInt32();
				break;
			case 8:
				equipFilterOptionData.Show = reader.ReadBoolean();
				break;
			case 9:
				equipFilterOptionData.LocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipFilterOptionData;
	}
}
