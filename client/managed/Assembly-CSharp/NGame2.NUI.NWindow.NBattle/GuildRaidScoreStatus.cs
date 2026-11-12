using System.Collections.Generic;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class GuildRaidScoreStatus : BaseWindow
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

	public const string PrefabPath = "Resources/Prefab2/Window/Battle/GuildRaidScoreStatus";

	public UILabel ScoreLabel;

	private BattleScoreContext _scoreContext = new BattleScoreContext();

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
		DataProvider = dataProvider;
		BattleScoreHelper.UpdateScore(ScoreLabel, _scoreContext);
	}

	public override void Reset()
	{
		base.Reset();
		_scoreContext.Reset();
	}

	public override void UpdateDerived()
	{
		base.UpdateDerived();
		BattleScoreHelper.UpdateScore(ScoreLabel, _scoreContext);
		UpdatePosition();
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
