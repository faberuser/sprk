using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class GatewayToTrialDataFormatter : IMessagePackFormatter<GatewayToTrialData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, GatewayToTrialData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.Index);
		writer.Write(value.TowerIndex);
		writer.Write(value.Step);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		writer.Write(value.RewardIndex);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonName, options);
	}

	public GatewayToTrialData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		GatewayToTrialData gatewayToTrialData = new GatewayToTrialData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				gatewayToTrialData.Index = reader.ReadInt32();
				break;
			case 1:
				gatewayToTrialData.TowerIndex = reader.ReadInt32();
				break;
			case 2:
				gatewayToTrialData.Step = reader.ReadInt32();
				break;
			case 3:
				gatewayToTrialData.ChapterIndex = reader.ReadInt32();
				break;
			case 4:
				gatewayToTrialData.DungeonIndex = reader.ReadInt32();
				break;
			case 5:
				gatewayToTrialData.RewardIndex = reader.ReadInt32();
				break;
			case 6:
				gatewayToTrialData.DungeonName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return gatewayToTrialData;
	}
}
