using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MatchBanPickStateDataFormatter : IMessagePackFormatter<MatchBanPickStateData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MatchBanPickStateData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(13);
		resolver.GetFormatterWithVerify<MatchBanPickState>().Serialize(ref writer, value.State, options);
		resolver.GetFormatterWithVerify<TeamType>().Serialize(ref writer, value.TeamType, options);
		writer.Write(value.IsPick);
		writer.Write(value.SelectableCount4Deck);
		writer.Write(value.SelectableCount8Deck);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalMessage1P, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.LocalMessage2P, options);
		writer.Write(value.WaitTime4Deck);
		writer.Write(value.PenaltyTime4Deck1);
		writer.Write(value.PenaltyTime4Deck2);
		writer.Write(value.PenaltyTime4Deck3);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AnimMe1P, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.AnimMe2P, options);
	}

	public MatchBanPickStateData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MatchBanPickStateData matchBanPickStateData = new MatchBanPickStateData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				matchBanPickStateData.State = resolver.GetFormatterWithVerify<MatchBanPickState>().Deserialize(ref reader, options);
				break;
			case 1:
				matchBanPickStateData.TeamType = resolver.GetFormatterWithVerify<TeamType>().Deserialize(ref reader, options);
				break;
			case 2:
				matchBanPickStateData.IsPick = reader.ReadBoolean();
				break;
			case 3:
				matchBanPickStateData.SelectableCount4Deck = reader.ReadInt32();
				break;
			case 4:
				matchBanPickStateData.SelectableCount8Deck = reader.ReadInt32();
				break;
			case 5:
				matchBanPickStateData.LocalMessage1P = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				matchBanPickStateData.LocalMessage2P = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				matchBanPickStateData.WaitTime4Deck = reader.ReadInt32();
				break;
			case 8:
				matchBanPickStateData.PenaltyTime4Deck1 = reader.ReadInt32();
				break;
			case 9:
				matchBanPickStateData.PenaltyTime4Deck2 = reader.ReadInt32();
				break;
			case 10:
				matchBanPickStateData.PenaltyTime4Deck3 = reader.ReadInt32();
				break;
			case 11:
				matchBanPickStateData.AnimMe1P = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 12:
				matchBanPickStateData.AnimMe2P = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return matchBanPickStateData;
	}
}
