using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class MazeInfoDataFormatter : IMessagePackFormatter<MazeInfoData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, MazeInfoData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(8);
		writer.Write(value.TowerIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TowerNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.InfoTitleKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReqClassDescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattleInfoTitleKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BattleInfoDescKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ReqClass, options);
		resolver.GetFormatterWithVerify<CreatureTagType>().Serialize(ref writer, value.ClassType, options);
	}

	public MazeInfoData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		MazeInfoData mazeInfoData = new MazeInfoData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				mazeInfoData.TowerIndex = reader.ReadInt32();
				break;
			case 1:
				mazeInfoData.TowerNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 2:
				mazeInfoData.InfoTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				mazeInfoData.ReqClassDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 4:
				mazeInfoData.BattleInfoTitleKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				mazeInfoData.BattleInfoDescKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				mazeInfoData.ReqClass = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				mazeInfoData.ClassType = resolver.GetFormatterWithVerify<CreatureTagType>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return mazeInfoData;
	}
}
