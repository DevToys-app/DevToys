using DevToys.Api;

namespace DevToys.Business.Models;

internal record NavigationParameters<T>(IMefProvider MefProvider, T Parameter);
