using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KingsPassMissionKey2Formatter : IMessagePackFormatter<KingsPassMissionKey2>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KingsPassMissionKey2 value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.SeasonIndex);
		writer.Write(value.MissionIndex);
	}

	public KingsPassMissionKey2 Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int seasonIndex = 0;
		int missionIndex = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				seasonIndex = reader.ReadInt32();
				break;
			case 1:
				missionIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KingsPassMissionKey2 result = new KingsPassMissionKey2(seasonIndex, missionIndex);
		reader.Depth--;
		return result;
	}
}
