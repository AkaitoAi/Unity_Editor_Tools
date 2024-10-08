using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Events;

namespace AkaitoAi.Timeline
{
    [RequireComponent(typeof(PlayableDirector))]
    [RequireComponent(typeof(SignalReceiver))]
    public class TimelineCutsceneManager : MonoBehaviour
    {
        //[SerializeField] private GameManager gameManager;
        //[SerializeField] private GameObject cutsceneScreen, resumeScreen;
        [SerializeField] private bool playLevelStartCS = false;
        [SerializeField] private GameObject cutsceneContainer;
        public CutSceneSetup[] levelCutScenes;
        private int cutSceneCount = 0;
        public bool PlayLevelStartCS => playLevelStartCS;
        public int CutSceneCount { get => cutSceneCount; set => cutSceneCount = value; }

        [Serializable]
        public struct CutSceneSetup
        {
            public GameObject cutsceneObj;
            public PlayableDirector director;
            public PlayableAsset[] cutsceneParts;
            internal int cutScenePart;
        }

        public static event Action OnCutSceneStartAction;
        public static event Action<int> OnCutScenePartAction;
        public static event Action OnCutSceneCompleteAction;
        public static event Action<float> OnFadeInOutScreenAction;

        public UnityEvent OnCutSceneStartEvent;
        public UnityEvent OnCutSceneNextPartStartEvent;
        public UnityEvent OnCutSceneCompleteEvent;

        private void Start()
        {
            //if(gameManager == null) gameManager = GameManager.Instance;

            if (playLevelStartCS) PlayCutScene();
        }

        #region Signals
        public void PlayCutScene()
        {
            //if (gameManager == null) gameManager = GameManager.Instance;

            foreach (CutSceneSetup cs in levelCutScenes)
                cs.cutsceneObj.SetActive(false);

            if (cutSceneCount >= levelCutScenes.Length) return;

            if (levelCutScenes[cutSceneCount].director == null) return;

            EnableCutScene(levelCutScenes[cutSceneCount].cutsceneObj);

            //TODO Sound Calling

        } // TODO Call to play cutscene
        public void PlayNextCutscenePart() //TODO Add Emitter at the end of timeline length
        {
            if (levelCutScenes[cutSceneCount].cutsceneParts == null) return;

            if (levelCutScenes[cutSceneCount].cutScenePart >
                levelCutScenes[cutSceneCount].cutsceneParts.Length) return;

            levelCutScenes[cutSceneCount].cutScenePart++;

            OnCutScenePartAction?.Invoke(levelCutScenes[cutSceneCount].cutScenePart);
            OnCutSceneNextPartStartEvent?.Invoke();

            levelCutScenes[cutSceneCount].director.playableAsset =
                levelCutScenes[cutSceneCount].cutsceneParts[levelCutScenes[cutSceneCount].cutScenePart];

            levelCutScenes[cutSceneCount].director.Play();
        }
        public void StopCutScene()  //TODO Add Emitter at the end of timeline length
        {
            if (cutSceneCount >= levelCutScenes.Length) return;

            DisableCutScene(levelCutScenes[cutSceneCount].cutsceneObj);

            //TODO Sound Calling
        }
        public void FadeInOut(float time) => OnFadeInOutScreenAction?.Invoke(time); // TODO FadeInOut using CanvasGroup
        public void CutSceneTimeScale(float scale) //TODO Timeline timescale
        {
            if (!levelCutScenes[cutSceneCount].director.playableGraph.IsValid()) return;

            levelCutScenes[cutSceneCount].director.Play();

            //levelCutScenes[cutSceneCount].director.
            //    playableGraph.GetRootPlayable
            //    (levelCutScenes[cutSceneCount].cutScenePart).
            //    SetSpeed(scale);

            levelCutScenes[cutSceneCount].director.
                playableGraph.GetRootPlayable
                (0).
                SetSpeed(scale);

            Time.timeScale = scale;
        } 
        #endregion

        #region CutsceneController
        private void EnableCutScene(GameObject csObj)
        {
            if (cutSceneCount > levelCutScenes.Length) return;

            cutsceneContainer.SetActive(true);

            //TODO Switch to Cutscene Screen
            //gameManager.state = GameplayScreens.CutScene;
            //gameManager.UpdateGameplayState();

            //TODO Controller
            //gameManager.selectedVehicleRCC.rigid.constraints =
            //    RigidbodyConstraints.FreezeAll;

            EnableCutScene(csObj);

            void EnableCutScene(GameObject csObj)
            {
                levelCutScenes[cutSceneCount].director.playableAsset =
                levelCutScenes[cutSceneCount].cutsceneParts[levelCutScenes[cutSceneCount].cutScenePart];

                csObj.SetActive(true);

                OnCutSceneStartAction?.Invoke();
                OnCutSceneStartEvent?.Invoke();

                levelCutScenes[cutSceneCount].director.RebuildGraph();
                levelCutScenes[cutSceneCount].director.time = 0.0;
                levelCutScenes[cutSceneCount].director.Play();
            }
        }

        private void DisableCutScene(GameObject csObj)
        {
            if (cutSceneCount > levelCutScenes.Length) return;

            DisableCutScene(csObj);

            void DisableCutScene(GameObject csObj)
            {
                csObj.SetActive(false);

                //TODO Switch to Gameplay Screen
                //gameManager.state = GameplayScreens.Resume;
                //gameManager.UpdateGameplayState();

                //TODO Controller
                //gameManager.selectedVehicleRCC.canControl = true;
                //gameManager.selectedVehicleRCC.rigid.constraints =
                //    RigidbodyConstraints.None;

                OnCutSceneCompleteAction?.Invoke();
                OnCutSceneCompleteEvent?.Invoke();

                levelCutScenes[cutSceneCount].director.Stop();

                cutsceneContainer.SetActive(false);

                levelCutScenes[cutSceneCount].cutScenePart = 0;

                cutSceneCount++;
            }
        }

        #endregion
    }
}
