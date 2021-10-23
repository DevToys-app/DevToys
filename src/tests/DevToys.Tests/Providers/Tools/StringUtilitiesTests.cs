using DevToys.Core.Threading;
using DevToys.ViewModels.Tools.StringUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace DevToys.Tests.Providers.Tools
{
    [TestClass]
    public class StringUtilitiesTests : MefBaseTest
    {
        private const string Text
= @"UWP is one of many ways to create client applications for Windows. UWP apps use WinRT APIs to provide powerful UI and advanced asynchronous features that are ideal for internet-connected devices.

To download the tools you need to start creating UWP apps, see Get set up, and then write your first app.

Where does UWP fit in the Microsoft development story?
UWP is one choice for creating apps that run on Windows 10 devices, and can be combined with other platforms. UWP apps can make use of Win32 APIs and .NET classes (see API Sets for UWP apps, Dlls for UWP apps, and .NET for UWP apps).

The Microsoft development story continues to evolve, and along with initiatives such as WinUI, MSIX, and Project Reunion, UWP is a powerful tool for creating client apps.

Features of a UWP app
A UWP app is:

Secure: UWP apps declare which device resources and data they access. The user must authorize that access.
Able to use a common API on all devices that run Windows 10.
Able to use device specific capabilities and adapt the UI to different device screen sizes, resolutions, and DPI.
Available from the Microsoft Store on all devices (or only those that you specify) that run on Windows 10. The Microsoft Store provides multiple ways to make money on your app.
Able to be installed and uninstalled without risk to the machine or incurring ""machine rot"".
Engaging: use live tiles, push notifications, and user activities that interact with Windows Timeline and Cortana's Pick Up Where I Left Off, to engage users.
Programmable in C#, C++, Visual Basic, and Javascript. For UI, use WinUI, XAML, HTML, or DirectX.
Let's look at these in more detail.";

        [TestMethod]
        public async Task CalculateSelectionStatisticsAsync()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;
            viewModel.SelectionStart = 0;

            await Task.Delay(100);

            Assert.AreEqual(1, viewModel.Line);
            Assert.AreEqual(0, viewModel.Column);

            viewModel.SelectionStart = 1;

            await Task.Delay(100);

            Assert.AreEqual(1, viewModel.Line);
            Assert.AreEqual(1, viewModel.Column);

            viewModel.SelectionStart = 2;

            await Task.Delay(100);

            Assert.AreEqual(1, viewModel.Line);
            Assert.AreEqual(2, viewModel.Column);

            viewModel.SelectionStart = 810;

            await Task.Delay(100);

            Assert.AreEqual(11, viewModel.Line);
            Assert.AreEqual(13, viewModel.Column);

            viewModel.SelectionStart = 812;

            await Task.Delay(100);

            Assert.AreEqual(12, viewModel.Line);
            Assert.AreEqual(0, viewModel.Column);
        }

        [TestMethod]
        public async Task CalculateTextStatisticsAsync()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            await Task.Delay(100);

            Assert.AreEqual(1666, viewModel.Characters);
            Assert.AreEqual(288, viewModel.Words);
            Assert.AreEqual(20, viewModel.Lines);
            Assert.AreEqual(22, viewModel.Sentences);
            Assert.AreEqual(6, viewModel.Paragraphs);
            Assert.AreEqual(1666, viewModel.Bytes);
        }

        [TestMethod]
        public void SentenceCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.SentenceCaseCommand.Execute(null);

            string expectedResult
= @"Uwp is one of many ways to create client applications for windows. Uwp apps use winrt apis to provide powerful ui and advanced asynchronous features that are ideal for internet-connected devices.

To download the tools you need to start creating uwp apps, see get set up, and then write your first app.

Where does uwp fit in the microsoft development story?
Uwp is one choice for creating apps that run on windows 10 devices, and can be combined with other platforms. Uwp apps can make use of win32 apis and .Net classes (see api sets for uwp apps, dlls for uwp apps, and .Net for uwp apps).

The microsoft development story continues to evolve, and along with initiatives such as winui, msix, and project reunion, uwp is a powerful tool for creating client apps.

Features of a uwp app
A uwp app is:

