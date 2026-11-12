using NGame2.NBattleField;
using NGame2.NUI.NWindow;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class ShardGaugeComponent : LinkFactoryComponent, InitOnCreate
{
	private enum EInfoType
	{
		Free,
		Premium,
		Time,
		Hide
	}

	public static string PrefabPath = "Resources/Prefab2/Component/Raid/ShardGaugeComponent";

	public static PrefabAuto<ShardGaugeComponent> PrefabAuto = new PrefabAuto<ShardGaugeComponent>(PrefabPath);

	public UIBundleLoader FlaskBudnleLoader;

	public UISprite GaugeSprite;

	public GameObject Root_FreeFlask_OuterFrame;

	public UILabel FreeLabel;

	public GameObject Root_PremiumFlask_OuterFrame;

	public UILabel PremiumLabel;

	public UILabel TimeLabel;

	public UIGame3DWidget FullCountLoopEffect;

	public UIGame3DWidget FullGagueEffect;

	public bool _isFree;

	private long _totalCount;

	private long _flaskGague;

	private long _prevFullFlaskGague;

	private float _curFlaskGague;

	private long _totalFlaskGauge;

	private long _cachedFlaskCount;

	private bool _isFull;

	private NGame2.NUI.NWindow.IntervalUpdater _updater;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		_updater = null;
	}

	public void OpenShardItem(int index, int totalCount, bool isBoss)
	{
		SetGaugeSpriteInvert(isInvert: true);
		_totalCount = totalCount;
		ShardItemData data = GameRoot.Instance.ShardItemDataContainer.GetData(index);
		if (data == null)
		{
			return;
		}
		_isFree = GameRoot.Instance.ShardFlaskItemDataContainer.IsDefaultShardFlask(index);
		string battleIconPath = data.BattleIconPath;
		UIHelper.SetBundleTexture(FlaskBudnleLoader, battleIconPath);
		EInfoType showLabel = ((totalCount > 0) ? EInfoType.Time : EInfoType.Hide);
		SetShowLabel(showLabel);
		SetShowFrame(_isFree, isBoss);
		float gauge = ((totalCount == 0) ? 0f : 1f);
		SetGauge(gauge);
		int duration = ((totalCount != 0) ? (data.Duration * totalCount * 1000) : 0);
		int num = duration;
		int num2 = 1000;
		int num3 = num;
		string remainTimeTextAll = UIHelper.GetRemainTimeTextAll((long)num3 / (long)num2, showDay: false);
		UIHelper.SetLabelText(TimeLabel, remainTimeTextAll);
		if (duration <= 0)
		{
			return;
		}
		_updater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
		{
			long globalCurrentTimeMs = SingletonAuto<BattleFacade>.instance.GetBattleCache().GlobalCurrentTimeMs;
			if (duration < globalCurrentTimeMs)
			{
				SetGauge(0f);
				int showSec = 1;
				int showMin = 1;
				int showHour = 1;
				int showDay = 0;
				int num4 = 0;
				string remainTimeTextAll2 = UIHelper.GetRemainTimeTextAll(num4, (byte)showDay != 0, (byte)showHour != 0, (byte)showMin != 0, (byte)showSec != 0);
				UIHelper.SetLabelText(TimeLabel, remainTimeTextAll2);
				_updater = null;
			}
			else
			{
				int num5 = duration;
				long num6 = globalCurrentTimeMs;
				int num7 = num5;
				long num8 = checked(num7 - num6);
				float gauge2 = (float)num8 / (float)duration;
				long num9 = num8;
				int num10 = 0;
				if (num9 < num10)
				{
					int num11 = 0;
					num8 = num11;
				}
				long num12 = num8;
				int num13 = 1000;
				string remainTimeTextAll3 = UIHelper.GetRemainTimeTextAll(num12 / num13, showDay: false);
				SetGauge(gauge2);
				UIHelper.SetLabelText(TimeLabel, remainTimeTextAll3);
			}
		}, 0.1f);
	}

	public void OpenShardFlask(int index, int totalCount, long prevFullFlaskGauge)
	{
		_prevFullFlaskGague = prevFullFlaskGauge;
		ShardFlaskItemData data = GameRoot.Instance.ShardFlaskItemDataContainer.GetData(index);
		if (data == null)
		{
			return;
		}
		string battleIconPath = data.BattleIconPath;
		UIHelper.SetBundleTexture(FlaskBudnleLoader, battleIconPath);
		_totalCount = totalCount;
		_isFree = data.IsDefault;
		_flaskGague = data.FlaskGauge;
		_totalFlaskGauge = _flaskGague * totalCount;
		EInfoType showLabel = ((!_isFree) ? EInfoType.Premium : EInfoType.Free);
		SetShowFrame(_isFree, hide: false);
		SetShowLabel(showLabel);
		SetShardFlaskLabelText(0L);
		SetGauge(0f);
		_updater = new NGame2.NUI.NWindow.IntervalUpdater(delegate
		{
			float num = _curFlaskGague - (float)prevFullFlaskGauge;
			if (!(num < 0f) && !_isFull)
			{
				long num2 = (long)(num / (float)_flaskGague);
				float gauge = num % (float)_flaskGague / (float)_flaskGague;
				SetGauge(gauge);
				if (num2 != _cachedFlaskCount)
				{
					if (!_isFree && (float)_totalFlaskGauge < num)
					{
						SetGauge(1f);
						FullCountLoopEffect.Play(null);
						_isFull = true;
					}
					else
					{
						FullGagueEffect.Play(null);
					}
					_cachedFlaskCount = num2;
					SetShardFlaskLabelText(_cachedFlaskCount);
				}
			}
		}, 0.1f);
	}

	public void SetShardFlaskLabelText(long count)
	{
		if (_isFree)
		{
			UIHelper.SetLabelText(FreeLabel, count);
		}
		else
		{
			UIHelper.SetLabelText(PremiumLabel, $"[ffe12e]{count}[-]/ {_totalCount}");
		}
	}

	private void SetGaugeSpriteInvert(bool isInvert)
	{
		if (GaugeSprite != null)
		{
			GaugeSprite.invert = true;
		}
	}

	private void SetShowLabel(EInfoType eInfoType)
	{
		UIHelper.EnableGameObject(TimeLabel, eInfoType == EInfoType.Time);
		UIHelper.EnableGameObject(FreeLabel, eInfoType == EInfoType.Free);
		UIHelper.EnableGameObject(PremiumLabel, eInfoType == EInfoType.Premium);
	}

	private void SetShowFrame(bool isFree, bool hide)
	{
		UIHelper.EnableGameObject(Root_FreeFlask_OuterFrame, !hide && isFree);
		UIHelper.EnableGameObject(Root_PremiumFlask_OuterFrame, !hide && !isFree);
	}

	public void OnUpdate()
	{
		if (_updater != null)
		{
			_updater.Update();
		}
	}

	public void OnUpdate(float flaskGaguge)
	{
		_curFlaskGague = flaskGaguge;
		if (_updater != null)
		{
			_updater.Update();
		}
	}

	public void SetGauge(float fillAmount)
	{
		if (!(GaugeSprite == null))
		{
			GaugeSprite.fillAmount = fillAmount;
		}
	}
}
