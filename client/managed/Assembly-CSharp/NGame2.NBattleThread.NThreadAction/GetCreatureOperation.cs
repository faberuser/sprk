using System.Collections.Generic;
using System.Text;
using NGame2.NBattleField;
using NShared;
using NShared.NCampaign;
using NVespa;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NBattleThread.NThreadAction;

public class GetCreatureOperation : IThreadAction
{
	public class OperationSummary
	{
		public string OperationName;

		public int SkillIndex;

		public int SkillLevel;

		public int FiredSkillIndex;

		public int OperationNo;

		public int DurationMs;

		public int ElapsedTimeMs;

		public OperationSummary(OperationInfo opInfo)
		{
			OperationName = ((opInfo.Runtime != null) ? opInfo.Runtime.GetTypeName() : "Null");
			SkillIndex = opInfo.SkillIndex;
			SkillLevel = opInfo.SkillLevel;
			FiredSkillIndex = opInfo.FiredSkillIndex;
			OperationNo = opInfo.OperationNo;
			DurationMs = opInfo.DurationMs;
			ElapsedTimeMs = opInfo.ElapsedTimeMs;
		}

		public virtual string Dump()
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append($"[{OperationName}]");
			stringBuilder.Append($" Skill={SkillIndex}");
			if (FiredSkillIndex != 0)
			{
				stringBuilder.Append($", FiredSkill={FiredSkillIndex}");
			}
			stringBuilder.Append($", OpNo={OperationNo}");
			if (DurationMs > 0)
			{
				stringBuilder.Append($", DurationMs={DurationMs}");
			}
			if (ElapsedTimeMs > 0)
			{
				stringBuilder.Append($", ElapsedTimeMs={ElapsedTimeMs}");
			}
			return stringBuilder.ToString();
		}
	}

	public class DelayedOperationSummary : OperationSummary
	{
		public int DelayMs;

		public int DelayedElapsedMs;

		public DelayedOperationSummary(DelayedOperationInfo delayedInfo)
			: base(delayedInfo.OperationInfo)
		{
			DelayMs = delayedInfo.DelayMs;
			DelayedElapsedMs = delayedInfo.ElapsedTimeMs;
		}

		public override string Dump()
		{
			return string.Concat(base.Dump() + $"DelayMs={DelayMs}", $"DelayedElapsedMs={DelayedElapsedMs}");
		}
	}

	public class OperationBundleData
	{
		public List<OperationSummary> SummaryList = new List<OperationSummary>();

		public List<DelayedOperationSummary> DelayedSummaryList = new List<DelayedOperationSummary>();
	}

	public static bool GlobalIsEnabled;

	public static bool Foldout;

	public int CreatureUid;

	private int _globalFilterSkill;

	private OperationBundleData _operation;

	private OperationBundleData _threadOperation;

	private bool isFoldOutOperationList;

	private bool isFoldOutDelayedOperationList;

	private bool isUseFilter;

	public bool IsWaiting { get; set; }

	public GetCreatureOperation(int creatureUid)
	{
		CreatureUid = creatureUid;
	}

	public void ChainUpdate()
	{
		if (GlobalIsEnabled && !IsWaiting)
		{
			if (_operation == null)
			{
				_operation = new OperationBundleData();
			}
			if (_threadOperation == null)
			{
				_threadOperation = new OperationBundleData();
			}
			Math.Swap(ref _operation, ref _threadOperation);
			SingletonAuto<BattleFacade>.instance.AddThreadSync(this);
		}
	}

	void IThreadAction.Action(BattleInstance battleInstance, CampaignUpdater campaignUpdater)
	{
		if (_threadOperation == null)
		{
			return;
		}
		BattleCreature creature = battleInstance.CreatureContainer.GetCreature(CreatureUid);
		if (creature == null)
		{
			return;
		}
		_threadOperation.SummaryList.Clear();
		_threadOperation.DelayedSummaryList.Clear();
		OperationController operationController = creature.OperationController;
		IndexableSnapshotDictionary<long, OperationInfo> operationDictionary = operationController.OperationDictionary;
		SnapshotList<DelayedOperationInfo> delayedOperationList = operationController.DelayedOperationList;
		foreach (KeyValuePair<long, OperationInfo> item3 in operationDictionary)
		{
			OperationSummary item = new OperationSummary(item3.Value);
			_threadOperation.SummaryList.Add(item);
		}
		foreach (DelayedOperationInfo item4 in (IEnumerable<DelayedOperationInfo>)delayedOperationList)
		{
			DelayedOperationSummary item2 = new DelayedOperationSummary(item4);
			_threadOperation.DelayedSummaryList.Add(item2);
		}
	}

	public void DrawToggleGUI()
	{
		GlobalIsEnabled = GUILayout.Toggle(GlobalIsEnabled, "Is Enabled");
		using (new BlockManager().Horizontal())
		{
			int globalFilterSkill = _globalFilterSkill;
			isUseFilter = GUILayout.Toggle(isUseFilter, "Use");
			GUILayout.Label("Filter Skill : ");
			string text = GUILayout.TextField(_globalFilterSkill.ToString(), GUILayout.Width(200f));
			_globalFilterSkill = Vespa_String_Extension_Method.TOINT(text, globalFilterSkill);
		}
		if (_operation == null)
		{
			return;
		}
		isFoldOutOperationList = GUILayout.Toggle(isFoldOutOperationList, "OperationList");
		if (isFoldOutOperationList)
		{
			using (new BlockManager())
			{
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_operation.SummaryList); i++)
				{
					OperationSummary operationSummary = _operation.SummaryList[i];
					if (_globalFilterSkill == 0 || !isUseFilter || operationSummary.SkillIndex == _globalFilterSkill)
					{
						GUILayout.Label(operationSummary.Dump());
					}
				}
			}
		}
		isFoldOutDelayedOperationList = GUILayout.Toggle(isFoldOutDelayedOperationList, "DelayedOperationList");
		if (!isFoldOutDelayedOperationList)
		{
			return;
		}
		using (new BlockManager())
		{
			for (int j = 0; j < __Extension_Method_ICollection__.COUNT(_operation.DelayedSummaryList); j++)
			{
				DelayedOperationSummary delayedOperationSummary = _operation.DelayedSummaryList[j];
				if (_globalFilterSkill == 0 || delayedOperationSummary.SkillIndex == _globalFilterSkill)
				{
					GUILayout.Label(delayedOperationSummary.Dump());
				}
			}
		}
	}
}
