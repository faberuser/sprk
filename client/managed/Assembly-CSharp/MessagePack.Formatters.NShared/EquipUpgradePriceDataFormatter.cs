using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipUpgradePriceDataFormatter : IMessagePackFormatter<EquipUpgradePriceData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipUpgradePriceData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		writer.Write(value.Level);
		writer.Write(value.ReqGold);
	}

	public EquipUpgradePriceData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipUpgradePriceData equipUpgradePriceData = new EquipUpgradePriceData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipUpgradePriceData.Tier = reader.ReadInt16();
				break;
			case 1:
				equipUpgradePriceData.DetailIndex = reader.ReadInt32();
				break;
			case 2:
				equipUpgradePriceData.Level = reader.ReadInt16();
				break;
			case 3:
				equipUpgradePriceData.ReqGold = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipUpgradePriceData;
	}
}
