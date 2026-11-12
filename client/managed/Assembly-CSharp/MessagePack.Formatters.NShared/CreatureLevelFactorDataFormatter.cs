using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureLevelFactorDataFormatter : IMessagePackFormatter<CreatureLevelFactorData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureLevelFactorData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.FactorIndex);
		writer.Write(value.Level);
		writer.Write(value.Value1);
		writer.Write(value.Value2);
	}

	public CreatureLevelFactorData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureLevelFactorData creatureLevelFactorData = new CreatureLevelFactorData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureLevelFactorData.FactorIndex = reader.ReadInt32();
				break;
			case 1:
				creatureLevelFactorData.Level = reader.ReadInt16();
				break;
			case 2:
				creatureLevelFactorData.Value1 = reader.ReadInt64();
				break;
			case 3:
				creatureLevelFactorData.Value2 = reader.ReadInt64();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureLevelFactorData;
	}
}
