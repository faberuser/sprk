using System;
using System.Collections.Generic;
using NVespa.NGui.NBlockManager;
using UnityEngine;

namespace NVespa.NGui;

public class BlockManager : IDisposable
{
	private int _disposeCountInArray;

	private IDisposable[] _disposeArray = new IDisposable[10];

	private List<IDisposable> _extraDisposeList;

	public BlockManager Profile(string name)
	{
		AppendDisposable(new ProfileBlock().Open(name));
		return this;
	}

	public BlockManager Space(float space)
	{
		AppendDisposable(new LayoutSpace().Open(space));
		return this;
	}

	public BlockManager Vertical(params GUILayoutOption[] options)
	{
		AppendDisposable(new VerticalBlock().Open(options));
		return this;
	}

	public BlockManager Vertical(GUIStyle style, params GUILayoutOption[] options)
	{
		AppendDisposable(new VerticalBlock().Open(style, options));
		return this;
	}

	public BlockManager VerticalBox(Color i_boxColor, params GUILayoutOption[] options)
	{
		VerticalBlock verticalBlock = new VerticalBlock().Open(GUI.skin.box, options);
		verticalBlock.ColorBox(i_boxColor);
		AppendDisposable(verticalBlock);
		return this;
	}

	public BlockManager VerticalBox(Color i_boxColor, float thickness, params GUILayoutOption[] options)
	{
		VerticalBlock verticalBlock = new VerticalBlock().Open(GUI.skin.box, options);
		verticalBlock.ColorBox(i_boxColor, thickness);
		AppendDisposable(verticalBlock);
		return this;
	}

	public BlockManager Horizontal(params GUILayoutOption[] options)
	{
		AppendDisposable(new HorizontalBlock().Open(options));
		return this;
	}

	public BlockManager Horizontal(GUIStyle style, params GUILayoutOption[] options)
	{
		AppendDisposable(new HorizontalBlock().Open(style, options));
		return this;
	}

	public BlockManager HorizontalBox(Color i_boxColor, params GUILayoutOption[] options)
	{
		HorizontalBlock horizontalBlock = new HorizontalBlock().Open(GUI.skin.box, options);
		horizontalBlock.ColorBox(i_boxColor, 3f);
		AppendDisposable(horizontalBlock);
		return this;
	}

	public BlockManager HorizontalBox(Color i_boxColor, float i_thickness, params GUILayoutOption[] options)
	{
		HorizontalBlock horizontalBlock = new HorizontalBlock().Open(GUI.skin.box, options);
		horizontalBlock.ColorBox(i_boxColor, i_thickness);
		AppendDisposable(horizontalBlock);
		return this;
	}

	public BlockManager TitleBox(string i_title, params GUILayoutOption[] options)
	{
		AppendDisposable(new TitleBoxBlock().Open(i_title, options));
		return this;
	}

	public BlockManager TitleBox(string i_title, float boxWidth, params GUILayoutOption[] options)
	{
		AppendDisposable(new TitleBoxBlock().Open(i_title, boxWidth, options));
		return this;
	}

	public BlockManager TitleBox(string i_title, Color i_color, params GUILayoutOption[] options)
	{
		AppendDisposable(new TitleBoxBlock().Open(i_title, i_color, options));
		return this;
	}

	public BlockManager TitleBox(string i_title, float boxWidth, Color i_color, params GUILayoutOption[] options)
	{
		AppendDisposable(new TitleBoxBlock().Open(i_title, boxWidth, i_color, options));
		return this;
	}

	public BlockManager TitleBoxAdvance(string i_title, Color i_color, object keyObj, string keyName, out bool isExpand, Action onDrawGui, params GUILayoutOption[] options)
	{
		AppendDisposable(new TitleBoxAdvanceBlock().Open(i_title, i_color, keyObj, keyName, out isExpand, onDrawGui, options));
		return this;
	}

	public BlockManager Enable()
	{
		AppendDisposable(new EnabledBlock().Open());
		return this;
	}

	public BlockManager Enable(bool i_enable)
	{
		AppendDisposable(new EnabledBlock().Open(i_enable));
		return this;
	}

	public BlockManager GUIColor()
	{
		AppendDisposable(new GUIColorBlock().Open());
		return this;
	}

	public BlockManager Scroll(object i_keyObj, string i_keyName)
	{
		AppendDisposable(new ScrollBlock(i_keyObj, i_keyName));
		return this;
	}

	public BlockManager Scroll(object i_keyObj, string i_keyName, bool alwaysShowHorizontal, bool alwaysShowVertical)
	{
		AppendDisposable(new ScrollBlock(i_keyObj, i_keyName, alwaysShowHorizontal, alwaysShowVertical));
		return this;
	}

	protected void AppendDisposable(IDisposable disposable)
	{
		if (_disposeCountInArray < _disposeArray.Length)
		{
			_disposeArray[_disposeCountInArray] = disposable;
			_disposeCountInArray++;
			return;
		}
		if (_extraDisposeList == null)
		{
			_extraDisposeList = new List<IDisposable>();
		}
		_extraDisposeList.Add(disposable);
	}

	public virtual void Dispose()
	{
		try
		{
			if (_extraDisposeList != null)
			{
				for (int num = _extraDisposeList.Count - 1; num >= 0; num--)
				{
					_extraDisposeList[num].Dispose();
				}
				_extraDisposeList.Clear();
			}
			for (int num2 = _disposeCountInArray - 1; num2 >= 0; num2--)
			{
				_disposeArray[num2].Dispose();
				_disposeArray[num2] = null;
			}
		}
		catch (InvalidOperationException ex)
		{
			Debug.LogWarning($"Exception: {Event.current.rawType}\n{ex.Message}");
		}
	}
}
