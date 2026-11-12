using System.Collections.Generic;
using System.Text;
using NGame2.NConfig;
using NGame2.NMessage;
using NShared;
using NVespa;

namespace NGame2.NUI.NWindow.NBattle;

public class BattleLog : BaseWindow, GlobalManager.IListener
{
	private class FireSkill
	{
		public NShared.FireSkill CallBack;

		private int _clientFireTick;

		public FireSkill()
		{
			_clientFireTick = GlobalTimer.tick;
		}

		public void Dump(StringBuilder builder, int clientBeginFireTick, int endTick)
		{
			builder.AppendLine($"    효과 발동: {_clientFireTick - clientBeginFireTick}, {endTick - _clientFireTick}");
		}
	}

	private class BeginFireSkill
	{
		public NShared.BeginFireSkill CallBack;

		public bool IsEnd;

		public long EndTimeStamp;

		public List<FireSkill> FireSkillList = new List<FireSkill>();

		private int _clientBeginFireTick;

		private int _clientEndFireTick;

		public BeginFireSkill()
		{
			_clientBeginFireTick = GlobalTimer.tick;
		}

		public void Dump(StringBuilder builder)
		{
			int num = (IsEnd ? _clientEndFireTick : GlobalTimer.tick);
			builder.AppendLine($"스킬 사용({CallBack.SkillId}): {_clientBeginFireTick}, {num - _clientBeginFireTick}");
			for (int i = 0; i < FireSkillList.Count; i++)
			{
				FireSkillList[i].Dump(builder, _clientBeginFireTick, num);
			}
			if (IsEnd)
			{
				builder.AppendLine("스킬 끝");
			}
		}

		public bool Listen(IMessage message)
		{
			if (OnMessage(message.CastCallback<EndFireSkill>()))
			{
				return true;
			}
			if (OnMessage(message.CastCallback<NShared.FireSkill>()))
			{
				return true;
			}
			return false;
		}

		public bool OnMessage(CallbackMessage<EndFireSkill> message)
		{
			if (message == null)
			{
				return false;
			}
			if (CallBack.CreatureUid != message.Callback.CreatureUid)
			{
				return false;
			}
			if (CallBack.SkillId != message.Callback.SkillId)
			{
				return false;
			}
			IsEnd = true;
			EndTimeStamp = message.Callback.GlobalTimestamp;
			_clientEndFireTick = GlobalTimer.tick;
			return true;
		}

		public bool OnMessage(CallbackMessage<NShared.FireSkill> message)
		{
			if (message == null)
			{
				return false;
			}
			if (CallBack.CreatureUid != message.Callback.CreatureUid)
			{
				return false;
			}
			if (CallBack.SkillId != message.Callback.SkillId)
			{
				return false;
			}
			FireSkill fireSkill = new FireSkill();
			fireSkill.CallBack = message.Callback;
			FireSkillList.Add(fireSkill);
			return true;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BattleLog";

	private static WindowPrefab<BattleLog> _instance = new WindowPrefab<BattleLog>("Resources/Prefab2/Window/Battle/BattleLog", GameUIRoot.ELayerType.UI);

	public UILabel LogLabel;

	private List<BeginFireSkill> _skillList = new List<BeginFireSkill>();

	private List<int> _filterNextSkill = new List<int>();

	public static BattleLog instance => _instance.instance;

	public static bool IsValidInstance => _instance.IsValidInstance;

	public static void CreateSingletonInstance()
	{
		_instance.CreateInstance();
	}

	public void OpenLog()
	{
		Open(delegate
		{
			Init();
		}, null);
	}

	public void Init()
	{
		SingletonAuto<GlobalManager>.instance.AddListerer(this);
	}

	void GlobalManager.IListener.Listen(IMessage message)
	{
		if (OnMessage(message.CastCallback<NShared.BeginFireSkill>()))
		{
			UpdateLabel();
			return;
		}
		for (int i = 0; i < _skillList.Count; i++)
		{
			if (_skillList[i].Listen(message))
			{
				UpdateLabel();
				break;
			}
		}
	}

	private bool OnMessage(CallbackMessage<NShared.BeginFireSkill> message)
	{
		if (message == null)
		{
			return false;
		}
		_filterNextSkill.Clear();
		int num = SingletonAuto<ClientDebugBattle>.instance.BattleLogFilterSkill;
		while (num > 0)
		{
			SkillData data = GameRoot.Instance.SkillDataContainer.GetData(num);
			if (data == null)
			{
				VespaDebug.LogError("Invalid SkillIndex=" + num);
				break;
			}
			if (data.NextIndex == 0 || _filterNextSkill.Contains(data.NextIndex))
			{
				break;
			}
			_filterNextSkill.Add(data.NextIndex);
			num = data.NextIndex;
		}
		int battleLogFilterSkill = SingletonAuto<ClientDebugBattle>.instance.BattleLogFilterSkill;
		if (battleLogFilterSkill == 0)
		{
			BeginFireSkill beginFireSkill = new BeginFireSkill();
			beginFireSkill.CallBack = message.Callback;
			_skillList.Insert(0, beginFireSkill);
			return true;
		}
		if (battleLogFilterSkill == message.Callback.SkillId)
		{
			_skillList.Clear();
		}
		else if (!_filterNextSkill.Contains(message.Callback.SkillId))
		{
			return false;
		}
		BeginFireSkill beginFireSkill2 = new BeginFireSkill();
		beginFireSkill2.CallBack = message.Callback;
		_skillList.Add(beginFireSkill2);
		return true;
	}

	private void Update()
	{
		UpdateLabel();
	}

	private void UpdateLabel()
	{
		if (_skillList.Count > 10)
		{
			_skillList.RemoveAt(_skillList.Count - 1);
		}
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < _skillList.Count; i++)
		{
			_skillList[i].Dump(stringBuilder);
			if (i < _skillList.Count - 1)
			{
				stringBuilder.AppendLine();
			}
		}
		UIHelper.SetLabelText(LogLabel, stringBuilder.ToString());
	}
}
