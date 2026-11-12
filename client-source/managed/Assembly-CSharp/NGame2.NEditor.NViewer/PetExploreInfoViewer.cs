using NShared;
using UnityEngine;

namespace NGame2.NEditor.NViewer;

public class PetExploreInfoViewer : BaseGenericViewer<PetExploreInfo>
{
	private EditorVariable<int> _editorableEndTime = new EditorVariable<int>("남은 시간", 0);

	private int _endTime;

	protected override PetExploreInfo DrawValue(int index, PetExploreInfo value)
	{
		if (value != null)
		{
			base.DrawValue(index, value);
			if (GUILayout.Button("탐험 시간 종료"))
			{
				Cheat($"endpetexplore {value.ExploreIndex} {value.DeckIndex}", null);
			}
		}
		return value;
	}
}
