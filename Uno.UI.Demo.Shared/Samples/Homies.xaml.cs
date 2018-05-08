using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Newtonsoft.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.UI.Demo.Samples
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Homies : Page
	{
		public Homies()
		{
			this.InitializeComponent();

			var data = JsonConvert.DeserializeObject<ViewModel>(DataJson);

			DataContext = data;
		}

		private void feedButton_Click(object sender, RoutedEventArgs e)
		{
			feedBorder.Visibility = Visibility.Visible;
			groupsBorder.Visibility = Visibility.Collapsed;

			feedButton.Foreground = new SolidColorBrush((Windows.UI.Color) Windows.UI.Xaml.Application.Current.Resources["Color11"]);
			groupsButton.Foreground = new SolidColorBrush((Windows.UI.Color) Windows.UI.Xaml.Application.Current.Resources["Color05"]);

			feedListView.Visibility = Visibility.Visible;
			groupsListView.Visibility = Visibility.Collapsed;
		}

		private void groupsButton_Click(object sender, RoutedEventArgs e)
		{
			feedBorder.Visibility = Visibility.Collapsed;
			groupsBorder.Visibility = Visibility.Visible;

			feedButton.Foreground = new SolidColorBrush((Windows.UI.Color) Windows.UI.Xaml.Application.Current.Resources["Color05"]);
			groupsButton.Foreground = new SolidColorBrush((Windows.UI.Color) Windows.UI.Xaml.Application.Current.Resources["Color11"]);

			feedListView.Visibility = Visibility.Collapsed;
			groupsListView.Visibility = Visibility.Visible;
		}

		public class ViewModel
		{
			public FeedPost[] FeedPosts { get; set; }
			public GroupPost[] GroupPosts { get; set; }
		}

		public class FeedPost
		{
			public string Artist { get; set; }
			public string ProfileSource { get; set; }
			public string Message { get; set; }
			public string PhotoSource { get; set; }
			public string Time { get; set; }
			public bool IsLiked { get; set; }
			public int Likes { get; set; }
			public int Comments { get; set; }
			public string Tags { get; set; }
			public string Location { get; set; }
		}

		public class GroupPost
		{
			public string Artist { get; set; }
			public string ProfileSource { get; set; }
			public string Action { get; set; }
			public bool IsAddPost { get; set; }
			public string ActionParty { get; set; }
			public string ActionGroup { get; set; }
			public string[] Pictures { get; set; }
		}

		private static string DataJson = @"{ ""FeedPosts"": 
   [  
      {  
         ""artist"":""Robert Santiago"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan01.jpg"",
         ""message"":""Record Day… Vintage sound truely never gets old 🎵🎵"",
         ""photoSource"":""ms-appx:///Assets/Post01.jpg"",
         ""time"":""18 HOURS AGO"",
         ""isLiked"":false,
         ""likes"":875,
         ""comments"":8,
         ""tags"":""#vinyls #music #vinylsrecordsnevergetold"",
         ""location"":""Mile End, Montreal""
      },
      {  
         ""artist"":""Tammy Boyd"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl01.jpg"",
         ""message"":""Summer time! Group is all back together 🙌🏼"",
         ""photoSource"":""ms-appx:///Assets/Post02.jpg"",
         ""time"":""5 DAYS AGO"",
         ""isLiked"":true,
         ""likes"":378,
         ""comments"":10,
         ""tags"":""#goodtimes #dayones"",
         ""location"":""Bananas Beach Club, Wasaga Beach""
      },
      {  
         ""artist"":""Karl Fields"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan02.jpg"",
         ""message"":""God's Brew! Perfect way to start the day !"",
         ""photoSource"":""ms-appx:///Assets/Post03.jpg"",
         ""time"":""6 DAYS AGO"",
         ""isLiked"":false,
         ""likes"":278,
         ""comments"":4,
         ""tags"":""#goodcoffee #greatday"",
         ""location"":""Entertainment District, Toronto""
      },
      {  
         ""artist"":""Roberta Black"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl02.jpg"",
         ""message"":""Toast to summer! 2 scoops of sunshine please :)"",
         ""photoSource"":""ms-appx:///Assets/Post04.jpg"",
         ""time"":""8 DAYS AGO"",
         ""isLiked"":true,
         ""likes"":89362,
         ""comments"":12,
         ""tags"":""#icecream #sunshine"",
         ""location"":""Long Island, New York""
      },
      {  
         ""artist"":""Karl Fields"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan02.jpg"",
         ""message"":""Vacation throughback... Missed seeing this every morning..."",
         ""photoSource"":""ms-appx:///Assets/Post05.jpg"",
         ""time"":""12 DAYS AGO"",
         ""isLiked"":true,
         ""likes"":389,
         ""comments"":9,
         ""tags"":""#perfecttea #vacationmemories"",
         ""location"":""Hangzhou, China""
      },
      {  
         ""artist"":""Robert Santiago"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan01.jpg"",
         ""message"":""Kale! Super food, more like super blah"",
         ""photoSource"":""ms-appx:///Assets/Post06.jpg"",
         ""time"":""18 DAYS AGO"",
         ""isLiked"":false,
         ""likes"":89154,
         ""comments"":1,
         ""tags"":""#superfood #healthyeating"",
         ""location"":""West End, Vancouver""
      },
      {  
         ""artist"":""Vicki Adams"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl03.jpg"",
         ""message"":""Momo in Bed... Ready for the day!"",
         ""photoSource"":""ms-appx:///Assets/Post07.jpg"",
         ""time"":""22 DAYS AGO"",
         ""isLiked"":true,
         ""likes"":99,
         ""comments"":45,
         ""tags"":""#catloaf #kittenlife"",
         ""location"":""Centretown, Ottawa""
      },
      {  
         ""artist"":""Timothy Wilkerson"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan03.jpg"",
         ""message"":""Good boy! All times are good times!"",
         ""photoSource"":""ms-appx:///Assets/Post08.jpg"",
         ""time"":""26 DAYS AGO"",
         ""isLiked"":false,
         ""likes"":678,
         ""comments"":4,
         ""tags"":""#dogpark #goodboy #summer"",
         ""location"":""Public Garden, Boston""
      },
      {  
         ""artist"":""Roberta Black"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl02.jpg"",
         ""message"":""Night out! Brewery is finally open!!!"",
         ""photoSource"":""ms-appx:///Assets/Post09.jpg"",
         ""time"":""1 MONTH AGO"",
         ""isLiked"":false,
         ""likes"":71836,
         ""comments"":9,
         ""tags"":""#goodbeer #openingnight"",
         ""location"":""Downtown, Toronto""
      },
      {  
         ""artist"":""Timothy Wilkerson"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan03.jpg"",
         ""message"":""Stream night! Streaming tomorrow 'til 2:00 am..."",
         ""photoSource"":""ms-appx:///Assets/Post10.jpg"",
         ""time"":""1 MONTH AGO"",
         ""isLiked"":false,
         ""likes"":5463,
         ""comments"":3,
         ""tags"":""#streaming #gamingnight"",
         ""location"":""Orlando, Florida""
      },
      {  
         ""artist"":""Robert Santiago"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan01.jpg"",
         ""message"":""Sunday Hike Day!"",
         ""photoSource"":""ms-appx:///Assets/Post11.jpg"",
         ""time"":""2 MONTHS AGO"",
         ""isLiked"":false,
         ""likes"":2354,
         ""comments"":11,
         ""tags"":""#sunday #hike #trail #outdoors"",
         ""location"":""Mont-Tremblant National Park, Quebec""
      },
      {  
         ""artist"":""Tammy Boyd"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl01.jpg"",
         ""message"":""Great Team Work! Efficiency and Focus!"",
         ""photoSource"":""ms-appx:///Assets/Post12.jpg"",
         ""time"":""2 MONTHS AGO"",
         ""isLiked"":true,
         ""likes"":5423,
         ""comments"":6,
         ""tags"":""#teamwork #efficiency #focus"",
         ""location"":""Downtown, Vancouver""
      }
   ],
