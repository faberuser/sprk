using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NGame2.NBattleContext;
using NGame2.NBattleField.NManager.NPlay;
using NGame2.NUI.NComponent2;
using NGame2.NUI.NManager;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow.NBattle;

public class BoosterStatus : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/Battle/BoosterStatus";

	private List<BoosterComponent> _ListBoosterComponent = new List<BoosterComponent>();

	public UISimpleGrid BoosterGrid;

	public UIButton ButtonBoosterToMax;

	public UIButton ButtonBoosterToMin;

	public UILabel LabelBoosterCount;

	public UIGame3DWidget UiEffect_Border;

	public void Init()
	{
		base.Open();
		UiEffect_Border.Stop(immediatelyClear: true);
		BoosterGrid.ClearGridItem<BoosterComponent>();
		SetupBooster();
	}

	public override void Close()
	{
		base.Close();
	}

	public override void Reset()
	{
		base.Reset();
		FactoryBase.ReleaseAndClearList(_ListBoosterComponent);
	}

	public void SetupBooster()
	{
		FactoryBase.ReleaseAndClearList(_ListBoosterComponent);
		BattleContext instance = SingletonAuto<BattleContext>.instance;
		if (instance == null)
		{
			return;
		}
		NGame2.NBattleField.NManager.NPlay.IDataProvider dataProvider = instance.DataProvider;
		if (dataProvider == null)
		{
			return;
		}
		BattleType battleType = dataProvider.GetBattleType();
		if (!GlobalAutoBehaviour<AccountManager>.isValidInstance || GlobalAutoBehaviour<AccountManager>.instance.ItemTimeDurationManager == null)
		{
			return;
		}
		GlobalAutoBehaviour<BoosterManager>.instance.InitAllEffectiveBooster(battleType, includeCash: false);
		List<BoosterInfo> allEffectiveBooster = GlobalAutoBehaviour<BoosterManager>.instance.GetAllEffectiveBooster();
		if (__Extension_Method_ICollection__.COUNT(allEffectiveBooster) > 0)
		{
			foreach (BoosterInfo item in allEffectiveBooster)
			{
				if (item.IsShowAtBattle())
				{
					BoosterComponent boosterComponent = UIHelper.AddGridItem(BoosterGrid, BoosterComponent.PrefabAuto);
					boosterComponent.Open(item);
					_ListBoosterComponent.Add(boosterComponent);
				}
			}
		}
		UIHelper.RepositionGrid(BoosterGrid);
		if (_ListBoosterComponent.Count >= 2)
		{
			if (AutoRepeatBattle.IsBoosterExpand)
			{
				ButtonBoosterToMax.gameObject.SetActive(value: false);
				ButtonBoosterToMin.gameObject.SetActive(value: true);
				BoosterComponent last = Vespa_Generic_IList_Extension_Method.GetLast(_ListBoosterComponent);
				Vector3 localPosition = ButtonBoosterToMin.transform.localPosition;
				localPosition.x = last.transform.localPosition.x - 70f;
				ButtonBoosterToMin.transform.localPosition = localPosition;
			}
			else
			{
				ButtonBoosterToMax.gameObject.SetActive(value: true);
				UiEffect_Border.Play(null);
				string stringDirect = LocalizationManager.GetStringDirect("RESCOLLECT_ITEMCOUNT", _ListBoosterComponent.Count);
				UIHelper.SetLabelText(LabelBoosterCount, stringDirect);
				ButtonBoosterToMin.gameObject.SetActive(value: false);
				BoosterGrid.gameObject.SetActive(value: false);
			}
		}
	}

	public void OnClickButton_BoosterMax()
	{
		BoosterGrid.gameObject.SetActive(value: true);
		if (_ListBoosterComponent.Count >= 2)
		{
			AutoRepeatBattle.IsBoosterExpand = true;
			ButtonBoosterToMax.gameObject.SetActive(value: false);
			ButtonBoosterToMin.gameObject.SetActive(value: true);
			ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, BoosterMaxAnim_Co());
		}
	}

	private IEnumerator BoosterMaxAnim_Co()
	{
		List<float> listOriginX = new List<float>();
		float elapsedTime = 0f;
		UIHelper.RepositionGrid(BoosterGrid);
		BoosterComponent last = Vespa_Generic_IList_Extension_Method.GetLast(_ListBoosterComponent);
		Vector3 localPosition = ButtonBoosterToMin.transform.localPosition;
		localPosition.x = last.transform.localPosition.x - 70f;
		ButtonBoosterToMin.transform.localPosition = localPosition;
		Vector3 localPosition2;
		foreach (BoosterComponent item in _ListBoosterComponent)
		{
			listOriginX.Add(item.transform.localPosition.x);
			localPosition2 = item.transform.localPosition;
			localPosition2.x = 0f;
			item.transform.localPosition = localPosition2;
			item.Texture_Booster.SetDirty();
		}
		float buttonBoosterMinOriginX = ButtonBoosterToMin.transform.localPosition.x;
		Vector3 localPosition3 = ButtonBoosterToMin.transform.localPosition;
		localPosition3.x = 0f;
		ButtonBoosterToMin.transform.localPosition = localPosition3;
		while (true)
		{
			float x;
			for (int i = 0; i < _ListBoosterComponent.Count; i++)
			{
				BoosterComponent boosterComponent = _ListBoosterComponent[i];
				float b = listOriginX[i];
				x = Mathf.Lerp(0f, b, elapsedTime);
				localPosition2 = boosterComponent.transform.localPosition;
				localPosition2.x = x;
				boosterComponent.transform.localPosition = localPosition2;
			}
			x = Mathf.Lerp(0f, buttonBoosterMinOriginX, elapsedTime);
			localPosition2 = ButtonBoosterToMin.transform.localPosition;
			localPosition2.x = x;
			ButtonBoosterToMin.transform.localPosition = localPosition2;
			elapsedTime += Time.deltaTime * 8f;
			if (elapsedTime > 1f)
			{
				break;
			}
			yield return null;
		}
		for (int j = 0; j < _ListBoosterComponent.Count; j++)
		{
			BoosterComponent boosterComponent2 = _ListBoosterComponent[j];
			float x2 = listOriginX[j];
			localPosition2 = boosterComponent2.transform.localPosition;
			localPosition2.x = x2;
			boosterComponent2.transform.localPosition = localPosition2;
			boosterComponent2.Texture_Booster.SetDirty();
		}
		localPosition2 = ButtonBoosterToMin.transform.localPosition;
		localPosition2.x = buttonBoosterMinOriginX;
		ButtonBoosterToMin.transform.localPosition = localPosition2;
	}

	public void OnClickButton_BoosterMin()
	{
		AutoRepeatBattle.IsBoosterExpand = false;
		ExtensionMethod_VespaCoroutine.StartCoroutineThis(this, BoosterMinAnim_Co());
	}

	private IEnumerator BoosterMinAnim_Co()
	{
		float elapsedTime = 0f;
		float to = 0f;
		List<float> listOriginX = new List<float>();
		float buttonBoosterMinOriginX = ButtonBoosterToMin.transform.localPosition.x;
		foreach (BoosterComponent item in _ListBoosterComponent)
		{
			listOriginX.Add(item.transform.localPosition.x);
		}
		Vector3 localPosition;
		while (true)
		{
			float x;
			for (int i = 0; i < _ListBoosterComponent.Count; i++)
			{
				x = Mathf.Lerp(listOriginX[i], to, elapsedTime);
				BoosterComponent boosterComponent = _ListBoosterComponent[i];
				localPosition = boosterComponent.transform.localPosition;
				localPosition.x = x;
				boosterComponent.transform.localPosition = localPosition;
			}
			x = Mathf.Lerp(buttonBoosterMinOriginX, to, elapsedTime);
			localPosition = ButtonBoosterToMin.transform.localPosition;
			localPosition.x = x;
			ButtonBoosterToMin.transform.localPosition = localPosition;
			elapsedTime += Time.deltaTime * 8f;
			if (elapsedTime > 1f)
			{
				break;
			}
			yield return null;
		}
		for (int j = 0; j < _ListBoosterComponent.Count; j++)
		{
			BoosterComponent boosterComponent2 = _ListBoosterComponent[j];
			localPosition = boosterComponent2.transform.localPosition;
			localPosition.x = to;
			boosterComponent2.transform.localPosition = localPosition;
		}
		localPosition = ButtonBoosterToMin.transform.localPosition;
		localPosition.x = to;
		ButtonBoosterToMin.transform.localPosition = localPosition;
		ButtonBoosterToMax.gameObject.SetActive(value: true);
		UiEffect_Border.Play(null);
		ButtonBoosterToMin.gameObject.SetActive(value: false);
		string stringDirect = LocalizationManager.GetStringDirect("RESCOLLECT_ITEMCOUNT", _ListBoosterComponent.Count);
		UIHelper.SetLabelText(LabelBoosterCount, stringDirect);
		BoosterGrid.gameObject.SetActive(value: false);
	}
}
