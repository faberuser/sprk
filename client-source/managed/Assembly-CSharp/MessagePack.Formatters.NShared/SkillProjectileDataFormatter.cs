using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillProjectileDataFormatter : IMessagePackFormatter<SkillProjectileData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillProjectileData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.Index);
		writer.Write(value.SkillIndex);
		writer.Write(value.DurationMs);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TickIntervalMs, options);
		writer.Write(value.RadiusMm);
		writer.Write(value.Width);
		writer.Write(value.SpeedMms);
		writer.Write(value.FollowTarget);
	}

	public SkillProjectileData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SkillProjectileData skillProjectileData = new SkillProjectileData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				skillProjectileData.Index = reader.ReadInt32();
				break;
			case 1:
				skillProjectileData.SkillIndex = reader.ReadInt32();
				break;
			case 2:
				skillProjectileData.DurationMs = reader.ReadInt32();
				break;
			case 3:
				skillProjectileData.TickIntervalMs = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				skillProjectileData.RadiusMm = reader.ReadInt32();
				break;
			case 5:
				skillProjectileData.Width = reader.ReadInt32();
				break;
			case 6:
				skillProjectileData.SpeedMms = reader.ReadInt32();
				break;
			case 7:
				skillProjectileData.FollowTarget = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return skillProjectileData;
	}
}
