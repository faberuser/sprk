using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class PcAvatarDataFormatter : IMessagePackFormatter<PcAvatarData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, PcAvatarData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		writer.Write(value.ChapterIndex);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ConstructorFactory, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.FactoryValues, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.Condition1, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.MoveNodes, options);
		resolver.GetFormatterWithVerify<float[]>().Serialize(ref writer, value.StayTimes, options);
		writer.Write(value.MoveSpeedRate);
		writer.Write(value.RewardOpen);
		writer.Write(value.RewardCoolTime);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TouchActions, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.TouchText, options);
	}

	public PcAvatarData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		PcAvatarData pcAvatarData = new PcAvatarData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				pcAvatarData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				pcAvatarData.Index = reader.ReadInt32();
				break;
			case 2:
				pcAvatarData.ConstructorFactory = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 3:
				pcAvatarData.FactoryValues = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 4:
				pcAvatarData.Condition1 = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 5:
				pcAvatarData.MoveNodes = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 6:
				pcAvatarData.StayTimes = resolver.GetFormatterWithVerify<float[]>().Deserialize(ref reader, options);
				break;
			case 7:
				pcAvatarData.MoveSpeedRate = reader.ReadSingle();
				break;
			case 8:
				pcAvatarData.RewardOpen = reader.ReadBoolean();
				break;
			case 9:
				pcAvatarData.RewardCoolTime = reader.ReadInt32();
				break;
			case 10:
				pcAvatarData.TouchActions = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			case 11:
				pcAvatarData.TouchText = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return pcAvatarData;
	}
}
