using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipGachaClientDataFormatter : IMessagePackFormatter<EquipGachaClientData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipGachaClientData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(3);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<HighGachaCategoryType>().Serialize(ref writer, value.HighGachaCategoryType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.UniqueWeaponHeroIndex, options);
	}

	public EquipGachaClientData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipGachaClientData equipGachaClientData = new EquipGachaClientData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipGachaClientData.Index = reader.ReadInt32();
				break;
			case 1:
				equipGachaClientData.HighGachaCategoryType = resolver.GetFormatterWithVerify<HighGachaCategoryType>().Deserialize(ref reader, options);
				break;
			case 2:
				equipGachaClientData.UniqueWeaponHeroIndex = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipGachaClientData;
	}
}
