using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipUpgradeExpDataFormatter : IMessagePackFormatter<EquipUpgradeExpData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipUpgradeExpData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		resolver.GetFormatterWithVerify<EquipPartType>().Serialize(ref writer, value.PartType, options);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		writer.Write(value.Level);
		writer.Write(value.ReqExp);
		writer.Write(value.GiveExp);
	}

	public EquipUpgradeExpData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		EquipUpgradeExpData equipUpgradeExpData = new EquipUpgradeExpData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipUpgradeExpData.PartType = resolver.GetFormatterWithVerify<EquipPartType>().Deserialize(ref reader, options);
				break;
			case 1:
				equipUpgradeExpData.Tier = reader.ReadInt16();
				break;
			case 2:
				equipUpgradeExpData.DetailIndex = reader.ReadInt32();
				break;
			case 3:
				equipUpgradeExpData.Level = reader.ReadInt16();
				break;
			case 4:
				equipUpgradeExpData.ReqExp = reader.ReadInt32();
				break;
			case 5:
				equipUpgradeExpData.GiveExp = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipUpgradeExpData;
	}
}
