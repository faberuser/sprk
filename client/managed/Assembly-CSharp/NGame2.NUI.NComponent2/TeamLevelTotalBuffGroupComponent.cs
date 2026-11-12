using System.Collections.Generic;
using NVespa.NGlobal;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class TeamLevelTotalBuffGroupComponent : LinkFactoryComponent, NVespa.NPrefab2.InitOnCreate
{
	public interface IListener
	{
		void OnClickFoldOut(TeamLevelBuffManager.EBuffType teamLevelBuffType);
	}

	public const string PrefabPath = "Prefab2/Component/TeamLevelBuff/TeamLevelTotalBuffGroupComponent";

	public static PrefabAuto<TeamLevelTotalBuffGroupComponent> PrefabAuto = new PrefabAuto<TeamLevelTotalBuffGroupComponent>("Prefab2/Component/TeamLevelBuff/TeamLevelTotalBuffGroupComponent");

	public UIWidget Widget;

	public UILabel LabelName;

	public Transform GridRoot;

	public UISimpleGrid SimpleGrid;

	public TweenRotation Tween;

	public UISprite SpriteNotice;

	private bool _isFoldOut;

	private IListener _listener;

	private Vector2 _initWidgetSize;

	private TeamLevelBuffManager.EBuffType _teamLevelBuffType;

	private List<TeamLevelTotalBuffComponent> _listComp = new List<TeamLevelTotalBuffComponent>();

	void NVespa.NPrefab2.InitOnCreate.OnCreate()
	{
		if (_initWidgetSize == Vector2.zero)
		{
			_initWidgetSize = new Vector2(Widget.width, Widget.height);
		}
	}

	void NVespa.NPrefab2.InitOnCreate.OnRelease()
	{
		Close();
	}

	public void Open<T>(IListener listener, string titleKey, TeamLevelBuffManager.EBuffType buffType, List<T> teamLevelBuffDatas)
	{
		Init(listener, titleKey, buffType);
		AddBuffComponent(teamLevelBuffDatas);
	}

	private void Init(IListener listener, string titleKey, TeamLevelBuffManager.EBuffType buffType)
	{
		ReleaseComp();
		_listener = listener;
		_isFoldOut = false;
		_teamLevelBuffType = buffType;
		SetLabel(titleKey);
		SetTween(_isFoldOut);
		FoldOut(_isFoldOut);
		bool enable = GlobalAutoBehaviour<TeamLevelBuffManager>.instance.IsChangedBuffType(_teamLevelBuffType);
		UIHelper.EnableGameObject(SpriteNotice, enable);
	}

	private void AddBuffComponent<T>(List<T> infos)
	{
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(infos); i++)
		{
			T val = infos[i];
			if (val != null)
			{
				TeamLevelTotalBuffComponent teamLevelTotalBuffComponent = UIHelper.AddGridItem(SimpleGrid, TeamLevelTotalBuffComponent.PrefabAuto);
				if (!(teamLevelTotalBuffComponent == null))
				{
					teamLevelTotalBuffComponent.Open(val);
					_listComp.Add(teamLevelTotalBuffComponent);
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

	private void SetNotice(bool show)
	{
		UIHelper.EnableGameObject(SpriteNotice, show);
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_listComp); i++)
		{
			TeamLevelTotalBuffComponent teamLevelTotalBuffComponent = _listComp[i];
			if (!(teamLevelTotalBuffComponent == null))
			{
				teamLevelTotalBuffComponent.SetNotice(show);
			}
		}
	}

	private void ReleaseComp()
	{
		FactoryBase.ReleaseAndClearList(_listComp);
	}

	public void Close()
	{
		_listener = null;
		_isFoldOut = true;
		UIHelper.EnableGameObject(SpriteNotice, enable: false);
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
			_listener.OnClickFoldOut(_teamLevelBuffType);
			if (!_isFoldOut)
			{
				SetNotice(show: false);
			}
		}
	}
}
