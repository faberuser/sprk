using System;
using System.Collections.Generic;
using NGame2.NUI.NComponent2.NBattle;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class TacticStatus : BaseWindow
{
	[Serializable]
	public class ComponentRoot
	{
		public BattleTacticType Type;

		public Transform Root;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/TacticStatus";

	public List<ComponentRoot> ComponentRoots = new List<ComponentRoot>();

	private TacticComponent _tacticComponent;

	private CampaignTacticData _tacticData;

	public void Open(int chapterIndex, int dungeonIndex, DungeonDifficultyType difficulty)
	{
		_tacticData = GameRoot.Instance.CampaignTacticDataContainer.GetData(chapterIndex, dungeonIndex, difficulty);
		if (_tacticData != null)
		{
			base.Open();
			ComponentRoot componentRoot = ComponentRoots.Find((ComponentRoot rootItem) => rootItem.Type == _tacticData.TacticType);
			_tacticComponent = NGame2.NUI.NComponent2.NBattle.TacticFactory.CreateTactic(_tacticData.TacticType, _tacticData.ComponentName, componentRoot.Root);
			if (_tacticComponent == null)
			{
				VespaDebug.LogError("Tactic Component is null : chapterIndex=" + _tacticData.ChapterIndex + " dungeonIndex=" + _tacticData.DungeonIndex + " type=" + _tacticData.TacticType);
			}
			else
			{
				Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_tacticComponent.transform);
				_tacticComponent.Open(_tacticData);
			}
		}
	}

	public override void Reset()
	{
		base.Reset();
		ClearTactic();
	}

	public void ClearTactic()
	{
		FactoryBase.SafeReleaseComponent(ref _tacticComponent);
	}
}
