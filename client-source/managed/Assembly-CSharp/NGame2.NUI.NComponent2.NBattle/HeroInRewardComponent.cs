using System;
using System.Collections;
using NGame2.N3D;
using NGame2.NAccount;
using NGame2.NRender;
using NGame2.NRender.NRenderFacade;
using NGame2.NUI.NManager;
using NGame2.NUI.NWindow;
using NGame2.NUI.NWindow.NBattle;
using NGameAsset;
using NGameAsset.NCreature;
using NGameAsset.NSound;
using NNguiExtension;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NBattle;

public class HeroInRewardComponent : LinkFactoryComponent, IInspectorGuiDrawer
{
	[Serializable]
	public class FlaskSourceIconInfo
	{
		public FlaskSourceType Type;

		public GameObject Icon;
	}

	[Serializable]
	public class FlaskFilledEffect
	{
		public string Code;

		public UIGame3DWidget FilledEffect;
	}

	public const string PrefabPath = "Prefab2/Component/Battle/HeroInRewardComponent";

	public static PrefabAuto<HeroInRewardComponent> PrefabAuto = new PrefabAuto<HeroInRewardComponent>("Prefab2/Component/Battle/HeroInRewardComponent");

	private const string VictoryAnimationKey = "Victory";

	private const string LoseAnimationKey = "Lose";

	public UIWidget RootWidget;

	public UILabel AddExpLabel;

	public UILabel LevelLabel;

	public UISprite ExpGauge;

	public UISprite FlaskExpGauge;

	public UIGame3DWidget FlaskExpLuckyEffect;

	public UIGame3DWidget FlaskExpFailEffect;

	public FlaskSourceIconInfo[] FlaskSourceIcons;

	public UILabel ExpPercentLabel;

	public Transform RenderPivot;

	public UIWidget DeadWidget;

	public float LevelupEffectExtraScale = 0.5f;

	public float LevelupEffectOffsetMultiply = 0.7f;

	public float LevelupTextOffset = 0.1f;

	public UIWidget NickNameRoot;

	public UILabel NickNameLabel;

	public UIButton RequestFriendButton;

	private CreatureData _creatureData;

	protected bool _isDead;

	public UISpriteSelector flaskSelector;

	public UISpriteSelector gaugeSelector;

	private float _time;

	private long _prev_hero_exp;

	private long _cur_hero_exp;

	private int _prev_hero_level;

	private int _cur_hero_level;

	private int _add_booster_exp;

	private int _add_teamlevel_exp;

	private int _prev_flask_exp;

	private int _cur_flask_exp;

	private int _flask_capacity;

	private bool _isPlayLevelupText;

	private RenderFacade _renderFacade;

	private int _skipFrame;

	private string flaskItemCode = string.Empty;

	public FlaskFilledEffect[] flaskFilledEffects;

	private long _accountId;

	private string _nick;

	private ISound _heroSound;

	public CachedHeroInfo HeroInfo { get; private set; }

	private bool IsPlaySound { get; set; }

	public bool IsLevelUp { get; set; }

	public virtual bool Open(CachedHeroInfo heroInfo, string nick, long accountId, bool isMercenary, bool isDead, HeroExpResultInfo expResultInfo, FlaskEndCampaignResultInfo flaskResultInfo, bool isPlaySound = false)
	{
		if (!SetupUI(heroInfo, isMercenary, isDead, expResultInfo, flaskResultInfo, isPlaySound))
		{
			return false;
		}
		CreateRenderFacade();
		bool flag = accountId == GlobalAutoBehaviour<AccountManager>.instance.UserInfo.AccountId;
		bool flag2 = !flag && GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsFriend(accountId);
		bool flag3 = !flag && GlobalAutoBehaviour<AccountManager>.instance.FriendManager.IsInvitee(accountId);
		UIHelper.EnableGameObject(RequestFriendButton, !flag2 && !flag && !flag3);
		UIHelper.EnableGameObject(NickNameRoot, !flag);
		UIHelper.SetLabelText(NickNameLabel, nick);
		_accountId = accountId;
		_nick = nick;
		return true;
	}

