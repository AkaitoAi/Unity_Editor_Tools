using UnityEngine;

public class FarmerAnimator : MonoBehaviour
{
	[SerializeField] private Animator _animator;
    [SerializeField] private int totalClips;
    internal int randNum;

    private void OnEnable()
    {
        randNum = Random.Range(0, totalClips);

        _animator.Play(randNum.ToString());
    }
}
