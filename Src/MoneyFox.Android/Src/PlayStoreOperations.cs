namespace MoneyFox.Droid
{

    using Android.App;
    using Android.Content;
    using Android.Net;
    using Core.Common.Interfaces;

    /// <summary>
    ///     Gives access to the features of google play on android.
    /// </summary>
    public class PlayStoreOperations : IStoreOperations
    {
        private const string MARKET_URI = "market://details?id=";

        /// <summary>
        ///     Open the app page in the play store to rate the app. If the store page couldn't be opened,     the browser
        ///     will be opened.
        /// </summary>
        public void RateApp()
        {
            var appPackageName = Application.Context.PackageName ?? "";
            try
            {
                var intent = new Intent(action: Intent.ActionView, uri: Uri.Parse($"{MARKET_URI}{appPackageName}"));

                // we need to add this, because the activity is in a new context.
                // Otherwise the runtime will block the execution and throw an exception
                intent.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
            catch (ActivityNotFoundException)
            {
                var intent = new Intent(action: Intent.ActionView, uri: Uri.Parse($"http://play.google.com/store/apps/details?id={appPackageName}"));

                // we need to add this, because the activity is in a new context.
                // Otherwise the runtime will block the execution and throw an exception
                intent.AddFlags(ActivityFlags.NewTask);
                Application.Context.StartActivity(intent);
            }
        }
    }

}
