using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SwitchLocale : MonoBehaviour
{
    private bool active = false;

    IEnumerator SetLocale(int localeID)
    {
        yield return LocalizationSettings.InitializationOperation;

        LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[localeID];
    }

    public void BS_ChangeLocale(int locale)
    {
        if (active == true) return; // pour eviter erreurs au spam
        StartCoroutine(SetLocale(locale));
    }
}
