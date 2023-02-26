using DevToys.Api;

namespace DevToys.UI.Models;

internal record NavigationParameters<T>(IMefProvider MefProvider, T Parameter);
