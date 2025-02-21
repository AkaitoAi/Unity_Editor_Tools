using Cinemachine;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace Driveable.Core
{
    // All the controllers being used for enter exit system
    #region Enums
    public enum ControllerType
    {
        None,

        TPS,
        TPSDriveable,
        TPSDriveableBrain,

        Car,
        Bike,
    }
    #endregion

    // EventBus events for common notifications, can be accessed in any class without ref 
    #region Events
    public struct OnControllerChanged : IEvent { public ControllerType controller; }
    public struct OnTPSActive : IEvent { public Transform transform; }
    public struct OnProvideIK : IEvent
    {
        public int instanceID;
        public Transform lookAt;
        public Transform leftHand;
        public Transform rightHand;
        public Transform leftFoot;
        public Transform rightFoot;
    }
    #endregion

    // Structs being used in multiple classes
    #region Structs/ Classes
    [Serializable]
    public struct DriverIK
    {
        public Transform leftHandIK, rightHandIK, leftFootIK, rightFootIK;
    }
    [Serializable]
    public struct DoorOpenCloseSounds
    {
        public AudioClip openAudioClip;
        public AudioClip closeAudioClip;
        public AudioSource audioSource;

        public void PlayAudioClip(AudioClip audioClip)
        {
            if (audioSource == null) return;

            if (audioClip == null) return;

            audioSource.PlayOneShot(audioClip);
        }
    }
    [Serializable]
    public struct DriveableButtons
    {
        public Button enterButton;
        public Button exitButton;
    }
    [Serializable]
    public struct TPSCharacter
    {
        public Transform tpsCharacterController;
        public Transform tpsGetToDriveableCharacter;
        public Transform tpsDriverCharacter;
        public CinemachineBrain brainFollowCamera;
        public CinemachineVirtualCamera vFollowCamera;
    }
    [Serializable]
    public struct CameraCanvas
    {
        public GameObject[] tps;
        public GameObject[] rcc;
        public GameObject[] abp;
    }
    #endregion
}
