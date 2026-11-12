using System.Collections;
using System.Collections.Generic;
using NGame2.NAccount;
using NShared;
using NVespa.NGlobal;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NWindow;

public class PunishmentRuneCraftWindow : BaseWindow
{
	public interface IListener
	{
		void OnCraftPunishmentRune(int voidRuneIndex, int legendRuneIndex, int[] materialRuneIndices);

		void OnClickHelp();
	}

	public const string PrefabPath = "Resources/Prefab2/Window/PunishmentRuneCraft/PunishmentRuneCraftWindow";

	private IListener _listener;

	[Header("------------------------------------------")]
	[Header("[PunishmentRuneCraftWindow]")]
	public List<RuneCraftStepBase> _listRuneCraftStep = new List<RuneCraftStepBase>();

	public Animator AnimatorL;

	public Animator AnimatorR;

	public AnimationClip AnimGoToCraft;

	public Color ColorGoldEnough = Color.white;

	public Color ColorGoldNotEnough = Color.red;

	public GameObject RootEffect;

	public UIButton ButtonHelp;

	public UILabel LabelPunishmentRuneCount;

	public Dictionary<int, int> DicMaterialCount = new Dictionary<int, int>();

	private int _currentStepIndex;

	private bool _isResultProcessing;

	private List<Coroutine> _coroutineList = new List<Coroutine>();

	public int CachedVoidRuneIndex { get; set; }

	public int CachedApostleBloodIndex { get; set; }

	public int CachedLegendRuneIndex { get; set; }

	public int[] CachedMaterialRuneIndices { get; set; }

	public PunishmentRuneItemCountData PunishmentRuneItemCountData { get; set; }

	public void Open(IListener listener)
	{
		base.Open();
		_listener = listener;
		Init();
	}

	public void Init()
	{
		_currentStepIndex = 0;
		_isResultProcessing = false;
		TopmostMenuType = ETopmostMenuType.BackButtonOnly;
		EnableEffect(enable: true);
		UpdateStep(isfirstStep: true);
		EquipItemInfo[] equipItemsByItemType = GlobalAutoBehaviour<AccountManager>.instance.EquipItemManager.GetEquipItemsByItemType(ItemType.PunishmentRune);
		UIHelper.SetLabelText(LabelPunishmentRuneCount, LocalizationManager.GetStringDirect("RUNE_POSSESSION_PUNISHMENT", __Extension_Method_ICollection__.COUNT(equipItemsByItemType)));
	}

	private IEnumerator coInit()
	{
		EnableEffect(enable: true);
		yield return new WaitForSeconds(0.1f);
		UpdateStep(isfirstStep: true);
	}

	public bool IsFirstStep()
	{
		return _currentStepIndex == 0;
	}

	public bool IsLastStep()
	{
		return _currentStepIndex == _listRuneCraftStep.Count - 1;
	}

	public void UpdateStep(bool isfirstStep = false)
	{
		_coroutineList.Add(VespaCoroutine.Start(coGoToNextStep(isfirstStep)));
	}

	private IEnumerator coGoToNextStep(bool isfirstStep)
	{
		RuneCraftStepBase curStep;
		AnimationClip curAnimL;
		AnimationClip curAnimR;
		if (!isfirstStep)
		{
			if (_currentStepIndex == _listRuneCraftStep.Count - 1)
			{
				_isResultProcessing = true;
				TopmostMenuType = ETopmostMenuType.HideAll;
			}
			curStep = _listRuneCraftStep[_currentStepIndex];
			curStep.PlayNextStepSound();
			curAnimL = curStep.AnimEndLeft;
			curAnimR = curStep.AnimEndRight;
			AnimatorL.enabled = true;
			AnimatorR.enabled = true;
			AnimatorL.StopPlayback();
			AnimatorR.StopPlayback();
			AnimatorL.PlayInFixedTime(curAnimL.name);
			AnimatorR.PlayInFixedTime(curAnimR.name);
			yield return new WaitForSeconds(curAnimL.length);
			AnimatorL.enabled = false;
			AnimatorR.enabled = false;
			curStep.Close();
			_currentStepIndex++;
			if (_currentStepIndex >= __Extension_Method_ICollection__.COUNT(_listRuneCraftStep))
			{
				AnimatorL.enabled = true;
				AnimatorL.StopPlayback();
				AnimatorL.PlayInFixedTime(AnimGoToCraft.name, 0);
				yield return new WaitForSeconds(AnimGoToCraft.length);
				AnimatorL.enabled = false;
				EnableEffect(enable: false);
				OnCraftPunishmentRune();
				_isResultProcessing = false;
				yield break;
			}
		}
		curStep = _listRuneCraftStep[_currentStepIndex];
		curAnimL = curStep.AnimBeginLeft;
		curAnimR = curStep.AnimBeginRight;
		yield return new WaitForSeconds(0.1f);
		AnimatorL.enabled = true;
		AnimatorR.enabled = true;
		AnimatorL.StopPlayback();
		AnimatorR.StopPlayback();
		AnimatorL.PlayInFixedTime(curAnimL.name);
		AnimatorR.PlayInFixedTime(curAnimR.name);
		yield return new WaitForSeconds(curAnimL.length);
		AnimatorL.enabled = false;
		AnimatorR.enabled = false;
		curStep.Init();
	}

	public override void Close()
	{
		if (!_isResultProcessing)
		{
			base.Close();
			VespaCoroutine.Stop(this, _coroutineList);
			_coroutineList.Clear();
			for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listRuneCraftStep); i++)
			{
				_listRuneCraftStep[i].Close();
			}
			DicMaterialCount.Clear();
			AnimatorL.StopPlayback();
			AnimatorR.StopPlayback();
			AnimatorL.enabled = false;
			AnimatorR.enabled = false;
		}
	}

	public void OnCraftPunishmentRune()
	{
		if (_listener != null)
		{
			_listener.OnCraftPunishmentRune(CachedVoidRuneIndex, CachedLegendRuneIndex, CachedMaterialRuneIndices);
		}
	}

	public void EnableEffect(bool enable)
	{
		UIHelper.EnableGameObject(RootEffect, enable);
	}

	public void OnClickHelp()
	{
		if (_listener != null)
		{
			_listener.OnClickHelp();
		}
	}
}
