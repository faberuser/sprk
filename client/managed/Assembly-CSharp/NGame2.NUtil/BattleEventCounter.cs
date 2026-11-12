using System.Diagnostics;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUtil;

public class BattleEventCounter : GlobalAutoBehaviour<BattleEventCounter>, IInspectorGuiDrawer
{
	public enum EEventType
	{
		BeginFireSkill,
		FireSkill,
		ReserveOperation,
		FlyingOperation,
		BeginOperation
	}

	public struct EventKeyData
	{
		public EEventType EventType;

		public int Arg1;

		public int Arg2;

		public EventKeyData(EEventType eventType, int arg)
		{
			EventType = eventType;
			Arg1 = arg;
			Arg2 = 0;
		}

		public EventKeyData(EEventType eventType, int arg1, int arg2)
		{
			EventType = eventType;
			Arg1 = arg1;
			Arg2 = arg2;
		}

		public string GetLabel()
		{
			return $"{EventType}: {Arg1}, {Arg2}";
		}
	}

	public class CountPerSecData
	{
		private float _lastTime;

		public int Total { get; private set; }

		public int Max { get; private set; }

		public int Cur { get; private set; }

		public void Increase()
		{
			float time = Time.time;
			if (time - _lastTime > 1f)
			{
				_lastTime = time;
				Cur = 0;
			}
			Total++;
			Cur++;
			Max = Mathf.Max(Max, Cur);
		}

		public string Dump()
		{
			return $"Cur={Cur}, Max={Max}, Total={Total}";
		}
	}

	public class CounterData
	{
		public CountPerSecData Callback = new CountPerSecData();

		public CountPerSecData NodeTree = new CountPerSecData();
	}

	private bool _clearOnSceneChanged = true;

	private IncreaseDic<EventKeyData, CounterData> _dic = new IncreaseDic<EventKeyData, CounterData>();

	[Conditional("UNITY_EDITOR")]
	public static void OnCallback(EEventType eventType, int arg1, int arg2 = 0)
	{
		EventKeyData key = new EventKeyData(eventType, arg1, arg2);
		GlobalAutoBehaviour<BattleEventCounter>.instance.GetCounter(key).Callback.Increase();
	}

	[Conditional("UNITY_EDITOR")]
	public static void OnNodeTree(EEventType eventType, int arg1, int arg2 = 0)
	{
		EventKeyData key = new EventKeyData(eventType, arg1, arg2);
		GlobalAutoBehaviour<BattleEventCounter>.instance.GetCounter(key).NodeTree.Increase();
	}

	[Conditional("UNITY_EDITOR")]
	public static void OnSceneChanged()
	{
		if (GlobalAutoBehaviour<BattleEventCounter>.isValidInstance && GlobalAutoBehaviour<BattleEventCounter>.instance._clearOnSceneChanged)
		{
			GlobalAutoBehaviour<BattleEventCounter>.instance._dic.Clear();
		}
	}

	internal CounterData GetCounter(EventKeyData key)
	{
		CounterData counterData = _dic.GETVALUE(key, null);
		if (counterData == null)
		{
			counterData = new CounterData();
			_dic.Add(key, counterData);
		}
		return counterData;
	}
}
