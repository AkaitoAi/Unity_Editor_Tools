using UnityEngine;
using TMPro;
using System.Collections;
using AkaitoAi.Advertisement;

public class RuntimeInterstitial : MonoBehaviour
{
	[SerializeField] private TMP_Text text;
    [SerializeField] private int seconds = 60;

    private void Start()
    {
        text.gameObject.SetActive(false);

        StartCoroutine(StartTimer());
    }

    private IEnumerator StartTimer()
    {
        while (true)
        {
            for (int i = seconds; i > 0; i--)
            {
                yield return new WaitForSeconds(1);
            }

            for (int i = 3; i > 0; i--)
            {
                AdsWrapper.GetInstance().InternetReachability( () =>
                {
                    text.gameObject.SetActive(true);
                    text.text = $"Loading Advertisement in {i}sec";
                });
                
                yield return new WaitForSeconds(1);
            }

            AdsWrapper.GetInstance().InternetReachability(() =>
            {
                text.gameObject.SetActive(false);
                AdsWrapper.GetInstance().ShowInterstitial();
            });

            yield return new WaitForSeconds(2);
        }
    }
}
