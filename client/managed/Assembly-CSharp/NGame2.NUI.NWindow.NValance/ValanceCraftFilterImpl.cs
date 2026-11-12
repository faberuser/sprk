using NShared;

namespace NGame2.NUI.NWindow.NValance;

public class ValanceCraftFilterImpl : IValanceFilterImpl
{
	private ValanceCraftDataKey _selectedKey = new ValanceCraftDataKey();

	private int _selectDiplayEquipOpitonIndex;

	private int[] _equipOptionIndices;

	public ValanceCraftDataKey SelectedKey => _selectedKey;

	public int[] EquipOptionIndices => _equipOptionIndices;

	public int SelectDisplayEquipOptionIndex => _selectDiplayEquipOpitonIndex;

	public bool IsNoneSelected()
	{
		return _selectedKey.Equals(ValanceCraftDataKey.DefaultKey);
	}

	public bool IsAllSelected()
	{
		if (_selectedKey.SetType == EquipItemSetGroupType.None)
		{
			return false;
		}
		if (_selectedKey.CreatureTagType == CreatureTagType.None)
		{
			return false;
		}
		if (_selectedKey.EquipPartType == EquipPartType.None && _selectedKey.EquipItemSubType == EquipItemSubType.None)
		{
			return false;
		}
		return true;
	}

	public EValanceFilterSelected Select(IValanceFilterInfo info)
	{
		if (!(info is IValanceEquipFilterInfo valanceEquipFilterInfo))
		{
			return EValanceFilterSelected.UnSelect;
		}
		if (valanceEquipFilterInfo.SetType != EquipItemSetGroupType.None)
		{
			_selectedKey.SetType = valanceEquipFilterInfo.SetType;
			return EValanceFilterSelected.SetType;
		}
		if (valanceEquipFilterInfo.CreatureTagType != CreatureTagType.None)
		{
			if (_selectedKey.CreatureTagType == valanceEquipFilterInfo.CreatureTagType)
			{
				_selectDiplayEquipOpitonIndex = 0;
				_equipOptionIndices = null;
				_selectedKey.CreatureTagType = CreatureTagType.None;
				return EValanceFilterSelected.UnSelect;
			}
			_selectedKey.CreatureTagType = valanceEquipFilterInfo.CreatureTagType;
			return EValanceFilterSelected.CreatureTagType;
		}
		if (valanceEquipFilterInfo.EquipPartType != EquipPartType.None || valanceEquipFilterInfo.EquipItemSubType != EquipItemSubType.None)
		{
			if (_selectedKey.EquipPartType == valanceEquipFilterInfo.EquipPartType && _selectedKey.EquipItemSubType == valanceEquipFilterInfo.EquipItemSubType)
			{
				_selectDiplayEquipOpitonIndex = 0;
				_equipOptionIndices = null;
				_selectedKey.EquipPartType = EquipPartType.None;
				_selectedKey.EquipItemSubType = EquipItemSubType.None;
				return EValanceFilterSelected.UnSelect;
			}
			_selectedKey.EquipPartType = valanceEquipFilterInfo.EquipPartType;
			_selectedKey.EquipItemSubType = valanceEquipFilterInfo.EquipItemSubType;
			return EValanceFilterSelected.EquipPartType;
		}
		if (_selectDiplayEquipOpitonIndex == valanceEquipFilterInfo.DisplayEquipOptionIndex)
		{
			_selectDiplayEquipOpitonIndex = 0;
			_equipOptionIndices = null;
			return EValanceFilterSelected.UnSelect;
		}
		_selectDiplayEquipOpitonIndex = valanceEquipFilterInfo.DisplayEquipOptionIndex;
		_equipOptionIndices = ((ValanceEquipFilterData)valanceEquipFilterInfo).ExtraEquipOptionIndices;
		return EValanceFilterSelected.ExtraEquipOptionIndex;
	}

	public void Clear()
	{
		_selectDiplayEquipOpitonIndex = 0;
		_equipOptionIndices = null;
		_selectedKey.SetType = EquipItemSetGroupType.None;
		_selectedKey.CreatureTagType = CreatureTagType.None;
		_selectedKey.EquipPartType = EquipPartType.None;
		_selectedKey.EquipItemSubType = EquipItemSubType.None;
	}
}
