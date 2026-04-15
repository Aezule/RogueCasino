using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CoinSpinner : MonoBehaviour
{
    public Sprite[] coinFrames;
    public Image coinImage;

    public void PlayAnimation()
    {
        if (coinImage == null) coinImage = GetComponent<Image>();
        coinImage.color = Color.gray;

        AnimateFrames();
    }

    public void ResetToGray()
    {
        if (coinImage == null) coinImage = GetComponent<Image>();
        coinImage.color = Color.gray;
        coinImage.sprite = coinFrames[0];
        DOTween.Kill(coinImage);
    }

    void AnimateFrames()
    {
        Sequence seq = DOTween.Sequence();

        for (int i = 0; i < 9; i++)
        {
            int frame = i;
            seq.AppendCallback(() => coinImage.sprite = coinFrames[frame]);
            seq.AppendInterval(0.08f);
        }
        seq.Append(coinImage.DOColor(Color.white, 0.3f));
    }
}