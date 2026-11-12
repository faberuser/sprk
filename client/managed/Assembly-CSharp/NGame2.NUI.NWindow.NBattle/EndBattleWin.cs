using System;
using System.Collections;
using NGame2.NAccount;
using NGame2.NUI.NManager.NBattle;
using NShared;
using NVespa;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class EndBattleWin : FrameControlWindow, IInspectorGuiDrawer
{
	public interface Listener
	{
		bool OnCloseRequest(EndBattleWin closeThis);

		void OnInputEnabled(EndBattleWin caller);

		void OnClickReturnToLobby(EndBattleWin caller);

		void ShowStatistics();
	}

	public enum EndType
	{
		Victory,
		Complete
	}

	[Serializable]
	public class UiEffectWidget
	{
		public float MarkTime = 0.5f;

		public float StarTime = 0.3f;

		public float LastStarTime = 0.1f;

		public EndType Type;

		public DungeonDifficultyType Difficulty;

		public UIGame3DWidget Mark;

		public UIGame3DWidget Text;

		public UIGame3DWidget[] Star;

		public void StopAll(bool immediately)
		{
			if (Mark != null)
			{
				Mark.Stop(immediately);
			}
			if (Text != null)
			{
				Text.Stop(immediately);
			}
			for (int i = 0; i < Star.Length; i++)
			{
				if (!(Star[i] == null))
				{
					Star[i].Stop(immediately);
				}
			}
		}
	}

	public enum ScoreResultType
	{
		None,
		GuildRaid,
		WorldBoss,
		EventWorldBoss,
		Rank,
		GuildSuppress,
		ChallengeRaidSingle,
		ChallengeRaidMulti
	}

	[Serializable]
	public class ScoreResultInfo
	{
		public ScoreResultType Type;

		public Transform Root;

		public UILabel RankLabel;

		public UILabel ChangedRankLabel;

		public UILabel TotalScoreLabel;

		public UILabel AddedScoreLabel;

		public UILabel RightRankScoreLabel;

		public UILabel CurrentRecordLabel;

		public UILabel BestScoreLabel;

		public UILabel BestRecordLabel;
	}

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/EndBattleWin";

	public UiEffectWidget[] UiEffects;

	public int InputDisabledTimeMs;

	public Transform MenuButtonRoot;

	public int CloseTimeoutTimeMs = 3000;

	public ScoreResultInfo[] ScoreResults;

	private Listener _listener;

	private bool _touchScreen;

	private EndCampaignProxy _endCampaignProxy;

	private Helper.LevelInfo _oldData;

	private Coroutine _effectCoroutine;

	private float _openedTime;

	private bool _inputEnabled;

	private float _closeTimeoutTime;

	public void Open(Listener listener, DungeonDifficultyType difficulty, EndType winType, int star)
	{
		Open(delegate
		{
			Init(listener, difficulty, winType, star);
		}, null);
	}

	public void Open(Listener listener, int resultStar)
	{
		int difficulty = resultStar / 10;
		int star = resultStar % 10;
		Open(listener, (DungeonDifficultyType)difficulty, EndType.Victory, star);
	}

	private void Init(Listener listener, DungeonDifficultyType difficulty, EndType endType, int star)
	{
		_listener = listener;
		_touchScreen = false;
		_openedTime = Time.time;
		_closeTimeoutTime = 0f;
		_inputEnabled = false;
		_oldData = Helper.GetAccountTeamLevelInfo();
		ShowMenuButton(show: false);
		if (_effectCoroutine != null)
		{
			VespaCoroutine.Stop(this, _effectCoroutine);
		}
		_effectCoroutine = VespaCoroutine.Start(this, coShowEffect(endType, difficulty, star));
		VespaCoroutine.Start(this, coLoop());
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		UpdateInputDisabled();
		UpdateCloseTimeout();
	}

	public void ActivateCloseTimeout()
	{
		_closeTimeoutTime = Time.time + (float)CloseTimeoutTimeMs / 1000f;
	}

	private void UpdateCloseTimeout()
	{
		if (!_touchScreen && !Mathf.Approximately(_closeTimeoutTime, 0f) && Time.time >= _closeTimeoutTime)
		{
			_touchScreen = true;
		}
	}

	public void OnTouchScreen()
	{
		if (_inputEnabled)
		{
			_touchScreen = true;
		}
	}

	private void UpdateInputDisabled()
	{
		if (!_inputEnabled && _endCampaignProxy != null && (Time.time - _openedTime) * 1000f >= (float)InputDisabledTimeMs)
		{
			_inputEnabled = true;
			if (_listener != null)
			{
				_listener.OnInputEnabled(this);
			}
		}
	}

	public void OnEndCampaignResponse(EndCampaignProxy endCampaignProxy)
	{
		_endCampaignProxy = endCampaignProxy;
	}

	private void ShowMenuButton(bool show)
	{
		UIHelper.EnableGameObject(MenuButtonRoot, show);
	}

	public void ShowChallengeRaidSingleResult(int rank, int changedRank, long bestScore, long bestRecord, long currentScore, long addedCurrentScore, long currentRecord)
	{
		ShowChallengeRaidSingleScoreResult(ScoreResultType.ChallengeRaidSingle, rank, changedRank, bestScore, bestRecord, currentScore, addedCurrentScore, currentRecord);
	}

	public void ShowChallengeRaidMultiResult(long bestScore, long currentScore, long addedCurrentScore, long bestRecord, long currentRecord)
	{
		ShowChallengeRaidMultiScoreResult(ScoreResultType.ChallengeRaidMulti, bestScore, currentScore, addedCurrentScore, bestRecord, currentRecord);
	}

	public void ShowGuildRaidResult(int rank, int changedRank, long totalScore, long addedScore)
	{
		ShowScoreResult(ScoreResultType.GuildRaid, rank, changedRank, totalScore, addedScore);
	}

	public void ShowWorldBossResult(int rank, int changedRank, long totalScore, long addedScore)
	{
		ShowScoreResult(ScoreResultType.WorldBoss, rank, changedRank, totalScore, addedScore);
	}

	public void ShowGuildSuppressResult(int rank, int changedRank, long totalScore, long addedScore)
	{
		ShowScoreResult(ScoreResultType.GuildSuppress, rank, changedRank, totalScore, addedScore);
	}

	private void ShowChallengeRaidSingleScoreResult(ScoreResultType type, int rank, int changedRank, long bestScore, long bestRecord, long currentcore, long addedCurrentScore, long currentRecord)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ScoreResults); i++)
		{
			ScoreResultInfo scoreResultInfo = ScoreResults[i];
			if (scoreResultInfo.Type != type)
			{
				UIHelper.EnableGameObject(scoreResultInfo.Root, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(scoreResultInfo.Root, enable: true);
			UIHelper.SetLabelText(scoreResultInfo.RankLabel, (rank > 0) ? rank.ToString() : "-");
			UIHelper.SetLabelText(scoreResultInfo.ChangedRankLabel, "+" + Mathf.Max(changedRank, 0));
			UIHelper.SetLabelText(scoreResultInfo.BestScoreLabel, bestScore.ToString());
			string elapseTimeLabelText = UIHelper.GetElapseTimeLabelText(bestRecord);
			UIHelper.SetLabelText(scoreResultInfo.BestRecordLabel, elapseTimeLabelText);
			UIHelper.SetLabelText(scoreResultInfo.TotalScoreLabel, currentcore.ToString());
			UIHelper.SetLabelText(scoreResultInfo.AddedScoreLabel, (addedCurrentScore >= 0) ? $"(+{addedCurrentScore.ToString()})" : "(-)");
			string elapseTimeLabelText2 = UIHelper.GetElapseTimeLabelText(currentRecord);
			UIHelper.SetLabelText(scoreResultInfo.CurrentRecordLabel, elapseTimeLabelText2);
			ShowMenuButton(show: true);
		}
	}

	private void ShowChallengeRaidMultiScoreResult(ScoreResultType type, long bestScore, long currentcore, long addedCurrentScore, long bestRecord, long currentRecord)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ScoreResults); i++)
		{
			ScoreResultInfo scoreResultInfo = ScoreResults[i];
			if (scoreResultInfo.Type != type)
			{
				UIHelper.EnableGameObject(scoreResultInfo.Root, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(scoreResultInfo.Root, enable: true);
			UIHelper.SetLabelText(scoreResultInfo.BestScoreLabel, bestScore.ToString());
			UIHelper.SetLabelText(scoreResultInfo.TotalScoreLabel, currentcore.ToString());
			UIHelper.SetLabelText(scoreResultInfo.AddedScoreLabel, (addedCurrentScore >= 0) ? $"(+{addedCurrentScore.ToString()})" : "(-)");
			string elapseTimeLabelText = UIHelper.GetElapseTimeLabelText(bestRecord);
			UIHelper.SetLabelText(scoreResultInfo.BestRecordLabel, elapseTimeLabelText);
			string elapseTimeLabelText2 = UIHelper.GetElapseTimeLabelText(currentRecord);
			UIHelper.SetLabelText(scoreResultInfo.CurrentRecordLabel, elapseTimeLabelText2);
			ShowMenuButton(show: true);
		}
	}

	private void ShowScoreResult(ScoreResultType type, int rank, int changedRank, long totalScore, long addedScore)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(ScoreResults); i++)
		{
			ScoreResultInfo scoreResultInfo = ScoreResults[i];
			if (scoreResultInfo.Type != type)
			{
				UIHelper.EnableGameObject(scoreResultInfo.Root, enable: false);
				continue;
			}
			UIHelper.EnableGameObject(scoreResultInfo.Root, enable: true);
			UIHelper.SetLabelText(scoreResultInfo.RankLabel, (rank > 0) ? rank.ToString() : "-");
			UIHelper.SetLabelText(scoreResultInfo.ChangedRankLabel, "+" + Mathf.Max(changedRank, 0));
			UIHelper.SetLabelText(scoreResultInfo.TotalScoreLabel, Vespa_Long_Extension_Method.ToCommaString(totalScore));
			UIHelper.SetLabelText(scoreResultInfo.AddedScoreLabel, (addedScore >= 0) ? Vespa_Long_Extension_Method.ToCommaString(addedScore) : "-");
			ShowMenuButton(show: true);
		}
	}

	public void OnClickReturnToLobby()
	{
		if (_listener != null)
		{
			_listener.OnClickReturnToLobby(this);
		}
	}

	public void OnClickStatistics()
	{
		if (_listener != null)
		{
			_listener.ShowStatistics();
		}
	}

	private IEnumerator coShowEffect(EndType endType, DungeonDifficultyType difficulty, int star)
	{
		UiEffectWidget uiEffect = null;
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(UiEffects); i++)
		{
			if (UiEffects[i].Type == endType && UiEffects[i].Difficulty == difficulty)
			{
				uiEffect = UiEffects[i];
				break;
			}
		}
		if (uiEffect != null)
		{
			uiEffect.StopAll(immediately: true);
			yield return null;
			if (uiEffect.Mark != null)
			{
				uiEffect.Mark.Play(null);
				yield return new WaitForSeconds(uiEffect.MarkTime);
			}
			int starCount = Mathf.Min(star, uiEffect.Star.Length);
			for (int index = 0; index < starCount; index++)
			{
				if (!(uiEffect.Star[index] == null))
				{
					uiEffect.Star[index].Play(null);
					if (index < starCount - 1)
					{
						yield return new WaitForSeconds(uiEffect.StarTime);
					}
					else
					{
						yield return new WaitForSeconds(uiEffect.LastStarTime);
					}
				}
			}
			if (uiEffect.Text != null)
			{
				uiEffect.Text.Play(null);
			}
		}
		_effectCoroutine = null;
	}

	private IEnumerator coLoop()
	{
		while (_endCampaignProxy == null)
		{
			yield return null;
		}
		yield return new WaitForSeconds(1f);
		_touchScreen = false;
		float elapsedTime = 0f;
		while (!_touchScreen)
		{
			elapsedTime += UnitySystemTimer.unscaledDeltaTime;
			if (elapsedTime > 1f)
			{
				if (GameOptionData.EditorCanAutoPlay)
				{
					_touchScreen = true;
					continue;
				}
				if (AutoRepeatBattle.IsEnabled() || AutoNextBattle.IsEnabled())
				{
					_touchScreen = true;
					continue;
				}
			}
			yield return null;
		}
		if (_listener.OnCloseRequest(this))
		{
			Close();
		}
	}
}