Secure: uwp apps declare which device resources and data they access. The user must authorize that access.
Able to use a common api on all devices that run windows 10.
Able to use device specific capabilities and adapt the ui to different device screen sizes, resolutions, and dpi.
Available from the microsoft store on all devices (or only those that you specify) that run on windows 10. The microsoft store provides multiple ways to make money on your app.
Able to be installed and uninstalled without risk to the machine or incurring ""machine rot"".
Engaging: use live tiles, push notifications, and user activities that interact with windows timeline and cortana's pick up where i left off, to engage users.
Programmable in c#, c++, visual basic, and javascript. For ui, use winui, xaml, html, or directx.
Let's look at these in more detail.";

            Assert.AreEqual(expectedResult, viewModel.Text);
        }

        [TestMethod]
        public void LowerCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.LowerCaseCommand.Execute(null);

            string expectedResult
= @"uwp is one of many ways to create client applications for windows. uwp apps use winrt apis to provide powerful ui and advanced asynchronous features that are ideal for internet-connected devices.

to download the tools you need to start creating uwp apps, see get set up, and then write your first app.

where does uwp fit in the microsoft development story?
uwp is one choice for creating apps that run on windows 10 devices, and can be combined with other platforms. uwp apps can make use of win32 apis and .net classes (see api sets for uwp apps, dlls for uwp apps, and .net for uwp apps).

the microsoft development story continues to evolve, and along with initiatives such as winui, msix, and project reunion, uwp is a powerful tool for creating client apps.

features of a uwp app
a uwp app is:

secure: uwp apps declare which device resources and data they access. the user must authorize that access.
able to use a common api on all devices that run windows 10.
able to use device specific capabilities and adapt the ui to different device screen sizes, resolutions, and dpi.
available from the microsoft store on all devices (or only those that you specify) that run on windows 10. the microsoft store provides multiple ways to make money on your app.
able to be installed and uninstalled without risk to the machine or incurring ""machine rot"".
engaging: use live tiles, push notifications, and user activities that interact with windows timeline and cortana's pick up where i left off, to engage users.
programmable in c#, c++, visual basic, and javascript. for ui, use winui, xaml, html, or directx.
let's look at these in more detail.";

            Assert.AreEqual(expectedResult, viewModel.Text);
        }

        [TestMethod]
        public void UpperCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.UpperCaseCommand.Execute(null);

            string expectedResult
= @"UWP IS ONE OF MANY WAYS TO CREATE CLIENT APPLICATIONS FOR WINDOWS. UWP APPS USE WINRT APIS TO PROVIDE POWERFUL UI AND ADVANCED ASYNCHRONOUS FEATURES THAT ARE IDEAL FOR INTERNET-CONNECTED DEVICES.

TO DOWNLOAD THE TOOLS YOU NEED TO START CREATING UWP APPS, SEE GET SET UP, AND THEN WRITE YOUR FIRST APP.

WHERE DOES UWP FIT IN THE MICROSOFT DEVELOPMENT STORY?
UWP IS ONE CHOICE FOR CREATING APPS THAT RUN ON WINDOWS 10 DEVICES, AND CAN BE COMBINED WITH OTHER PLATFORMS. UWP APPS CAN MAKE USE OF WIN32 APIS AND .NET CLASSES (SEE API SETS FOR UWP APPS, DLLS FOR UWP APPS, AND .NET FOR UWP APPS).

THE MICROSOFT DEVELOPMENT STORY CONTINUES TO EVOLVE, AND ALONG WITH INITIATIVES SUCH AS WINUI, MSIX, AND PROJECT REUNION, UWP IS A POWERFUL TOOL FOR CREATING CLIENT APPS.

FEATURES OF A UWP APP
A UWP APP IS:

SECURE: UWP APPS DECLARE WHICH DEVICE RESOURCES AND DATA THEY ACCESS. THE USER MUST AUTHORIZE THAT ACCESS.
ABLE TO USE A COMMON API ON ALL DEVICES THAT RUN WINDOWS 10.
ABLE TO USE DEVICE SPECIFIC CAPABILITIES AND ADAPT THE UI TO DIFFERENT DEVICE SCREEN SIZES, RESOLUTIONS, AND DPI.
AVAILABLE FROM THE MICROSOFT STORE ON ALL DEVICES (OR ONLY THOSE THAT YOU SPECIFY) THAT RUN ON WINDOWS 10. THE MICROSOFT STORE PROVIDES MULTIPLE WAYS TO MAKE MONEY ON YOUR APP.
ABLE TO BE INSTALLED AND UNINSTALLED WITHOUT RISK TO THE MACHINE OR INCURRING ""MACHINE ROT"".
ENGAGING: USE LIVE TILES, PUSH NOTIFICATIONS, AND USER ACTIVITIES THAT INTERACT WITH WINDOWS TIMELINE AND CORTANA'S PICK UP WHERE I LEFT OFF, TO ENGAGE USERS.
PROGRAMMABLE IN C#, C++, VISUAL BASIC, AND JAVASCRIPT. FOR UI, USE WINUI, XAML, HTML, OR DIRECTX.
LET'S LOOK AT THESE IN MORE DETAIL.";

            Assert.AreEqual(expectedResult, viewModel.Text);
        }

        [TestMethod]
        public void TitleCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.TitleCaseCommand.Execute(null);

            string expectedResult
