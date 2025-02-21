using System.Collections;
using UnityEngine;

public class PanelSwitchAnimator : MonoBehaviour
{
    [SerializeField] private Animator animator;
    
    internal string _currentState;
    public void ToggleObjectAnimation(string _anim, GameObject _enable, GameObject _disable)
    {
        StartCoroutine(ChangeAnimation(_anim, _enable, _disable));

        IEnumerator ChangeAnimation(string _anim, GameObject _enable, GameObject _disable)
        {
            ChangeAnimationState(_anim);

            yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

            _enable.SetActive(true);
            _disable.SetActive(false);

            ChangeAnimationState(_anim);


            void ChangeAnimationState(string _newState)
            {
                if (_currentState == _newState) return;

                animator.Play(_newState);

                _currentState = _newState;
            }
        }
    }
}
