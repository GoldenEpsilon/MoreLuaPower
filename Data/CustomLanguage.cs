using I2.Loc;

public class LuaPowerLang
{
    public static void ImportTerm(string termName, string translation, string language = "English") {
        var i2languagesPrefab = LocalizationManager.Sources[0];
        var termData = i2languagesPrefab.AddTerm(termName, eTermType.Text);

        // Find Language Index (or add the language if its a new one)
        int langIndex = i2languagesPrefab.GetLanguageIndex(language, false, false);
        if (langIndex < 0) {
            i2languagesPrefab.AddLanguage(language, GoogleLanguages.GetLanguageCode(language));
            langIndex = i2languagesPrefab.GetLanguageIndex(language, false, false);
        }

        termData.Languages[langIndex] = translation;
    }
}