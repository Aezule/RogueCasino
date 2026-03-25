using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MenuClick : MonoBehaviour, IPointerClickHandler
{
    public Animator animator;

    public void OnPointerClick(PointerEventData eventData)
    {
        animator.SetTrigger("ShowMenu");

    }
}
