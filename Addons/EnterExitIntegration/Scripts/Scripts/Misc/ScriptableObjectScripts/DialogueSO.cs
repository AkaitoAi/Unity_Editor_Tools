using UnityEngine;

[CreateAssetMenu(fileName = "Dialogue", menuName = "ScriptableObjects/Dialogue", order = 1)]
public class DialogueSO : ScriptableObject
{
    [TextArea] public string startDialogue;
    [TextArea] public string vehicleEnterDialogue;
    [TextArea] public string machineAttachedDialogue;
    [TextArea] public string fieldEnterDialogue;
    [TextArea] public string cargoDeliveryDialogue;
}
