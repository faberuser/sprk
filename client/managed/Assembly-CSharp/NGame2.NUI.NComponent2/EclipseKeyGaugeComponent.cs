using System;
using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NContents;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class EclipseKeyGaugeComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
	}

	public const string PrefabPath = "Prefab2/Component/SoulWeapon/EclipseKeyGaugeComponent";

	public static PrefabAuto<EclipseKeyGaugeComponent> PrefabAuto = new PrefabAuto<EclipseKeyGaugeComponent>("Prefab2/Component/SoulWeapon/EclipseKeyGaugeComponent");

	public const string PrefabPath2 = "Prefab2/Component/SoulWeapon/EclipseKeyGaugeComponent2";

	public static PrefabAuto<EclipseKeyGaugeComponent> PrefabAuto2 = new PrefabAuto<EclipseKeyGaugeComponent>("Prefab2/Component/SoulWeapon/EclipseKeyGaugeComponent2");

	public UIClipSprite Sprite_Gauge;

	public UILabel Label_Gauge;

	public UISimpleGrid Grid_Root;

	public GameObject BlueMoonRoot;

	public GameObject RedMoonRoot;

	public GameObject EffectRoot;

	public TweenAlpha FullTween1;

	public TweenAlpha FullTween2;

	public EclipseKeyTemplate _eclipseDummyKey;

	[HideInInspector]
	public List<EclipseKeyTemplate> eclipseDummyKeys = new List<EclipseKeyTemplate>();

	private IListener _listener;

	private CurrencyResultInfo3 _currencyResultInfo;

	private long _currentValue;

	private int _maxEclipsePoint = 50000;

	private float _deltaTime;

	private long _currentStamina;

	public static bool ShowEclipseKeyGauge()
	{
		int valueInt = GameRoot.Instance.ConstantDataContainer.GetValueInt("EclipseGaugeTutorial", 65027);
		bool num = GlobalAutoBehaviour<AccountManager>.instance.TutorialManager.IsComplete(valueInt);
		bool flag = SingletonAuto<MenuOpenCheckManager>.instance.IsOpen(ERightMenuButton.SoulWeapon);
		return num && flag;
	}

	public void Init(long currentStamina, CurrencyResultInfo3 currencyResultInfo, bool playFillAnimation = true, Action complte = null)
	{
		if (currencyResultInfo != null && (currencyResultInfo.CurrencyType == CurrencyType.EclipsePoint || currencyResultInfo.CurrencyType == CurrencyType.EclipseKey))
		{
			eclipseDummyKeys.ForEach(delegate(EclipseKeyTemplate e)
			{
				VespaObject.DestroyGo(e);
			});
			eclipseDummyKeys.Clear();
			_maxEclipsePoint = GameRoot.Instance.CurrencyTypeDataContainer.GetCurrencyData(CurrencyType.EclipsePoint).ExchangeCostValue;
			_currentStamina = currentStamina;
			_currencyResultInfo = currencyResultInfo;
			InitMoonRoot();
			InitStamina();
			InitGauge(checked(_currencyResultInfo.NewValue - _currencyResultInfo.AddValue));
			if (playFillAnimation && _currencyResultInfo.AddValue >= 0)
			{
				StartCoroutine(coFillGauge(_currencyResultInfo.AddValue, complte));
			}
			ChangeFullTweenState(isActive: false);
		}
	}

	private void InitMoonRoot()
	{
		UIHelper.EnableGameObject(EffectRoot, enable: false);
		if (BlueMoonRoot != null)
		{
			GameObject blueMoonRoot = BlueMoonRoot;
			long currentStamina = _currentStamina;
			int num = 1;
			UIHelper.EnableGameObject(blueMoonRoot, currentStamina < num);
		}
		if (RedMoonRoot != null)
		{
			GameObject redMoonRoot = RedMoonRoot;
			long currentStamina2 = _currentStamina;
			int num2 = 1;
			UIHelper.EnableGameObject(redMoonRoot, currentStamina2 >= num2);
		}
	}

	private void InitGauge(long currentValue)
	{
		double num = _maxEclipsePoint;
		double num2 = currentValue;
		_currentValue = currentValue;
		bool flag = false;
		foreach (EclipseKeyTemplate eclipseDummyKey in eclipseDummyKeys)
		{
			if (eclipseDummyKey.IsEmptyKey())
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			num2 = _maxEclipsePoint;
		}
		double num3 = System.Math.Truncate(((num > 0.0) ? (num2 / num) : 0.0) * 100.0) / 100.0;
		UIHelper.SetSpriteFillAmount(Sprite_Gauge, (float)num3);
		string text = $"{Sprite_Gauge.fillAmount:P0}";
		if (!flag)
		{
			text = LocalizationManager.GetStringDirect("CONTENTS_ECLIPSE_GAUGE_MAX");
		}
		UIHelper.SetLabelText(Label_Gauge, text);
	}

	private void InitStamina()
	{
		long maxStamina = GlobalAutoBehaviour<AccountManager>.instance.UserManager.StaminaManager.GetMaxStamina(StaminaType.EclipseKey);
		float cellWidth = Grid_Root.Grid.cellWidth;
		int num = 1;
		float num2 = 0f - cellWidth * (float)checked(maxStamina - num) / 2f;
		int num3 = 0;
		while (true)
		{
			int num4 = num3;
			long num5 = maxStamina;
			int num6 = num4;
			if (num6 < num5)
			{
				EclipseKeyTemplate eclipseKeyTemplate = UnityEngine.Object.Instantiate(_eclipseDummyKey);
				eclipseKeyTemplate.transform.parent = Grid_Root.GetComponentInChildren<UIGrid>().transform;
				eclipseKeyTemplate.transform.localScale = Vector2.one;
				eclipseKeyTemplate.transform.localPosition = new Vector2(num2 + (float)num3 * cellWidth, 0f);
				int num7 = num3;
				long currentStamina = _currentStamina;
				int num8 = num7;
				if (num8 < currentStamina)
				{
					eclipseKeyTemplate.SetKeyState(exist: true);
				}
				else
				{
					eclipseKeyTemplate.SetKeyState(exist: false);
				}
				eclipseDummyKeys.Add(eclipseKeyTemplate);
				num3++;
				continue;
			}
			break;
		}
	}

	private IEnumerator coFillGauge(long addValue, Action complte = null)
	{
		yield return new WaitForSeconds(1f);
		float minDelay = 2f;
		int num = 0;
		if (addValue <= num || !eclipseDummyKeys.Exists(key => key.IsEmptyKey()))
		{
			yield return new WaitForSeconds(minDelay);
			Vespa_System_Action_Extension_Method.fire(complte);
			yield break;
		}
		double addValue2 = 0.0;
		float num5;
		checked
		{
			_currentValue += addValue;
			double num2 = _currentValue;
			if (num2 >= (double)_maxEclipsePoint)
			{
				addValue2 = num2 - (double)_maxEclipsePoint;
				num2 = _maxEclipsePoint;
			}
			double num3 = _maxEclipsePoint;
			double ratio = ((num3 > 0.0) ? (num2 / num3) : 0.0);
			double temp = System.Math.Truncate(ratio * 100.0) / 100.0;
			float startFill = Sprite_Gauge.fillAmount;
			_deltaTime = Time.time;
			while (true)
			{
				float progress = Mathf.Clamp01((Time.time - _deltaTime) * 0.5f);
				num5 = Mathf.Lerp(startFill, (float)temp, progress);
				UIHelper.SetSpriteFillAmount(Sprite_Gauge, num5);
				UIHelper.SetLabelText(Label_Gauge, $"{num5:P0}");
				// Lerp reaches its target but cannot overshoot it. Waiting for an
				// overshoot stalls at 0% for small gains and at 100% on conversion.
				if (progress >= 1f)
				{
					break;
				}
				yield return null;
			}
		}
		if (num5 >= 1f)
		{
			AddEclipseKey();
			EclipseKeyGaugeComponent eclipseKeyGaugeComponent = this;
			int num6 = 0;
			eclipseKeyGaugeComponent.InitGauge(num6);
			PlayRedMoonEffect();
			// Finish all conversions before notifying the reward popup once.
			yield return coFillGauge((long)addValue2);
		}
		yield return new WaitForSeconds(minDelay);
		Vespa_System_Action_Extension_Method.fire(complte);
	}

	private void PlayRedMoonEffect()
	{
		if (RedMoonRoot.activeSelf)
		{
			return;
		}
		ParticleSystem component = EffectRoot.GetComponent<ParticleSystem>();
		if (!(component == null))
		{
			for (int i = 0; i < 2; i++)
			{
				component.gameObject.SetActive(value: false);
				component.gameObject.SetActive(value: true);
			}
			VespaCoroutine.Start(coPlayRedMoonEffect());
		}
	}

	private IEnumerator coPlayRedMoonEffect()
	{
		yield return new WaitForSeconds(0.5f);
		UIHelper.EnableGameObject(BlueMoonRoot, enable: false);
		UIHelper.EnableGameObject(RedMoonRoot, enable: true);
	}

	private void AddEclipseKey()
	{
		foreach (EclipseKeyTemplate eclipseDummyKey in eclipseDummyKeys)
		{
			if (eclipseDummyKey.IsEmptyKey())
			{
				eclipseDummyKey.PlayEffect();
				break;
			}
		}
	}

	public void ChangeFullTweenState(bool isActive)
	{
		if (!(FullTween1 == null) && !(FullTween2 == null))
		{
			UIHelper.EnableGameObject(FullTween1, isActive);
			UIHelper.EnableGameObject(FullTween2, isActive);
			if (isActive)
			{
				FullTween1.PlayForwardReset();
				FullTween2.PlayForwardReset();
			}
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Clear();
	}

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		Clear();
	}

	private void Clear()
	{
		eclipseDummyKeys.ForEach(delegate(EclipseKeyTemplate e)
		{
			VespaObject.DestroyGo(e);
		});
		eclipseDummyKeys.Clear();
	}
}
