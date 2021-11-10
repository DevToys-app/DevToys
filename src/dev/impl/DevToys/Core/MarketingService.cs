#nullable enable

using System;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using DevToys.Api.Core;
using DevToys.Core.Threading;
using DevToys.Models;
using Newtonsoft.Json;
using Windows.Services.Store;
using Windows.Storage;

namespace DevToys.Core
{
    [Export(typeof(IMarketingService))]
    [Shared]
    internal sealed class MarketingService : IMarketingService, IDisposable
    {
        private const string StoredFileName = "marketingInfo.json";

        private readonly INotificationService _notificationService;
        private readonly DisposableSempahore _semaphore = new();
        private readonly AsyncLazy<MarketingState> _marketingState;
        private readonly object _lock = new();

        private bool _rateOfferInProgress;

        [ImportingConstructor]
        public MarketingService(INotificationService notificationService)
        {
            _notificationService = notificationService;
            _marketingState = new AsyncLazy<MarketingState>(LoadStateAsync);
        }

        public void Dispose()
        {
            _semaphore.Dispose();
        }

        public async Task NotifyAppEncounteredAProblemAsync()
        {
            await UpdateMarketingStateAsync(state =>
            {
                state.LastProblemEncounteredDate = DateTime.Now;
                state.StartSinceLastProblemEncounteredCount = 0;
            });
        }

        public void NotifyAppJustUpdated()
        {
            UpdateMarketingStateAsync(state =>
            {
                state.LastUpdateDate = DateTime.Now;
            }).ForgetSafely();
        }

        public void NotifyAppStarted()
        {
            UpdateMarketingStateAsync(state =>
            {
                state.StartSinceLastProblemEncounteredCount++;
            }).ForgetSafely();
        }

        public void NotifySmartDetectionWorked()
        {
            UpdateMarketingStateAsync(state =>
            {
                state.SmartDetectionCount++;
            }).ContinueWith(_ =>
            {
                TryOfferUserToRateApp();
            }).ForgetSafely();
        }

        public void NotifyToolSuccessfullyWorked()
        {
            UpdateMarketingStateAsync(state =>
            {
                state.ToolSuccessfulyWorkedCount++;
            }).ContinueWith(_ =>
            {
                TryOfferUserToRateApp();
            }).ForgetSafely();
        }

        private void TryOfferUserToRateApp()
        {
            lock (_lock)
            {
                if (_rateOfferInProgress)
                {
                    return;
                }

                if (_marketingState.IsValueCreated
                    && DetermineWhetherAppRatingShouldBeOffered(_marketingState.GetValueAsync().Result))
                {
                    _rateOfferInProgress = true;

                    _notificationService.ShowInAppNotification(
                        LanguageManager.Instance.MainPage.NotificationRateAppTitle,
                        LanguageManager.Instance.MainPage.NotificationRateAppActionableActionText,
                        () =>
                        {
                            RateAsync().ForgetSafely();
                        },
                        LanguageManager.Instance.MainPage.NotificationRateAppMessage);
                }
            }
        }

        private async Task RateAsync()
        {
            await UpdateMarketingStateAsync(state =>
            {
                state.AppRatingOfferCount++;
                state.LastAppRatingOfferDate = DateTime.Now;
            });

            var storeContext = StoreContext.GetDefault();

            StoreRateAndReviewResult result = await ThreadHelper.RunOnUIThreadAsync(async () =>
            {
                return await storeContext.RequestRateAndReviewAppAsync();
            }).ConfigureAwait(false);

            if (result.Status == StoreRateAndReviewStatus.Succeeded)
            {
                await UpdateMarketingStateAsync(state =>
                {
                    state.AppGotRated = true;
                });
            }

            lock (_lock)
            {
                _rateOfferInProgress = false;
            }
        }

