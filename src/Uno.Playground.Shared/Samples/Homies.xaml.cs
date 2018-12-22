using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Json;
using Uno.Playground.Extensions;

#if HAS_JSON_NET
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
#endif

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Uno.UI.Demo.Samples
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class Homies : Page
	{
		private string GetHomiesData()
		{
			if (this.GetType().Assembly.GetManifestResourceNames().Where(m => m.EndsWith("homies.json")).FirstOrDefault() is string dataName)
			{
				using (var stream = new StreamReader(this.GetType().Assembly.GetManifestResourceStream(dataName)))
				{
					return stream.ReadToEnd();
				}
			}

			throw new InvalidOperationException($"Failed to find homies.js in embedded resources");
		}

		public Homies()
		{
			this.InitializeComponent();

#if HAS_JSON_NET
			var data = JsonConvert.DeserializeObject<ViewModel>(GetHomiesData());
#else
			var root = System.Json.JsonObject.Parse(GetHomiesData());

			var vm = new ViewModel();

			var feedPosts = new List<FeedPost>();
			foreach (var item in root["FeedPosts"] as JsonArray)
			{
				feedPosts.Add(new FeedPost
				{
					Artist = item["artist"],
					Comments = item["comments"],
					IsLiked = item["isLiked"],
					Likes = item["likes"],
					Location = item["location"],
					Message = item["message"],
					PhotoSource = item["photoSource"],
					ProfileSource = item["profileSource"],
					Tags = item["tags"],
					Time = item["time"],
				});
			}
			vm.FeedPosts = feedPosts.ToArray();

			var groupPosts = new List<GroupPost>();
			foreach (var item in root["GroupPosts"] as JsonArray)
			{
				var groupPost = new GroupPost
				{
					Action = item.ContainsKey("action") ? (string)item["action"] : "",
					ActionGroup = item.ContainsKey("actionGroup") ? (string)item["actionGroup"] : "",
					ActionParty = item.ContainsKey("actionParty") ? (string)item["actionParty"] : "",
					Artist = item["artist"],
					IsAddPost = item["isAddPost"],
					ProfileSource = item["profileSource"],
				};

				var pictures = new List<string>();
				foreach (var picture in item["pictures"] as JsonArray)
				{
					pictures.Add(picture);
				}

				groupPost.Pictures = pictures.ToArray();
				groupPosts.Add(groupPost);
			}
			vm.GroupPosts = groupPosts.ToArray();

			var data = vm;
#endif

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
	}
}
