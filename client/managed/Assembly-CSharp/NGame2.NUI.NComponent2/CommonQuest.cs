using System.Collections.Generic;
using NNguiExtension;
using NVespa.NNGUIExtension;
using NVespa.NPrefab2;
using UnityEngine;

namespace NGame2.NUI.NComponent2;

public class CommonQuest : UIScrollItemBehaviour, InitOnCreate
{
	public new interface IListener<T>
	{
		void OnClick(T t);
	}

	public enum EStateIconType
	{
		None,
		GiftBox,
		CheckMark,
		ExclamationMark
	}

	[Header("[CommonQuest]")]
	public Transform BackTransform;

	public Transform BackMainTranform;

	public UILabel TitleLabel;

	public UILabel RequireLabel;

	public UILabel RequireWithRemainTimeLabel;

	public UILabel RemainTimeLabel;

	public UISprite CompleteBlackSprite;

	public Transform LockTransform;

	public UISpriteSelector SpriteSelectorStateIcon;

	public UISimpleGrid RewardListGrid;

	protected List<ItemIconComponent> _rewardItemIconComponentList = new List<ItemIconComponent>();

	protected List<CurrencyIconComponent> _rewardCurrencyIconComponentList = new List<CurrencyIconComponent>();

	void InitOnCreate.OnCreate()
	{
		Clear();
	}

	void InitOnCreate.OnRelease()
	{
		Clear();
	}

	protected void Open()
	{
		Init();
	}

	private void Init()
	{
		EnableBack();
		EnableRemainTime(isEnable: false);
	}

	private void Clear()
	{
		FactoryBase.ReleaseAndClearList(_rewardCurrencyIconComponentList);
		FactoryBase.ReleaseAndClearList(_rewardItemIconComponentList);
	}

	private void EnableBack(bool isEnable)
	{
		if (!(BackTransform == null))
		{
			UIHelper.EnableGameObject(BackTransform, isEnable);
		}
	}

	private void EnableBackMain(bool isEnable)
	{
		if (!(BackMainTranform == null))
		{
			UIHelper.EnableGameObject(BackMainTranform, isEnable);
		}
	}

	protected void EnableBack()
	{
		EnableBack(isEnable: true);
		EnableBackMain(isEnable: false);
	}

	protected void EnableBackMain()
	{
		EnableBackMain(isEnable: true);
		EnableBack(isEnable: false);
	}

	protected void SetTitleLabel(string text)
	{
		if (!(TitleLabel == null))
		{
			UIHelper.SetLabelText(TitleLabel, text);
		}
	}

	protected void SetRequireLabel(string text)
	{
		if (!(RequireLabel == null))
		{
			UIHelper.SetLabelText(RequireLabel, text);
			if (!(RequireWithRemainTimeLabel == null))
			{
				UIHelper.SetLabelText(RequireWithRemainTimeLabel, text);
			}
		}
	}

	protected void EnableRemainTime(bool isEnable)
	{
		if (!(RequireWithRemainTimeLabel == null) && !(RequireLabel == null))
		{
			UIHelper.EnableGameObject(RequireWithRemainTimeLabel, isEnable);
			UIHelper.EnableGameObject(RequireLabel, !isEnable);
		}
	}

	protected void SetStateIcon(EStateIconType type, bool isGrayScale)
	{
		if (SpriteSelectorStateIcon != null)
		{
			SpriteSelectorStateIcon.SelectEnum(type);
			SpriteSelectorStateIcon.GetTargetSprite().isGrayscale = isGrayScale;
		}
	}

	protected void EnableStateIcon(bool isEnable)
	{
		if (SpriteSelectorStateIcon != null)
		{
			UIHelper.EnableGameObject(SpriteSelectorStateIcon, isEnable);
		}
	}
}
