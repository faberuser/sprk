using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillProjectileGroupDataFormatter : IMessagePackFormatter<SkillProjectileGroupData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillProjectileGroupData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(2);
		writer.Write(value.Index);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.SkillProjectileGroupIndex, options);
	}

	public SkillProjectileGroupData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SkillProjectileGroupData skillProjectileGroupData = new SkillProjectileGroupData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				skillProjectileGroupData.Index = reader.ReadInt32();
				break;
			case 1:
				skillProjectileGroupData.SkillProjectileGroupIndex = resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return skillProjectileGroupData;
	}
}
