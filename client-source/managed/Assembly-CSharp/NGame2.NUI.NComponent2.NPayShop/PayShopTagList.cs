using System;
using System.Collections;
using System.Collections.Generic;
using NShared;
using NVespa;
using NVespa.NPrefab2;
using NVespa.NUtil;
using UnityEngine;

namespace NGame2.NUI.NComponent2.NPayShop;

public class PayShopTagList : MonoBehaviour, PayShopTagComponent.IListener
{
	public interface IListener
	{
		void OnClickTag(int tag);

		void OnClickClose();
	}

	public TweenPosition Tween_TagPanel;

	public UIScrollView TagScrollView;

	public GameObject HotTagRoot;

	public Transform HotTagPos;

	public Transform AllTagPos;

	public Transform SelectedTagPos;

	public UIWidget Widget_AllTagArea;

	public UISprite Sprite_OpenArrow;

	public Vector2 Padding;

	private bool _isOpen;

	private bool _isOpenAnimation;

	private IListener _listener;

	private List<PayShopTagComponent> _hotTagComponents = new List<PayShopTagComponent>();

	private List<PayShopTagComponent> _allTagComponents = new List<PayShopTagComponent>();

	private PayShopTagComponent _selectedTagComponent;

	public string SelectedCategory { get; private set; }

	public int SelectedTagIndex { get; private set; }

	public void Open(IListener listener)
	{
		_listener = listener;
	}

	public void ClearAll()
	{
		_isOpen = false;
		_isOpenAnimation = false;
		SelectedCategory = "";
		SelectedTagIndex = 0;
		ReleaseComponents();
		ForceAllTagPanelClose();
		CloseSeletedTag();
	}

	public void SelectCategory(string category)
	{
		if (SelectedTagIndex != 0)
		{
			CloseSeletedTag();
		}
		if (String_Extension_Method.IsNullOrEmpty(SelectedCategory) || !GameRoot.Instance.NewPayShopTagDataContainer.IsEqualCategory(SelectedCategory, category))
		{
			CreateTagComponents(category);
		}
	}

	private void CreateTagComponents(string category)
	{
		SelectedCategory = category;
		ReleaseComponents();
		NewPayShopTagData[] tags = GameRoot.Instance.NewPayShopTagDataContainer.GetTags(category);
		Array.Sort(tags, (NewPayShopTagData a, NewPayShopTagData b) => a.Sort.CompareTo(b.Sort));
		int num = 0;
		NewPayShopTagData[] array = tags;
		foreach (NewPayShopTagData tagData in array)
		{
			if (num < 3)
			{
				PayShopTagComponent item = CreateTagComponent(tagData, PayShopTagComponent.Hot_PrefabAuto, HotTagPos);
				_hotTagComponents.Add(item);
				num++;
			}
			PayShopTagComponent item2 = CreateTagComponent(tagData, PayShopTagComponent.PrefabAuto, AllTagPos);
			_allTagComponents.Add(item2);
		}
		SingletonUpdater<Executer>.instance.NextRun(Reposition);
	}

	private PayShopTagComponent CreateTagComponent(NewPayShopTagData tagData, PrefabAuto<PayShopTagComponent> prefabAuto, Transform parent)
	{
		PayShopTagComponent payShopTagComponent = prefabAuto.Factory.Create(parent);
		payShopTagComponent.Open(tagData, this);
		return payShopTagComponent;
	}

	private void ReleaseComponents()
	{
		FactoryBase.ReleaseAndClearList(_allTagComponents);
		FactoryBase.ReleaseAndClearList(_hotTagComponents);
		ReleaseSelectedTagComponent();
	}

	private void ReleaseSelectedTagComponent()
	{
		FactoryBase.SafeReleaseComponent(ref _selectedTagComponent);
	}

	public void OnClickOpen()
	{
		if (!_isOpenAnimation && !(Tween_TagPanel == null))
		{
			_isOpen = !_isOpen;
			UpdateOpenArrow(_isOpen);
			VespaCoroutine.Start(CoOpenPanel(_isOpen));
		}
	}

