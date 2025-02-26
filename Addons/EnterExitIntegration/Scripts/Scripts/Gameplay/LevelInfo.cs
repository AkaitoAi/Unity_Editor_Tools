using System.Collections;
using UnityEngine;

public class LevelInfo : MonoBehaviour
{
    public Transform vehicleSpawnPosition;
    public bool useTimer = false;
    public float winDelay = 5f;

    [Header("Objective Dialogues")]
    public DialogueSO dialogueSO;

    [Header("CutScene Setup")]
    [SerializeField] private bool playLevelStartCS = false;
    [SerializeField] private GameObject brainCamera;
    [SerializeField] private GameObject cutsceneContainer;
    public CutSceneSetup[] levelCutScenes;
    [HideInInspector] public int cutSceneCount = 0;

    [System.Serializable]
    public struct CutSceneSetup
    {
        public GameObject cutsceneObj;
        public float duration;
    }

    private GameManager gameManager;

    private void Start()
    {
        gameManager = GameManager.GetInstance();

        if (dialogueSO) gameManager.DisplayDialogue(dialogueSO.startDialogue);
        if (playLevelStartCS) PlayCutScene();
    }
    public void PlayCutScene()
    {
        if (cutSceneCount >= levelCutScenes.Length) return;

        EnableCutScene(levelCutScenes[cutSceneCount].cutsceneObj,
                levelCutScenes[cutSceneCount].duration);
    }

    private void EnableCutScene(GameObject csObj, float duration)
    {
        if (cutSceneCount > levelCutScenes.Length) return;

        cutsceneContainer.SetActive(true);

        gameManager.state = GameplayScreens.CutScene;
        gameManager.UpdateGameplayState();

        //gameManager.selectedVehicleRCC.Rigid.constraints = RigidbodyConstraints.FreezeAll;

        StartCoroutine(EnableCutScene(csObj));

        IEnumerator EnableCutScene(GameObject csObj)
        {
            yield return new WaitForSeconds(gameManager.cutSceneBarsAnimator.GetCurrentAnimatorStateInfo(0).length);

            csObj.SetActive(true);

            StartCoroutine(DisableCutScene(csObj, duration));

            IEnumerator DisableCutScene(GameObject csObj, float delay)
            {
                yield return new WaitForSeconds(delay);

                gameManager.cutSceneBarsAnimator.Play("OutCutSceneAnimation");

                yield return new WaitForSeconds(gameManager.cutSceneBarsAnimator.GetCurrentAnimatorStateInfo(0).length);

                //gameManager.selectedVehicleRCC.Rigid.constraints = RigidbodyConstraints.None;

                csObj.SetActive(false);

                gameManager.state = GameplayScreens.Resume;
                gameManager.UpdateGameplayState();

                //gameManager.selectedVehicleRCC.canControl = true;

                cutsceneContainer.SetActive(false);

                cutSceneCount++;
            }
        }
    }
}