	protected virtual bool SetupUI(CachedHeroInfo heroInfo, bool isMercenary, bool isDead, HeroExpResultInfo expResultInfo, FlaskEndCampaignResultInfo flaskResultInfo, bool isPlaySound)
	{
		_isDead = isDead;
		IsPlaySound = isPlaySound;
		IsLevelUp = false;
		UIHelper.SetWidgetGrayScale(RootWidget, grayScale: false);
		UIHelper.EnableGameObject(DeadWidget, enable: false);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(FlaskSourceIcons); i++)
		{
			UIHelper.EnableGameObject(FlaskSourceIcons[i].Icon, enable: false);
		}
		if (!isMercenary)
		{
			HeroInfo = heroInfo;
			if (flaskResultInfo != null)
			{
				InitFlaskLevelExp(expResultInfo, flaskResultInfo);
				VespaCoroutine.Start(this, coUpdateFlaskLevelExp(delegate
				{
				}));
			}
			else if (expResultInfo != null)
			{
				if (InitHeroLevelExp(expResultInfo))
				{
					VespaCoroutine.Start(this, coUpdateHeroLevelExp());
				}
			}
			else
			{
				ShowExpSprite();
				UIHelper.EnableGameObject(AddExpLabel, enable: false);
				long totalExp = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(HeroInfo.Level, HeroInfo.Exp);
				SetHeroLevelExp(totalExp);
			}
		}
		else
		{
			HeroInfo = heroInfo;
			UIHelper.EnableGameObject(AddExpLabel, enable: false);
			UIHelper.SetLabelText(LevelLabel, HeroInfo.Level.ToString());
			UIHelper.SetSpriteFillAmount(ExpGauge, 0f);
			UIHelper.SetLabelText(ExpPercentLabel, string.Empty);
		}
		if (expResultInfo != null && !IsLevelUp)
		{
			IsLevelUp = expResultInfo.OldLevel < expResultInfo.NewLevel;
		}
		if (HeroInfo == null)
		{
			return false;
		}
		_creatureData = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
		if (_creatureData == null)
		{
			return false;
		}
		base.gameObject.SetActive(value: true);
		return true;
	}

	private void ShowExpSprite(bool bFlask = false, string itemCode = null)
	{
		UIHelper.EnableGameObject(ExpGauge, !bFlask);
		UIHelper.EnableGameObject(FlaskExpGauge, bFlask);
		if (flaskSelector != null)
		{
			if (bFlask && itemCode != null)
			{
				UIHelper.EnableGameObject(flaskSelector, enable: true);
				flaskSelector.Select(itemCode);
			}
			else
			{
				UIHelper.EnableGameObject(flaskSelector, enable: false);
			}
		}
	}

	public void Close()
	{
		if (_renderFacade != null)
		{
			RenderFacadeBuilder.ReleaseRenderFacade(_renderFacade);
			_renderFacade = null;
			_isPlayLevelupText = false;
		}
	}

	private void CreateRenderFacade()
	{
		LayerInfo layerInfo = GameUIRoot.instance.GetLayerInfo(GameUIRoot.ELayerType.Bottom3D);
		AnimationKeyData victoryAnimationKeyData = GetVictoryAnimationKeyData();
		RenderFacadeBuilder renderFacadeBuilder = new RenderFacadeBuilder(RenderPivot, TeamType.Left, HeroInfo);
		renderFacadeBuilder.LoadLevel = CreatureStock.ELoadLevel.Normal;
		renderFacadeBuilder.AddPreload(victoryAnimationKeyData);
		_renderFacade = renderFacadeBuilder.Build().DISABLEGROUNDCLIP();
		if (_renderFacade != null)
		{
			Vespa_Unity_GameObject_Extension_Method.ChangeLayerRecursive(_renderFacade.gameObject, layerInfo.Camera.gameObject.layer);
			_renderFacade.Action.SetBattleMode(isBattleAction: false, blendMotion: false);
			_renderFacade.AssetCreature.SetShaderTransform(new ShaderTransformData(-5f, depthShift: false), null);
			_renderFacade.RenderGroupModifier.SetExtraAlpha(0f);
			SetMotion(GetVictoryAnimationKeyData(), DefineCreature.EMotionEnd.End);
			_renderFacade.ChainUpdate();
			_skipFrame = 1;
		}
	}

	private void SetMotion(AnimationKeyData animationKeyData, DefineCreature.EMotionEnd endMotionType)
	{
		if (_renderFacade == null)
		{
			return;
		}
		NGame2.NRender.NRenderFacade.AnimationData.StockData stockData = new NGame2.NRender.NRenderFacade.AnimationData.StockData();
		if (stockData != null)
		{
			stockData.State = NGame2.NRender.NRenderFacade.AnimationData.EState.Action;
			stockData.AnimationKey = animationKeyData;
			stockData.MotionEnd = endMotionType;
			if (animationKeyData.PlayName == "Victory")
			{
				bool flag = !TryPlaySound("Victory");
				stockData.IsPlaySound = flag && IsPlaySound;
			}
			_renderFacade.Action.Motion(stockData, blendMotion: false, null);
		}
	}

	private AnimationKeyData GetVictoryAnimationKeyData()
	{
		return AnimationKeyData.CreateNamedAction("Victory");
	}

	private AnimationKeyData GetLoseAnimationKeyData()
	{
		return AnimationKeyData.CreateNamedAction("Lose");
	}

	private bool TryPlaySound(string motionName)
	{
		ClearSound();
		if (!IsPlaySound)
		{
			return false;
		}
		SoundStock stock = default(SoundStock);
		if (HeroInfo == null)
		{
			return false;
		}
		string codeName = HeroInfo.CreatureData.CodeName;
		string costumeVoicePostfix = SoundHelper.GetCostumeVoicePostfix(HeroInfo);
		string soundCode = SoundHelper.GetSoundCode(motionName, codeName, costumeVoicePostfix);
		stock.Code = soundCode;
		_heroSound = SingletonAuto<SoundContainer>.instance.CreateSoundExact(stock);
		if (_heroSound == null)
		{
			return false;
		}
		SingletonAuto<SoundContainer>.instance.PlayOnce(_heroSound, 10f);
		return true;
	}

	protected void ClearSound()
	{
		if (_heroSound != null)
		{
			SoundContainer.ReleaseSound(_heroSound);
			_heroSound = null;
		}
	}

	private void Update()
	{
		if (!(_renderFacade != null))
		{
			return;
		}
		if (_skipFrame > 0)
		{
			_skipFrame--;
			return;
		}
		if (_renderFacade.RenderGroupModifier != null)
		{
			_renderFacade.RenderGroupModifier.SetExtraAlpha(1f);
		}
		_renderFacade.ChainUpdate();
	}

	private IEnumerator coUpdateHeroLevelExp()
	{
		yield return null;
		bool isCompleted = false;
		_ = _prev_hero_exp;
		int prev_hero_level = _prev_hero_level;
		_time = 0f;
		CreatureLevelData expDataForLevel = GameRoot.Instance.CreatureLevelDataContainer.GetData(prev_hero_level + 1);
		if (expDataForLevel == null)
		{
			yield break;
		}
		SingletonAuto<SoundContainer>.instance.PlayUnique("UI.Campaign.GetExp", this);
		while (!isCompleted)
		{
			_time += Time.deltaTime;
			long num;
			if (_time >= 1f)
			{
				isCompleted = true;
				_time = 1f;
				num = _cur_hero_exp;
			}
			else
			{
				num = (long)((float)_prev_hero_exp + (float)(_cur_hero_exp - _prev_hero_exp) * _time / 1f);
			}
			if (expDataForLevel.TotalExp <= num)
			{
				PlayLevelupEffect();
				expDataForLevel = GameRoot.Instance.CreatureLevelDataContainer.GetDataByExp(num, isNextLevelData: true);
				if (expDataForLevel == null)
				{
					isCompleted = true;
				}
			}
			SetHeroLevelExp(num);
			yield return null;
		}
		SoundContainer.StopUnique("UI.Campaign.GetExp", this);
	}

	private IEnumerator coUpdateFlaskLevelExp(System.Action onComplete)
	{
		bool isCompleted = false;
		bool showLevelUp = false;
		float update_exp = _prev_flask_exp;
		_time = 0f;
		while (!isCompleted)
		{
			if ((int)update_exp == _cur_flask_exp)
			{
				isCompleted = true;
			}
			else
			{
				_time += Time.deltaTime;
				update_exp = Mathf.Lerp(_prev_flask_exp, _cur_flask_exp, _time);
			}
			float flaskExp = ((update_exp >= (float)_flask_capacity) ? (update_exp - (float)_flask_capacity) : update_exp);
			SetFlaskExp(flaskExp);
			if (!showLevelUp && update_exp >= (float)_flask_capacity)
			{
				showLevelUp = true;
				for (int i = 0; i < __Extension_Method_ICollection__.COUNT(flaskFilledEffects); i++)
				{
					if (flaskFilledEffects[i].Code == flaskItemCode)
					{
						flaskFilledEffects[i].FilledEffect.Play(null);
						break;
					}
				}
			}
			yield return null;
		}
		Vespa_System_Action_Extension_Method.fire(onComplete);
	}

	protected virtual void PlayLevelupEffect()
	{
		if (_renderFacade == null)
		{
			return;
		}
		float num = 2f;
		if (HeroInfo != null)
		{
			CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(HeroInfo.HeroIndex);
			if (data != null)
			{
				num = (float)data.HeightMm * 0.001f;
			}
		}
		Vector3 position = _renderFacade.transform.localToWorldMatrix.MultiplyPoint(new Vector3(0f, num * LevelupEffectOffsetMultiply, 0f));
		IAsyncLoad<IEffect> asyncLoad = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Common.HeroesLevelUp", _renderFacade.gameObject.layer);
		asyncLoad.Value.SetDummyParent(position, Quaternion.identity, _renderFacade.transform.lossyScale * LevelupEffectExtraScale, "Level Up Effect");
		SingletonUpdater<AssetUpdateManager>.instance.Add(asyncLoad, isGlobal: false);
		if (!_isPlayLevelupText)
		{
			_isPlayLevelupText = true;
			Vector3 position2 = _renderFacade.transform.localToWorldMatrix.MultiplyPoint(new Vector3(0f, LevelupTextOffset, 0f));
			IAsyncLoad<IEffect> asyncLoad2 = SingletonAuto<EffectContainer>.instance.CreateAsync("UI.Common.HeroesLevelUp.Text", _renderFacade.gameObject.layer);
			asyncLoad2.Value.SetDummyParent(position2, Quaternion.identity, _renderFacade.transform.lossyScale * LevelupEffectExtraScale, "Level Up Text Effect");
			SingletonUpdater<AssetUpdateManager>.instance.Add(asyncLoad2, isGlobal: false);
		}
		SoundStock stock = new SoundStock
		{
			Code = "UI.Campaign.LevelUp",
			LocalTimer = GlobalTimer.DefaultLocal
		};
		SingletonAuto<SoundContainer>.instance.PlayOnce(stock, 30f);
	}

	private bool InitHeroLevelExp(HeroExpResultInfo expResultInfo)
	{
		ShowExpSprite();
		int num = expResultInfo.NewValue - expResultInfo.AddValue;
		_prev_hero_exp = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(expResultInfo.OldLevel, num);
		_prev_hero_level = expResultInfo.OldLevel;
		_cur_hero_exp = GameRoot.Instance.CreatureLevelDataContainer.GetTotalExp(expResultInfo.NewLevel, expResultInfo.NewValue);
		_cur_hero_level = expResultInfo.NewLevel;
		_add_booster_exp = expResultInfo.AddBoosterValue;
		_add_teamlevel_exp = expResultInfo.AddTeamLevelValue;
		SetHeroLevelExp(_prev_hero_exp);
		string text = ((int)_cur_hero_exp - _prev_hero_exp).ToString();
		UIHelper.EnableGameObject(AddExpLabel, enable: true);
		if (_add_booster_exp > 0 || _add_teamlevel_exp > 0)
		{
			UIHelper.SetLabelColor(AddExpLabel, Color.green);
		}
		else
		{
			UIHelper.SetLabelColor(AddExpLabel, Color.white);
		}
		UIHelper.SetLabelText(AddExpLabel, "Exp+" + text);
		return _prev_hero_exp != _cur_hero_exp;
	}

	private void InitFlaskLevelExp(HeroExpResultInfo expResultInfo, FlaskEndCampaignResultInfo flaskResultInfo)
	{
		int itemIndex = flaskResultInfo.Info.ItemIndex;
		ItemData<FlaskItemData> itemData = null;
		if (itemIndex != 0)
		{
			itemData = GameRoot.Instance.ItemDataContainer.GetData<FlaskItemData>(itemIndex);
			gaugeSelector.Select(itemData.SubData.Type.ToString());
			flaskItemCode = itemData.Code;
			_flask_capacity = itemData.GetSubData<FlaskItemData>().FlaskExp;
			_cur_flask_exp = flaskResultInfo.Info.NewExp;
			for (_prev_flask_exp = _cur_flask_exp - flaskResultInfo.Info.AddExp; _prev_flask_exp < 0; _prev_flask_exp += _flask_capacity)
			{
			}
			if (flaskResultInfo.FilledFlaskCount > 0)
			{
				_cur_flask_exp += _flask_capacity;
			}
		}
		else if (flaskResultInfo.FilledFlaskIndex != 0)
		{
			foreach (FlaskItemData allDatum in GameRoot.Instance.FlaskItemDataContainer.GetAllData())
			{
				if (allDatum.PotionItemIndex == flaskResultInfo.FilledFlaskIndex)
				{
					gaugeSelector.Select(allDatum.Type.ToString());
					flaskItemCode = allDatum.ItemCode;
					_flask_capacity = allDatum.FlaskExp;
					_cur_flask_exp = _flask_capacity;
					for (_prev_flask_exp = _cur_flask_exp - flaskResultInfo.Info.AddExp; _prev_flask_exp < 0; _prev_flask_exp += _flask_capacity)
					{
					}
					break;
				}
			}
		}
		ShowExpSprite(bFlask: true, flaskItemCode);
		UIHelper.EnableGameObject(AddExpLabel, enable: true);
		if (_add_booster_exp > 0 || _add_teamlevel_exp > 0)
		{
			UIHelper.SetLabelColor(AddExpLabel, Color.green);
		}
		else
		{
			UIHelper.SetLabelColor(AddExpLabel, Color.white);
		}
		UIHelper.SetLabelText(AddExpLabel, "+" + flaskResultInfo.Info.AddExp);
		if (itemData != null)
		{
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(FlaskSourceIcons); i++)
			{
				UIHelper.EnableGameObject(FlaskSourceIcons[i].Icon, FlaskSourceIcons[i].Type == itemData.SubData.SourceType);
			}
		}
		int addExpRatio = flaskResultInfo.Info.AddExpRatio;
		if (addExpRatio != 0 && addExpRatio < 1000)
		{
			if ((bool)FlaskExpFailEffect)
			{
				FlaskExpFailEffect.Play(null);
			}
		}
		else if (addExpRatio > 1000 && (bool)FlaskExpLuckyEffect)
		{
			FlaskExpLuckyEffect.Play(null);
		}
	}

	private void SetHeroLevelExp(long totalExp)
	{
		int level = 1;
		long localExp = 0L;
		long localExpOfLevel = 0L;
		GameRoot.Instance.CreatureLevelDataContainer.ParseTotalExpToLocal(totalExp, out level, out localExp, out localExpOfLevel);
		level = System.Math.Max(level, HeroInfo.Level);
		int maxLevel = HeroInfo.GetMaxLevel();
		if (level >= maxLevel)
		{
			UIHelper.SetLabelText(LevelLabel, level.ToString());
			UIHelper.SetSpriteFillAmount(ExpGauge, 1f);
			UIHelper.SetLabelText(ExpPercentLabel, "Max");
		}
		else
		{
			float num = (float)localExp / Mathf.Max(1f, localExpOfLevel);
			UIHelper.SetLabelText(LevelLabel, level.ToString());
			UIHelper.SetSpriteFillAmount(ExpGauge, num);
			UIHelper.SetLabelText(ExpPercentLabel, $"{(int)(num * 100f)}%");
		}
	}

	private void SetFlaskExp(float curExp)
	{
		float num = curExp / (float)Mathf.Max(1, _flask_capacity);
		UIHelper.SetLabelText(LevelLabel, HeroInfo.Level.ToString());
		UIHelper.SetSpriteFillAmount(FlaskExpGauge, num);
		UIHelper.SetLabelText(ExpPercentLabel, $"{(int)(num * 100f)}%");
	}

	public void OnClickRquestFriend()
	{
		if (AutoRepeatBattle.IsEnabled() && AutoRepeatBattle.IsPartyPlay)
		{
			CenterMessage.Open(LocalizationManager.GetStringDirect("REPEATBATTLE_MULTI_DISABLE_CONTENTS"));
			return;
		}
		string stringDirect = LocalizationManager.GetStringDirect("COMMON_NOTIFICATION");
		string stringDirect2 = LocalizationManager.GetStringDirect("FRIEND_ADD_REQUEST_TEXT", _nick);
		MessagePopup.Open(stringDirect, stringDirect2, delegate
		{
			SceneAutoBehaviour<MyFriendManagement>.instance.ProcessInviteFriend(_accountId, _nick);
			UIHelper.EnableGameObject(RequestFriendButton, enable: false);
		}, null);
	}
}