= @"Uwp Is One Of Many Ways To Create Client Applications For Windows. Uwp Apps Use Winrt Apis To Provide Powerful Ui And Advanced Asynchronous Features That Are Ideal For Internet-Connected Devices.

To Download The Tools You Need To Start Creating Uwp Apps, See Get Set Up, And Then Write Your First App.

Where Does Uwp Fit In The Microsoft Development Story?
Uwp Is One Choice For Creating Apps That Run On Windows 10 Devices, And Can Be Combined With Other Platforms. Uwp Apps Can Make Use Of Win32 Apis And .Net Classes (See Api Sets For Uwp Apps, Dlls For Uwp Apps, And .Net For Uwp Apps).

The Microsoft Development Story Continues To Evolve, And Along With Initiatives Such As Winui, Msix, And Project Reunion, Uwp Is A Powerful Tool For Creating Client Apps.

Features Of A Uwp App
A Uwp App Is:

Secure: Uwp Apps Declare Which Device Resources And Data They Access. The User Must Authorize That Access.
Able To Use A Common Api On All Devices That Run Windows 10.
Able To Use Device Specific Capabilities And Adapt The Ui To Different Device Screen Sizes, Resolutions, And Dpi.
Available From The Microsoft Store On All Devices (Or Only Those That You Specify) That Run On Windows 10. The Microsoft Store Provides Multiple Ways To Make Money On Your App.
Able To Be Installed And Uninstalled Without Risk To The Machine Or Incurring ""Machine Rot"".
Engaging: Use Live Tiles, Push Notifications, And User Activities That Interact With Windows Timeline And Cortana'S Pick Up Where I Left Off, To Engage Users.
Programmable In C#, C++, Visual Basic, And Javascript. For Ui, Use Winui, Xaml, Html, Or Directx.
Let'S Look At These In More Detail.";

            Assert.AreEqual(expectedResult, viewModel.Text);
        }

        [TestMethod]
        public void CamelCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.CamelCaseCommand.Execute(null);

            string expectedResult
= @"uwpIsOneOfManyWaysToCreateClientApplicationsForWindowsUwpAppsUseWinrtApisToProvidePowerfulUiAndAdvancedAsynchronousFeaturesThatAreIdealForInternetConnectedDevices

ToDownloadTheToolsYouNeedToStartCreatingUwpAppsSeeGetSetUpAndThenWriteYourFirstApp

WhereDoesUwpFitInTheMicrosoftDevelopmentStory
UwpIsOneChoiceForCreatingAppsThatRunOnWindows10DevicesAndCanBeCombinedWithOtherPlatformsUwpAppsCanMakeUseOfWin32ApisAndNetClassesSeeApiSetsForUwpAppsDllsForUwpAppsAndNetForUwpApps

TheMicrosoftDevelopmentStoryContinuesToEvolveAndAlongWithInitiativesSuchAsWinuiMsixAndProjectReunionUwpIsAPowerfulToolForCreatingClientApps

FeaturesOfAUwpApp
AUwpAppIs

SecureUwpAppsDeclareWhichDeviceResourcesAndDataTheyAccessTheUserMustAuthorizeThatAccess
AbleToUseACommonApiOnAllDevicesThatRunWindows10
AbleToUseDeviceSpecificCapabilitiesAndAdaptTheUiToDifferentDeviceScreenSizesResolutionsAndDpi
AvailableFromTheMicrosoftStoreOnAllDevicesOrOnlyThoseThatYouSpecifyThatRunOnWindows10TheMicrosoftStoreProvidesMultipleWaysToMakeMoneyOnYourApp
AbleToBeInstalledAndUninstalledWithoutRiskToTheMachineOrIncurringMachineRot
EngagingUseLiveTilesPushNotificationsAndUserActivitiesThatInteractWithWindowsTimelineAndCortanaSPickUpWhereILeftOffToEngageUsers
ProgrammableInCCVisualBasicAndJavascriptForUiUseWinuiXamlHtmlOrDirectx
LetSLookAtTheseInMoreDetail";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void PascalCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.PascalCaseCommand.Execute(null);

            string expectedResult
