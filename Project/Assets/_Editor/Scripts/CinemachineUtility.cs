using AkaitoAi.Extensions;
using Cinemachine;
using UnityEngine;

public class CinemachineUtility : MonoBehaviour
{
    [SerializeField] private CinemachineBrain cBrain;

    private void Start()
    {
        cBrain.GetOrAddComponent<CinemachineUtility>();
    }
    public void ChangeBlendTime(float _time) => cBrain.m_DefaultBlend.m_Time = _time;
}
