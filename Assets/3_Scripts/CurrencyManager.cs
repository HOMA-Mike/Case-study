using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

using Object = System.Object;

/// <summary>Manages the currencies the player has</summary>
public class CurrencyManager : MonoBehaviour
{
    static CurrencyManager instance;

    public enum Currency
    {
        Money,
        // Add new currencies here
    }

    [Header("References")]
    public List<CurrencyUI> UIs;

    List<CurrencyStack> currencies;

    void Awake()
    {
        instance = this;
        currencies = new List<CurrencyStack>();

        foreach (Object value in Enum.GetValues(typeof(Currency)))
            currencies.Add(new CurrencyStack() { currency = (Currency)value, amount = 0 });
    }

    public static int GetCurrency(Currency currency)
    {
        return instance.currencies.Find(item => item.currency == currency).amount;
    }

    public static void AddCurrency(Currency currency, int amount)
    {
        // I consider this safe since we initialized every possible value of Currency during Awake
        CurrencyStack selected = instance.currencies.Find(item => item.currency == currency);

        selected.amount += amount;
        AnimateUI(selected);
    }

    public static void TakeCurrency(Currency current, int amount)
    {
        CurrencyStack selected = instance.currencies.Find(item => item.currency == current);

        // This will prevent spending negative currency amounts
        selected.amount = Mathf.Clamp(selected.amount - amount, 0, selected.amount);
        AnimateUI(selected);
    }

    static void AnimateUI(CurrencyStack stack)
    {
        CurrencyUI selected = instance.UIs.Find(item => item.currency == stack.currency);

        if (selected == null)
        {
            Debug.LogError("Couldn't find UI for currency " + stack.currency);
            return;
        }

        if (selected.animation != null)
            instance.StopCoroutine(selected.animation);

        selected.animation = instance.StartCoroutine(AnimateIcon(selected, stack.amount, Vector3.one * 1.5f, 0.3f));
    }

    // didn't want to clog the project with DOTween
    static IEnumerator AnimateIcon(CurrencyUI ui, int newValue, Vector3 targetScale, float duration)
    {
        int oldValue = int.Parse(ui.amount.text);
        float timer = 0;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float percent = timer / duration;

            if (percent < 0.5f)
                ui.icon.transform.localScale = Vector3.Lerp(Vector3.one, targetScale, percent * 2);
            else
                ui.icon.transform.localEulerAngles = Vector3.Lerp(targetScale, Vector3.one, (percent - 0.5f) * 2);

            ui.amount.text = Mathf.FloorToInt(Mathf.Lerp(oldValue, newValue, timer / duration)).ToString();
            yield return null;
        }

        ui.animation = null;
    }

    /// <summary>Keeps track of a certain currency amount</summary>
    [Serializable] // for debugging purposes
    public class CurrencyStack
    {
        public Currency currency;
        public int amount;
    }

    /// <summary>Links a UI panel to a currency type</summary>
    [Serializable]
    public class CurrencyUI
    {
        public Currency currency;
        public TMP_Text amount;
        public Image icon;
        public Coroutine animation;
    }
}