	private IEnumerator CoOpenPanel(bool isOpen)
	{
		_isOpenAnimation = true;
		if (_isOpen)
		{
			TagScrollView.ResetPosition();
			UIHelper.EnableGameObject(Tween_TagPanel, enable: true);
			yield return null;
			yield return null;
			yield return null;
			if ((bool)Widget_AllTagArea)
			{
				Widget_AllTagArea.ForceUpdateAnchors(includeChildren: true);
			}
			Reposition();
			RefreshTagPanelComponent();
		}
		Tween_TagPanel.Play(_isOpen);
		while (Tween_TagPanel.enabled)
		{
			UIHelper.EnableGameObject(Tween_TagPanel, enable: true, isForce: true);
			yield return null;
		}
		if (!_isOpen)
		{
			UIHelper.EnableGameObject(Tween_TagPanel, enable: false);
		}
		_isOpenAnimation = false;
	}

	private void RefreshTagPanelComponent()
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(_allTagComponents))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(_allTagComponents); i++)
		{
			PayShopTagComponent payShopTagComponent = _allTagComponents[i];
			if (!(payShopTagComponent == null))
			{
				int index = payShopTagComponent.TagData.Index;
				payShopTagComponent.SeletedTag(SelectedTagIndex == index);
			}
		}
	}

	private void Reposition()
	{
		Reposition(_hotTagComponents, wordWrap: false);
		Reposition(_allTagComponents);
	}

	private void Reposition(List<PayShopTagComponent> lst, bool wordWrap = true)
	{
		Reposition(lst, Padding, wordWrap, Widget_AllTagArea.width);
	}

	public static void Reposition(List<PayShopTagComponent> lst, Vector2 padding, bool wordWrap = true, int maxWidth = 0)
	{
		Vector2 zero = Vector2.zero;
		foreach (PayShopTagComponent item in lst)
		{
			item.CalcWidth();
			if (wordWrap && zero.x + item.Width > (float)maxWidth)
			{
				zero.x = 0f;
				zero.y -= padding.y;
			}
			item.transform.localPosition = zero;
			zero.x += item.Width + padding.x;
		}
	}

	void PayShopTagComponent.IListener.OnClickTag(int tagIndex)
	{
		if (!_isOpenAnimation && SelectedTagIndex != tagIndex)
		{
			if (_listener != null)
			{
				_listener.OnClickTag(tagIndex);
			}
			UIHelper.EnableGameObject(HotTagRoot, enable: false);
			NewPayShopTagData data = GameRoot.Instance.NewPayShopTagDataContainer.GetData(tagIndex);
			if (_selectedTagComponent == null)
			{
				_selectedTagComponent = CreateTagComponent(data, PayShopTagComponent.Selected_PrefabAuto, SelectedTagPos);
			}
			else
			{
				_selectedTagComponent.Open(data, this);
			}
			SelectedTagIndex = tagIndex;
			if (_isOpen)
			{
				ForceAllTagPanelClose();
			}
		}
	}

	void PayShopTagComponent.IListener.OnCloseTag()
	{
		if (!_isOpenAnimation)
		{
			CloseSeletedTag();
			if (_listener != null)
			{
				_listener.OnClickClose();
			}
		}
	}

	public void CloseSeletedTag()
	{
		SelectedTagIndex = 0;
		UIHelper.EnableGameObject(HotTagRoot, enable: true);
		ReleaseSelectedTagComponent();
		if (_isOpen)
		{
			ForceAllTagPanelClose();
		}
	}

	public void ForceAllTagPanelClose()
	{
		_isOpen = false;
		UpdateOpenArrow(_isOpen);
		Tween_TagPanel.ResetToBeginning();
		UIHelper.EnableGameObject(Tween_TagPanel, enable: false);
	}

	private void UpdateOpenArrow(bool isOpen)
	{
		if (Sprite_OpenArrow != null)
		{
			Sprite_OpenArrow.flip = (isOpen ? UIBasicSprite.Flip.Vertically : UIBasicSprite.Flip.Nothing);
		}
	}
}
