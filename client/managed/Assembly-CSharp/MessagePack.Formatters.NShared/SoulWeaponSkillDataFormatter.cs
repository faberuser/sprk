using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SoulWeaponSkillDataFormatter : IMessagePackFormatter<SoulWeaponSkillData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SoulWeaponSkillData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(12);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.ItemCode, options);
		resolver.GetFormatterWithVerify<int[]>().Serialize(ref writer, value.ActiveConditionSkillIndices, options);
		writer.Write(value.BasicSkillIndex);
		writer.Write(value.MaxUseCount);
		writer.Write(value.AddSkillIndex1);
		writer.Write(value.AddSkillIndex2);
		writer.Write(value.AddSkillIndex3);
		writer.Write(value.AddSkillIndex4);
		writer.Write(value.AddSkillIndex5);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.CheckState, options);
		resolver.GetFormatterWithVerify<string>().Serialize(ref writer, value.StackTypeName, options);
		resolver.GetFormatterWithVerify<string[]>().Serialize(ref writer, value.UpgradeDescription, options);
	}

	public SoulWeaponSkillData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		SoulWeaponSkillData soulWeaponSkillData = new SoulWeaponSkillData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				soulWeaponSkillData.ItemCode = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 1:
				soulWeaponSkillData.ActiveConditionSkillIndices = resolver.GetFormatterWithVerify<int[]>().Deserialize(ref reader, options);
				break;
			case 2:
				soulWeaponSkillData.BasicSkillIndex = reader.ReadInt32();
				break;
			case 3:
				soulWeaponSkillData.MaxUseCount = reader.ReadInt32();
				break;
			case 4:
				soulWeaponSkillData.AddSkillIndex1 = reader.ReadInt32();
				break;
			case 5:
				soulWeaponSkillData.AddSkillIndex2 = reader.ReadInt32();
				break;
			case 6:
				soulWeaponSkillData.AddSkillIndex3 = reader.ReadInt32();
				break;
			case 7:
				soulWeaponSkillData.AddSkillIndex4 = reader.ReadInt32();
				break;
			case 8:
				soulWeaponSkillData.AddSkillIndex5 = reader.ReadInt32();
				break;
			case 9:
				soulWeaponSkillData.CheckState = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 10:
				soulWeaponSkillData.StackTypeName = resolver.GetFormatterWithVerify<string>().Deserialize(ref reader, options);
				break;
			case 11:
				resolver.GetFormatterWithVerify<string[]>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return soulWeaponSkillData;
	}
}
