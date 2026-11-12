using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using MiniJSON;
using NShared.NUtil;
using NShared.SaveReplay;
using NVespa;

namespace NShared;

public class ReplayUtil
{
	public static string CreateBattleLogString(BattleInstance battleInstance, Func<object, string> serializer, bool compress)
	{
		List<BaseBattleLog> allBattleLogList = battleInstance.BattleLogContainer.GetAllBattleLogList();
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append('[');
		for (int i = 0; i < allBattleLogList.Count; i++)
		{
			BaseBattleLog arg = allBattleLogList[i];
			string value = serializer(arg);
			stringBuilder.Append(value);
			if (i != allBattleLogList.Count - 1)
			{
				stringBuilder.Append(',');
			}
		}
		stringBuilder.Append(']');
		string text = stringBuilder.ToString();
		if (compress)
		{
			text = Lz4.Compress(text);
		}
		return text;
	}

	public static List<BaseBattleLog> ParseBattleLogString(string battleLogString, Func<IDictionary, BaseBattleLog> parser, bool decompress)
	{
		try
		{
			string text = (decompress ? Lz4.Decompress(battleLogString) : battleLogString);
			if (string.IsNullOrEmpty(text))
			{
				return null;
			}
			if (!(Json.Deserialize(text) is IList list))
			{
				return null;
			}
			List<BaseBattleLog> list2 = new List<BaseBattleLog>();
			foreach (object item in list)
			{
				BaseBattleLog baseBattleLog = parser(item as Dictionary<string, object>);
				if (baseBattleLog == null)
				{
					return null;
				}
				list2.Add(baseBattleLog);
			}
			return list2;
		}
		catch (Exception ex)
		{
			VespaDebug.LogError("Failed to parse battle logs : error=" + ex);
			return null;
		}
	}

	public static TReq CreateSaveReplayRequest<TReq, T>(BattleInstance battleInstance, ReplayType replayType, T replayInfo, long[] accountIds, Func<object, string> serializer) where TReq : NShared.SaveReplay.Request, new() where T : class
	{
		if (battleInstance == null)
		{
			return null;
		}
		string text = CreateBattleLogString(battleInstance, serializer, compress: true);
		if (string.IsNullOrEmpty(text))
		{
			VespaDebug.LogError("Failed to compress battle logs");
			return null;
		}
		string text2 = string.Empty;
		if (replayInfo != null)
		{
			text2 = serializer(replayInfo);
			if (string.IsNullOrEmpty(text2))
			{
				VespaDebug.LogError("Failed to encode replay info : replayType = " + replayType);
				return null;
			}
		}
		return new TReq
		{
			Type = replayType,
			Info = text2,
			BattleLogs = text,
			AccountIds = accountIds
		};
	}
}
