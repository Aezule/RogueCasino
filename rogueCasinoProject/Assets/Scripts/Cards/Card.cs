using UnityEngine;

public class Card : MonoBehaviour
{
    public int value;
    public string suit;

    private bool selected = false;

    void OnMouseDown()
    {
        selected = !selected;

        if(selected)
            transform.localScale = Vector3.one * 1.2f;
        else
            transform.localScale = Vector3.one;
    }
}