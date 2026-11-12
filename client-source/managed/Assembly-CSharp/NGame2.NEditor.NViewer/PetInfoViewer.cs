using NShared;
using NVespa.NGui;
using UnityEngine;

namespace NGame2.NEditor.NViewer;

public class PetInfoViewer : BaseGenericViewer<PetInfo>
{
	private EditorVariable<int> _editorableFood = new EditorVariable<int>("Food", 100);

	private int _food = 100;

	protected override PetInfo DrawValue(int index, PetInfo value)
	{
		if (value != null)
		{
			base.DrawValue(index, value);
			if (GUILayout.Button("펫 정보 초기화"))
			{
				Cheat($"resetpet {value.PetIndex}", null);
			}
			using (new BlockManager().Horizontal())
			{
				_food = _editorableFood.Edit(_food);
			}
			if (GUILayout.Button(_food + " 먹이주기"))
			{
				Cheat($"setpetfullpoint {value.PetIndex} {_food}", null);
			}
		}
		return value;
	}
}
