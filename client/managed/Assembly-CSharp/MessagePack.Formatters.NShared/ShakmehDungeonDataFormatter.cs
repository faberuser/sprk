using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShakmehDungeonDataFormatter : IMessagePackFormatter<ShakmehDungeonData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShakmehDungeonData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(7);
		writer.Write(value.ShakmehIndex);
		writer.Write(value.Level);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.OpenCondition, options);
		writer.Write(value.FirstClearRewardIndex);
		writer.Write(value.RewardIndex);
		writer.Write(value.BanRuleIndex);
	}

	public ShakmehDungeonData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShakmehDungeonData shakmehDungeonData = new ShakmehDungeonData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shakmehDungeonData.ShakmehIndex = reader.ReadInt32();
				break;
			case 1:
				shakmehDungeonData.Level = reader.ReadInt32();
				break;
			case 2:
				shakmehDungeonData.DungeonIndex = reader.ReadInt32();
				break;
			case 3:
				shakmehDungeonData.OpenCondition = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				shakmehDungeonData.FirstClearRewardIndex = reader.ReadInt32();
				break;
			case 5:
				shakmehDungeonData.RewardIndex = reader.ReadInt32();
				break;
			case 6:
				shakmehDungeonData.BanRuleIndex = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shakmehDungeonData;
	}
}
