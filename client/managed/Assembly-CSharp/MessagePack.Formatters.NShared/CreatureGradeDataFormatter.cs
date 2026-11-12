using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureGradeDataFormatter : IMessagePackFormatter<CreatureGradeData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureGradeData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.Grade);
		writer.Write(value.Level);
		writer.Write(value.Gold);
		writer.Write(value.Stone);
	}

	public CreatureGradeData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureGradeData creatureGradeData = new CreatureGradeData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureGradeData.Grade = reader.ReadInt32();
				break;
			case 1:
				creatureGradeData.Level = reader.ReadInt32();
				break;
			case 2:
				creatureGradeData.Gold = reader.ReadInt32();
				break;
			case 3:
				creatureGradeData.Stone = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureGradeData;
	}
}
