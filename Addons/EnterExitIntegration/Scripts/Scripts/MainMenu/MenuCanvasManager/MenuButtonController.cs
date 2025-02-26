using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum ButtonType
{
    Play_Game,
    Settings,
    Settings_Close,
    Quit_Game,
    Quit_Game_No,
    Quit_Game_Yes,
    Back_From_Vehicle,
    Vehicle_Select,
    Vehicle_Customization,
    Back_From_Customization,
    Mode_Select,
    Back_From_Mode,
    Level_Select,
    Back_From_level,
    Vertical_Loading,
    Skip_Level_Select
}

[RequireComponent(typeof(Button))]
public class MenuButtonController : MonoBehaviour
{
    public ButtonType buttonType;

    [SerializeField] private ButtonParam _buttonParam;

    private MenuManager menuManager;
    private MenuScreenManager canvasManager;
    private Button button;
    
    private Animator animator;
    internal string _currentState;

    [System.Serializable]
    public struct ButtonParam
    {
        public ScreenType screen;
        public string closeAnimation;
        public string openAnimation;
        public bool showAdsLoading;
        public bool showAdaptive;
    }


    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnButtonClicked);
        canvasManager = MenuScreenManager.GetInstance();
        menuManager = MenuManager.GetInstance();
        animator = menuManager.transitionAnimator;
    }

    private void OnButtonClicked()
    {
        //TODO Sound Calling
        SoundManager.Instance.PlayOnButtonSound();

        Transition(_buttonParam);

        switch (buttonType)
        {
            case ButtonType.Play_Game:

                break;

            case ButtonType.Quit_Game:

                break;
            
            case ButtonType.Quit_Game_No:

                break;

            case ButtonType.Quit_Game_Yes:
                Application.Quit();

                break;

            case ButtonType.Settings:

                break;

            case ButtonType.Settings_Close:

                break;

            case ButtonType.Vehicle_Select:

                break;

            case ButtonType.Back_From_Vehicle:

                break;

            case ButtonType.Mode_Select:

                break;

            case ButtonType.Back_From_Mode:

                break;

            case ButtonType.Vehicle_Customization:

                break;

            case ButtonType.Back_From_Customization:

                break;

            case ButtonType.Level_Select:

                break;

            case ButtonType.Back_From_level:

                break;
            
            case ButtonType.Vertical_Loading:

                break;
            
            case ButtonType.Skip_Level_Select:

                break;

            default: break;
        }
    }

    private void Transition(ButtonParam _param)
    {
        if (menuManager.transitionAnimator)
            StartCoroutine(ChangeAnimation(_param.closeAnimation, _param.openAnimation, _param.screen, _param.showAdsLoading));
        else 
            ChangePanel();

        void ChangePanel()
        {
            if (_param.showAdsLoading && Application.internetReachability != NetworkReachability.NotReachable)
            {
                menuManager.adsLoadingScreen.SetActive(true);

                StartCoroutine(ShowInterstitial());
            }
            else
            {
                canvasManager.SwitchCanvas(_param.screen);
            }

            IEnumerator ShowInterstitial()
            {
                Time.timeScale = 0f;

                //TODO Ads Calling
                //AdsController.Instance.HideAllBanners_Admob();

                yield return new WaitForSecondsRealtime(1f);

                //TODO Ads Calling
                //AdsController.Instance.ShowInterstitialAd_Admob();

                yield return new WaitForSecondsRealtime(.5f);

                Time.timeScale = 1f;

                menuManager.adsLoadingScreen.SetActive(false);

                canvasManager.SwitchCanvas(_param.screen);

                //TODO Ads Calling
                //if (!_param.showAdaptive) { AdsController.Instance.ShowSmallBannerTop(); yield break; }
                //if (_param.showAdaptive) { AdsController.Instance.ShowAdaptiveBannerTop(); yield break; }
            }
        }

        IEnumerator ChangeAnimation(string _anim_1, string _anim_2, ScreenType _screen, bool _showAdsLoading)
        {
            ChangeAnimationState(_anim_1);

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            if (_showAdsLoading && Application.internetReachability != NetworkReachability.NotReachable)
            {
                menuManager.adsLoadingScreen.SetActive(true);

                StartCoroutine(ShowInterstitial());
            }
            else
            {
                canvasManager.SwitchCanvas(_screen);
            }
            
            ChangeAnimationState(_anim_2);

            void ChangeAnimationState(string _newState)
            {
                if (_currentState == _newState) return;

                animator.Play(_newState);

                _currentState = _newState;
            }

            IEnumerator ShowInterstitial()
            {
                Time.timeScale = 0f;

                //TODO Ads Calling
                //AdsController.Instance.HideAllBanners_Admob();

                yield return new WaitForSecondsRealtime(1f);

                //TODO Ads Calling
                //AdsController.Instance.ShowInterstitialAd_Admob();

                yield return new WaitForSecondsRealtime(.5f);

                Time.timeScale = 1f;
               
                //ChangeAnimationState(_anim_1);

                //yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                menuManager.adsLoadingScreen.SetActive(false);

                canvasManager.SwitchCanvas(_screen);
                
                ChangeAnimationState(_anim_2);
                
                //TODO Ads Calling
                //AdsController.Instance.ShowSmallBannerTop();
            }
        }
    }

    private void SwitchCanvas(OnSwitchPanel screen)
    {
        canvasManager.SwitchCanvas(screen.type);
    }

    EventBinding<OnSwitchPanel> panelSwitchEventBinding;
    private void OnEnable()
    {
        panelSwitchEventBinding = new EventBinding<OnSwitchPanel>(SwitchCanvas);
        
        EventBus<OnSwitchPanel>.Register(panelSwitchEventBinding);
    }

    private void OnDisable()
    {
        EventBus<OnSwitchPanel>.Deregister(panelSwitchEventBinding);
    }
}
