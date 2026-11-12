using NGame2.NBattleField;
using NShared;
using NShared.NLocalization;
using NShared.NWorldMap.NEventSubData;
using NVespa;
using NVespa.NGlobal;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class QuestStatus : BaseWindow
{
	public interface Listener
	{
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/QuestStatus";

	public UILabel TitleLabel;

	public UILabel TitleCompleteLabel;

	public UILabel ProgressLabel;

	public UILabel ProgressValueLabel;

	public UILabel ProgressValueCompleteLabel;

	private Listener _listener;

	private float _lastUpdatedTime;

	public QuestDungeon QuestDugeonData { get; private set; }

	public int KillCount { get; set; }

	public void Open(Listener listener, QuestDungeon questDugeonData)
	{
		Open(delegate
		{
			Init(listener, questDugeonData);
		}, null);
	}

	public void Init(Listener listener, QuestDungeon questDugeonData)
	{
		_listener = listener;
		QuestDugeonData = questDugeonData;
		Init();
		UpdateStatus();
	}

	public override void Reset()
	{
		base.Reset();
		QuestDugeonData = null;
		KillCount = 0;
		_lastUpdatedTime = 0f;
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateMonsterKill();
	}

	private void Init()
	{
		int monsterIndex = QuestDugeonData.MonsterIndex;
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(monsterIndex);
		if (data != null)
		{
			string text = data.Name;
			string text2 = LocalizationManager.GetString(WorldMapEvent.OBJECTIVE_KILL_MONSTER, text);
			UIHelper.SetLabelText(ProgressLabel, text2);
			KillCount = QuestDugeonData.ProgressCount;
		}
	}

	private void UpdateStatus()
	{
		int requireCount = QuestDugeonData.RequireCount;
		int num = Mathf.Min(KillCount, requireCount);
		string text = num + " / " + requireCount;
		bool flag = num >= requireCount;
		UIHelper.EnableGameObject(TitleCompleteLabel, flag);
		UIHelper.EnableGameObject(ProgressValueCompleteLabel, flag);
		UIHelper.EnableGameObject(ProgressValueLabel, !flag);
		if (flag)
		{
			UIHelper.SetLabelText(ProgressValueCompleteLabel, text);
		}
		else
		{
			UIHelper.SetLabelText(ProgressValueLabel, text);
		}
	}

	private void UpdateMonsterKill()
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance || Time.time - _lastUpdatedTime < 0.3f)
		{
			return;
		}
		_lastUpdatedTime = Time.time;
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			KillCount = battleInstance.Snapshot.StatisticsManager.GetKillCreatureIndex(QuestDugeonData.MonsterIndex);
			SingletonUpdater<Executer>.instance.NextRun(delegate
			{
				UpdateStatus();
			});
		});
	}
}
