using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CreatureStarDataFormatter : IMessagePackFormatter<CreatureStarData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CreatureStarData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(19);
		writer.Write(value.Star);
		writer.Write(value.Transcended);
		writer.Write(value.GetHeroTeamExp);
		writer.Write(value.AwakeHeroTeamExp);
		writer.Write(value.MaxHeroLevel);
		writer.Write(value.HeroAwakeItemCount);
		writer.Write(value.HeroTranscendItemIndex);
		writer.Write(value.HeroTranscendItemCount);
		writer.Write(value.MaxPurified);
		writer.Write(value.Gold);
		writer.Write(value.PurifyAmount1);
		writer.Write(value.Rate1);
		writer.Write(value.PurifyAmount2);
		writer.Write(value.Rate2);
		writer.Write(value.PurifyAmount3);
		writer.Write(value.Rate3);
		writer.Write(value.ExchangeItemCount);
		writer.Write(value.ExchangeTranscendedItemCount);
		writer.Write(value.AllOwnedRewardIndex);
	}

	public CreatureStarData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CreatureStarData creatureStarData = new CreatureStarData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				creatureStarData.Star = reader.ReadInt32();
				break;
			case 1:
				creatureStarData.Transcended = reader.ReadInt32();
				break;
			case 2:
				creatureStarData.GetHeroTeamExp = reader.ReadInt32();
				break;
			case 3:
				creatureStarData.AwakeHeroTeamExp = reader.ReadInt32();
				break;
			case 4:
				creatureStarData.MaxHeroLevel = reader.ReadInt32();
				break;
			case 5:
				creatureStarData.HeroAwakeItemCount = reader.ReadInt32();
				break;
			case 6:
				creatureStarData.HeroTranscendItemIndex = reader.ReadInt32();
				break;
			case 7:
				creatureStarData.HeroTranscendItemCount = reader.ReadInt32();
				break;
			case 8:
				creatureStarData.MaxPurified = reader.ReadInt32();
				break;
			case 9:
				creatureStarData.Gold = reader.ReadInt32();
				break;
			case 10:
				creatureStarData.PurifyAmount1 = reader.ReadInt32();
				break;
			case 11:
				creatureStarData.Rate1 = reader.ReadInt32();
				break;
			case 12:
				creatureStarData.PurifyAmount2 = reader.ReadInt32();
				break;
			case 13:
				creatureStarData.Rate2 = reader.ReadInt32();
				break;
			case 14:
				creatureStarData.PurifyAmount3 = reader.ReadInt32();
				break;
			case 15:
				creatureStarData.Rate3 = reader.ReadInt32();
				break;
			case 16:
				creatureStarData.ExchangeItemCount = reader.ReadInt32();
				break;
			case 17:
				creatureStarData.ExchangeTranscendedItemCount = reader.ReadInt32();
				break;
			case 18:
				creatureStarData.AllOwnedRewardIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return creatureStarData;
	}
}
