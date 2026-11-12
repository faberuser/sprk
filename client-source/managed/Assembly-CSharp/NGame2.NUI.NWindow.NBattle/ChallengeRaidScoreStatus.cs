using System;
using System.Collections.Generic;
using NGame2.NBattleField;
using NGame2.NUI.NManager.NLobby;
using NShared;
using NVespa;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class ChallengeRaidScoreStatus : BaseWindow
{
	public interface Listener
	{
	}

	public interface IDataProvider
	{
		Transform GetBossStatusBottomDummy();
	}

	public class ScoreContext
	{
		public bool _initialized;

		public long _initialTotalHp;

		public HashSet<int> _initialUids = new HashSet<int>();

		public long _currentTotalHp;

		public bool _isRequesting;

		public float _lastUpdatedTime;

		public void Reset()
		{
			_initialized = false;
			_initialTotalHp = 0L;
			_currentTotalHp = 0L;
			_isRequesting = false;
			_initialUids.Clear();
			_lastUpdatedTime = 0f;
		}
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/ChallengeRaidScoreStatus";

	public UILabel ScoreLabel;

	public UITexture IconTexture;

	private ScoreContext _scoreContext = new ScoreContext();

	private IDataProvider DataProvider;

	public void Open(Listener listener, IDataProvider dataProvider)
	{
		Open(delegate
		{
			Init(listener, dataProvider);
		}, null);
	}

	public void Init(Listener listener, IDataProvider dataProvider)
	{
		_ = SceneAutoBehaviour<RaidManagement>.instance.GetChallengeRaidSeasonInfo().ChallengeRaidIndex;
		DataProvider = dataProvider;
		UpdateScore(_scoreContext);
		UpdateLabel(ScoreLabel, _scoreContext);
	}

	public override void Reset()
	{
		base.Reset();
		_scoreContext.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateScore(ScoreLabel, _scoreContext);
		UpdatePosition();
	}

	public static void UpdateScore(UILabel scoreLabel, ScoreContext scoreContext)
	{
		if (Time.time - scoreContext._lastUpdatedTime > 0.3f)
		{
			scoreContext._lastUpdatedTime = Time.time;
			UpdateScore(scoreContext);
		}
		UpdateLabel(scoreLabel, scoreContext);
	}

	private static void UpdateLabel(UILabel scoreLabel, ScoreContext scoreContext)
	{
		long num = System.Math.Max(scoreContext._initialTotalHp - scoreContext._currentTotalHp, 0L);
		UIHelper.SetLabelText(scoreLabel, Vespa_Long_Extension_Method.ToCommaString(num));
	}

	private static void UpdateScore(ScoreContext scoreContext)
	{
		if (!SingletonAuto<BattleFacade>.isValidInstance || scoreContext._isRequesting)
		{
			return;
		}
		scoreContext._isRequesting = true;
		SingletonAuto<BattleFacade>.instance.AddThreadCommonAction(delegate(BattleInstance battleInstance)
		{
			scoreContext._isRequesting = false;
			BattleCreatureList allCreaturesOfTeam = battleInstance.CreatureContainer.GetAllCreaturesOfTeam(1);
			if (allCreaturesOfTeam.Count != 0)
			{
				if (!scoreContext._initialized)
				{
					scoreContext._initialized = true;
					scoreContext._initialTotalHp = 0L;
					scoreContext._initialUids.Clear();
					for (int i = 0; i < allCreaturesOfTeam.Count; i++)
					{
						BattleCreature battleCreature = allCreaturesOfTeam[i];
						int uid = battleCreature.Uid;
						scoreContext._initialUids.Add(uid);
						scoreContext._initialTotalHp += battleCreature.StatController.Hp;
					}
				}
				long num = 0L;
				for (int j = 0; j < allCreaturesOfTeam.Count; j++)
				{
					BattleCreature battleCreature2 = allCreaturesOfTeam[j];
					int uid2 = battleCreature2.Uid;
					if (scoreContext._initialUids.Contains(uid2))
					{
						long hp = battleCreature2.StatController.Hp;
						num += hp;
					}
				}
				scoreContext._currentTotalHp = num;
			}
		});
	}

	public void UpdatePosition()
	{
		Vector3 vector = Vector3.zero;
		if (DataProvider != null)
		{
			Transform bossStatusBottomDummy = DataProvider.GetBossStatusBottomDummy();
			if (bossStatusBottomDummy != null)
			{
				vector = bossStatusBottomDummy.position;
				vector = WindowRoot.transform.parent.InverseTransformPoint(vector);
			}
		}
		if (!(vector == WindowRoot.transform.localPosition))
		{
			float num = 5f;
			float t = Time.deltaTime * num;
			WindowRoot.transform.localPosition = Vector3.Lerp(WindowRoot.transform.localPosition, vector, t);
		}
	}
}
