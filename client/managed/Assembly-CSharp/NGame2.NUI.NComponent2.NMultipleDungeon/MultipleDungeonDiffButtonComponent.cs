using NGame2.NAccount;
using NNguiExtension;
using NShared;
using NVespa.NPrefab2;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NMultipleDungeon;

public class MultipleDungeonDiffButtonComponent : LinkFactoryComponent
{
	public interface IListener
	{
		void SelectDiff(int diff);
	}

	public interface IDataProvider
	{
		UnderPrisonType GetUnderPrisonType();
	}

	public class MultipleDungeonDiffButtonInfo
	{
		public IListener IListener { get; set; }

		public int Diff { get; set; }

		public bool IsLock { get; set; }

		public bool IsNew { get; set; }

		public string ButtonNameLocalize { get; set; }
	}

	public const string PrefabPath = "Prefab2/Component/MultipleDungeon/MultipleDungeonDiffButtonComponent";

	public static PrefabAuto<MultipleDungeonDiffButtonComponent> PrefabAuto = new PrefabAuto<MultipleDungeonDiffButtonComponent>("Prefab2/Component/MultipleDungeon/MultipleDungeonDiffButtonComponent");

	public Transform LockRootTransform;

	public Transform NewTransform;

	public UILabel NameLabel;

	public UIButton DiffButton;

	public UISpriteSelector DiffSpriteSelector;

	private IDataProvider _iDataProvider;

	private MultipleDungeonDiffButtonInfo _diffButtonInfo;

	public bool IsNew
	{
		get
		{
			if (_diffButtonInfo == null)
			{
				return false;
			}
			return _diffButtonInfo.IsNew;
		}
	}

	public void Open(IDataProvider iDataProvider, MultipleDungeonDiffButtonInfo diffButtonInfo)
	{
		Init(iDataProvider, diffButtonInfo);
	}

	private void Init(IDataProvider iDataProvider, MultipleDungeonDiffButtonInfo diffButtonInfo)
	{
		_iDataProvider = iDataProvider;
		_diffButtonInfo = diffButtonInfo;
		SetName();
		SetDiff();
		SetLock();
		SetIsNew();
	}

	private void SetName()
	{
		if (_diffButtonInfo != null)
		{
			UIHelper.SetLabelText(NameLabel, _diffButtonInfo.ButtonNameLocalize);
		}
	}

	private void SetDiff()
	{
		if (_diffButtonInfo != null && !(DiffSpriteSelector == null))
		{
			DiffSpriteSelector.Select(_diffButtonInfo.Diff.ToString());
		}
	}

	private void SetLock()
	{
		if (_diffButtonInfo != null)
		{
			UIHelper.EnableGameObject(LockRootTransform, _diffButtonInfo.IsLock);
		}
	}

	private void SetIsNew()
	{
		if (_diffButtonInfo != null)
		{
			UIHelper.EnableGameObject(NewTransform, _diffButtonInfo.IsNew);
		}
	}

	private void SetIsNewState()
	{
		if (IsNew && _diffButtonInfo != null)
		{
			_diffButtonInfo.IsNew = false;
			SetIsNew();
			if (_iDataProvider != null)
			{
				GlobalAutoBehaviour<AccountManager>.instance.UnderPrisonManager.SetIsNewState(_iDataProvider.GetUnderPrisonType(), _diffButtonInfo.Diff);
			}
		}
	}

	public void OnClick()
	{
		if (_diffButtonInfo != null && !_diffButtonInfo.IsLock && _diffButtonInfo.IListener != null)
		{
			SetIsNewState();
			_diffButtonInfo.IListener.SelectDiff(_diffButtonInfo.Diff);
		}
	}
}
