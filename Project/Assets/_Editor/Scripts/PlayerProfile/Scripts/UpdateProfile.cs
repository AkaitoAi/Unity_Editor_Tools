using UnityEngine;
using UnityEngine.UI;

namespace AkaitoAi
{
    public class UpdateProfile : MonoBehaviour
    {
        [SerializeField] private Image avatar;
        [SerializeField] private Image flag;
        [SerializeField] private Image progressFiller;
        [SerializeField] private Text nameText;
        [SerializeField] private Text ageText;

        [SerializeField] private PlayerProfileSO profileSO;

        public void UpdatePlayerProfile()
        {
            avatar.sprite = profileSO.GetSprite(0);
            flag.sprite = profileSO.GetSprite(1);

            nameText.text = profileSO.GetString(0);
            ageText.text = "Age: " + profileSO.GetString(1);
        }
    }
}
