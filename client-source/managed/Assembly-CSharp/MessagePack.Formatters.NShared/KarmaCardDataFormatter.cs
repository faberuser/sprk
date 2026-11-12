using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class KarmaCardDataFormatter : IMessagePackFormatter<KarmaCardData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, KarmaCardData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(18);
		writer.Write(value.Index);
		writer.Write(value.GroupIndex);
		writer.Write(value.GroupChance);
		writer.Write(value.UniqueChance);
		resolver.GetFormatterWithVerify<EKarmaCardType>().Serialize(ref writer, value.CardType, options);
		resolver.GetFormatterWithVerify<EKarmaRarity>().Serialize(ref writer, value.Rarity, options);
		writer.Write(value.MaxStack);
		writer.Write(value.AppearChance);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SkillOptions, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffIconPath, options);
		writer.Write(value.BuffValue);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.RarityDesc, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CardDescCode, options);
		resolver.GetFormatterWithVerify<CreatureTagType[]>().Serialize(ref writer, value.ClassTypes, options);
		writer.Write(value.ShowIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.IconPath, options);
		writer.Write(value.BuffIconIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BuffValueUnit, options);
	}

	public KarmaCardData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		KarmaCardData karmaCardData = new KarmaCardData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				karmaCardData.Index = reader.ReadInt32();
				break;
			case 1:
				karmaCardData.GroupIndex = reader.ReadInt32();
				break;
			case 2:
				karmaCardData.GroupChance = reader.ReadInt32();
				break;
			case 3:
				karmaCardData.UniqueChance = reader.ReadInt32();
				break;
			case 4:
				karmaCardData.CardType = resolver.GetFormatterWithVerify<EKarmaCardType>().Deserialize(ref reader, options);
				break;
			case 5:
				karmaCardData.Rarity = resolver.GetFormatterWithVerify<EKarmaRarity>().Deserialize(ref reader, options);
				break;
			case 6:
				karmaCardData.MaxStack = reader.ReadInt32();
				break;
			case 7:
				karmaCardData.AppearChance = reader.ReadInt32();
				break;
			case 8:
				karmaCardData.SkillOptions = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 9:
				karmaCardData.BuffIconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				karmaCardData.BuffValue = reader.ReadInt32();
				break;
			case 11:
				karmaCardData.RarityDesc = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				karmaCardData.CardDescCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				karmaCardData.ClassTypes = resolver.GetFormatterWithVerify<CreatureTagType[]>().Deserialize(ref reader, options);
				break;
			case 14:
				karmaCardData.ShowIndex = reader.ReadInt32();
				break;
			case 15:
				karmaCardData.IconPath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 16:
				karmaCardData.BuffIconIndex = reader.ReadInt32();
				break;
			case 17:
				karmaCardData.BuffValueUnit = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return karmaCardData;
	}
}
