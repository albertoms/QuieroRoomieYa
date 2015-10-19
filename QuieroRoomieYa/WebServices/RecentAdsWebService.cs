using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace QuieroRoomieYa
{
	public class RecentAdsWebService
	{
		public async Task GetRecentAdsService(CancellationToken token)
		{
			await Task.Run (async () => {
				for ( long i=0; i<long.MaxValue; i++ ) {
					token.ThrowIfCancellationRequested ();
					await Task.Delay(300000); //5 minutos
					RoomOfferAd[] recentAds = await GetRecentAdsAsync();

					Device.BeginInvokeOnMainThread(() => {
						MessagingCenter.Send<RoomOfferAd[]>(recentAds, "RoomOffersAds");
					});	
				}
			}, token);
		}

		public async Task<RoomOfferAd[]> GetRecentAdsAsync() {
			var client = new System.Net.Http.HttpClient ();		
			StringContent payload = new StringContent ("latitude=19.432608&longitude=-99.13320799999997&is_recent=true&rooms_type=room_ofrezco", Encoding.UTF8, "application/x-www-form-urlencoded");
			var response = await client.PostAsync (new Uri("http://www.dadaroom.com/busqueda/display"), payload);
			var roomsJSON = response.Content.ReadAsStringAsync ().Result;
			RoomOfferAd[] roomOffers = null;

			if ( roomsJSON != "" ) {
				RecentAd[] rooms = JsonConvert.DeserializeObject<RecentAd[]> (roomsJSON);
				List<string> roomsLast48Hours = new List<string>();
				roomsLast48Hours.Add ("["); // abrir formato de json array
				foreach ( RecentAd room in rooms ) {
					DateTime dateAd = Convert.ToDateTime (room.creationDate);
					DateTime twoDaysBeforeNow = Convert.ToDateTime(DateTime.Today.AddDays (-2).ToString ("yyyy-M-d HH:mm:ss"));

					// Queremos los anuncios que sean de tipo "Ofrezco cuarto" y que hayan sido publicados <= 2 días.
					if ( room.category == "room_ofrezco" && DateTime.Compare(twoDaysBeforeNow, dateAd) <= 0) {
						var roomAdResponse = await client.GetAsync (new Uri (string.Format ("http://www.dadaroom.com/anuncio/detalle/{0}", room.id)));
						var adDetails = roomAdResponse.Content.ReadAsStringAsync ().Result;
						if (adDetails != "") {
							roomsLast48Hours.Add (adDetails);
							roomsLast48Hours.Add(",");	
						}
					}
				}
				roomsLast48Hours.RemoveAt(roomsLast48Hours.Count-1); // Quitar última coma
				roomsLast48Hours.Add ("]"); // cerrar formato de json array
				// el replace ayuda a que la url de la foto quede formateada correctamente
				var roomsJoined = String.Join ("", roomsLast48Hours).Replace("\\/", "/");
				roomOffers = JsonConvert.DeserializeObject<RoomOfferAd[]> (roomsJoined);
			}

			return roomOffers;
		}
	}
}

