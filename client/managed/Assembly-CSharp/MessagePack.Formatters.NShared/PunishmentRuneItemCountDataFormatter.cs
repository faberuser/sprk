using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRuneItemCountDataFormatter : IMessagePackFormatter<PunishmentRuneItemCountData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRuneItemCountData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.PunishmentRuneCount);
		writer.Write(value.RuneFragmentCount);
		writer.Write(value.ApostleBloodCount);
	}

	public PunishmentRuneItemCountData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		PunishmentRuneItemCountData punishmentRuneItemCountData = new PunishmentRuneItemCountData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRuneItemCountData.PunishmentRuneCount = reader.ReadInt32();
				break;
			case 1:
				punishmentRuneItemCountData.RuneFragmentCount = reader.ReadInt32();
				break;
			case 2:
				punishmentRuneItemCountData.ApostleBloodCount = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRuneItemCountData;
	}
}
