using NGame2.NUI.NHelper.NAffix;
using NGame2.NUI.NWindow.NAffix;
using NShared;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class AffixSelectPopupComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickComponent(AffixSelectPopupComponent caller);
	}

	public const string PrefabPath = "Prefab2/Component/AffixSystem/AffixSelectPopupComponent";

	public static PrefabAuto<AffixSelectPopupComponent> PrefabAuto = new PrefabAuto<AffixSelectPopupComponent>("Prefab2/Component/AffixSystem/AffixSelectPopupComponent");

	public GameObject SelectRoot;

	public GameObject AdvantageRoot;

	public GameObject AdvantageDisableRoot;

	public UILabel AdvantageDescLabel;

	public UILabel AdvantagePointLabel;

	public GameObject PenaltyRoot;

	public UILabel PenaltyDescLabel;

	public UILabel PenlatyPointLabel;

	private IListener _listener;

	private IAffixDataProvider _dataProvider;

	private AffixData _affixData;

	void InitOnCreate.OnCreate()
	{
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	public void Open(IListener listener, IAffixDataProvider provider, AffixData affixData)
	{
		if (affixData != null && provider != null)
		{
			_listener = listener;
			_dataProvider = provider;
			_affixData = affixData;
			Setup();
		}
	}

	public void Change(AffixData affixData)
	{
		if (affixData != null && _affixData != affixData)
		{
			_affixData = affixData;
			Setup();
		}
	}

	protected virtual void Setup()
	{
		bool isPenalty = _affixData.IsPenalty;
		SetRoot(isPenalty);
		SetDesc(isPenalty);
		SetPoint(isPenalty);
		SetState();
	}

	private void SetRoot(bool isPenalty)
	{
		UIHelper.EnableGameObject(AdvantageRoot, !isPenalty);
		UIHelper.EnableGameObject(PenaltyRoot, isPenalty);
	}

	private UILabel GetDescLabel(bool isPenalty)
	{
		if (isPenalty)
		{
			return PenaltyDescLabel;
		}
		return AdvantageDescLabel;
	}

	private void SetDesc(bool isPenatly)
	{
		UIHelper.SetLabelText(GetDescLabel(isPenatly), GetAffixDesc());
	}

	private string GetAffixDesc()
	{
		return AffixHelper.GetAffixDesc(_affixData);
	}

	private UILabel GetPointLabel(bool isPenalty)
	{
		if (isPenalty)
		{
			return PenlatyPointLabel;
		}
		return AdvantagePointLabel;
	}

	private void SetPoint(bool isPenalty)
	{
		UILabel pointLabel = GetPointLabel(isPenalty);
		int num = ((_affixData != null) ? _affixData.Point : 0);
		UIHelper.SetLabelText(pointLabel, num);
	}

	private void SetState()
	{
		EAffixCompState eAffixCompState = ((_dataProvider == null) ? EAffixCompState.Disable : _dataProvider.GetAffixCompState(_affixData));
		UIHelper.EnableGameObject(AdvantageDisableRoot, eAffixCompState == EAffixCompState.Disable);
		UIHelper.EnableGameObject(SelectRoot, eAffixCompState == EAffixCompState.Select);
	}

	protected virtual void Clear()
	{
		_listener = null;
		_dataProvider = null;
		_affixData = null;
	}

	public void Refresh()
	{
		SetState();
	}

	public AffixData GetAffixData()
	{
		return _affixData;
	}

	public void OnClickComponent()
	{
		if (_listener != null)
		{
			_listener.OnClickComponent(this);
		}
	}
}
