using UnityEngine;

public class SimpleLevelWin : MonoBehaviour
{
    private GameManager gameManager;

    public void LevelWin()
    {
        gameManager = GameManager.GetInstance();

        //gameManager.selectedVehicleRCC.canControl = false;
        gameManager.state = GameplayScreens.CutScene;
        gameManager.UpdateGameplayState();

        //gameManager.selectedVehicleRCC.Rigid.constraints = RigidbodyConstraints.FreezePositionX
        //       | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;

        gameManager.LevelWin();
    }
}
