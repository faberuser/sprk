using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemStarDataFormatter : IMessagePackFormatter<EquipItemStarData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemStarData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(5);
		writer.Write(value.Star);
		writer.Write(value.RuneSlotOpen);
		writer.Write(value.BuyPriceFactor);
		writer.Write(value.SellPriceFactor);
		writer.Write(value.StatFactor);
	}

	public EquipItemStarData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipItemStarData equipItemStarData = new EquipItemStarData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipItemStarData.Star = reader.ReadInt32();
				break;
			case 1:
				equipItemStarData.RuneSlotOpen = reader.ReadInt32();
				break;
			case 2:
				equipItemStarData.BuyPriceFactor = reader.ReadInt32();
				break;
			case 3:
				equipItemStarData.SellPriceFactor = reader.ReadInt32();
				break;
			case 4:
				equipItemStarData.StatFactor = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipItemStarData;
	}
}
