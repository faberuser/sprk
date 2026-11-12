using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipOptionUpgradeDataFormatter : IMessagePackFormatter<EquipOptionUpgradeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipOptionUpgradeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.Tier);
		writer.Write(value.DetailIndex);
		writer.Write(value.ReqGold);
		writer.Write(value.MaterialItemIndex);
		writer.Write(value.MaterialItemCount);
	}

	public EquipOptionUpgradeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipOptionUpgradeData equipOptionUpgradeData = new EquipOptionUpgradeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipOptionUpgradeData.Tier = reader.ReadInt16();
				break;
			case 1:
				equipOptionUpgradeData.DetailIndex = reader.ReadInt32();
				break;
			case 2:
				equipOptionUpgradeData.ReqGold = reader.ReadInt32();
				break;
			case 3:
				equipOptionUpgradeData.MaterialItemIndex = reader.ReadInt32();
				break;
			case 4:
				equipOptionUpgradeData.MaterialItemCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipOptionUpgradeData;
	}
}
