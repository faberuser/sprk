using System;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NUI.NWindow;
using NNguiExtension;
using NShared;
using NShared.NLocalization;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class HeroGetExpComponent : LinkFactoryComponent, InitOnCreate
{
	public enum ExpStyle
	{
		None,
		HeroExp,
		FlaskExp
	}

	public enum ECheckAddHeroExpResult
	{
		None,
		Success,
		MaxHeroLevel,
		ItemNotSelected,
		FlaskEquipped,
		CanLimitBreak
	}

	public interface Listener
	{
		void ApplyUseExpPotion(CachedHeroInfo heroInfo, int ItemIndex, int UseItemCount);

		void UsePotion(int count);

		void EquipFlask(CachedHeroInfo heroInfo, int itemIndex);

		void UnEquipFlask(CachedHeroInfo heroInfo);
	}

	public interface IDataProvider
	{
		ItemInfo GetSelectedItemInfo();

		int GetAddHeroExp();
	}

	public const string PrefabPath = "Prefab2/Component/Hero/HeroGetExpComponent";

	public static PrefabAuto<HeroGetExpComponent> PrefabAuto = new PrefabAuto<HeroGetExpComponent>("Prefab2/Component/Hero/HeroGetExpComponent");

	public UISimpleGrid HeroSmallPortraitGrid;

	public UILabel Label_HeroLevel;

	public UILabel Label_HeroName;

	public UIWidget HeroExpRoot;

	public UILabel Label_HeroExp;

	public UILabel Label_HeroExpPercent;

	public UILabel Label_AddedHeroExp;

	public UISprite GaugeSprite;

	public AnimationCurve GaugeCurve;

	public float MinTimeLength;

	public float MaxTimeLength;

	public float PressDelaySecond = 1f;

	public float UseSoundPlayInterval = 1f;

	public UIPlayAssetSound UseSound;

	private float _elapsedTime;

	public UISpriteSelector flaskSelector;

	public UIWidget Root;

	public UIGame3DWidget LevelUpEffect;

	public UIButton ButtonUseMax;

	private float _startTime;

	private HeroPortraitComponent _heroPortrait;

	private GaugeAnimator _gaugeAnimator;

	private bool _pressed;

	private DateTime _pressedTime = DateTime.MinValue;

	public UIWidget FlaskExpRoot;

	public UILabel Label_FlaskExp;

	public UISprite FlaskExpGuageSprite;

	public UISprite AddFlaskSprite;

	public UISpriteSelector EquipFlaskSelector;

	public UISpriteSelector GaugeSelector;

	private ExpStyle CurrentExpStyle = ExpStyle.HeroExp;

	private int _UsedPotion;

	private int _TotalAddedExp;

	private int _AddHeroExp;

	private int _PrevHeroLevel;

	private int _ChangeHeroLevel;

	private List<int> _ListNeededExp = new List<int>();

	private int _SelectedExpPotionItemIndex;

	private Listener _listener;

	private IDataProvider _dataProvider;

	private int LevelUpCount;

	private DateTime _LastUseSoundPlayTime = DateTime.MinValue;

	private ECheckAddHeroExpResult _CheckResult;

	public CachedHeroInfo HeroInfo { get; private set; }

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		BoxCollider component = _heroPortrait.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = true;
		}
		FactoryBase.SafeReleaseComponent(ref _heroPortrait);
		ResetExpGaugeAniVariables();
		_TotalAddedExp = 0;
		_gaugeAnimator = null;
		_listener = null;
		_dataProvider = null;
		_LastUseSoundPlayTime = DateTime.MinValue;
	}

	public void Open(CachedHeroInfo heroInfo, Listener listener, IDataProvider dataProvider)
	{
		HeroInfo = heroInfo;
		_listener = listener;
		_dataProvider = dataProvider;
		Init();
	}

	public void Init()
	{
		SetupHeroPortrait();
		SetupHeroInfo();
		SetupFlaskInfo();
	}

	private void Update()
	{
		UpdateUseExpPotionAni();
		UpdateExpGauge();
		PlayLevelUpEffect();
		PlayUsePotionSound();
	}

	private void UpdateExpGauge()
	{
		if (_gaugeAnimator != null && !_gaugeAnimator.IsCompleted)
		{
			_gaugeAnimator.Update();
			int num = _PrevHeroLevel + (int)(_gaugeAnimator.CurrentPercentage / 100f);
			UIHelper.SetLabelText(Label_HeroLevel, "Lv " + num);
		}
	}

	public void SetupFlaskInfo()
	{
		if (HeroInfo.FlaskItemIndex != 0)
		{
			ItemData<FlaskItemData> data = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(HeroInfo.FlaskItemIndex);
			if (data != null)
			{
				if (flaskSelector != null)
				{
					UIHelper.EnableGameObject(flaskSelector, enable: true);
					flaskSelector.Select(data.Code);
				}
				if (EquipFlaskSelector != null)
				{
					UIHelper.EnableGameObject(EquipFlaskSelector, enable: true);
					EquipFlaskSelector.Select(data.Code);
				}
				UIHelper.EnableGameObject(AddFlaskSprite, enable: false);
				if (CurrentExpStyle == ExpStyle.HeroExp)
				{
					UIHelper.SetWidgetGrayScale(Root, grayScale: true);
				}
				GaugeSelector.Select(data.SubData.Type.ToString());
				SetFlaskExp(HeroInfo.FlaskExp, data.SubData.FlaskExp);
				return;
			}
		}
		UIHelper.EnableGameObject(flaskSelector, enable: false);
		UIHelper.EnableGameObject(EquipFlaskSelector, enable: false);
		UIHelper.EnableGameObject(AddFlaskSprite, enable: true);
		SetUnequipFlaskExp();
		if (CurrentExpStyle == ExpStyle.HeroExp)
		{
			UIHelper.SetWidgetGrayScale(Root, grayScale: false);
		}
	}

	private void SetupHeroPortrait()
	{
		_heroPortrait = UIHelper.AddGridItem(HeroSmallPortraitGrid, HeroPortraitComponent.SmallPrefabAuto);
		_heroPortrait.Open(HeroInfo, null, null, HeroPortraitComponent.EPortraitMode.Icon);
		_heroPortrait.SetLevel(show: false);
		BoxCollider component = _heroPortrait.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.enabled = false;
		}
		UIHelper.RepositionGrid(HeroSmallPortraitGrid);
	}

	private void SetupHeroInfo()
	{
		UIHelper.SetLabelText(Label_HeroLevel, "Lv" + HeroInfo.Level);
		CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		UIHelper.SetLabelText(Label_HeroName, (data != null) ? data.Name : string.Empty);
		int exp = HeroInfo.Exp;
		long expForNextLevel = 0L;
		Helper.GetHeroExpForNextLevel(HeroInfo, out expForNextLevel);
		if (expForNextLevel == 0L || HeroInfo.IsMaxLevel())
		{
			UIHelper.SetLabelText(Label_HeroExp, "MAX");
			UIHelper.SetSpriteFillAmount(GaugeSprite, 1f);
			UIHelper.SetLabelText(Label_HeroExpPercent, "100%");
			UIHelper.EnableButton(ButtonUseMax, enable: false);
		}
		else
		{
			string text = $"{exp}/{expForNextLevel}";
			UIHelper.SetLabelText(Label_HeroExp, text);
			float num = (float)exp / (float)expForNextLevel;
			UIHelper.SetSpriteFillAmount(GaugeSprite, num);
			UIHelper.SetLabelText(Label_HeroExpPercent, GetHeroExpPercent(num * 100f));
			UIHelper.EnableButton(ButtonUseMax, enable: true);
		}
		SetTotalAddedHeroExp();
		_ChangeHeroLevel = HeroInfo.Level;
	}

	private string GetHeroExpDisplayValue(int curTotalExp)
	{
		int level = 1;
		long localExp = 0L;
		long localExpOfLevel = 0L;
		GameRoot.Instance.CreatureLevelDataContainer.ParseTotalExpToLocal(curTotalExp, out level, out localExp, out localExpOfLevel);
		level = System.Math.Max(level, HeroInfo.Level);
		if (level >= HeroInfo.GetMaxLevel())
		{
			return "MAX";
		}
		int num = 0;
		int num2 = 0;
		if (curTotalExp == Helper.GetMaxHeroTotalExp())
		{
			return "MAX";
		}
		for (int i = 0; i < _ListNeededExp.Count; i += 2)
		{
			num = _ListNeededExp[i];
			num2 = _ListNeededExp[i + 1];
			if (curTotalExp < num2 && curTotalExp >= num)
			{
				break;
			}
		}
		return $"{curTotalExp - num}/{Mathf.Max(0, num2 - num)}";
	}

	public void OnPressComponent()
	{
		if (CurrentExpStyle != ExpStyle.FlaskExp)
		{
			_pressed = true;
			InitExpGaugeAniVariables();
		}
	}

	private void TryUseExpPotionAni()
	{
		ECheckAddHeroExpResult eCheckAddHeroExpResult = HeroExpAddable();
		if (eCheckAddHeroExpResult != ECheckAddHeroExpResult.Success)
		{
			if (_UsedPotion > 0)
			{
				FinalizeReleaseComponent();
			}
			if (_CheckResult != eCheckAddHeroExpResult)
			{
				_CheckResult = eCheckAddHeroExpResult;
				DisplayCannotApplyHeroExpMessage();
			}
		}
		else if (InitGaugeAnimator())
		{
			if (_listener != null)
			{
				_listener.UsePotion(1);
			}
			_UsedPotion++;
			_TotalAddedExp += _AddHeroExp;
			SetTotalAddedHeroExp();
		}
	}

	private void UpdateUseExpPotionAni()
	{
		if (!_pressed)
		{
			return;
		}
		if (CurrentExpStyle == ExpStyle.FlaskExp)
		{
			if (_pressed)
			{
				OnReleaseComponent();
			}
			return;
		}
		DateTime now = DateTime.Now;
		if (_pressedTime.CompareTo(DateTime.MinValue) == 0)
		{
			_pressedTime = now;
		}
		if (!((now - _pressedTime).TotalSeconds < (double)PressDelaySecond) && ExpAniCompleted())
		{
			TryUseExpPotionAni();
		}
	}

	private string GetHeroExpPercent(float value)
	{
		if (_gaugeAnimator != null && _gaugeAnimator.CurrentValue == Helper.GetMaxHeroTotalExp())
		{
			return "100%";
		}
		return (int)value % 100 + "%";
	}

	private bool InitGaugeAnimator()
	{
		if (_gaugeAnimator == null)
		{
			_gaugeAnimator = new GaugeAnimator();
			_gaugeAnimator.SetCurve(GaugeCurve);
			_gaugeAnimator.SetGaugeSprite(new UISprite[1] { GaugeSprite });
			_gaugeAnimator.SetLabel(Label_HeroExpPercent, GetHeroExpPercent);
			_gaugeAnimator.SetLabel2(Label_HeroExp, GetHeroExpDisplayValue);
			_gaugeAnimator.SetMaxButton(ButtonUseMax);
		}
		_ListNeededExp.Clear();
		_startTime = Time.time;
		long totalExp = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(HeroInfo.GetMaxLevel(), 0L);
		long totalExp2 = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(HeroInfo.Level, HeroInfo.Exp);
		long num = System.Math.Min(totalExp, totalExp2 + _AddHeroExp * _UsedPotion);
		long num2 = System.Math.Min(totalExp, num + _AddHeroExp);
		_PrevHeroLevel = _ChangeHeroLevel;
		Helper.GetChangedHeroLevelExpInfo(_PrevHeroLevel, num2, _ListNeededExp, out _ChangeHeroLevel);
		if (_ListNeededExp.Count == 0)
		{
			return false;
		}
		GameRoot.Instance.CreatureLevelDataContainer.ParseTotalExpToLocal(num, out _PrevHeroLevel, out var localExp, out var localExpOfLevel);
		_PrevHeroLevel = System.Math.Max(_PrevHeroLevel, HeroInfo.Level);
		GameRoot.Instance.CreatureLevelDataContainer.ParseTotalExpToLocal(num2, out _ChangeHeroLevel, out var localExp2, out var localExpOfLevel2);
		_ChangeHeroLevel = System.Math.Max(_ChangeHeroLevel, HeroInfo.Level);
		float startPercentage = ((localExpOfLevel != 0L) ? ((float)localExp / (float)localExpOfLevel * 100f) : 100f);
		float targetPercentage = ((localExpOfLevel2 == 0L) ? ((float)((_ChangeHeroLevel - _PrevHeroLevel) * 100)) : ((float)((_ChangeHeroLevel - _PrevHeroLevel) * 100) + (float)localExp2 / (float)localExpOfLevel2 * 100f));
		if (_ChangeHeroLevel >= HeroInfo.GetMaxLevel())
		{
			_ChangeHeroLevel = HeroInfo.GetMaxLevel();
			targetPercentage = (_ChangeHeroLevel - _PrevHeroLevel) * 100;
			num2 = totalExp;
		}
		_gaugeAnimator.Init(startPercentage, targetPercentage, num2 == totalExp);
		_gaugeAnimator.InitDisplayValue((int)num, (int)num2);
		_gaugeAnimator.TimeLength = GetTimeLength();
		LevelUpCount += _ChangeHeroLevel - _PrevHeroLevel;
		return true;
	}

	private void SetTotalAddedHeroExp()
	{
		string text = ((_TotalAddedExp == 0) ? string.Empty : ("+" + _TotalAddedExp));
		UIHelper.SetLabelText(Label_AddedHeroExp, text);
	}

	private float GetTimeLength()
	{
		float num = 1f;
		_elapsedTime += (float)UnitySystemTimer.unscaledDeltaTick * 0.001f;
		float t = Mathf.Clamp01(_elapsedTime / num);
		return MaxTimeLength - Mathf.Lerp(0f, MaxTimeLength - MinTimeLength, t);
	}

	private void PlayLevelUpEffect()
	{
		if (!(LevelUpEffect == null) && LevelUpCount != 0)
		{
			if (LevelUpEffect.IsPlaying)
			{
				LevelUpEffect.Stop(immediatelyClear: true);
			}
			LevelUpEffect.Play(null);
			LevelUpCount--;
		}
	}

	private void PlayUsePotionSound()
	{
		if (_UsedPotion != 0 && !(UseSound == null))
		{
			DateTime now = DateTime.Now;
			if (!((now - _LastUseSoundPlayTime).TotalSeconds < (double)UseSoundPlayInterval))
			{
				_LastUseSoundPlayTime = now;
				UseSound.Play();
			}
		}
	}

	private bool ExpAniCompleted()
	{
		if (_gaugeAnimator != null)
		{
			return _gaugeAnimator.IsCompleted;
		}
		return true;
	}

	public void FinalizeReleaseComponent()
	{
		if (_listener != null && _UsedPotion > 0)
		{
			_listener.ApplyUseExpPotion(HeroInfo, _SelectedExpPotionItemIndex, _UsedPotion);
		}
		ResetExpGaugeAniVariables();
	}

	public void OnReleaseComponent()
	{
		if (_pressed)
		{
			if (_UsedPotion == 0)
			{
				FinalizeReleaseComponent();
				OnClickMax();
				SprkHeroExpLevel.Attach(this, _AddHeroExp);
			}
			else
			{
				FinalizeReleaseComponent();
			}
		}
	}

	private void InitExpGaugeAniVariables()
	{
		_AddHeroExp = _dataProvider.GetAddHeroExp();
		_SelectedExpPotionItemIndex = _dataProvider.GetSelectedItemInfo()?.ItemIndex ?? 0;
	}

	private void ResetExpGaugeAniVariables()
	{
		_CheckResult = ECheckAddHeroExpResult.None;
		_AddHeroExp = 0;
		_UsedPotion = 0;
		_SelectedExpPotionItemIndex = 0;
		_pressed = false;
		_elapsedTime = 0f;
		_pressedTime = DateTime.MinValue;
	}

	private void DisplayCannotApplyHeroExpMessage()
	{
		string text = null;
		switch (_CheckResult)
		{
		case ECheckAddHeroExpResult.ItemNotSelected:
			text = LocalizationManager.GetString(Item.EXPPOTION_NOT_SELECTED);
			break;
		case ECheckAddHeroExpResult.MaxHeroLevel:
			text = LocalizationManager.GetString(Item.EXPPOTION_HERO_MAXLEVEL);
			break;
		case ECheckAddHeroExpResult.FlaskEquipped:
			text = LocalizationManager.GetString(Item.EXPPOTION_DESELECT_POTION_MESSAGE);
			break;
		case ECheckAddHeroExpResult.CanLimitBreak:
			text = LocalizationManager.GetStringDirect("LIMITBREAK_NORMAL_EXPUP_NOT_INFO");
			break;
		}
		if (text != null)
		{
			CenterMessage.Open(text);
		}
	}

	private ECheckAddHeroExpResult HeroExpAddable()
	{
		if (HeroInfo.FlaskItemIndex != 0)
		{
			return ECheckAddHeroExpResult.FlaskEquipped;
		}
		if (HeroInfo.IsMaxLevel())
		{
			if (HeroInfo.CanIncLimitBreakLevel())
			{
				return ECheckAddHeroExpResult.CanLimitBreak;
			}
			return ECheckAddHeroExpResult.MaxHeroLevel;
		}
		int maxHeroTotalExp = Helper.GetMaxHeroTotalExp();
		long num = (GameRoot.Instance.CreatureLevelDataContainer.GetData(HeroInfo.Level)?.TotalExp ?? 0) + HeroInfo.Exp;
		if (Mathf.Min(maxHeroTotalExp, num + _AddHeroExp * _UsedPotion) == (float)maxHeroTotalExp)
		{
			if (HeroInfo.CanIncLimitBreakLevel())
			{
				return ECheckAddHeroExpResult.CanLimitBreak;
			}
			return ECheckAddHeroExpResult.MaxHeroLevel;
		}
		if (_dataProvider.GetSelectedItemInfo() == null)
		{
			return ECheckAddHeroExpResult.ItemNotSelected;
		}
		return ECheckAddHeroExpResult.Success;
	}

	public void SwitchExpMode(ExpStyle style)
	{
		CurrentExpStyle = style;
		bool enable = style == ExpStyle.HeroExp;
		bool enable2 = style == ExpStyle.FlaskExp;
		UIHelper.EnableGameObject(HeroExpRoot, enable);
		UIHelper.EnableGameObject(FlaskExpRoot, enable2);
		switch (style)
		{
		case ExpStyle.HeroExp:
			UIHelper.EnableGameObject(ButtonUseMax, enable: true);
			SetupFlaskInfo();
			break;
		case ExpStyle.FlaskExp:
			UIHelper.EnableGameObject(ButtonUseMax, enable: false);
			UIHelper.SetWidgetGrayScale(Root, grayScale: false);
			break;
		}
	}

	private void SetFlaskExp(int flaskExp, int flaskMaxExp)
	{
		if (flaskMaxExp == 0)
		{
			VespaDebug.LogError("FlaskMaxExp is 0. index=" + HeroInfo.FlaskItemIndex);
			return;
		}
		int num = Mathf.Clamp(flaskExp * 100 / flaskMaxExp, 0, 100);
		UIHelper.SetLabelText(Label_FlaskExp, $"{num} %");
		UIHelper.SetSpriteFillAmount(FlaskExpGuageSprite, (float)num / 100f);
	}

	private void SetUnequipFlaskExp()
	{
		string text = LocalizationManager.GetString(HeroFlask.ENABLE_FLASK_EQUIP);
		UIHelper.SetLabelText(Label_FlaskExp, text);
		UIHelper.SetSpriteFillAmount(FlaskExpGuageSprite, 0f);
	}

	public void OnClickComponent()
	{
		if (CurrentExpStyle == ExpStyle.FlaskExp)
		{
			OnClickFlask();
		}
	}

	private void OnClickFlask()
	{
		if (_listener == null)
		{
			return;
		}
		if (HeroInfo.FlaskItemIndex != 0)
		{
			_listener.UnEquipFlask(HeroInfo);
			return;
		}
		ItemInfo selectedItemInfo = _dataProvider.GetSelectedItemInfo();
		int itemIndex = 0;
		if (selectedItemInfo != null)
		{
			itemIndex = selectedItemInfo.ItemIndex;
		}
		_listener.EquipFlask(HeroInfo, itemIndex);
	}

	public void OnClickMax()
	{
		ECheckAddHeroExpResult eCheckAddHeroExpResult = HeroExpAddable();
		if (eCheckAddHeroExpResult != ECheckAddHeroExpResult.Success)
		{
			if (_UsedPotion > 0)
			{
				FinalizeReleaseComponent();
			}
			if (_CheckResult != eCheckAddHeroExpResult)
			{
				_CheckResult = eCheckAddHeroExpResult;
				DisplayCannotApplyHeroExpMessage();
			}
			return;
		}
		ItemInfo itemInfo = _dataProvider.GetSelectedItemInfo();
		InitExpGaugeAniVariables();
		long MaxHeroExp = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(HeroInfo.GetMaxLevel(), 0L);
		long totalExp = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(HeroInfo.Level, HeroInfo.Exp);
		long curHeroTotalExp = System.Math.Min(MaxHeroExp, totalExp + _AddHeroExp * _UsedPotion);
		long num = MaxHeroExp - curHeroTotalExp;
		int usablePotion = Convert.ToInt32(num / _AddHeroExp);
		if (num % _AddHeroExp > 0)
		{
			usablePotion++;
		}
		usablePotion = System.Math.Min(usablePotion, itemInfo.Count);
		long nextHeroTotalExp = System.Math.Min(MaxHeroExp, curHeroTotalExp + _AddHeroExp * usablePotion);
		_ListNeededExp.Clear();
		int changeLevel = HeroInfo.Level;
		short num2 = HeroInfo.Level;
		while (true)
		{
			CreatureLevelData data = GameRoot.Instance.CreatureLevelDataContainer.GetData(num2);
			if (data == null || data.TotalExp > nextHeroTotalExp)
			{
				break;
			}
			changeLevel = num2;
			num2++;
		}
		LevelUpMaxPopup.Open(HeroInfo, itemInfo, usablePotion, usablePotion * _AddHeroExp, changeLevel, delegate
		{
			UseMax(itemInfo, MaxHeroExp, curHeroTotalExp, nextHeroTotalExp, usablePotion);
		}, null);
	}

	private void UseMax(ItemInfo itemInfo, long MaxHeroExp, long curHeroTotalExp, long nextHeroTotalExp, int usablePotion)
	{
		_UsedPotion = usablePotion;
		_PrevHeroLevel = _ChangeHeroLevel;
		Helper.GetChangedHeroLevelExpInfo(_PrevHeroLevel, nextHeroTotalExp, _ListNeededExp, out _ChangeHeroLevel);
		GameRoot.Instance.CreatureLevelDataContainer.ParseTotalExpToLocal(curHeroTotalExp, out _PrevHeroLevel, out var _, out var _);
		_PrevHeroLevel = System.Math.Max(_PrevHeroLevel, HeroInfo.Level);
		GameRoot.Instance.CreatureLevelDataContainer.ParseTotalExpToLocal(nextHeroTotalExp, out _ChangeHeroLevel, out var localExp2, out var localExpOfLevel2);
		_ChangeHeroLevel = System.Math.Max(_ChangeHeroLevel, HeroInfo.Level);
		if (_ChangeHeroLevel >= HeroInfo.GetMaxLevel())
		{
			_ChangeHeroLevel = HeroInfo.GetMaxLevel();
			nextHeroTotalExp = MaxHeroExp;
		}
		UIHelper.SetLabelText(Label_HeroLevel, "Lv " + _ChangeHeroLevel);
		UIHelper.SetLabelText(Label_HeroExp, GetHeroExpDisplayValue((int)nextHeroTotalExp));
		if (_ChangeHeroLevel >= HeroInfo.GetMaxLevel())
		{
			UIHelper.SetSpriteFillAmount(GaugeSprite, 1f);
			UIHelper.EnableButton(ButtonUseMax, enable: false);
		}
		else
		{
			UIHelper.SetSpriteFillAmount(GaugeSprite, (float)localExp2 / (float)localExpOfLevel2);
			UIHelper.EnableButton(ButtonUseMax, enable: true);
		}
		if (_PrevHeroLevel < _ChangeHeroLevel)
		{
			LevelUpCount++;
			PlayLevelUpEffect();
		}
		if (_listener != null)
		{
			_listener.UsePotion(_UsedPotion);
		}
		FinalizeReleaseComponent();
	}
}