""GroupPosts"": 
   [  
      {  
         ""artist"":""Virg_snydr"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl01.jpg"",
         ""action"":""Likes 9 posts."",
         ""isAddPost"":""false"",
         ""pictures"":[""ms-appx:///Assets/Post03.jpg"",""ms-appx:///Assets/Post04.jpg"",""ms-appx:///Assets/Post05.jpg"",""ms-appx:///Assets/Post06.jpg"",""ms-appx:///Assets/Post09.jpg"",""ms-appx:///Assets/Post10.jpg"",""ms-appx:///Assets/Post11.jpg"",""ms-appx:///Assets/Post07.jpg"",""ms-appx:///Assets/Post08.jpg""]
      },
	  {  
         ""artist"":""Virg_snydr"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl01.jpg"",
		 ""isAddPost"":""true"",
         ""actionParty"":""Karl Fields"",
         ""actionGroup"":""UdeM"",
         ""pictures"":[]
      },
	  {  
         ""artist"":""mily_west"",
         ""profileSource"":""ms-appx:///Assets/ProfileGirl02.jpg"",
         ""action"":""Likes 2 posts."",
         ""isAddPost"":""false"",
         ""pictures"":[""ms-appx:///Assets/Post11.jpg"",""ms-appx:///Assets/Post12.jpg""]
      },
	  {  
         ""artist"":""Karl Fields"",
         ""profileSource"":""ms-appx:///Assets/ProfileMan01.jpg"",
         ""action"":""Commented on 1 post."",
         ""isAddPost"":""false"",
         ""pictures"":[""ms-appx:///Assets/Post09.jpg""]
      },
	]
}";
	}
}