        private bool DetermineWhetherAppRatingShouldBeOffered(MarketingState state)
        {
            // The user already rated the app. Let's not offer him to rate it again.
            if (state.AppGotRated)
            {
                return false;
            }

            // We already offered the user to rate the app many times.
            // It's very unlikely that he will rate it at this point. Let's stop asking.
            if (state.AppRatingOfferCount >= 10)
            {
                return false;
            }

            // If it's been less than 8 days since the last time the app crashed or that the app
            // has been installed on the machine. Let's not ask the user to rate the app.
            if (DateTime.Now - state.LastProblemEncounteredDate < TimeSpan.FromDays(8))
            {
                return false;
            }

            // If the app have been started less than 4 times since the last crash or since the app
            // got installed on the machine, let's not ask the user to rate the app.
            if (state.StartSinceLastProblemEncounteredCount < 4)
            {
                return false;
            }

            // The app got updated 2 days ago. Potentially, we introduced some instability (not necessarily crash,
            // but maybe visual issues, inconsistencies...etc).
            // Let's make sure we don't offer the user to rate the app as soon as it got updated, just in case
            // if the app is completely broken.
            if (DateTime.Now - state.LastUpdateDate < TimeSpan.FromDays(2))
            {
                return false;
            }

            // Let's make sure we don't offer to rate the app more than once within 2 days.
            if (state.AppRatingOfferCount > 0 && DateTime.Now - state.LastAppRatingOfferDate < TimeSpan.FromDays(2))
            {
                return false;
            }

            // If we already offered to rate the app more than 2 times, let's make sure we
            // don't offer it again before the next 5 days.
            if (state.AppRatingOfferCount > 2 && DateTime.Now - state.LastAppRatingOfferDate < TimeSpan.FromDays(5))
            {
                return false;
            }

            // If we already offered to rate the app more than 5 times, let's make sure we
            // don't offer it again before the next 10 days.
            if (state.AppRatingOfferCount > 5 && DateTime.Now - state.LastAppRatingOfferDate < TimeSpan.FromDays(10))
            {
                return false;
            }

            // If we already offered to rate the app more than 7 times, let's make sure we
            // don't offer it again before the next 60 days.
            if (state.AppRatingOfferCount > 7 && DateTime.Now - state.LastAppRatingOfferDate < TimeSpan.FromDays(60))
            {
                return false;
            }

            // Smart Detection has been used at least twice. Let's offer the use to rate the app.
            if (state.SmartDetectionCount > 3)
            {
                return true;
            }

            // The user used tools at least 10 times already. Let's offer the use to rate the app.
            if (state.ToolSuccessfulyWorkedCount > 10)
            {
                return true;
            }

            return false;
        }

        private async Task UpdateMarketingStateAsync(Action<MarketingState> updateAction)
        {
            await TaskScheduler.Default;

            MarketingState state = await _marketingState.GetValueAsync();

            using (await _semaphore.WaitAsync(CancellationToken.None))
            {
                updateAction(state);
            }

            await SaveStateAsync(state);
        }

        private async Task SaveStateAsync(MarketingState state)
        {
            await TaskScheduler.Default;

            try
            {
                using (await _semaphore.WaitAsync(CancellationToken.None))
                {
                    StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;

                    StorageFile file = await localCacheFolder.CreateFileAsync(StoredFileName, CreationCollisionOption.ReplaceExisting);

                    string? fileContent
                        = JsonConvert.SerializeObject(
                            state,
                            Formatting.Indented);

                    await FileIO.WriteTextAsync(file, fileContent);
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task<MarketingState> LoadStateAsync()
        {
            await TaskScheduler.Default;

            try
            {
                using (await _semaphore.WaitAsync(CancellationToken.None))
                {
                    StorageFolder localCacheFolder = ApplicationData.Current.LocalCacheFolder;

                    IStorageItem? file = await localCacheFolder.TryGetItemAsync(StoredFileName);

                    if (file is not null and StorageFile storageFile)
                    {
                        string? fileContent = await FileIO.ReadTextAsync(storageFile);
                        MarketingState? result = JsonConvert.DeserializeObject<MarketingState>(fileContent);
                        if (result is not null)
                        {
                            return result;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }

            return new MarketingState
            {
                LastAppRatingOfferDate = DateTime.Now,
                LastProblemEncounteredDate = DateTime.Now,
                LastUpdateDate = DateTime.Now
            };
        }
    }
}
