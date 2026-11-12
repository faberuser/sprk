using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaRewardExponentialKeyFormatter : IMessagePackFormatter<KarmaRewardExponentialKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaRewardExponentialKey value, MessagePackSerializerOptions options)
	{
		writer.WriteArrayHeader(2);
		writer.Write(value.PunishmentGroupIndex);
		writer.Write(value.Level);
	}

	public KarmaRewardExponentialKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		int punishmentGroupIndex = 0;
		int level = 0;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentGroupIndex = reader.ReadInt32();
				break;
			case 1:
				level = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		KarmaRewardExponentialKey result = new KarmaRewardExponentialKey(punishmentGroupIndex, level);
		reader.Depth--;
		return result;
	}
}
