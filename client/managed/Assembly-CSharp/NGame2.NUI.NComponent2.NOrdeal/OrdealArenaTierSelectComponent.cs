using System;
using NGame2.NContentsDefine;
using NGame2.NOrdeal;
using NShared;
using NVespa.NGlobal;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NOrdeal;

public class OrdealArenaTierSelectComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickButton(MatchTierType selectTierType);
	}

	public const string PrefabPath = "Resources/Prefab2/Component/OrdealArena/OrdealArenaTierSelectComponent";

	public static PrefabAuto<OrdealArenaTierSelectComponent> PrefabAuto = new PrefabAuto<OrdealArenaTierSelectComponent>("Resources/Prefab2/Component/OrdealArena/OrdealArenaTierSelectComponent");

	public GameObject GoDisable;

	public GameObject GoCompleted;

	public UILabel LabelDesc;

	public UILabel LabelButton;

	public Transform SelectorTier;

	public UIButton ButtonEnter;

	private IListener _listener;

	private MatchTierType _tierType;

	private MyMatchTierIconComponent _tierIconComponent;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(MatchTierType matchTierType, IListener listener)
	{
		_listener = listener;
		_tierType = matchTierType;
		SetTier();
	}

	private void SetTier()
	{
		FactoryBase.SafeReleaseComponent(ref _tierIconComponent);
		if ((bool)SelectorTier)
		{
			_tierIconComponent = MyMatchTierIconComponent.PrefabAuto.Factory.Create(SelectorTier);
			Vespa_Unity_Transform_Extension_Method.ResetLocalTM(_tierIconComponent.transform);
			_tierIconComponent.Open(_tierType, 0, VisualTierRank.NoRank, MatchArenaType.Ordeal);
			_tierIconComponent.HideName();
			_tierIconComponent.DisableLevel();
		}
	}

	public void SetComplete(bool isCompleted)
	{
		UIHelper.EnableGameObject(GoCompleted, isCompleted);
	}

	public void SetEnable(bool isEnable)
	{
		UIHelper.EnableGameObject(GoDisable, !isEnable);
		UIHelper.EnableButton(ButtonEnter, isEnable);
		SetLabelButton(isEnable);
	}

	public void SetLabelButton(bool isEnable)
	{
		string text = "";
		text = ((!isEnable) ? "ORDEALARENA_BUTTON_CANNOT_ENTER" : "ORDEALARENA_BUTTON_ENTER");
		string stringDirect = LocalizationManager.GetStringDirect(text);
		UIHelper.SetLabelText(LabelButton, stringDirect);
	}

	public void SetDescLabel(string descKey, MatchTierType tierType)
	{
		string tierText = OrdealHelper.GetTierText(tierType);
		string stringDirect = LocalizationManager.GetStringDirect(descKey, tierText);
		UIHelper.SetLabelText(LabelDesc, stringDirect);
	}

	public void SetLabelDesc(TimeSpan remainTime)
	{
		string remainTimeText = UIHelper.GetRemainTimeText((int)remainTime.TotalSeconds);
		string stringDirect = LocalizationManager.GetStringDirect("ORDEALARENA_TIME_LEFT", remainTimeText);
		UIHelper.SetLabelText(LabelDesc, stringDirect);
	}

	public void OnClickButton()
	{
		if (_listener != null)
		{
			_listener.OnClickButton(_tierType);
		}
	}

	private void Close()
	{
		_listener = null;
		FactoryBase.SafeReleaseComponent(ref _tierIconComponent);
	}
}
