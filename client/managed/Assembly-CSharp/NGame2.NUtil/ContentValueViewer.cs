using System.Collections.Generic;
using System.Text;
using NGame2.NContents;
using NShared;
using NVespa;
using NVespa.NGlobal;
using NVespa.NGui;
using NVespa.NSingleton;
using UnityEngine;

namespace NGame2.NUtil;

public class ContentValueViewer : GlobalAutoBehaviour<ContentValueViewer>
{
	private StringBuilder stringBuilder = new StringBuilder(1000);

	private void Start()
	{
		GlobalAutoBehaviour<GuiManager>.instance.Add("ContentValueViewer", this, DrawControlPanel);
	}

	private void DrawControlPanel()
	{
		using (new BlockManager().Vertical())
		{
			if (GUILayout.Button("Update Content Value"))
			{
				UpdateContentValue();
			}
			GUILayout.Label(stringBuilder.ToString());
		}
	}

	private void UpdateContentValue()
	{
		ContentsValueManager contentsValueManager = SingletonAuto<ContentsValueManager>.instance;
		stringBuilder.Clear();
		if (contentsValueManager == null)
		{
			stringBuilder.Append("ContentsValueManager instance is null");
			return;
		}
		if (contentsValueManager.Dic.Count < 1)
		{
			stringBuilder.Append("ContentsValueManager is Empty");
			return;
		}
		foreach (KeyValuePair<string, ContentsValueInfo> item in contentsValueManager.Dic)
		{
			stringBuilder.AppendFormat("[{0}]  {1}\n", item.Value.Key, item.Value.Value);
		}
	}
}
