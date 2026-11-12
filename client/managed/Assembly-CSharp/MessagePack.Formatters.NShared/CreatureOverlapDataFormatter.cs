using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureOverlapDataFormatter : IMessagePackFormatter<CreatureOverlapData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureOverlapData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		writer.Write(value.OverlapRewardIndex);
	}

	public CreatureOverlapData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureOverlapData creatureOverlapData = new CreatureOverlapData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureOverlapData.Index = reader.ReadInt32();
				break;
			case 1:
				creatureOverlapData.OverlapRewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureOverlapData;
	}
}
