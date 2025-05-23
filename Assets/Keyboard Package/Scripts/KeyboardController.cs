using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyboardController : MonoBehaviour
{
    [SerializeField] GameObject han_smallAlphaRow1;
    [SerializeField] GameObject han_smallAlphaRow2;
    [SerializeField] GameObject han_smallAlphaRow3;

    [SerializeField] GameObject han_capitalAlphaRow1;
    [SerializeField] GameObject han_capitalAlphaRow2;
    [SerializeField] GameObject han_capitalAlphaRow3;

    [SerializeField] GameObject eng_smallAlphaRow1;
    [SerializeField] GameObject eng_smallAlphaRow2;
    [SerializeField] GameObject eng_smallAlphaRow3;

    [SerializeField] GameObject eng_capitalAlphaRow1;
    [SerializeField] GameObject eng_capitalAlphaRow2;
    [SerializeField] GameObject eng_capitalAlphaRow3;


    /*
    [SerializeField] GameObject numbers;
    [SerializeField] GameObject splCharsNum1;
    [SerializeField] GameObject splCharsNum2;
    [SerializeField] GameObject splChars1;
    [SerializeField] GameObject splChars2;
    */

    [SerializeField] GameObject actionHan;
    [SerializeField] GameObject actionEng;
    //[SerializeField] GameObject actionCapital;

    private bool isHanSmallLettersShown = true;
    private bool isEngSmallLettersShown = true; 

    public void ShowHanCapitalLetters() {
        isHanSmallLettersShown = false;

        actionHan.SetActive(true);
        actionEng.SetActive(true);
        //actionCapital.SetActive(true);

        han_smallAlphaRow1.SetActive(false);
        han_smallAlphaRow2.SetActive(false);
        han_smallAlphaRow3.SetActive(false);

        han_capitalAlphaRow1.SetActive(true);
        han_capitalAlphaRow2.SetActive(true);
        han_capitalAlphaRow3.SetActive(true);


        eng_smallAlphaRow1.SetActive(false);
        eng_smallAlphaRow2.SetActive(false);
        eng_smallAlphaRow3.SetActive(false);

        eng_capitalAlphaRow1.SetActive(false);
        eng_capitalAlphaRow2.SetActive(false);
        eng_capitalAlphaRow3.SetActive(false);
    }
    
    public void ShowHanSmallLetters() {
        isHanSmallLettersShown = true;

        actionHan.SetActive(true);
        actionEng.SetActive(true);
        //actionCapital.SetActive(true);

        han_smallAlphaRow1.SetActive(true);
        han_smallAlphaRow2.SetActive(true);
        han_smallAlphaRow3.SetActive(true);

        han_capitalAlphaRow1.SetActive(false);
        han_capitalAlphaRow2.SetActive(false);
        han_capitalAlphaRow3.SetActive(false);


        eng_smallAlphaRow1.SetActive(false);
        eng_smallAlphaRow2.SetActive(false);
        eng_smallAlphaRow3.SetActive(false);

        eng_capitalAlphaRow1.SetActive(false);
        eng_capitalAlphaRow2.SetActive(false);
        eng_capitalAlphaRow3.SetActive(false);
    }
    public void ShowEngCapitalLetters()
    {
        isEngSmallLettersShown = false;

        actionHan.SetActive(true);
        actionEng.SetActive(true);
        //actionCapital.SetActive(true);

        eng_smallAlphaRow1.SetActive(false);
        eng_smallAlphaRow2.SetActive(false);
        eng_smallAlphaRow3.SetActive(false);

        eng_capitalAlphaRow1.SetActive(true);
        eng_capitalAlphaRow2.SetActive(true);
        eng_capitalAlphaRow3.SetActive(true);


        han_smallAlphaRow1.SetActive(false);
        han_smallAlphaRow2.SetActive(false);
        han_smallAlphaRow3.SetActive(false);

        han_capitalAlphaRow1.SetActive(false);
        han_capitalAlphaRow2.SetActive(false);
        han_capitalAlphaRow3.SetActive(false);
    }

    public void ShowEngSmallLetters()
    {
        isEngSmallLettersShown = true;

        actionHan.SetActive(true);
        actionEng.SetActive(true);
        //actionCapital.SetActive(true);

        eng_smallAlphaRow1.SetActive(true);
        eng_smallAlphaRow2.SetActive(true);
        eng_smallAlphaRow3.SetActive(true);

        eng_capitalAlphaRow1.SetActive(false);
        eng_capitalAlphaRow2.SetActive(false);
        eng_capitalAlphaRow3.SetActive(false);

        han_smallAlphaRow1.SetActive(false);
        han_smallAlphaRow2.SetActive(false);
        han_smallAlphaRow3.SetActive(false);

        han_capitalAlphaRow1.SetActive(false);
        han_capitalAlphaRow2.SetActive(false);
        han_capitalAlphaRow3.SetActive(false);
    }

}
