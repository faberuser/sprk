using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureLevelDataFormatter : IMessagePackFormatter<CreatureLevelData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureLevelData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(3);
		writer.Write(value.Level);
		writer.Write(value.LocalExp);
		writer.Write(value.TotalExp);
	}

	public CreatureLevelData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureLevelData creatureLevelData = new CreatureLevelData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureLevelData.Level = reader.ReadInt32();
				break;
			case 1:
				creatureLevelData.LocalExp = reader.ReadInt64();
				break;
			case 2:
				creatureLevelData.TotalExp = reader.ReadInt64();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureLevelData;
	}
}
