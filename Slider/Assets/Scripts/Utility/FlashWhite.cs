using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(SpriteRenderer))]
public class FlashWhite : MonoBehaviour
{
    private SpriteRenderer mySprite;
    public Material whiteSpriteMat;
    private Material oldMat;

    void Awake()
    {
        mySprite = GetComponent<SpriteRenderer>();
        oldMat = mySprite.material;
    }
    
    public void Flash(int n, Action callback = null)
    {
        StopAllCoroutines();
        StartCoroutine(_Flash(n, callback));
    }

    public void SetSpriteActive(bool value)
    {
        mySprite.enabled = value;
    }

    private IEnumerator _Flash(int n, Action callback, float time = 0.25f)
    {
        for (int i = 0; i < n; i++)
        {
            mySprite.material = whiteSpriteMat;

            yield return new WaitForSeconds(time);

            mySprite.material = oldMat;

            yield return new WaitForSeconds(time);
        }

        callback?.Invoke();
    }
}
