using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipOptionChangeDataFormatter : IMessagePackFormatter<EquipOptionChangeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipOptionChangeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.OptionIndex);
		writer.Write(value.BeforeTier);
		writer.Write(value.AfterTier);
		writer.Write(value.ResultOptionIndex);
	}

	public EquipOptionChangeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipOptionChangeData equipOptionChangeData = new EquipOptionChangeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipOptionChangeData.OptionIndex = reader.ReadInt32();
				break;
			case 1:
				equipOptionChangeData.BeforeTier = reader.ReadInt32();
				break;
			case 2:
				equipOptionChangeData.AfterTier = reader.ReadInt32();
				break;
			case 3:
				equipOptionChangeData.ResultOptionIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipOptionChangeData;
	}
}
