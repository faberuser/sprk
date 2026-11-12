namespace NShared.GetEquipGacha;

[Response]
public class Response : GenericResponse<ResultType>
{
	public EquipGachaInfo[] EquipGachaInfos { get; set; }

	public StarterPickupEquipGachaInfo[] StarterPickupEquipGachaInfos { get; set; }

	public StepUpEquipGachaInfo[] StepUpEquipGachaInfos { get; set; }
}
