using UnityEngine;
using UnityEngine.UI;
using AkaitoAi.Singleton;

public class MenuManager : Singleton<MenuManager>
{
    [SerializeField] private Text coinsText;
    [SerializeField] private GameObject garage;
    public GameObject eventSystem;

    internal SetupSO setupScriptable;

    [Header("Screens")]
    public GameObject adsLoadingScreen;
    public GameObject vehicleSelectScreen;

    [Header("Panel Switch Bar")]
    public Animator transitionAnimator;
    public string openAnimation, closeAnimation;

    [Header("For Developer")]
    [SerializeField] private bool addCoins = false;
    [SerializeField] private int addCoinsAmount = 9999;
    [SerializeField] private bool clearData = false;

    //Bindings
    EventBinding<OnCoinsCharged> coinsChargedEventBinding;

    private void Awake()
    {
        Time.timeScale = 1f;

        setupScriptable = Resources.Load("ScriptableObjects/Setup/Setup") as SetupSO;
        
        if (clearData) PlayerPrefs.DeleteAll(); // ! Checks and Clear all saved data
        if (addCoins) PlayerPrefs.SetInt(setupScriptable.totalCoinsPref, addCoinsAmount); // ! Checks and add money from inspector


        UpdateTotalCoins();

        garage.SetActive(true);
    }

    private void Start()
    {
        if (transitionAnimator != null)
        {
            transitionAnimator.gameObject.SetActive(true);
            transitionAnimator.Play(openAnimation);
        }
        
        adsLoadingScreen.SetActive(false);

        //TODO Sound Calling
        SoundManager.Instance?.PlayMenuBG();

        //TODO Firebase Calling
        //if (Application.internetReachability != NetworkReachability.NotReachable)
        //    FirebaseManager.MainMenu();

        //TODO Ads Calling
        //AdsManager.instance.hideAllAdmobBanners();
        //AdsManager.instance.showAdMobBannerTop();
    }

    public void OpenURL(string _url)
    {
        //TODO Sound Calling
        SoundManager.Instance.PlayOnButtonSound();

        Application.OpenURL(_url);
    }

    private void UpdateTotalCoins() => coinsText.text = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref).ToString(); //! Shows Total money text

    private void OnEnable()
    {
        coinsChargedEventBinding = new EventBinding<OnCoinsCharged>(UpdateTotalCoins);
        EventBus<OnCoinsCharged>.Register(coinsChargedEventBinding);
    }
    private void OnDisable()
    {
        EventBus<OnCoinsCharged>.Deregister(coinsChargedEventBinding);
    }
}
