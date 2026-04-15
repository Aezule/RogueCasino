using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingManager : MonoBehaviour
{
    public CoinSpinner[] coins;

    void Start()
    {
        StartCoroutine(AnimateAndLoad());
    }

    IEnumerator AnimateAndLoad()
    {
        foreach (var coin in coins) coin.ResetToGray();

        coins[0].PlayAnimation();
        yield return new WaitForSeconds(1.2f);

        coins[1].PlayAnimation();
        yield return new WaitForSeconds(1.2f);

        coins[2].PlayAnimation();
        yield return new WaitForSeconds(1.2f);

        SceneManager.LoadScene("Shop");
    }
}