= @"UwpIsOneOfManyWaysToCreateClientApplicationsForWindowsUwpAppsUseWinrtApisToProvidePowerfulUiAndAdvancedAsynchronousFeaturesThatAreIdealForInternetConnectedDevices

ToDownloadTheToolsYouNeedToStartCreatingUwpAppsSeeGetSetUpAndThenWriteYourFirstApp

WhereDoesUwpFitInTheMicrosoftDevelopmentStory
UwpIsOneChoiceForCreatingAppsThatRunOnWindows10DevicesAndCanBeCombinedWithOtherPlatformsUwpAppsCanMakeUseOfWin32ApisAndNetClassesSeeApiSetsForUwpAppsDllsForUwpAppsAndNetForUwpApps

TheMicrosoftDevelopmentStoryContinuesToEvolveAndAlongWithInitiativesSuchAsWinuiMsixAndProjectReunionUwpIsAPowerfulToolForCreatingClientApps

FeaturesOfAUwpApp
AUwpAppIs

SecureUwpAppsDeclareWhichDeviceResourcesAndDataTheyAccessTheUserMustAuthorizeThatAccess
AbleToUseACommonApiOnAllDevicesThatRunWindows10
AbleToUseDeviceSpecificCapabilitiesAndAdaptTheUiToDifferentDeviceScreenSizesResolutionsAndDpi
AvailableFromTheMicrosoftStoreOnAllDevicesOrOnlyThoseThatYouSpecifyThatRunOnWindows10TheMicrosoftStoreProvidesMultipleWaysToMakeMoneyOnYourApp
AbleToBeInstalledAndUninstalledWithoutRiskToTheMachineOrIncurringMachineRot
EngagingUseLiveTilesPushNotificationsAndUserActivitiesThatInteractWithWindowsTimelineAndCortanaSPickUpWhereILeftOffToEngageUsers
ProgrammableInCCVisualBasicAndJavascriptForUiUseWinuiXamlHtmlOrDirectx
LetSLookAtTheseInMoreDetail";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void SnakeCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.SnakeCaseCommand.Execute(null);

            string expectedResult
= @"uwp_is_one_of_many_ways_to_create_client_applications_for_windows_uwp_apps_use_winrt_apis_to_provide_powerful_ui_and_advanced_asynchronous_features_that_are_ideal_for_internet_connected_devices

to_download_the_tools_you_need_to_start_creating_uwp_apps_see_get_set_up_and_then_write_your_first_app

where_does_uwp_fit_in_the_microsoft_development_story
uwp_is_one_choice_for_creating_apps_that_run_on_windows_10_devices_and_can_be_combined_with_other_platforms_uwp_apps_can_make_use_of_win32_apis_and_net_classes_see_api_sets_for_uwp_apps_dlls_for_uwp_apps_and_net_for_uwp_apps

the_microsoft_development_story_continues_to_evolve_and_along_with_initiatives_such_as_winui_msix_and_project_reunion_uwp_is_a_powerful_tool_for_creating_client_apps

features_of_a_uwp_app
a_uwp_app_is

secure_uwp_apps_declare_which_device_resources_and_data_they_access_the_user_must_authorize_that_access
able_to_use_a_common_api_on_all_devices_that_run_windows_10
able_to_use_device_specific_capabilities_and_adapt_the_ui_to_different_device_screen_sizes_resolutions_and_dpi
available_from_the_microsoft_store_on_all_devices_or_only_those_that_you_specify_that_run_on_windows_10_the_microsoft_store_provides_multiple_ways_to_make_money_on_your_app
able_to_be_installed_and_uninstalled_without_risk_to_the_machine_or_incurring_machine_rot
engaging_use_live_tiles_push_notifications_and_user_activities_that_interact_with_windows_timeline_and_cortana_s_pick_up_where_i_left_off_to_engage_users
programmable_in_c_c_visual_basic_and_javascript_for_ui_use_winui_xaml_html_or_directx
let_s_look_at_these_in_more_detail";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void ConstantCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.ConstantCaseCommand.Execute(null);

            string expectedResult
= @"UWP_IS_ONE_OF_MANY_WAYS_TO_CREATE_CLIENT_APPLICATIONS_FOR_WINDOWS_UWP_APPS_USE_WINRT_APIS_TO_PROVIDE_POWERFUL_UI_AND_ADVANCED_ASYNCHRONOUS_FEATURES_THAT_ARE_IDEAL_FOR_INTERNET_CONNECTED_DEVICES

