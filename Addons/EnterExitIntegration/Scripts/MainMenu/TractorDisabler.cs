using UnityEngine;

public class TractorDisabler : MonoBehaviour
{
    private Animator m_Animator;
    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<Animator>(out m_Animator)) return;

        m_Animator.gameObject.SetActive(false);
    }
}
