using System.Collections.Generic;
using NShared;

namespace MessagePack.Formatters.NShared;

public sealed class LoadingDataContainer_BattleLoadingInfoFormatter : IMessagePackFormatter<LoadingDataContainer.BattleLoadingInfo>, IMessagePackFormatter
{
	public void Serialize(ref MessagePackWriter writer, LoadingDataContainer.BattleLoadingInfo value, MessagePackSerializerOptions options)
	{
		if (value == null)
		{
			writer.WriteNil();
			return;
		}
		IFormatterResolver resolver = options.Resolver;
		writer.WriteArrayHeader(4);
		writer.Write(value.DungeonIndex_Begin);
		writer.Write(value.DungeonIndex_End);
		writer.Write(value.TotalRate);
		resolver.GetFormatterWithVerify<List<int>>().Serialize(ref writer, value.DataKeyList, options);
	}

	public LoadingDataContainer.BattleLoadingInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
	{
		if (reader.TryReadNil())
		{
			return null;
		}
		options.Security.DepthStep(ref reader);
		IFormatterResolver resolver = options.Resolver;
		int num = reader.ReadArrayHeader();
		LoadingDataContainer.BattleLoadingInfo battleLoadingInfo = new LoadingDataContainer.BattleLoadingInfo();
		for (int i = 0; i < num; i++)
		{
			switch (i)
			{
			case 0:
				battleLoadingInfo.DungeonIndex_Begin = reader.ReadInt32();
				break;
			case 1:
				battleLoadingInfo.DungeonIndex_End = reader.ReadInt32();
				break;
			case 2:
				battleLoadingInfo.TotalRate = reader.ReadInt32();
				break;
			case 3:
				battleLoadingInfo.DataKeyList = resolver.GetFormatterWithVerify<List<int>>().Deserialize(ref reader, options);
				break;
			default:
				reader.Skip();
				break;
			}
		}
		reader.Depth--;
		return battleLoadingInfo;
	}
}
