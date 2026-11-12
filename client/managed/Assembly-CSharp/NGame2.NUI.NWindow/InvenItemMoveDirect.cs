using System;
using System.Collections.Generic;
using NShared;

namespace NGame2.NUI.NWindow;

[Serializable]
public class InvenItemMoveDirect
{
	[Serializable]
	public class MoveDirectInfo
	{
		public CheckItemRegistInspector CheckItem;

		public MoveDirectRegistInspector MoveDirect;
	}

	public UIButton MoveDirectButton;

	public List<MoveDirectInfo> MoveDirectInfoList = new List<MoveDirectInfo>();

	private MoveDirectInfo _moveDirectInfo;

	public int MoveDirectChapterIndex
	{
		get
		{
			if (_moveDirectInfo == null)
			{
				return 0;
			}
			return _moveDirectInfo.MoveDirect.ChapterIndex;
		}
	}

	public int MoveDirectDungeonIndex
	{
		get
		{
			if (_moveDirectInfo == null)
			{
				return 0;
			}
			return _moveDirectInfo.MoveDirect.DungeonIndex;
		}
	}

	public MoveDirectRegistInspector MoveDirect
	{
		get
		{
			if (_moveDirectInfo == null)
			{
				return null;
			}
			return _moveDirectInfo.MoveDirect;
		}
	}

	public void Init(ItemData itemData)
	{
		_moveDirectInfo = GetMoveDirectInfo(itemData);
		SetEnableMoveDirectButton();
	}

	private void SetEnableMoveDirectButton()
	{
		UIHelper.EnableGameObject(MoveDirectButton, _moveDirectInfo != null);
	}

	private MoveDirectInfo GetMoveDirectInfo(ItemData itemData)
	{
		if (itemData == null)
		{
			return null;
		}
		if (__Extension_Method_ICollection__.IsNullOrEmpty(MoveDirectInfoList))
		{
			return null;
		}
		MoveDirectInfo moveDirectInfo = MoveDirectInfoList.Find((MoveDirectInfo info) => info.CheckItem.ItemIndex == itemData.Index);
		if (moveDirectInfo != null)
		{
			return moveDirectInfo;
		}
		moveDirectInfo = MoveDirectInfoList.Find((MoveDirectInfo info) => info.CheckItem.ItemType == itemData.Type);
		if (moveDirectInfo != null)
		{
			return moveDirectInfo;
		}
		return null;
	}
}
