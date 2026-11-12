using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PunishmentRaidDataFormatter : IMessagePackFormatter<PunishmentRaidData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PunishmentRaidData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(19);
		writer.Write(value.RaidIndex);
		writer.Write(value.RaidLevel);
		writer.Write(value.GroupIndex);
		writer.Write(value.Boss);
		writer.Write(value.EntryPositionX);
		writer.Write(value.EntryPositionY);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryEffectCode, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.TriggerIndices, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.EntryLocalKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ImagePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LabelColor, options);
		writer.Write(value.PortraitPositionX);
		writer.Write(value.PortraitPositionY);
		resolver.GetFormatterWithVerify<EPunishmentPortraitLineType>().Serialize(ref writer, value.PortraitLineType, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.CanEnterCondition, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.WaveGimmickValues, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossMiniPortraitPath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.WarningLocalKey, options);
		writer.Write(value.UsePunishmentRaidBossStatus);
	}

	public PunishmentRaidData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PunishmentRaidData punishmentRaidData = new PunishmentRaidData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				punishmentRaidData.RaidIndex = reader.ReadInt32();
				break;
			case 1:
				punishmentRaidData.RaidLevel = reader.ReadInt32();
				break;
			case 2:
				punishmentRaidData.GroupIndex = reader.ReadInt32();
				break;
			case 3:
				punishmentRaidData.Boss = reader.ReadByte();
				break;
			case 4:
				punishmentRaidData.EntryPositionX = reader.ReadInt32();
				break;
			case 5:
				punishmentRaidData.EntryPositionY = reader.ReadInt32();
				break;
			case 6:
				punishmentRaidData.EntryEffectCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				punishmentRaidData.TriggerIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 8:
				punishmentRaidData.EntryLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				punishmentRaidData.ImagePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				punishmentRaidData.LabelColor = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				punishmentRaidData.PortraitPositionX = reader.ReadInt32();
				break;
			case 12:
				punishmentRaidData.PortraitPositionY = reader.ReadInt32();
				break;
			case 13:
				punishmentRaidData.PortraitLineType = resolver.GetFormatterWithVerify<EPunishmentPortraitLineType>().Deserialize(ref reader, options);
				break;
			case 14:
				punishmentRaidData.CanEnterCondition = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 15:
				punishmentRaidData.WaveGimmickValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 16:
				punishmentRaidData.BossMiniPortraitPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 17:
				punishmentRaidData.WarningLocalKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 18:
				punishmentRaidData.UsePunishmentRaidBossStatus = reader.ReadBoolean();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return punishmentRaidData;
	}
}
