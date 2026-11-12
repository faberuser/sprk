using System;
using NGame2.NBattleContext;
using NShared;
using NShared.GetReplay;
using NShared.NUtil;
using NShared.SaveReplay;
using NVespa;
using NVespa.NGlobal;

namespace NGame2.NBattleField.NReplay;

public class ReplayManager : SingletonAuto<ReplayManager>
{
	public bool RequestSaveReplay(BattleInstance battleInstance, ReplayType replayType, long[] accountIds, Action<long> onSave)
	{
		return RequestSaveReplay<object>(battleInstance, replayType, null, accountIds, onSave);
	}

	public bool RequestSaveReplay<T>(BattleInstance battleInstance, ReplayType replayType, T replayInfo, long[] accountIds, Action<long> onSave) where T : class
	{
		NShared.SaveReplay.Request request = ReplayUtil.CreateSaveReplayRequest<NShared.SaveReplay.Request, T>(battleInstance, replayType, replayInfo, accountIds, JsonMarshaler.EncodeJsonString);
		if (request == null)
		{
			if (onSave != null)
			{
				onSave(0L);
			}
			return false;
		}
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.SaveReplay.Response response)
		{
			if (response != null && response.BaseResult == BaseResultType.Success && response.Result == NShared.SaveReplay.ResultType.Success)
			{
				long replayUid = response.ReplayUid;
				if (onSave != null)
				{
					onSave(replayUid);
				}
			}
		});
		return true;
	}

	public void StartReplay(long replayUid, string FieldName, Action<string, BaseBattleLog[]> onReplayInfo)
	{
		StartReplay(replayUid, delegate(object o, BaseBattleLog[] battleLogs)
		{
			Vespa_System_Action_Extension_Method.fire(onReplayInfo, FieldName, battleLogs);
		});
	}

	public void StartReplay<T>(long replayUid, Action<T, BaseBattleLog[]> onReplayInfo) where T : class
	{
		NShared.GetReplay.Request request = new NShared.GetReplay.Request
		{
			ReplayUid = replayUid
		};
		SingletonUpdater<WebServiceManager>.instance.Requester.Request(request, delegate(NShared.GetReplay.Response response)
		{
			if (response == null)
			{
				VespaDebug.LogError("Null response");
				Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
			}
			else if (response.BaseResult != BaseResultType.Success)
			{
				VespaDebug.LogError("BaseResult error : " + response.BaseResult);
				Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
			}
			else if (response.Result != NShared.GetReplay.ResultType.Success)
			{
				VespaDebug.LogError("Result error : " + response.Result);
				Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
			}
			else
			{
				Replay replay = response.Replay;
				if (replay == null)
				{
					VespaDebug.LogError("Null replay!");
					Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
				}
				else
				{
					T i_argA = null;
					if (!string.IsNullOrEmpty(replay.Info))
					{
						i_argA = JsonMarshaler.Parse<T>(replay.Info);
					}
					string data = replay.Data;
					if (string.IsNullOrEmpty(data))
					{
						VespaDebug.LogError("Empty compressed battle logs!");
						Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
					}
					else
					{
						string value = Lz4.Decompress(data);
						if (string.IsNullOrEmpty(value))
						{
							VespaDebug.LogError("Failed to decompress battle logs! compressed=" + data);
							Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
						}
						else
						{
							BaseBattleLog[] array = BaseJsonMarshaler.ParseArray(value, JsonMarshaler.Parse<BaseBattleLog>);
							if (array == null || array.Length == 0)
							{
								VespaDebug.LogError("Empty battle logs!");
								Vespa_System_Action_Extension_Method.fire(onReplayInfo, null, null);
							}
							else
							{
								Vespa_System_Action_Extension_Method.fire(onReplayInfo, i_argA, array);
								SingletonAuto<BattleContext>.instance.MoveToBattleScene();
							}
						}
					}
				}
			}
		});
	}
}
