using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class EclipseStartDataFormatter : IMessagePackFormatter<EclipseStartData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, EclipseStartData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.RecordWave);
		writer.Write(value.StartWave);
	}

	public EclipseStartData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		EclipseStartData eclipseStartData = new EclipseStartData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				eclipseStartData.RecordWave = reader.ReadInt32();
				break;
			case 1:
				eclipseStartData.StartWave = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return eclipseStartData;
	}
}
