using System;
using NGame2.NBattleContext;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NLobby;
using NGame2.NUI.NWindow;
using NShared;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NScene.NLobby;

public class HeroDungeonReturnState : EventDungeonReturnState
{
	private MemoryFragmentData _memoryFragmentData;

	public HeroDungeonReturnState(EnterDungeonData enterDungeon, EventDungeonGroupData enterGroup, int[] eventDungeonGroupIndices, MemoryFragmentData memoryFragmentData = null)
	{
		_enterDungeon = enterDungeon;
		_enterGroup = enterGroup;
		_eventDungeonGroupIndices = eventDungeonGroupIndices;
		_memoryFragmentData = memoryFragmentData;
	}

	public override void Execute(LobbyScene2.ReturnStateInfo info, Action onEnd)
	{
		if (_memoryFragmentData == null)
		{
			return;
		}
		MemoryFragment window = SceneAutoBehaviour<MemoryFragmentManagement>.instance.GetInstance<MemoryFragment>();
		if (window == null)
		{
			Debug.LogError("MemoryFragment (window) is null");
			return;
		}
		SceneAutoBehaviour<EntryMenuManagement>.instance.CloseEntryMenu(delegate
		{
			GlobalAutoBehaviour<GlobalManagement>.instance.RegisterOpenedWindow(window);
			SceneAutoBehaviour<LobbyPopupQueue>.instance.RemoveAllClearedPopup("LobbyNoticePopup");
			SceneAutoBehaviour<EntryMenuManagement>.instance.OpenEntryMenuUnNode(_memoryFragmentData.OpenNpcCode, skipAni: false, playNPCSound: true, isIgnoreTrigger: false, delegate
			{
				base.Execute(info, onEnd);
			});
		});
	}
}