TO_DOWNLOAD_THE_TOOLS_YOU_NEED_TO_START_CREATING_UWP_APPS_SEE_GET_SET_UP_AND_THEN_WRITE_YOUR_FIRST_APP

WHERE_DOES_UWP_FIT_IN_THE_MICROSOFT_DEVELOPMENT_STORY
UWP_IS_ONE_CHOICE_FOR_CREATING_APPS_THAT_RUN_ON_WINDOWS_10_DEVICES_AND_CAN_BE_COMBINED_WITH_OTHER_PLATFORMS_UWP_APPS_CAN_MAKE_USE_OF_WIN32_APIS_AND_NET_CLASSES_SEE_API_SETS_FOR_UWP_APPS_DLLS_FOR_UWP_APPS_AND_NET_FOR_UWP_APPS

THE_MICROSOFT_DEVELOPMENT_STORY_CONTINUES_TO_EVOLVE_AND_ALONG_WITH_INITIATIVES_SUCH_AS_WINUI_MSIX_AND_PROJECT_REUNION_UWP_IS_A_POWERFUL_TOOL_FOR_CREATING_CLIENT_APPS

FEATURES_OF_A_UWP_APP
A_UWP_APP_IS

SECURE_UWP_APPS_DECLARE_WHICH_DEVICE_RESOURCES_AND_DATA_THEY_ACCESS_THE_USER_MUST_AUTHORIZE_THAT_ACCESS
ABLE_TO_USE_A_COMMON_API_ON_ALL_DEVICES_THAT_RUN_WINDOWS_10
ABLE_TO_USE_DEVICE_SPECIFIC_CAPABILITIES_AND_ADAPT_THE_UI_TO_DIFFERENT_DEVICE_SCREEN_SIZES_RESOLUTIONS_AND_DPI
AVAILABLE_FROM_THE_MICROSOFT_STORE_ON_ALL_DEVICES_OR_ONLY_THOSE_THAT_YOU_SPECIFY_THAT_RUN_ON_WINDOWS_10_THE_MICROSOFT_STORE_PROVIDES_MULTIPLE_WAYS_TO_MAKE_MONEY_ON_YOUR_APP
ABLE_TO_BE_INSTALLED_AND_UNINSTALLED_WITHOUT_RISK_TO_THE_MACHINE_OR_INCURRING_MACHINE_ROT
ENGAGING_USE_LIVE_TILES_PUSH_NOTIFICATIONS_AND_USER_ACTIVITIES_THAT_INTERACT_WITH_WINDOWS_TIMELINE_AND_CORTANA_S_PICK_UP_WHERE_I_LEFT_OFF_TO_ENGAGE_USERS
PROGRAMMABLE_IN_C_C_VISUAL_BASIC_AND_JAVASCRIPT_FOR_UI_USE_WINUI_XAML_HTML_OR_DIRECTX
LET_S_LOOK_AT_THESE_IN_MORE_DETAIL";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void KebabCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.KebabCaseCommand.Execute(null);

            string expectedResult
= @"uwp-is-one-of-many-ways-to-create-client-applications-for-windows-uwp-apps-use-winrt-apis-to-provide-powerful-ui-and-advanced-asynchronous-features-that-are-ideal-for-internet-connected-devices

to-download-the-tools-you-need-to-start-creating-uwp-apps-see-get-set-up-and-then-write-your-first-app

where-does-uwp-fit-in-the-microsoft-development-story
uwp-is-one-choice-for-creating-apps-that-run-on-windows-10-devices-and-can-be-combined-with-other-platforms-uwp-apps-can-make-use-of-win32-apis-and-net-classes-see-api-sets-for-uwp-apps-dlls-for-uwp-apps-and-net-for-uwp-apps

the-microsoft-development-story-continues-to-evolve-and-along-with-initiatives-such-as-winui-msix-and-project-reunion-uwp-is-a-powerful-tool-for-creating-client-apps

features-of-a-uwp-app
a-uwp-app-is

secure-uwp-apps-declare-which-device-resources-and-data-they-access-the-user-must-authorize-that-access
able-to-use-a-common-api-on-all-devices-that-run-windows-10
able-to-use-device-specific-capabilities-and-adapt-the-ui-to-different-device-screen-sizes-resolutions-and-dpi
available-from-the-microsoft-store-on-all-devices-or-only-those-that-you-specify-that-run-on-windows-10-the-microsoft-store-provides-multiple-ways-to-make-money-on-your-app
able-to-be-installed-and-uninstalled-without-risk-to-the-machine-or-incurring-machine-rot
engaging-use-live-tiles-push-notifications-and-user-activities-that-interact-with-windows-timeline-and-cortana-s-pick-up-where-i-left-off-to-engage-users
programmable-in-c-c-visual-basic-and-javascript-for-ui-use-winui-xaml-html-or-directx
let-s-look-at-these-in-more-detail";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void CobolCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.CobolCaseCommand.Execute(null);

            string expectedResult
