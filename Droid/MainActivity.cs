using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Xamarin.Forms;
using QuieroRoomieYa.Messages;
using Android.Support.V4.App;
using TaskStackBuilder = Android.Support.V4.App.TaskStackBuilder;

namespace QuieroRoomieYa.Android
{
	[Activity (Label = "QuieroRoomieYa.Droid", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsApplicationActivity
	{
		private static readonly int NewAdsNotificationId = 1000;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);

			global::Xamarin.Forms.Forms.Init (this, bundle);

			LoadApplication (new App ());

			WireUpGetRecentAdsService ();
			WireUpRoomOffersNotifications ();
		}

		void WireUpGetRecentAdsService() {
			MessagingCenter.Subscribe<StartRecentAdsMessage> (this, "StartRecentAdsMessage", message =>  {
				var intent = new Intent(this, typeof(RecentAdsService));
				Console.WriteLine("StartService");
				StartService(intent);
			});

			MessagingCenter.Subscribe<StopRecentAdsMessage> (this, "StopRecentAdsMessage", message =>  {
				var intent = new Intent(this, typeof(RecentAdsService));
				Console.WriteLine("StopService");
				StopService(intent);
			});
		}

		void WireUpRoomOffersNotifications() {
			MessagingCenter.Subscribe<NotificationMessage> (this, "NotificationMessage", message => {
				RoomOffersNotification(message);
			});
		}

		private void RoomOffersNotification(NotificationMessage notifContent) {
//			// Pass the current button press count value to the next activity:
//			Bundle valuesForActivity = new Bundle();
//			valuesForActivity.PutInt ("count", count);
//
//			// When the user clicks the notification, SecondActivity will start up.
			Intent intent = new Intent(this, typeof (MainActivity));

			const int pendingIntentId = 0;
			PendingIntent pendingIntent = 
				PendingIntent.GetActivity (this, pendingIntentId, intent, PendingIntentFlags.OneShot);
			

			NotificationCompat.Builder builder = new NotificationCompat.Builder (this)
				.SetAutoCancel (true)                         // Dismiss from the notif. area when clicked
				.SetContentIntent (pendingIntent)       // Start 2nd activity when the intent is clicked.
				.SetContentTitle (notifContent.Title)         // Set its title
//				.SetNumber (count)                            // Display the count in the Content Info
				.SetSmallIcon(Resource.Drawable.new_ad_icon)  // Display this icon
				.SetContentText (notifContent.Message);       // The message to display.

			builder.SetDefaults ((int)NotificationDefaults.Sound | (int)NotificationDefaults.Vibrate);
			builder.SetPriority ((int)NotificationPriority.High);

			if ((int) Build.VERSION.SdkInt >= 21) {
				builder.SetVisibility ((int)NotificationVisibility.Public);
				builder.SetCategory (Notification.CategoryService);
			}

			NotificationManager notificationManager = 
				(NotificationManager)GetSystemService(Context.NotificationService);
			notificationManager.Notify(NewAdsNotificationId, builder.Build());
		}
	}
}

