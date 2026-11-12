using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class DOWDungeonTextDataFormatter : IMessagePackFormatter<DOWDungeonTextData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, DOWDungeonTextData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(6);
		writer.Write(value.DungeonIndex);
		writer.Write(value.Diff);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MainRewardItemCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ClearTip, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.MonsterTip, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendHeroes, options);
	}

	public DOWDungeonTextData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		DOWDungeonTextData dOWDungeonTextData = new DOWDungeonTextData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				dOWDungeonTextData.DungeonIndex = reader.ReadInt32();
				break;
			case 1:
				dOWDungeonTextData.Diff = reader.ReadInt32();
				break;
			case 2:
				dOWDungeonTextData.MainRewardItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				dOWDungeonTextData.ClearTip = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				dOWDungeonTextData.MonsterTip = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				dOWDungeonTextData.RecommendHeroes = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return dOWDungeonTextData;
	}
}