= @"UWP-IS-ONE-OF-MANY-WAYS-TO-CREATE-CLIENT-APPLICATIONS-FOR-WINDOWS-UWP-APPS-USE-WINRT-APIS-TO-PROVIDE-POWERFUL-UI-AND-ADVANCED-ASYNCHRONOUS-FEATURES-THAT-ARE-IDEAL-FOR-INTERNET-CONNECTED-DEVICES

TO-DOWNLOAD-THE-TOOLS-YOU-NEED-TO-START-CREATING-UWP-APPS-SEE-GET-SET-UP-AND-THEN-WRITE-YOUR-FIRST-APP

WHERE-DOES-UWP-FIT-IN-THE-MICROSOFT-DEVELOPMENT-STORY
UWP-IS-ONE-CHOICE-FOR-CREATING-APPS-THAT-RUN-ON-WINDOWS-10-DEVICES-AND-CAN-BE-COMBINED-WITH-OTHER-PLATFORMS-UWP-APPS-CAN-MAKE-USE-OF-WIN32-APIS-AND-NET-CLASSES-SEE-API-SETS-FOR-UWP-APPS-DLLS-FOR-UWP-APPS-AND-NET-FOR-UWP-APPS

THE-MICROSOFT-DEVELOPMENT-STORY-CONTINUES-TO-EVOLVE-AND-ALONG-WITH-INITIATIVES-SUCH-AS-WINUI-MSIX-AND-PROJECT-REUNION-UWP-IS-A-POWERFUL-TOOL-FOR-CREATING-CLIENT-APPS

FEATURES-OF-A-UWP-APP
A-UWP-APP-IS

SECURE-UWP-APPS-DECLARE-WHICH-DEVICE-RESOURCES-AND-DATA-THEY-ACCESS-THE-USER-MUST-AUTHORIZE-THAT-ACCESS
ABLE-TO-USE-A-COMMON-API-ON-ALL-DEVICES-THAT-RUN-WINDOWS-10
ABLE-TO-USE-DEVICE-SPECIFIC-CAPABILITIES-AND-ADAPT-THE-UI-TO-DIFFERENT-DEVICE-SCREEN-SIZES-RESOLUTIONS-AND-DPI
AVAILABLE-FROM-THE-MICROSOFT-STORE-ON-ALL-DEVICES-OR-ONLY-THOSE-THAT-YOU-SPECIFY-THAT-RUN-ON-WINDOWS-10-THE-MICROSOFT-STORE-PROVIDES-MULTIPLE-WAYS-TO-MAKE-MONEY-ON-YOUR-APP
ABLE-TO-BE-INSTALLED-AND-UNINSTALLED-WITHOUT-RISK-TO-THE-MACHINE-OR-INCURRING-MACHINE-ROT
ENGAGING-USE-LIVE-TILES-PUSH-NOTIFICATIONS-AND-USER-ACTIVITIES-THAT-INTERACT-WITH-WINDOWS-TIMELINE-AND-CORTANA-S-PICK-UP-WHERE-I-LEFT-OFF-TO-ENGAGE-USERS
PROGRAMMABLE-IN-C-C-VISUAL-BASIC-AND-JAVASCRIPT-FOR-UI-USE-WINUI-XAML-HTML-OR-DIRECTX
LET-S-LOOK-AT-THESE-IN-MORE-DETAIL";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void TrainCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.TrainCaseCommand.Execute(null);

            string expectedResult
= @"Uwp-Is-One-Of-Many-Ways-To-Create-Client-Applications-For-Windows-Uwp-Apps-Use-Winrt-Apis-To-Provide-Powerful-Ui-And-Advanced-Asynchronous-Features-That-Are-Ideal-For-Internet-Connected-Devices

To-Download-The-Tools-You-Need-To-Start-Creating-Uwp-Apps-See-Get-Set-Up-And-Then-Write-Your-First-App

