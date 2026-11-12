using System.Collections.Generic;
using NGame2.NNodeTree;
using NGame2.NNodeTree.NUtil;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NUI.NRender3D.NCinema;

public class NodeTreeKeywordContainer
{
	private List<NodeTreeKeyword> _list = new List<NodeTreeKeyword>();

	public void AddRange(IList<NodeTreeKeyword> nodeTreeKeywords)
	{
		if (__Extension_Method_ICollection__.IsNullOrEmpty(nodeTreeKeywords))
		{
			return;
		}
		for (int i = 0; i < __Extension_Method_ICollection__.COUNT(nodeTreeKeywords); i++)
		{
			NodeTreeKeyword nodeTreeKeyword = nodeTreeKeywords[i];
			if (nodeTreeKeyword != null)
			{
				_list.Add(nodeTreeKeyword);
			}
		}
	}

	public void Add(StringKey key, long uid)
	{
		_list.Add(new NodeTreeKeyword(key, uid));
	}

	public void Add(StringKey key, string str)
	{
		_list.Add(new NodeTreeKeyword(key, str));
	}

	public void Remove(long uid)
	{
		if (uid == 0L)
		{
			return;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list[i].IsEqual(uid))
			{
				_list.RemoveAt(i);
				break;
			}
		}
	}

	public void Remove(string str)
	{
		if (String_Extension_Method.IsNullOrEmpty(str))
		{
			return;
		}
		for (int i = 0; i < _list.Count; i++)
		{
			if (_list[i].IsEqual(str))
			{
				_list.RemoveAt(i);
				break;
			}
		}
	}

	public void RuntimeAddKeywords(BuildChildRootRuntime runtime)
	{
		for (int i = 0; i < _list.Count; i++)
		{
			NodeTreeKeyword nodeTreeKeyword = _list[i];
			if (nodeTreeKeyword == null)
			{
				break;
			}
			nodeTreeKeyword.RuntimeAddKeyword(runtime);
		}
	}

	public void Clear()
	{
		_list.Clear();
	}

	public void DrawGUI()
	{
		using (new BlockManager().TitleBox("NodeTreeKeyWord").Vertical())
		{
			for (int i = 0; i < _list.Count; i++)
			{
				NodeTreeKeyword nodeTreeKeyword = _list[i];
				GUILayout.Label($"Keyword : {nodeTreeKeyword.ToString()}");
			}
		}
	}
}
