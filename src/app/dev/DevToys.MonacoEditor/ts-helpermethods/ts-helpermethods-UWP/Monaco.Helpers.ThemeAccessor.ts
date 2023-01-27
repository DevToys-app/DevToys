//namespace DevToys.MonacoEditor.Monaco.Helpers {
interface ThemeAccessor {
    accentColorHtmlHex: Promise<string>;
    currentThemeName: Promise<string>;
    isHighContrast: Promise<boolean>;
}
//}