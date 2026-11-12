using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ValanceIdentifiedDataFormatter : IMessagePackFormatter<ValanceIdentifiedData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ValanceIdentifiedData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(2);
		writer.Write(value.DetailIndex);
		writer.Write(value.ReqGold);
	}

	public ValanceIdentifiedData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		ValanceIdentifiedData valanceIdentifiedData = new ValanceIdentifiedData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				valanceIdentifiedData.DetailIndex = reader.ReadInt32();
				break;
			case 1:
				valanceIdentifiedData.ReqGold = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return valanceIdentifiedData;
	}
}
