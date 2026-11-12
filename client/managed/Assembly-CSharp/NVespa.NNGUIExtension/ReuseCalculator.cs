using UnityEngine;

namespace NVespa.NNGUIExtension;

public class ReuseCalculator
{
	private float _width;

	private float _reciprocalWidth;

	private float _height;

	private float _reciprocalHeight;

	private UIScrollView.Movement _movement;

	private int _maxPerLine;

	private bool _isValid;

	private Vector2 _clippingZeroPos;

	private Vector3 _clippingOffsetVec;

	private Vector2 _clippingScale;

	public void Setup(UIPanel clippingPanel, UIScrollView scrollView, UIGrid grid)
	{
		if (clippingPanel == null)
		{
			SetValid(isValid: false);
			return;
		}
		if (scrollView == null)
		{
			SetValid(isValid: false);
			return;
		}
		if (grid == null)
		{
			SetValid(isValid: false);
			return;
		}
		Vector4 finalClipRegion = clippingPanel.finalClipRegion;
		Vector4 baseClipRegion = clippingPanel.baseClipRegion;
		_clippingOffsetVec = finalClipRegion - baseClipRegion;
		_clippingScale = new Vector2(baseClipRegion.z, baseClipRegion.w);
		SetValid(isValid: true);
		_width = grid.cellWidth;
		if (_width != 0f)
		{
			_reciprocalWidth = 1f / _width;
		}
		_height = grid.cellHeight;
		if (_height != 0f)
		{
			_reciprocalHeight = 1f / _height;
		}
		_clippingZeroPos = -clippingPanel.clipOffset;
		_maxPerLine = grid.maxPerLine;
		_movement = scrollView.movement;
	}

	public Vector2 GetPositionByIndex(int index)
	{
		Vector2 result = default(Vector2);
		if (!IsValid())
		{
			return result;
		}
		int num = 0;
		int num2 = 0;
		if (_movement == UIScrollView.Movement.Vertical)
		{
			if (_maxPerLine == 0)
			{
				num = index;
			}
			else
			{
				num = index / _maxPerLine;
				num2 = index - _maxPerLine * num;
			}
		}
		else
		{
			if (_movement != UIScrollView.Movement.Horizontal)
			{
				VespaDebug.LogError("Not Developed!!");
				return result;
			}
			if (_maxPerLine == 0)
			{
				num2 = index;
			}
			else
			{
				num2 = index / _maxPerLine;
				num = index - num2 * _maxPerLine;
			}
		}
		result.x = (float)num2 * _width;
		result.y = (float)num * (0f - _height);
		return result;
	}

	public bool GetClippingPanelShowIndex(UIPanel clippingPanel, UIScrollView scrollView, out float firstItemIndex, out float lastItemIndex, out UIReuseScrollGrid.EScrollMoveDirection moveDirection)
	{
		firstItemIndex = 0f;
		lastItemIndex = 0f;
		moveDirection = UIReuseScrollGrid.EScrollMoveDirection.Left;
		Vector2 clipOffset = clippingPanel.clipOffset;
		if (_movement == UIScrollView.Movement.Horizontal)
		{
			float num2;
			float num = (num2 = clipOffset.x - _clippingOffsetVec.x) + _clippingScale.x;
			float x = scrollView.currentMomentum.x;
			firstItemIndex = num2 * _reciprocalWidth;
			lastItemIndex = num * _reciprocalWidth;
			if (x < 0f)
			{
				moveDirection = UIReuseScrollGrid.EScrollMoveDirection.Left;
			}
			else
			{
				moveDirection = UIReuseScrollGrid.EScrollMoveDirection.Right;
			}
		}
		else if (_movement == UIScrollView.Movement.Vertical)
		{
			float y = scrollView.currentMomentum.y;
			float num4;
			float num3 = (num4 = 0f - clipOffset.y + _clippingOffsetVec.y) + _clippingScale.y;
			firstItemIndex = num4 * _reciprocalHeight;
			lastItemIndex = num3 * _reciprocalHeight;
			if (y < 0f)
			{
				moveDirection = UIReuseScrollGrid.EScrollMoveDirection.Down;
			}
			else
			{
				moveDirection = UIReuseScrollGrid.EScrollMoveDirection.Up;
			}
		}
		return true;
	}

	public void GetClippingPanelFixedIndex(int fixedValue, int totalItemCount, ref float firstItemIndex, ref float lastItemIndex, ref float fixedFirstItemIndex, ref float fixedLastItemIndex)
	{
		fixedFirstItemIndex = firstItemIndex - (float)fixedValue;
		fixedLastItemIndex = lastItemIndex + (float)fixedValue;
		if (_maxPerLine > 0)
		{
			firstItemIndex *= _maxPerLine;
			fixedFirstItemIndex *= _maxPerLine;
			lastItemIndex *= _maxPerLine;
			fixedLastItemIndex *= _maxPerLine;
		}
		if (firstItemIndex < 0f)
		{
			firstItemIndex = 0f;
		}
		if (fixedFirstItemIndex < 0f)
		{
			fixedFirstItemIndex = 0f;
		}
		if ((float)(totalItemCount - 1) < fixedLastItemIndex)
		{
			fixedLastItemIndex = totalItemCount - 1;
		}
		if ((float)(totalItemCount - 1) < lastItemIndex)
		{
			lastItemIndex = totalItemCount - 1;
		}
	}

	public int GetIndexByPosition(Vector3 localPos)
	{
		int result = 0;
		if (!IsValid())
		{
			return result;
		}
		int num = Mathf.RoundToInt(localPos.y * _reciprocalHeight);
		int num2 = Mathf.RoundToInt(localPos.x * _reciprocalWidth);
		if (_movement == UIScrollView.Movement.Vertical)
		{
			result = ((_maxPerLine != 0) ? (num * _maxPerLine + num2) : num);
		}
		else if (_movement == UIScrollView.Movement.Horizontal)
		{
			result = ((_maxPerLine != 0) ? (num2 * _maxPerLine + num) : num2);
		}
		else
		{
			VespaDebug.LogError("Not Developed!!");
		}
		return result;
	}

	private void SetValid(bool isValid)
	{
		_isValid = isValid;
	}

	public bool IsValid()
	{
		return _isValid;
	}

	public int GetSwitchItemCount(int count)
	{
		if (_maxPerLine > 0)
		{
			return count * _maxPerLine;
		}
		return count;
	}
}
