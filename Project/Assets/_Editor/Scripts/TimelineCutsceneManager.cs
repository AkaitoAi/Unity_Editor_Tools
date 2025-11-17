using System;
using UnityEngine;
using UnityEngine.Events;
#if USE_TIMELINE_AkaitoAi
using UnityEngine.Playables;
using UnityEngine.Timeline;
#endif

namespace AkaitoAi.Timeline
{
#if USE_TIMELINE_AkaitoAi
    [RequireComponent(typeof(PlayableDirector))]
    [RequireComponent(typeof(SignalReceiver))]
#endif
    public class TimelineCutsceneManager : MonoBehaviour
    {
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
#if USE_TIMELINE_AkaitoAi
            public PlayableDirector director;
            public PlayableAsset[] cutsceneParts;
#endif
            public UnityEvent OnPlayEvent, OnEndEvent;
            internal int cutScenePart;
        }

        public static event Action OnCutSceneStartAction;
        public static event Action<int> OnCutScenePartAction;
        public static event Action OnCutSceneCompleteAction;
        public static event Action<float> OnFadeInOutScreenAction;
        public static event Action<float> OnFadeInScreenAction;
        public static event Action<float> OnFadeOutScreenAction;

        public UnityEvent OnStartEvent;
        public UnityEvent OnCutSceneStartEvent;
        public UnityEvent OnCutSceneNextPartStartEvent;
        public UnityEvent OnCutSceneCompleteEvent;
        
        private GameManager gameManager;

        private void Start()
        {
            if(gameManager == null) gameManager = GameManager.GetInstance();

            if (playLevelStartCS) PlayCutScene();

            OnStartEvent?.Invoke();
        }

        #region Signals
        [ContextMenu("Play Cutscene")]
        public void PlayCutScene()
        {
            if (gameManager == null) gameManager = GameManager.GetInstance();

            foreach (CutSceneSetup cs in levelCutScenes)
                cs.cutsceneObj.SetActive(false);

            if (cutSceneCount > levelCutScenes.Length - 1)
            {
                Debug.LogError($"Cutscene count is {cutSceneCount} but levelCutScenes length is {cutSceneCount > levelCutScenes.Length - 1}");

                return;
            }

#if USE_TIMELINE_AkaitoAi
            if (levelCutScenes[cutSceneCount].director == null)
            {
                Debug.LogError($"Cutscene {levelCutScenes[cutSceneCount]} director is null");

                return;
            }
#endif
            EnableCutScene(levelCutScenes[cutSceneCount].cutsceneObj);

            //TODO Sound Calling

        } // TODO Call to play cutscene

        [ContextMenu("Play Cutscene Next Part")]
        public void PlayCutsceneNextPart() //TODO Add Emitter at the end of timeline length
        {
            CutSceneSetup cs = levelCutScenes[cutSceneCount];

            if (cs.cutsceneParts == null || cs.cutsceneParts.Length == 0)
            {
                Debug.LogError("Cutscene parts are null or empty");
                return;
            }

            cs.cutScenePart = (cs.cutScenePart >= cs.cutsceneParts.Length - 1) ? 0 : cs.cutScenePart + 1;

            levelCutScenes[cutSceneCount] = cs;

            OnCutScenePartAction?.Invoke(cs.cutScenePart);
            OnCutSceneNextPartStartEvent?.Invoke();

#if USE_TIMELINE_AkaitoAi
            cs.director.playableAsset = cs.cutsceneParts[cs.cutScenePart];
            cs.director.Play();
#endif
        }
        [ContextMenu("Stop Cutscene")]
        public void StopCutScene()  //TODO Add Emitter at the end of timeline length
        {
            DisableCutScene(levelCutScenes[cutSceneCount].cutsceneObj);

            //TODO Sound Calling
        }
        public void FadeInOut(float time) => OnFadeInOutScreenAction?.Invoke(time); // TODO FadeInOut using CanvasGroup
        public void FadeIn(float time) => OnFadeInScreenAction?.Invoke(time); // TODO FadeInOut using CanvasGroup
        public void FadeOut(float time) => OnFadeOutScreenAction?.Invoke(time); // TODO FadeInOut using CanvasGroup
        public void CutSceneTimeScale(float scale) //TODO Timeline timescale
        {
#if USE_TIMELINE_AkaitoAi
            if (!levelCutScenes[cutSceneCount].director.playableGraph.IsValid())
            {
                Debug.LogError($"Cutscene {levelCutScenes[cutSceneCount]} director is not valid");

                return;
            }

            levelCutScenes[cutSceneCount].director.Play();
#endif

            //levelCutScenes[cutSceneCount].director.
            //    playableGraph.GetRootPlayable
            //    (levelCutScenes[cutSceneCount].cutScenePart).
            //    SetSpeed(scale);

#if USE_TIMELINE_AkaitoAi
            levelCutScenes[cutSceneCount].director.
                playableGraph.GetRootPlayable
                (0).
                SetSpeed(scale);

            Time.timeScale = scale;
#endif
        }
        #endregion

        #region CutsceneController
        private void EnableCutScene(GameObject csObj)
        {
            if (cutSceneCount > levelCutScenes.Length - 1)
            {
                Debug.LogError($"Cutscene count is {cutSceneCount} but levelCutScenes length is {cutSceneCount > levelCutScenes.Length - 1}");

                return;
            }

            cutsceneContainer.SetActive(true);

            //TODO Switch to Cutscene Screen
            gameManager.State = GameplayScreens.CutScene;
            gameManager.UpdateGameplayState();

            //TODO Controller
            //gameManager.selectedVehicleRCC.rigid.constraints =
            //    RigidbodyConstraints.FreezeAll;

#if USE_TIMELINE_AkaitoAi
            levelCutScenes[cutSceneCount].director.playableAsset =
            levelCutScenes[cutSceneCount].cutsceneParts[levelCutScenes[cutSceneCount].cutScenePart];
#endif
            csObj.SetActive(true);

            levelCutScenes[cutSceneCount].OnPlayEvent?.Invoke();

            OnCutSceneStartAction?.Invoke();
            OnCutSceneStartEvent?.Invoke();


#if USE_TIMELINE_AkaitoAi
            levelCutScenes[cutSceneCount].director.RebuildGraph();
            levelCutScenes[cutSceneCount].director.time = 0.0;
            levelCutScenes[cutSceneCount].director.Play();
#endif
        }

        private void DisableCutScene(GameObject csObj)
        {
            csObj.SetActive(false);

            //TODO Switch to Gameplay Screen
            //gameManager.State = GameplayScreens.Resume;
            //gameManager.UpdateGameplayState();

            //TODO Controller
            //gameManager.selectedVehicleRCC.canControl = true;
            //gameManager.selectedVehicleRCC.rigid.constraints =
            //    RigidbodyConstraints.None;

            levelCutScenes[cutSceneCount].OnEndEvent?.Invoke();

            OnCutSceneCompleteAction?.Invoke();
            OnCutSceneCompleteEvent?.Invoke();

#if USE_TIMELINE_AkaitoAi
            levelCutScenes[cutSceneCount].director.Stop();
#endif

            cutsceneContainer.SetActive(false);

            levelCutScenes[cutSceneCount].cutScenePart = 0;

            cutSceneCount = cutSceneCount >= levelCutScenes.Length - 1 ? 0 : ++cutSceneCount;
        }

#endregion
    }
}
