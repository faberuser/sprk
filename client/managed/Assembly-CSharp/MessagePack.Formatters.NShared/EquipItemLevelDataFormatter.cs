using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipItemLevelDataFormatter : IMessagePackFormatter<EquipItemLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipItemLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.Level);
		writer.Write(value.OptionOpen);
	}

	public EquipItemLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipItemLevelData equipItemLevelData = new EquipItemLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipItemLevelData.Level = reader.ReadInt32();
				break;
			case 1:
				equipItemLevelData.OptionOpen = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipItemLevelData;
	}
}
