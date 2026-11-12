using System;
using System.Collections.Generic;
using NVespa;
using UnityEngine;

namespace NNguiExtension;

public class UISpriteSelector : UIBasicSelector, IInspectorGuiDrawer
{
	[Serializable]
	public class SelectData
	{
		public string Name = string.Empty;

		public UIAtlas Atlas;

		public string SpriteName = string.Empty;

		public Color SpriteColor = Color.white;

		public Vector3 Position;

		public int Width;

		public int Height;

		public UIBasicSprite.Flip Flip;
	}

	[SerializeField]
	private UISprite _targetSprite;

	[HideInInspector]
	[SerializeField]
	private bool _applyAtlas;

	[HideInInspector]
	[SerializeField]
	private bool _applySprite = true;

	[HideInInspector]
	[SerializeField]
	private bool _applyColor = true;

	[HideInInspector]
	[SerializeField]
	private bool _applyPosition = true;

	[HideInInspector]
	[SerializeField]
	private bool _applySize = true;

	[HideInInspector]
	[SerializeField]
	private bool _applyFlip;

	[HideInInspector]
	public List<SelectData> SelectList = new List<SelectData>();

	[SerializeField]
	private UIButton _targetButton;

	public bool ApplyAtlas => _applyAtlas;

	public bool ApplySprite => _applySprite;

	public UISprite GetTargetSprite()
	{
		if (_targetSprite != null)
		{
			return _targetSprite;
		}
		return GetComponent<UISprite>();
	}

	private void Awake()
	{
	}

	public void Show(bool show)
	{
		UISprite targetSprite = GetTargetSprite();
		if ((bool)targetSprite)
		{
			targetSprite.gameObject.SetActive(show);
		}
	}

	public bool SelectToButton(string name, UIButtonColor.State buttonState)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SelectList))
		{
			return false;
		}
		if (_targetButton == null)
		{
			_targetButton = GetComponent<UIButton>();
		}
		for (int i = 0; i < SelectList.Count; i++)
		{
			SelectData selectData = SelectList[i];
			if (selectData.Name == name)
			{
				if (_targetButton != null)
				{
					_targetButton.SetStateUiSprite(buttonState, selectData.SpriteName);
				}
				return true;
			}
		}
		return true;
	}

	public override bool Select(string name)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(SelectList))
		{
			return false;
		}
		for (int i = 0; i < SelectList.Count; i++)
		{
			if (SelectList[i].Name == name)
			{
				SelectByIndex(i);
				return true;
			}
		}
		return false;
	}

	public override void SelectByIndex(int index)
	{
		if (Vespa_Generic_IList_Extension_Method.IsValidIndex(SelectList, index))
		{
			SelectData selectData = SelectList[index];
			UISprite targetSprite = GetTargetSprite();
			if (_applyAtlas)
			{
				targetSprite.atlas = selectData.Atlas;
			}
			if (_applySprite)
			{
				targetSprite.spriteName = selectData.SpriteName;
			}
			if (_applyColor)
			{
				targetSprite.color = selectData.SpriteColor;
			}
			if (_applyPosition)
			{
				targetSprite.transform.localPosition = selectData.Position;
			}
			if (_applySize)
			{
				targetSprite.width = selectData.Width;
				targetSprite.height = selectData.Height;
			}
			if (_applyFlip)
			{
				targetSprite.flip = selectData.Flip;
			}
			UIButton component = targetSprite.GetComponent<UIButton>();
			if (component != null)
			{
				component.normalSprite = selectData.SpriteName;
			}
		}
	}

	public void SetTargetSprite(UISprite uISprite)
	{
		_targetSprite = uISprite;
	}
}
