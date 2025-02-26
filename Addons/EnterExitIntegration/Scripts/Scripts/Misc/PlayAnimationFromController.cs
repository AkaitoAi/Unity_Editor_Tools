using System.Collections;
using UnityEngine;

public class PlayAnimationFromController : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    public void PlayAnimation(string _animName)
    {
        if (_animator == null) return;

        _animator.Play(_animName);
    }
    
    public void PlayAnimationWithDelay(string _animName, float time)
    {
        if (_animator == null) return;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(time);

            _animator.Play(_animName);
        }
    }
    
    public void PlayAnimationWithDelay(string _animName)
    {
        if (_animator == null) return;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(.5f);

            _animator.Play(_animName);
        }
    }
    
    public void PlayAnimationAfterPreviousAnimation(string _animName)
    {
        if (_animator == null) return;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);

            _animator.Play(_animName);
        }
    }
}
