using System;

using System.Collections.Generic;
using QuieroRoomieYa.Messages;
using Xamarin.Forms;

namespace QuieroRoomieYa
{
	public class App : Application
	{
		static ListView lstRooms = new ListView();
		static Label loadingInfo = new Label();
		static StackLayout mainLayout = new StackLayout();
		static Button searchRoomsButton = new Button ();
		static int recentAdsCounter = 0;

		public App ()
		{
			searchRoomsButton.Text = "Buscar habitaciones";
			searchRoomsButton.Clicked += SearchRoomsButton_Clicked;
			lstRooms.ItemTemplate = new DataTemplate (typeof(ImageCell));
			lstRooms.ItemTemplate.SetBinding (ImageCell.ImageSourceProperty, "photo");
			lstRooms.ItemTemplate.SetBinding (ImageCell.TextProperty, "location");
			lstRooms.ItemTemplate.SetBinding (ImageCell.DetailProperty, "cost");
			loadingInfo.Text = "Cargando habitaciones\n :cross_fingers: ...";
			loadingInfo.XAlign = TextAlignment.Center;

			mainLayout = new StackLayout () {
				Padding = new Thickness(10, Device.OnPlatform(20,0,0), 10, 0),
				Children = {
					searchRoomsButton,
				}
			};

			MainPage = new ContentPage {
				Content = mainLayout
			};

			UpdateRecentAdsList ();
		}

		static async void SearchRoomsButton_Clicked (object sender, EventArgs e)
		{
			mainLayout.Children.Clear ();
			mainLayout.Children.Add (searchRoomsButton);
			mainLayout.Children.Add (loadingInfo);
			RecentAdsWebService adsWebService = new RecentAdsWebService ();
			RoomOfferAd[] latestRooms = await adsWebService.GetRecentAdsAsync ();
			recentAdsCounter = latestRooms.Length;
			lstRooms.ItemsSource = latestRooms;
			mainLayout.Children.Remove (loadingInfo);
			mainLayout.Children.Add (lstRooms);
		}

		void UpdateRecentAdsList() {
			MessagingCenter.Subscribe<RoomOfferAd[]> (this, "RoomOffersAds", message => {
				Device.BeginInvokeOnMainThread(() => {
					if ( message.Length > recentAdsCounter ) {
						Console.WriteLine("Rooomieee!");
						recentAdsCounter = message.Length;
						var newNotification = new NotificationMessage() {
							Title = "QuieroRoomieYa!",
							Message = string.Format("Llegaron {0} nuevos anuncios de roomies", (message.Length - recentAdsCounter))
						};
						MessagingCenter.Send<NotificationMessage>(newNotification, "NotificationMessage");
					} else {
						Console.WriteLine("No roomie :(");
					}
//					SearchRoomsButton_Clicked (this, null);
				});
			});

			MessagingCenter.Subscribe<CancelledMessage> (this, "CancelledMessage", message => {
				Device.BeginInvokeOnMainThread(() => {
					Console.WriteLine("No buscar cuartos");
				});
			});	
		}

		protected override void OnStart ()
		{
			SearchRoomsButton_Clicked (this, null);
		}

		protected override void OnSleep ()
		{
			var message = new StartRecentAdsMessage ();
			MessagingCenter.Send (message, "StartRecentAdsMessage");
		}

		protected override void OnResume ()
		{
			var message = new StopRecentAdsMessage ();
			MessagingCenter.Send (message, "StopRecentAdsMessage");

			SearchRoomsButton_Clicked (this, null);
		}
	}
}

