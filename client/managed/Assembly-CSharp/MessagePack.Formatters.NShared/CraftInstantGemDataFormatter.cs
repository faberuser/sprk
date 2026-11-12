using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class CraftInstantGemDataFormatter : IMessagePackFormatter<CraftInstantGemData>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, CraftInstantGemData value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		writer.WriteArrayHeader(4);
		writer.Write(value.MinRemainTime);
		writer.Write(value.MaxRemainTime);
		writer.Write(value.BaseReqGem);
		writer.Write(value.ReqSecondPerGem);
	}

	public CraftInstantGemData Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		int num = reader.ReadArrayHeader();
		CraftInstantGemData craftInstantGemData = new CraftInstantGemData();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				craftInstantGemData.MinRemainTime = reader.ReadInt32();
				break;
			case 1:
				craftInstantGemData.MaxRemainTime = reader.ReadInt32();
				break;
			case 2:
				craftInstantGemData.BaseReqGem = reader.ReadInt32();
				break;
			case 3:
				craftInstantGemData.ReqSecondPerGem = reader.ReadInt32();
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return craftInstantGemData;
	}
}
