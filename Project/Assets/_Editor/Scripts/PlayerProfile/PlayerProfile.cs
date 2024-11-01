//using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace AkaitoAi
{
    public class PlayerProfile : MonoBehaviour
    {
        [SerializeField] private GameObject profilePanel;
        [SerializeField] private InputField[] inputFields;
        [SerializeField] private Select[] selects;
        [SerializeField] private ScrollRectTween[] scrollRectTweens;
        private int index;
        private int currentAge;

        [System.Serializable]
        public struct ScrollRectTween
        {
            public ScrollRect scrollRect;
            public float scrollAmount; // 100f
            public float scrollSpeed; // 1f

            //internal Tweener scrollingUpTweener;
            //internal Tweener scrollingDownTweener;
        }

        [System.Serializable]
        public struct Select
        {
            public int index;
            public Image[] select;
            public GameObject[] selected;
            public GameObject[] unSelected;
        }

        // Prefs
        private const string pfSetup = "Profile_Setup";
        private const string userDetailsPref = "User_Avatar_Gender_Country";
        private const string userNameAgePref = "User_Name_Age";

        private void Start()
        {
            int inputLn = inputFields.Length;
            int selectLn = selects.Length;

            if (PlayerPrefs.GetInt(pfSetup) == 1)
            {
                for (int i = 0; i < inputLn; i++)
                    inputFields[i].text
                        = PlayerPrefs.GetString(userNameAgePref + i).ToString();

                profilePanel.SetActive(false);
            }
            else
            {
                for (int i = 0; i < inputLn; i++)
                    OnInputField(i);

                profilePanel.SetActive(true);

                PlayerPrefs.SetInt(pfSetup, 1);
            }

            for (int i = 0; i < selectLn; i++)
            {
                OnSelectIndex(i);
                OnSelectButton(PlayerPrefs.GetInt(userDetailsPref + i));
            }

            // Stores player age
            currentAge = int.Parse(PlayerPrefs.GetString(userNameAgePref + 1));
        }

        #region Input Field

        // Input field will have different index donot assign select index to inputfield and assign this
        // to OnValueChanged
        public void OnInputField(int index)
        {
            PlayerPrefs.SetString(userNameAgePref + index, inputFields[index].text);

            inputFields[index].text = PlayerPrefs.GetString(userNameAgePref + index).ToString();
        }

        #endregion

        #region Select Button
        // Assign both functions to buttons

        // Assign index as per the pref index, all avatars will have same index,
        // all flags will have same index and all genders will have same index
        public void OnSelectIndex(int index)
        {

            this.index = index;
        }


        // Each button will have different index in same category
        public void OnSelectButton(int selectIndex)
        {
            if (selects == null) return;

            // Toggle selected avatar/flag select sprite
            if (selects[index].select.Length > 0)
            {
                foreach (Image item in selects[index].select)
                    item.enabled = false;

                selects[index].select[selectIndex].enabled = true;
            }

            // Toggle gender unselected and selected button sprites
            if (selects[index].unSelected.Length > 0)
            {
                foreach (GameObject unSelect in selects[index].unSelected)
                    unSelect.SetActive(true);

                selects[index].unSelected[selectIndex].SetActive(false);
            }

            if (selects[index].selected.Length > 0)
            {
                foreach (GameObject select in selects[index].selected)
                    select.SetActive(false);

                selects[index].selected[selectIndex].SetActive(true);
            }

            PlayerPrefs.SetInt(userDetailsPref + index, selectIndex);
        }

        #endregion

        #region Buttons Scroll

        // Add EventTrigger to the button and added pointer up and down events
        // assign the start and stop funtions to those events
        // Set scrollview to clammped


        public void OnStartScrollingUp(int index)
        {
            //if (scrollRectTweens == null) return;

            //if (scrollRectTweens[index].scrollingUpTweener == null
            //    || !scrollRectTweens[index].scrollingUpTweener.IsActive())
            //{
            //    float targetPosition = scrollRectTweens[index].scrollRect.normalizedPosition.y
            //        - (scrollRectTweens[index].scrollAmount
            //        / scrollRectTweens[index].scrollRect.content.sizeDelta.y);

            //    targetPosition = Mathf.Clamp(targetPosition, 0f, 1f);

                //scrollRectTweens[index].scrollingUpTweener
                //    = scrollRectTweens[index].scrollRect
                //    .DONormalizedPos(new Vector2(scrollRectTweens[index]
                //    .scrollRect.normalizedPosition.x, targetPosition)
                //    , scrollRectTweens[index].scrollSpeed)
                //    .SetEase(Ease.Linear)
                //    .SetLoops(-1, LoopType.Incremental);
            //}
        }

        public void OnStopScrollingUp(int index)
        {
            //if (scrollRectTweens == null) return;

            //if (scrollRectTweens[index]
            //    .scrollingUpTweener != null
            //    && scrollRectTweens[index].scrollingUpTweener.IsActive())
            //{
            //    scrollRectTweens[index].scrollingUpTweener.Kill();
            //    scrollRectTweens[index].scrollingUpTweener = null;
            //}
        }

        public void OnStartScrollingDown(int index)
        {
            //if (scrollRectTweens == null) return;

            //if (scrollRectTweens[index].scrollingDownTweener == null
            //    || !scrollRectTweens[index].scrollingDownTweener.IsActive())
            //{
            //    float targetPosition = scrollRectTweens[index].scrollRect.normalizedPosition.y
            //        + (scrollRectTweens[index].scrollAmount
            //        / scrollRectTweens[index].scrollRect.content.sizeDelta.y);

            //    targetPosition = Mathf.Clamp(targetPosition, 0f, 1f);

                //scrollRectTweens[index].scrollingDownTweener
                //    = scrollRectTweens[index].scrollRect.DONormalizedPos(new Vector2(scrollRectTweens[index]
                //    .scrollRect.normalizedPosition.x, targetPosition)
                //    , scrollRectTweens[index].scrollSpeed)
                //    .SetEase(Ease.Linear)
                //    .SetLoops(-1, LoopType.Incremental);
            //}
        }

        public void OnStopScrollingDown(int index)
        {
            //if (scrollRectTweens == null) return;

            //if (scrollRectTweens[index]
            //    .scrollingDownTweener != null
            //    && scrollRectTweens[index].scrollingDownTweener.IsActive())
            //{
            //    scrollRectTweens[index].scrollingDownTweener.Kill();
            //    scrollRectTweens[index].scrollingDownTweener = null;
            //}
        }
        #endregion

        #region Increment/DecrementAge

        public void OnIncrementAgeButton()
        {
            currentAge++;

            if (currentAge > 99) currentAge = 99;

            //Age inputfield
            UpdateAge();
        }

        public void OnDecrementAgeButton()
        {
            if (currentAge > 5) // Prevent age from going negative
            {
                currentAge--;

                //Age inputfield
                UpdateAge();
            }
        }

        private void UpdateAge()
        {
            // Convert age to string, update PlayerPrefs, and InputField
            inputFields[1].text = currentAge.ToString();
            OnInputField(1);

            //string ageString = currentAge.ToString();
            //PlayerPrefs.SetString("Age", ageString);
            //ageInputField.text = ageString;
        }

        #endregion
    }
}
