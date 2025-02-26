using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VehicleSelection : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button incrementButton, decrementButton;
    [SerializeField] private GameObject uiDrag;

    [Header("Total Coins")]
    [SerializeField] private Text coinsText;

    [Header("Vehicle Showcase")]
    [SerializeField] private VehicleDetails[] vehicles;
    [SerializeField] private CarSpecsUI carSpecsUI;
    [SerializeField] private bool lockAllVehiclesInStart = false;
    internal int vehicleIndex = 0;
    internal int pervehicleIndex = 0;
    internal Animator _animator;

    [Header("Buy Process")]
    [SerializeField] private GameObject vehicleLockedImageObj;
    [SerializeField] private GameObject priceContainer;
    [SerializeField] private bool useBuyCoinTextLerp;
    [SerializeField] private Text priceText;
    [SerializeField] private bool useDollarSignBeforePrice;

    [Header("Buy Error")]
    [SerializeField] private GameObject buyErrorObj;
    [SerializeField] private bool showCoinsRequired = false;
    [SerializeField] private Text buyErrorText;
    [SerializeField] private float showBuyErrorFor = 2.5f;

    [Header("Lerp Configration")]
    [SerializeField] private float priceLerpDuration = 3f;

    internal SetupSO setupScriptable;

    [Serializable]
    public struct VehicleDetails
    {
        public GameObject vehicleObj;
        public int price;
        [HideInInspector] public ScriptableObject scriptableObject;
    }

    private void Start()
    {
        setupScriptable = MenuManager.GetInstance().setupScriptable;

        coinsText.text = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref).ToString();
        
        EnableSlectedVehicle();

        if (carSpecsUI != null)
            carSpecsUI.UpdateVehicleSpecs((VehicleSpecsSO)vehicles[vehicleIndex].scriptableObject);

        if (buyErrorObj) buyErrorObj.SetActive(false);

        CheckUnlockedVehicles();

        void EnableSlectedVehicle() // Call it on the back of next panel
        {
            int length = vehicles.Length;
            for (int i = 0; i < length; i++)
            {
                vehicles[i].scriptableObject = 
                    Resources.Load("ScriptableObjects/Vehicle_Specs/VehicleSpecs_" + i) as ScriptableObject;

                vehicles[i].vehicleObj.SetActive(false);
            }
            if (!lockAllVehiclesInStart) PlayerPrefs.SetInt(setupScriptable.vehiclesPref + 0, 1);

            vehicleIndex = setupScriptable.sVehicleIndex;

            vehicles[vehicleIndex].vehicleObj.SetActive(true);
        }
    }

    public void OnSelectButton() => setupScriptable.sVehicleIndex = vehicleIndex;
    public void OnBuyButton()
    {
        if (PlayerPrefs.GetInt(setupScriptable.totalCoinsPref) 
            >= vehicles[vehicleIndex].price)
        {
            PlayerPrefs.SetInt(setupScriptable.vehiclesPref + vehicleIndex, 1);

            int totalCoins = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref);
            int chargedCoins = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref)
                - vehicles[vehicleIndex].price;

            PlayerPrefs.SetInt(setupScriptable.totalCoinsPref, chargedCoins);

            // TODO Sound Calling
            SoundManager.Instance.PlayPurchaseSound();

            buyButton.gameObject.SetActive(false);
            if (vehicleLockedImageObj) vehicleLockedImageObj.SetActive(false);
            selectButton.gameObject.SetActive(true);
            if(priceContainer) priceContainer.SetActive(false);

            if (!useBuyCoinTextLerp) StaticUpdateCoins();
            else StartCoroutine(UpdateCoins(totalCoins, chargedCoins));

            EventBus<OnCoinsCharged>.Raise(new OnCoinsCharged { });

            priceText.color = Color.green;
            priceText.text = "Purchased";
        }
        else
        {
            // TODO Sound Calling
            SoundManager.Instance.PlayPurchaseFailedSound();

            if (!buyErrorObj) return;

            buyButton.interactable = false;
            buyErrorObj.SetActive(true);
            if (vehicleLockedImageObj) vehicleLockedImageObj.SetActive(false);

            if (showCoinsRequired)
            {
                int needCoins = vehicles[vehicleIndex].price - PlayerPrefs.GetInt(setupScriptable.totalCoinsPref);
                buyErrorText.text = "You Need More Than $" + needCoins + " To Buy This Vehicle!";
            }
            else
            {
                buyErrorText.text = "You Have Not Enough Coins!";
            }

            StartCoroutine(DisableBuyErrorObj());
        }

        IEnumerator UpdateCoins(int _totalCoins, int _chargedCoins)
        {
            float timeElapsed = 0;
            while (timeElapsed < priceLerpDuration)
            {
                float price = Mathf.Lerp(_totalCoins, _chargedCoins, timeElapsed / priceLerpDuration);

                coinsText.text = Mathf.FloorToInt(price).ToString();

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            coinsText.text = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref).ToString();
        }
        void StaticUpdateCoins() => 
            coinsText.text = PlayerPrefs.GetInt(setupScriptable.totalCoinsPref).ToString();
        IEnumerator DisableBuyErrorObj()
        {
            yield return new WaitForSeconds(showBuyErrorFor);

            buyButton.interactable = true;
            buyErrorObj.SetActive(false);
            if (vehicleLockedImageObj) vehicleLockedImageObj.SetActive(true);
        }

    }
    public void OnIncrementButton()
    {
        // TODO Sound Calling
        SoundManager.Instance.PlayOnButtonSound();

        vehicleIndex++;

        if (vehicleIndex >= vehicles.Length)
            vehicleIndex = 0;

        vehicles[vehicleIndex].vehicleObj.SetActive(true);

        CheckUnlockedVehicles();
        
    }
    public void OnDecrementButton()
    {
        // TODO Sound Calling
        SoundManager.Instance.PlayOnButtonSound();

        vehicleIndex--;

        if (vehicleIndex < 0)
            vehicleIndex = vehicles.Length - 1;

        vehicles[vehicleIndex].vehicleObj.SetActive(true);

        CheckUnlockedVehicles();
    }
    private void CheckUnlockedVehicles()
    {
        int length = vehicles.Length;
        for (int i = 0; i < length; i++)
            vehicles[i].vehicleObj.SetActive(false);

        vehicles[vehicleIndex].vehicleObj.SetActive(true);

        if (carSpecsUI != null)
            carSpecsUI.UpdateVehicleSpecs((VehicleSpecsSO)vehicles[vehicleIndex].scriptableObject);

        if (PlayerPrefs.GetInt(setupScriptable.vehiclesPref + vehicleIndex) == 1)
        {
            buyButton.gameObject.SetActive(false);
            if (vehicleLockedImageObj) vehicleLockedImageObj.SetActive(false);
            if (priceContainer) priceContainer.SetActive(false);
            selectButton.gameObject.SetActive(true);

            priceText.text = "Purchased";
        }
        else
        {
            if (priceContainer) priceContainer.SetActive(true);
            buyButton.gameObject.SetActive(true);
            if (vehicleLockedImageObj) vehicleLockedImageObj.SetActive(true);
            selectButton.gameObject.SetActive(false);
            StartCoroutine(VehicleStatsTextLerp(vehicleIndex));
        }

        if (carSpecsUI == null) return;

        if (PlayerPrefs.GetInt(setupScriptable.vehiclesPref + vehicleIndex) == 1) priceText.color = Color.green;
        else if (PlayerPrefs.GetInt(setupScriptable.totalCoinsPref) >= vehicles[vehicleIndex].price
            && PlayerPrefs.GetInt(setupScriptable.vehiclesPref + vehicleIndex) == 0)
        {
            priceText.color = Color.yellow;
            //GetBuyButtonShiny().enabled = true;
        }
        else
        {
            priceText.color = Color.red;
            //GetBuyButtonShiny().enabled = false;
        }

        //UIShiny GetBuyButtonShiny()
        //{
        //    if (buyButton.TryGetComponent<UIShiny>(out UIShiny _btnShiny))
        //        return _btnShiny;

        //    return null;
        //}

        IEnumerator VehicleStatsTextLerp(int _priceIndex)
        {
            float timeElapsed = 0;
            while (timeElapsed < priceLerpDuration)
            {
                float price = Mathf.Lerp(0f, vehicles[vehicleIndex].price, timeElapsed / priceLerpDuration);

                if (!useDollarSignBeforePrice) priceText.text = Mathf.FloorToInt(price).ToString();
                else priceText.text = "$" + Mathf.FloorToInt(price).ToString();

                timeElapsed += Time.deltaTime;
                yield return null;
            }

            if (PlayerPrefs.GetInt(setupScriptable.vehiclesPref + vehicleIndex) == 0)
            {
                if (!useDollarSignBeforePrice) priceText.text = vehicles[vehicleIndex].price.ToString();
                else priceText.text = "$" + vehicles[vehicleIndex].price.ToString();
            }
            else
            {
                priceText.text = "Purchased";
            }
        }
    }
}
