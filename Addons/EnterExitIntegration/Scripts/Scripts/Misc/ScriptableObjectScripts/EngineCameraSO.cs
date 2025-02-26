using UnityEngine;

[CreateAssetMenu(fileName = "EngineCamera", menuName = "ScriptableObjects/EngineCamera", order = 1)]
public class EngineCameraSO : ScriptableObject
{
    public int toolIndex;
    public Vector2 maxEngineTorque;
    public float cameraTPSDistance, cameraTPSHeight;
}
