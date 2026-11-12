using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class ShakmehBossDataFormatter : IMessagePackFormatter<ShakmehBossData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, ShakmehBossData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(15);
		writer.Write(value.ChapterIndex);
		writer.Write(value.DungeonIndex);
		resolver.GetFormatterWithVerify<ShakmehBossType>().Serialize(ref writer, value.BossType, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.SequenceSkillProjectileGroup, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BossNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonNameKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.DungeonPrefixKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TribeKey, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.TacticKey, options);
		writer.Write(value.ShakmehIndex);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.RecommendedHero, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BgNodeTreeCode, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.BgNodeTreePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewNodeTreePath, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.PreviewNodeTreeCode, options);
	}

	public ShakmehBossData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		ShakmehBossData shakmehBossData = new ShakmehBossData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				shakmehBossData.ChapterIndex = reader.ReadInt32();
				break;
			case 1:
				shakmehBossData.DungeonIndex = reader.ReadInt32();
				break;
			case 2:
				shakmehBossData.BossType = resolver.GetFormatterWithVerify<ShakmehBossType>().Deserialize(ref reader, options);
				break;
			case 3:
				shakmehBossData.SequenceSkillProjectileGroup = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 4:
				shakmehBossData.BossNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 5:
				shakmehBossData.DungeonNameKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 6:
				shakmehBossData.DungeonPrefixKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 7:
				shakmehBossData.TribeKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 8:
				shakmehBossData.TacticKey = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 9:
				shakmehBossData.ShakmehIndex = reader.ReadInt32();
				break;
			case 10:
				shakmehBossData.RecommendedHero = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 11:
				shakmehBossData.BgNodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 12:
				shakmehBossData.BgNodeTreePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 13:
				shakmehBossData.PreviewNodeTreePath = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 14:
				shakmehBossData.PreviewNodeTreeCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return shakmehBossData;
	}
}