Where-Does-Uwp-Fit-In-The-Microsoft-Development-Story
Uwp-Is-One-Choice-For-Creating-Apps-That-Run-On-Windows-10-Devices-And-Can-Be-Combined-With-Other-Platforms-Uwp-Apps-Can-Make-Use-Of-Win32-Apis-And-Net-Classes-See-Api-Sets-For-Uwp-Apps-Dlls-For-Uwp-Apps-And-Net-For-Uwp-Apps

The-Microsoft-Development-Story-Continues-To-Evolve-And-Along-With-Initiatives-Such-As-Winui-Msix-And-Project-Reunion-Uwp-Is-A-Powerful-Tool-For-Creating-Client-Apps

Features-Of-A-Uwp-App
A-Uwp-App-Is

Secure-Uwp-Apps-Declare-Which-Device-Resources-And-Data-They-Access-The-User-Must-Authorize-That-Access
Able-To-Use-A-Common-Api-On-All-Devices-That-Run-Windows-10
Able-To-Use-Device-Specific-Capabilities-And-Adapt-The-Ui-To-Different-Device-Screen-Sizes-Resolutions-And-Dpi
Available-From-The-Microsoft-Store-On-All-Devices-Or-Only-Those-That-You-Specify-That-Run-On-Windows-10-The-Microsoft-Store-Provides-Multiple-Ways-To-Make-Money-On-Your-App
Able-To-Be-Installed-And-Uninstalled-Without-Risk-To-The-Machine-Or-Incurring-Machine-Rot
Engaging-Use-Live-Tiles-Push-Notifications-And-User-Activities-That-Interact-With-Windows-Timeline-And-Cortana-S-Pick-Up-Where-I-Left-Off-To-Engage-Users
Programmable-In-C-C-Visual-Basic-And-Javascript-For-Ui-Use-Winui-Xaml-Html-Or-Directx
Let-S-Look-At-These-In-More-Detail";

            Assert.AreEqual(expectedResult.Replace("\r\n", "\r"), viewModel.Text);
        }

        [TestMethod]
        public void AlterningCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.AlternatingCaseCommand.Execute(null);

            string expectedResult
= @"uWp iS OnE Of mAnY WaYs tO CrEaTe cLiEnT ApPlIcAtIoNs fOr wInDoWs. UwP ApPs uSe wInRt aPiS To pRoViDe pOwErFuL Ui aNd aDvAnCeD AsYnChRoNoUs fEaTuReS ThAt aRe iDeAl fOr iNtErNeT-CoNnEcTeD DeViCeS.

To dOwNlOaD ThE ToOlS YoU NeEd tO StArT CrEaTiNg uWp aPpS, sEe gEt sEt uP, aNd tHeN WrItE YoUr fIrSt aPp.

wHeRe dOeS UwP FiT In tHe mIcRoSoFt dEvElOpMeNt sToRy?
uWp iS OnE ChOiCe fOr cReAtInG ApPs tHaT RuN On wInDoWs 10 DeViCeS, aNd cAn bE CoMbInEd wItH OtHeR PlAtFoRmS. uWp aPpS CaN MaKe uSe oF WiN32 ApIs aNd .NeT ClAsSeS (sEe aPi sEtS FoR UwP ApPs, DlLs fOr uWp aPpS, aNd .NeT FoR UwP ApPs).

ThE MiCrOsOfT DeVeLoPmEnT StOrY CoNtInUeS To eVoLvE, aNd aLoNg wItH InItIaTiVeS SuCh aS WiNuI, mSiX, aNd pRoJeCt rEuNiOn, UwP Is a pOwErFuL ToOl fOr cReAtInG ClIeNt aPpS.

FeAtUrEs oF A UwP ApP
a uWp aPp iS:

