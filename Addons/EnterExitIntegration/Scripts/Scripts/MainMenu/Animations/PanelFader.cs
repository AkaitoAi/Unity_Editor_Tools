using UnityEngine;
using DG.Tweening;

public class PanelFader : MonoBehaviour
{
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private float _duration = 0.5f;

    private void OnEnable()
    {
        FadeIN();
    }

    public void FadeOut(GameObject panelToActivate)
    {
        //TODO Sounds Calling
        SoundManager.Instance?.PlayOnButtonSound();

        if (_canvasGroup == null) return;

        MenuManager.GetInstance()?.eventSystem.SetActive(false);

        _canvasGroup.alpha = 1;
       //cg.interactable = false;
       //cg.DOFade(0f, _duration);//.OnComplete(() => cg.interactable = false);
       _canvasGroup.DOFade(0f, _duration).SetEase(Ease.OutBack).OnComplete(() => { 
           
           settingsPanel.SetActive(false);

           //if(panelToActivate == menuPanel)
           //    menu.OpenMenu();

           panelToActivate.SetActive(true);
       });

    }

    public void FadeIN()
    {
        if (_canvasGroup == null) return;

        settingsPanel.SetActive(true);

        MenuManager.GetInstance()?.eventSystem.SetActive(false);

        _canvasGroup.alpha = 0;
        //cg.interactable = false;
        _canvasGroup.DOFade(1f, _duration).SetEase(Ease.OutBack).OnComplete(() => MenuManager.GetInstance()?.eventSystem.SetActive(true));
    }
}
