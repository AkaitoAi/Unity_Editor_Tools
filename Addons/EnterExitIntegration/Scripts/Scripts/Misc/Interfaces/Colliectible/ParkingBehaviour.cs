using System.Collections;
using UnityEngine;

public class ParkingBehaviour : MonoBehaviour
{
    private GameManager gameManager;

    private void Parking(Transform align, ParkingType parkingType)
    {
        gameManager = GameManager.GetInstance();

        switch (parkingType)
        {
            case ParkingType.SimpleWin:

                LevelWin();

            break;

            case ParkingType.WinAfterCutscene:

                StartCoroutine(LevelWinAfterCutscene());

                IEnumerator LevelWinAfterCutscene()
                {
                    //if (gameManager.selectedMode == 0)
                    //{
                    //    if (gameManager.levelNumber < gameManager.modeOneLevelsLength - 1) 
                    //        gameManager.selectedVehicle.SetActive(false);
                    //}

                    gameManager.levelInfo.PlayCutScene();
                    yield return new WaitForSeconds
                        (gameManager.levelInfo.levelCutScenes
                        [gameManager.levelInfo.levelCutScenes.Length - 1].duration);

                    LevelWin();
                }

                break;

            default: break;
        }
    }

    private void LevelWin()
    {
        //gameManager.selectedVehicleRCC.canControl = false;
        gameManager.state = GameplayScreens.CutScene;
        gameManager.UpdateGameplayState();

        //gameManager.selectedVehicleRCC.Rigid.constraints = RigidbodyConstraints.FreezePositionX
        //       | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        gameManager.LevelWin();
    }


    private void OnEnable()
    {
        ParkingCollectibles.OnParked += Parking;
    }
    private void OnDisable()
    {
        ParkingCollectibles.OnParked -= Parking;
    }
}
