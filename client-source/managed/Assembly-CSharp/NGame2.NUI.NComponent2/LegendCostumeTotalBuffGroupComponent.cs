using System.Collections.Generic;
using NGame2.NUI.NManager.NBooster;
using NShared;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class LegendCostumeTotalBuffGroupComponent : LinkFactoryComponent, InitOnCreate
{
	public interface IListener
	{
		void OnClickFoldOut(ECostumeBuffType costumeBuffType);
	}

	public const string PrefabPath = "Prefab2/Component/LegendCostume/LegendCostumeTotalBuffGroupComponent";

	public static PrefabAuto<LegendCostumeTotalBuffGroupComponent> PrefabAuto = new PrefabAuto<LegendCostumeTotalBuffGroupComponent>("Prefab2/Component/LegendCostume/LegendCostumeTotalBuffGroupComponent");

	public UIWidget Widget;

	public UILabel LabelName;

	public Transform GridRoot;

	public UISimpleGrid SimpleGrid;

	public TweenRotation Tween;

	private bool _isFoldOut;

	private IListener _listener;

	private Vector2 _initWidgetSize;

	private ECostumeBuffType _costumeBuffType;

	private List<LegendCostumeTotalBuffComponent> _listComp = new List<LegendCostumeTotalBuffComponent>();

	void InitOnCreate.OnCreate()
	{
		if (_initWidgetSize == Vector2.zero)
		{
			_initWidgetSize = new Vector2(Widget.width, Widget.height);
		}
	}

	void InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open(IListener listener, string titleKey, ECostumeBuffType costumeBuffType, List<BoosterInfo> buffDatas)
	{
		Init(listener, titleKey, costumeBuffType);
		AddBuffComponent(buffDatas);
	}

	private void Init(IListener listener, string titleKey, ECostumeBuffType costumeBuffType)
	{
		ReleaseComp();
		_listener = listener;
		_isFoldOut = false;
		_costumeBuffType = costumeBuffType;
		SetLabel(titleKey);
		SetTween(_isFoldOut);
		FoldOut(_isFoldOut);
	}

	private void AddBuffComponent(List<BoosterInfo> infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			BoosterInfo boosterInfo = infos[i];
			if (boosterInfo != null)
			{
				LegendCostumeTotalBuffComponent legendCostumeTotalBuffComponent = UIHelper.AddGridItem(SimpleGrid, LegendCostumeTotalBuffComponent.PrefabAuto);
				if (!(legendCostumeTotalBuffComponent == null))
				{
					legendCostumeTotalBuffComponent.Open(boosterInfo);
					_listComp.Add(legendCostumeTotalBuffComponent);
				}
			}
		}
		UIHelper.RepositionGrid(SimpleGrid);
	}

	public void SetTween(bool isForward)
	{
		if (!(Tween == null))
		{
			if (isForward)
			{
				Tween.PlayForward();
			}
			else
			{
				Tween.PlayReverse();
			}
		}
	}

	public void SetLabel(string key)
	{
		string stringDirect = LocalizationManager.GetStringDirect(key);
		UIHelper.SetLabelText(LabelName, stringDirect);
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listComp);
	}

	public void Close()
	{
		_listener = null;
		_isFoldOut = true;
		ReleaseComp();
	}

	public void FoldOut(bool isFoldOut)
	{
		if (!(Widget == null))
		{
			if (isFoldOut)
			{
				int num = Mathf.CeilToInt((float)__Extension_Method_ICollection__.COUNT(_listComp) / (float)SimpleGrid.Grid.maxPerLine);
				Widget.height = (int)_initWidgetSize.y + num * (int)SimpleGrid.Grid.cellHeight;
			}
			else
			{
				Widget.height = (int)_initWidgetSize.y;
			}
			NGUITools.UpdateWidgetCollider(Widget.gameObject);
			UIHelper.EnableGameObject(SimpleGrid, isFoldOut);
		}
	}

	public void OnClickFoldOut()
	{
		if (_listener != null)
		{
			_isFoldOut = !_isFoldOut;
			FoldOut(_isFoldOut);
			SetTween(_isFoldOut);
			_listener.OnClickFoldOut(_costumeBuffType);
		}
	}
}
