using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class RoulettaClick : MonoBehaviour, IPointerClickHandler
{
    public Animator animator;

    public void OnPointerClick(PointerEventData eventData)
    {
        animator.SetTrigger("ShowRouletta");

    }
}
