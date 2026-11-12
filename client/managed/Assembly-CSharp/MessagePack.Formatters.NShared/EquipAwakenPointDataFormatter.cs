using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EquipAwakenPointDataFormatter : IMessagePackFormatter<EquipAwakenPointData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EquipAwakenPointData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.Star);
		writer.Write(value.AwakenPoint);
	}

	public EquipAwakenPointData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EquipAwakenPointData equipAwakenPointData = new EquipAwakenPointData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				equipAwakenPointData.Star = reader.ReadInt32();
				break;
			case 1:
				equipAwakenPointData.AwakenPoint = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return equipAwakenPointData;
	}
}
