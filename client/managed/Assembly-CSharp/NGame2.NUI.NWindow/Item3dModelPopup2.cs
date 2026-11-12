using System;
using NGame2.NUI.NComponent2;
using NShared;

namespace NGame2.NUI.NWindow;

public class Item3dModelPopup2 : BaseWindow
{
	public const string PrefabPath = "Resources/Prefab2/Window/MessagePopup/Item3dModelPopup_2";

	public UILabel[] ItemNameLabel;

	public UILabel[] HeroNameLabel;

	public ClassTagIconComponent[] HeroClassIcon;

	public UIBundleLoader[] HeroPortrait;

	public UIButton ConfirmButton;

	private ItemData[] _itemDatas;

	private Action<int> _onSelect;

	private Action<int> _onClose;

	private int _selectItemIndex;

	private WindowPrefab<Item3dModelPopup2> _windowPrefab;

	private GlobalWindowPrefab<Item3dModelPopup2> _globalWindowPrefab;

	public static Item3dModelPopup2 PreLoad(ItemData[] itemData)
	{
		WindowPrefab<Item3dModelPopup2> windowPrefab = new WindowPrefab<Item3dModelPopup2>("Resources/Prefab2/Window/MessagePopup/Item3dModelPopup_2", GameUIRoot.ELayerType.UI);
		windowPrefab.instance._windowPrefab = windowPrefab;
		windowPrefab.instance.PreLoadInternal(itemData);
		return windowPrefab.instance;
	}

	public static Item3dModelPopup2 PreLoadGlobal(ItemData[] itemData)
	{
		GlobalWindowPrefab<Item3dModelPopup2> globalWindowPrefab = new GlobalWindowPrefab<Item3dModelPopup2>("Resources/Prefab2/Window/MessagePopup/Item3dModelPopup_2", GlobalUIRoot.ELayerType.GlobalUI);
		globalWindowPrefab.instance._globalWindowPrefab = globalWindowPrefab;
		globalWindowPrefab.instance.PreLoadInternal(itemData);
		return globalWindowPrefab.instance;
	}

	public void OpenPopup(Action<int> onSelect, Action<int> onClose)
	{
		if (_windowPrefab != null)
		{
			_windowPrefab.instance.OpenInternal(onSelect, onClose);
		}
		if (_globalWindowPrefab != null)
		{
			_globalWindowPrefab.instance.OpenInternal(onSelect, onClose);
		}
	}

	private void OpenInternal(Action<int> onSelect, Action<int> onClose)
	{
		OpenAndPlay(onSelect, onClose);
	}

	private void OpenAndPlay(Action<int> onSelect, Action<int> onClose)
	{
		_onClose = onClose;
		_onSelect = onSelect;
		UIHelper.EnableButton(ConfirmButton, enable: false, grayscaleIfDisable: true);
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_itemDatas))
		{
			return;
		}
		for (int i = 0; i < ItemNameLabel.Length; i++)
		{
			if (_itemDatas.Length > i)
			{
				UIHelper.SetLabelText(ItemNameLabel[i], _itemDatas[i].Name);
			}
		}
		for (int j = 0; j < HeroPortrait.Length; j++)
		{
			if (_itemDatas.Length > j)
			{
				int valueInt = _itemDatas[j].GetValueInt(0);
				CreatureData data = GameRoot.Instance.CreatureDataContainer.GetData(valueInt);
				if (data != null)
				{
					UIHelper.SetLabelText(HeroNameLabel[j], data.Name);
					string battleHeroPortraitPath = UIHelper.GetBattleHeroPortraitPath(data.CodeName, 5, 0);
					HeroPortrait[j].SetAssetPath(battleHeroPortraitPath);
					HeroClassIcon[j].Open(data.TagType, null);
				}
			}
		}
	}

	private void PreLoadInternal(ItemData[] itemData)
	{
		_itemDatas = itemData;
	}

	public override void Reset()
	{
		base.Reset();
	}

	private void CloseAction()
	{
		Vespa_System_Action_Extension_Method.fire(_onClose, _selectItemIndex);
		_onClose = null;
		_onSelect = null;
		if (_windowPrefab != null)
		{
			_windowPrefab = null;
		}
		if (_globalWindowPrefab != null)
		{
			_globalWindowPrefab = null;
		}
		Close();
	}

	public void OnClickItem(string name)
	{
		_selectItemIndex = 0;
		UIHelper.EnableButton(ConfirmButton, enable: true, grayscaleIfDisable: true);
		int num = Convert.ToInt32(name);
		if (!__Extension_Method_ICollection__.IsNullOrEmpty(_itemDatas) && _itemDatas.Length > num)
		{
			_selectItemIndex = _itemDatas[num].Index;
			Vespa_System_Action_Extension_Method.fire(_onSelect, _selectItemIndex);
		}
	}

	public void OnClickSelect()
	{
		if (_selectItemIndex != 0)
		{
			CloseAction();
		}
	}

	public override void OnSystemBackButton()
	{
	}
}
