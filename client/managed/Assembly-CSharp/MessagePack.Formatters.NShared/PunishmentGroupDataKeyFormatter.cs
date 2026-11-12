using System;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentGroupDataKeyFormatter : IMessagePackFormatter<PunishmentGroupDataKey>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentGroupDataKey value, MessagePackSerializerOptions options)
	{
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.GroupIndex);
		resolver.GetFormatterWithVerify<EPunishmentDungeonType>().Serialize(ref writer, value.DungeonType, options);
	}

	public PunishmentGroupDataKey Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			throw new InvalidOperationException("typecode is null, struct not supported");
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		int groupIndex = 0;
		EPunishmentDungeonType dungeonType = EPunishmentDungeonType.Scenario;
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				groupIndex = reader.ReadInt32();
				break;
			case 1:
				dungeonType = resolver.GetFormatterWithVerify<EPunishmentDungeonType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		PunishmentGroupDataKey result = new PunishmentGroupDataKey(groupIndex, dungeonType);
		reader.Depth--;
		return result;
	}
}
