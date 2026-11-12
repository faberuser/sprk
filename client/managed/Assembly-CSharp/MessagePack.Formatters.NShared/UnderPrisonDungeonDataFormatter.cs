using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class UnderPrisonDungeonDataFormatter : IMessagePackFormatter<UnderPrisonDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, UnderPrisonDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Diff);
	}

	public UnderPrisonDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		UnderPrisonDungeonData underPrisonDungeonData = new UnderPrisonDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				underPrisonDungeonData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				underPrisonDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				underPrisonDungeonData.Diff = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return underPrisonDungeonData;
	}
}
