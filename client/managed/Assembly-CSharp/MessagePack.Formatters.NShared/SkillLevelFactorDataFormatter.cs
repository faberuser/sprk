using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class SkillLevelFactorDataFormatter : IMessagePackFormatter<SkillLevelFactorData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, SkillLevelFactorData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(11);
		writer.Write(value.Level);
		writer.Write(value.Factor1);
		writer.Write(value.Factor2);
		writer.Write(value.Factor3);
		writer.Write(value.Factor4);
		writer.Write(value.Factor5);
		writer.Write(value.Factor6);
		writer.Write(value.Factor7);
		writer.Write(value.Factor8);
		writer.Write(value.Factor9);
		writer.Write(value.Factor10);
	}

	public SkillLevelFactorData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		SkillLevelFactorData skillLevelFactorData = new SkillLevelFactorData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				skillLevelFactorData.Level = reader.ReadInt32();
				break;
			case 1:
				skillLevelFactorData.Factor1 = reader.ReadInt32();
				break;
			case 2:
				skillLevelFactorData.Factor2 = reader.ReadInt32();
				break;
			case 3:
				skillLevelFactorData.Factor3 = reader.ReadInt32();
				break;
			case 4:
				skillLevelFactorData.Factor4 = reader.ReadInt32();
				break;
			case 5:
				skillLevelFactorData.Factor5 = reader.ReadInt32();
				break;
			case 6:
				skillLevelFactorData.Factor6 = reader.ReadInt32();
				break;
			case 7:
				skillLevelFactorData.Factor7 = reader.ReadInt32();
				break;
			case 8:
				skillLevelFactorData.Factor8 = reader.ReadInt32();
				break;
			case 9:
				skillLevelFactorData.Factor9 = reader.ReadInt32();
				break;
			case 10:
				skillLevelFactorData.Factor10 = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return skillLevelFactorData;
	}
}