SeCuRe: UwP ApPs dEcLaRe wHiCh dEvIcE ReSoUrCeS AnD DaTa tHeY AcCeSs. ThE UsEr mUsT AuThOrIzE ThAt aCcEsS.
AbLe tO UsE A CoMmOn aPi oN AlL DeViCeS ThAt rUn wInDoWs 10.
AbLe tO UsE DeViCe sPeCiFiC CaPaBiLiTiEs aNd aDaPt tHe uI To dIfFeReNt dEvIcE ScReEn sIzEs, ReSoLuTiOnS, aNd dPi.
aVaIlAbLe fRoM ThE MiCrOsOfT StOrE On aLl dEvIcEs (Or oNlY ThOsE ThAt yOu sPeCiFy) ThAt rUn oN WiNdOwS 10. ThE MiCrOsOfT StOrE PrOvIdEs mUlTiPlE WaYs tO MaKe mOnEy oN YoUr aPp.
aBlE To bE InStAlLeD AnD UnInStAlLeD WiThOuT RiSk tO ThE MaChInE Or iNcUrRiNg ""MaChInE RoT"".
eNgAgInG: uSe lIvE TiLeS, pUsH NoTiFiCaTiOnS, aNd uSeR AcTiViTiEs tHaT InTeRaCt wItH WiNdOwS TiMeLiNe aNd cOrTaNa's pIcK Up wHeRe i lEfT OfF, tO EnGaGe uSeRs.
pRoGrAmMaBlE In c#, c++, ViSuAl bAsIc, AnD JaVaScRiPt. FoR Ui, UsE WiNuI, xAmL, hTmL, oR DiReCtX.
LeT'S LoOk aT ThEsE In mOrE DeTaIl.";

            Assert.AreEqual(expectedResult, viewModel.Text);
        }

        [TestMethod]
        public void InverseCase()
        {
            var viewModel = ExportProvider.Import<StringUtilitiesToolViewModel>();

            viewModel.Text = Text;

            viewModel.InverseCaseCommand.Execute(null);

            string expectedResult
= @"UwP Is oNe oF MaNy wAyS To cReAtE ClIeNt aPpLiCaTiOnS FoR WiNdOwS. uWp aPpS UsE WiNrT ApIs tO PrOvIdE PoWeRfUl uI AnD AdVaNcEd aSyNcHrOnOuS FeAtUrEs tHaT ArE IdEaL FoR InTeRnEt-cOnNeCtEd dEvIcEs.

tO DoWnLoAd tHe tOoLs yOu nEeD To sTaRt cReAtInG UwP ApPs, SeE GeT SeT Up, AnD ThEn wRiTe yOuR FiRsT ApP.

WhErE DoEs uWp fIt iN ThE MiCrOsOfT DeVeLoPmEnT StOrY?
UwP Is oNe cHoIcE FoR CrEaTiNg aPpS ThAt rUn oN WiNdOwS 10 dEvIcEs, AnD CaN Be cOmBiNeD WiTh oThEr pLaTfOrMs. UwP ApPs cAn mAkE UsE Of wIn32 aPiS AnD .nEt cLaSsEs (SeE ApI SeTs fOr uWp aPpS, dLlS FoR UwP ApPs, AnD .nEt fOr uWp aPpS).

tHe mIcRoSoFt dEvElOpMeNt sToRy cOnTiNuEs tO EvOlVe, AnD AlOnG WiTh iNiTiAtIvEs sUcH As wInUi, MsIx, AnD PrOjEcT ReUnIoN, uWp iS A PoWeRfUl tOoL FoR CrEaTiNg cLiEnT ApPs.

fEaTuReS Of a uWp aPp
A UwP ApP Is:

sEcUrE: uWp aPpS DeClArE WhIcH DeViCe rEsOuRcEs aNd dAtA ThEy aCcEsS. tHe uSeR MuSt aUtHoRiZe tHaT AcCeSs.
aBlE To uSe a cOmMoN ApI On aLl dEvIcEs tHaT RuN WiNdOwS 10.
aBlE To uSe dEvIcE SpEcIfIc cApAbIlItIeS AnD AdApT ThE Ui tO DiFfErEnT DeViCe sCrEeN SiZeS, rEsOlUtIoNs, AnD DpI.
AvAiLaBlE FrOm tHe mIcRoSoFt sToRe oN AlL DeViCeS (oR OnLy tHoSe tHaT YoU SpEcIfY) tHaT RuN On wInDoWs 10. tHe mIcRoSoFt sToRe pRoViDeS MuLtIpLe wAyS To mAkE MoNeY On yOuR ApP.
AbLe tO Be iNsTaLlEd aNd uNiNsTaLlEd wItHoUt rIsK To tHe mAcHiNe oR InCuRrInG ""mAcHiNe rOt"".
EnGaGiNg: UsE LiVe tIlEs, PuSh nOtIfIcAtIoNs, AnD UsEr aCtIvItIeS ThAt iNtErAcT WiTh wInDoWs tImElInE AnD CoRtAnA'S PiCk uP WhErE I LeFt oFf, To eNgAgE UsErS.
PrOgRaMmAbLe iN C#, C++, vIsUaL BaSiC, aNd jAvAsCrIpT. fOr uI, uSe wInUi, XaMl, HtMl, Or dIrEcTx.
lEt's lOoK At tHeSe iN MoRe dEtAiL.";

            Assert.AreEqual(expectedResult, viewModel.Text);
        }
    }
}
