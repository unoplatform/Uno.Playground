using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Newtonsoft.Json;
using Uno.Disposables;
using Uno.Extensions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using Uno.Logging;
using Uno.UI.Demo.Behaviors;
using Uno.UI.Toolkit;

namespace Uno.UI.Demo.Samples
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame. 
	/// </summary>
	public sealed partial class Playground : Page
	{
		private const string BaseApiUrl = "https://uno-ui-api.azurewebsites.net/api/samples";
		private const string PlaygroundUrl = "http://playground.platform.uno/";
		private const int CodeUpdateThrottleDelayMs = 550;
		private const int HintDisplayTime = 5000;

		private readonly List<INotifyPropertyChanged> _registrations = new List<INotifyPropertyChanged>();

		private bool _isDirty = false;
		private bool _codeChangePending = false;
		private DateTimeOffset _lastChange;

		public Playground()
		{
			MeasureStartup();

			this.InitializeComponent();

			Loaded += Playground_Loaded;

			xamlText.PropertyChanged += OnPropertyChanged;
			xamlText.Loaded += OnEditorLoaded;

			jsonDataContext.TextChanged += OnDataContextTextChanged;

			content.SizeChanged += (snd, args) =>
			{
				resolution.Text = $"{content.ActualWidth}x{content.ActualHeight}";
			};

			var abc = DynamicAnimation.StoryboardProperty.ToString();
			DynamicAnimation.GetStoryboard(this);
			DynamicAnimation.SetStoryboard(this, null);

			SizeChanged += (snd, e) => RestoreCodePaneSize(e.PreviousSize, e.NewSize);
#if false // __WASM__
			Uno.Foundation.WebAssemblyRuntime.InvokeJS("Uno.UI.WindowManager.current.setStyle(\"" + splitter.HtmlId + "\", {\"cursor\": \"col-resize\"});");
#endif
			InputPane.GetForCurrentView().Showing += OnInputPaneShowing;
			InputPane.GetForCurrentView().Hiding += OnInputPaneHiding; ;

			
		}
		private void OnInputPaneShowing(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			VisibleBoundsPadding.SetPaddingMask(TabsPane, VisibleBoundsPadding.PaddingMask.Top);
			VisibleBoundsPadding.SetPaddingMask(root, VisibleBoundsPadding.PaddingMask.None);
			toolbarRow.Height = new GridLength(0);
		}
		private void OnInputPaneHiding(InputPane sender, InputPaneVisibilityEventArgs args)
		{
			VisibleBoundsPadding.SetPaddingMask(TabsPane, VisibleBoundsPadding.PaddingMask.None);
			VisibleBoundsPadding.SetPaddingMask(root, VisibleBoundsPadding.PaddingMask.Top);
			toolbarRow.Height = new GridLength(72);
		}

		private async Task LoadSamples()
		{
			samples.ItemsSource = new [] {new SampleForDisplay(new SampleCategory(), new Sample {Title = "Loading Snippets..."})};
			samples.IsEnabled = false;

			await Task.Delay(50);

			try
			{
				SampleCategory[] categories;
				using (var httpClient = CreateHttp())
				{
					var response = await httpClient.GetAsync(BaseApiUrl);
					var responsePayload = await response.Content.ReadAsStringAsync();
					categories = JsonConvert.DeserializeObject<List<SampleCategory>>(responsePayload).ToArray();
				}

				var samplesForDisplay = categories
					.SelectMany(cat => cat.Samples.Select(s => new SampleForDisplay(cat, s)))
					.ToArray();

				samples.ItemsSource = samplesForDisplay;
				samples.SelectionChanged += Samples_SelectionChanged;
				samples.IsEnabled = true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				samples.ItemsSource = new[] {new SampleForDisplay(new SampleCategory(), new Sample {Title = "[Error loading samples]"})};
			}
		}

		private void MeasureStartup()
		{
			var sw = Stopwatch.StartNew();
			Dispatcher.RunIdleAsync(_ => { Console.WriteLine($"First idle dispatch: {sw.Elapsed}"); });
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

#if __WASM__
			void OnNavigatedToFragment(object snd, Wasm.NewFragmentEventArgs newFragmentEventArgs)
			{
				var t = LoadSample(newFragmentEventArgs.Fragment);
			}
			Wasm.FragmentHavigationHandler.NavigatedToFragment += OnNavigatedToFragment;

			var fragment = Wasm.FragmentHavigationHandler.CurrentFragment;
			if (!string.IsNullOrWhiteSpace(fragment))
			{
				await LoadSample(fragment);
			}
			else
#endif
			{
				await LoadSample("wasm-start");
			}

//			if (e.Parameter is string args)
//			{
//				var parts = args.Split(';');
//				var keyValues = parts.Select(p => p.Split(new[] { '=' }, 2)).Where(kv => kv.Length == 2);
//				var sampleId = keyValues
//					.FirstOrDefault(kv => kv[0].Equals("sample", StringComparison.OrdinalIgnoreCase) || kv[0].Equals("snippet", StringComparison.OrdinalIgnoreCase))
//					?[1].Trim();
//				if (!string.IsNullOrEmpty(sampleId))
//				{
//					await LoadSample(sampleId);
//				}
//#if __WASM__
//				else
//				{
//					await LoadSample("wasm-start");
//				}
//#endif
//			}
		}

		private async void Samples_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var sample = samples.SelectedItem as SampleForDisplay;
			if (sample != null)
			{
#if !__WASM__
				if (MainSplitView.DisplayMode == SplitViewDisplayMode.Inline
					|| MainSplitView.DisplayMode == SplitViewDisplayMode.Overlay)
				{
					MainSplitView.IsPaneOpen = false;
				}
#endif
				await LoadSample(sample.Id);
			}
		}
		private void OnBackClicked(object sender, RoutedEventArgs e)
		{
			Frame.GoBack();
		}

		private async void Playground_Loaded(object sender, RoutedEventArgs e)
		{
#if !__WASM__
			backButton.Visibility = base.Frame.CanGoBack ? Visibility.Visible : Visibility.Collapsed;

			xamlText.Text = @"<StackPanel Orientation=""Vertical"">
	<!-- Type your favorite XAML code here
	...or pick a snippet in the menu. -->

	<TextBlock Text=""{Binding message}"" />
</StackPanel>
";

			jsonDataContext.Text = @"{
	message: ""Hello World!""
}

/* Tip: two-way bindings supported */";

			LaunchUpdate();
#endif
					//await LoadSamples();
		}

		private static readonly Regex CommentStripperRegex = new Regex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)");

		private void OnDataContextTextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				_registrations.ForEach(r => r.PropertyChanged -= OnExpandoChanged);
				_registrations.Clear();
				_currentError = null;

				var jsonData = jsonDataContext.Text;
				var uncommentedJsonData = CommentStripperRegex.Replace(jsonData, "");
				var data = JsonConvert.DeserializeObject<ExpandoObject>(uncommentedJsonData);

				DataContext = data;

				ClearError();

				var allSub = data
					.Flatten(i => (i as IDictionary<string, object>)?.Values.OfType<ExpandoObject>())
					.OfType<INotifyPropertyChanged>();

				_registrations.AddRange(allSub);

				foreach (var sub in allSub)
				{
					sub.PropertyChanged += OnExpandoChanged;
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void OnExpandoChanged(object sender, PropertyChangedEventArgs e)
		{
			var data = JsonConvert.SerializeObject(DataContext);

			jsonDataContext.Text = data;
		}

		private async void OnEditorLoaded(object sender, RoutedEventArgs e)
		{
			xamlText.CodeLanguage = "xml";
			await LoadSamples();
		}

		private void OnPropertyChanged(object sender, PropertyChangedEventArgs e )
		{
			Console.WriteLine("Property: " + e.PropertyName);
			if (e.PropertyName == "Text")
			{
				OnTextChanged(sender, null);
			}
		}


		private void OnTextChanged(object sender, TextChangedEventArgs e)
		{
			_lastChange = DateTimeOffset.Now;

			SetCodeDirtyState();

			if (!autoUpdate.IsChecked == true)
			{
				return;
			}

			StartCodeThrottle();
		}

		private async void StartCodeThrottle()
		{
			if (_codeChangePending)
			{
				return;
			}

			_codeChangePending = true;

			try
			{
				while (true)
				{
					var now = DateTimeOffset.Now;
					var nextupdate = _lastChange.AddMilliseconds(CodeUpdateThrottleDelayMs);
					if (now < nextupdate)
					{
						await Task.Delay(nextupdate - now);
						continue;
					}

					await Update();
					break;
				}
			}
			finally
			{
				_codeChangePending = false;
			}
		}

		private void SetCodeDirtyState(bool isDirty = true)
		{
			if (_isDirty == isDirty)
			{
				return;
			}

			if (isDirty)
			{
				_isDirty = true;
				linkBlock.Visibility = Visibility.Collapsed;
				saveBtn.IsEnabled = false;
				if (autoUpdate.IsChecked == false)
				{
					updateBtn.IsEnabled = true;
				}
			}
			else
			{
				_isDirty = false;
				saveBtn.IsEnabled = true;
				updateBtn.IsEnabled = false;
			}
		}

		private async void LaunchUpdate()
		{
			await Update();
		}

		private async Task Update()
		{
			if (!_isDirty)
			{
				return;
			}

			content.Content = null;
			loading.Visibility = Visibility.Visible;

			await Task.Delay(25); // give time to semi-transaprent to appear

			var sw = Stopwatch.StartNew();
			try
			{
				var r = XamlReader.Load(GetXamlInput());
				Console.WriteLine($@"Read Xaml in {sw.Elapsed}");

				await Task.Yield();

				content.Content = r;

				ClearError();
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
			finally
			{
				await Task.Delay(25); // give time to content to appear
				loading.Visibility = Visibility.Collapsed;
				SetCodeDirtyState(isDirty: false);
			}
		}

		private string GetXamlInput()
		{
			Console.WriteLine("Current text: " + xamlText.Text);
			return string.Concat(
				@"<Grid
					xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
					xmlns:x= ""http://schemas.microsoft.com/winfx/2006/xaml""
					xmlns:behaviors=""using:Uno.UI.Demo.Behaviors"">",
				xamlText.Text,
				"\n</Grid>"
			);
		}

		private static readonly string _appName =
			string.Concat(
					typeof(Playground).GetAssembly().FullName,
					", v=",
					typeof(Playground).GetAssembly().GetAssemblyAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "unkn");

		private async void Save_Clicked(object sender, RoutedEventArgs e)
		{
			var xaml = UniformizeLineEndings(xamlText.Text);
			var data = UniformizeLineEndings(jsonDataContext.Text);

			var request = new JObject(
				new JProperty("xaml", xaml),
				new JProperty("data", data),
				new JProperty("app", _appName)
			).ToString();

			string id;
			using (var httpClient = CreateHttp())
			{
				var httpContent = new StringContent(request, Encoding.UTF8, "text/json");
				var response = await httpClient.PostAsync(BaseApiUrl, httpContent);
				var responsePayload = await response.Content.ReadAsStringAsync();
				id = JsonConvert.DeserializeObject<string>(responsePayload);
			}

			SetLink(id, clipboard: true);
			ShowHint();
		}

		private async void ShowHint()
		{
			savedMessage.Visibility = Visibility.Visible;
			await Task.Delay(HintDisplayTime);
			savedMessage.Visibility = Visibility.Collapsed;
		}

		private void SetLink(string id, bool clipboard = false)
		{
			var url = new Uri($"{PlaygroundUrl}#{Uri.EscapeUriString(id)}");
			if (clipboard)
			{
				var clipboardData = new DataPackage();
				clipboardData.SetText(url.ToString());
				Clipboard.SetContent(clipboardData);
			}

			link.NavigateUri = url;
			link.Inlines.Clear();
			link.Inlines.Add(new Run{Text = id});
			linkBlock.Visibility = Visibility.Visible;

#if __WASM__
			Wasm.FragmentHavigationHandler.CurrentFragment = "#" + id;
#endif
		}

		private static readonly Regex _lineEndingRegex = new Regex(@"\r\n|\n\r|\n|\r");
		private Exception _currentError;
		private bool _isLoadingSample;

		private static string UniformizeLineEndings(string xamlTextText)
		{
			return _lineEndingRegex.Replace(xamlTextText, "\r\n");
		}

		private async Task LoadSample(string id)
		{
			return;//
			using (var httpClient = CreateHttp())
			{
				var previousAutoUpdate = autoUpdate.IsChecked;
				try
				{
					autoUpdate.IsChecked = false;
					xamlText.Text = "<!-- loading... -->";
					jsonDataContext.Text = "/* loading... */";

					var response = await httpClient.GetAsync($"{BaseApiUrl}/{Uri.EscapeUriString(id)}");
					var responsePayload = await response.Content.ReadAsStringAsync();
					var json = JObject.Parse(responsePayload);

					_isLoadingSample = true;
					ClearError();

					var xaml = ((string) json["Xaml"]) ?? "<!-- empty xaml -->";
					var data = ((string)json["Data"]) ?? "// empty";
					xamlText.Text = UniformizeLineEndings(xaml);
					jsonDataContext.Text = UniformizeLineEndings(data);

					await Task.Delay(10);

					SetCodeDirtyState();

					await Update();

					SetLink(id);
				}
				catch (Exception ex)
				{
					xamlText.Text = $@"<!-- UNABLE TO LOAD YOUR SNIPPET! -->
<TextBlock>
	Error loading snippet {id}:
	<LineBreak/>
	<Run FontWeight=""Bold"" Foreground=""Red"">
		{ex.Message}
	</Run>
</TextBlock>";

					jsonDataContext.Text = "";

					SetCodeDirtyState();

					await Update();
				}
				finally
				{
					Analytics.ReportPageView($"snippet/{id}", "");

					_isLoadingSample = false;
					ShowError(_currentError);

					autoUpdate.IsChecked = previousAutoUpdate;
				}
			}
		}
		private static HttpClient CreateHttp()
		{
			var httpClient = new HttpClient();
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/json"));
			return httpClient;
		}

		private void ToggleSplitView(object sender, RoutedEventArgs e)
		{
			if (MainSplitView.IsPaneOpen)
			{
				MainSplitView.IsPaneOpen = false;
				openPaneButton.Visibility = Visibility.Visible;
			}
			else
			{
				MainSplitView.IsPaneOpen = true;
				openPaneButton.Visibility = Visibility.Collapsed;
			}
		}

		private void AutoUpdate_OnChecked(object sender, RoutedEventArgs e)
		{
			LaunchUpdate();
		}

		private void Update_OnTapped(object sender, RoutedEventArgs e)
		{
			SetCodeDirtyState();
			LaunchUpdate();
		}

		private void ClearError()
		{
			_currentError = null;
			errorBorder.Visibility = Visibility.Collapsed;
			errorText.Text = "No error";
		}

		private void ShowError(Exception error)
		{
			_currentError = error;
			if (error != null && !_isLoadingSample)
			{
				errorText.Text = error.Message;
				errorBorder.Visibility = Visibility.Visible;
			}
		}

		private void CopyError(object sender, RoutedEventArgs e)
		{
			var clipboardData = new DataPackage();
			clipboardData.SetText(_currentError?.ToString() ?? "No error");
			Clipboard.SetContent(clipboardData);
		}

		private void CloseErrorPane(Object sender, RoutedEventArgs e)
		{
			errorBorder.Visibility = Visibility.Collapsed;
		}

		private void BeginResizeCodePane(object sender, PointerRoutedEventArgs e)
		{
#if !__WASM__ // Disable for pointer capture issues
			var splitter = (UIElement)sender;
			if (!splitter.CapturePointer(e.Pointer))
			{
				return;
			}

			var capturedWidth = codePane.ActualWidth;
			var capturedPoint = e.GetCurrentPoint(this).Position;
			var transform = splitter.RenderTransform as TranslateTransform 
				?? (TranslateTransform) (splitter.RenderTransform = new TranslateTransform());

			splitter.PointerMoved += Move;
			splitter.PointerReleased += Release;
#if !__WASM__
			splitter.PointerCaptureLost += Lost;
#endif

			splitter.Opacity = .5;


			void Move(object o, PointerRoutedEventArgs args)
			{
				transform.X = args.GetCurrentPoint(this).Position.X - capturedPoint.X;
			}

			void Release(object o, PointerRoutedEventArgs args)
			{
				splitter.PointerMoved -= Move;
				splitter.PointerReleased -= Release;
#if !__WASM__
				splitter.PointerCaptureLost -= Lost;
#endif

				codePaneColumn.Width = new GridLength(capturedWidth + args.GetCurrentPoint(this).Position.X - capturedPoint.X);
				transform.X = 0;
				splitter.Opacity = 1;
			}

			void Lost(object o, PointerRoutedEventArgs args)
			{
				splitter.PointerMoved -= Move;
				splitter.PointerReleased -= Release;
#if !__WASM__
				splitter.PointerCaptureLost -= Lost;
#endif

				transform.X = 0;
				splitter.Opacity = 1;
			}
#endif
		}

#if __WASM__
		private const int _breakPointWidth = 720;
#else
		private const int _breakPointWidth = 550;
#endif
		private bool? _originalIsChecked = null;
		private void RestoreCodePaneSize(Size oldSize, Size newSize)
		{
			if (newSize.Width < _breakPointWidth)
			{
				if (oldSize.Width >= _breakPointWidth || oldSize.Width == 0)
				{
					runPane.Visibility = Visibility.Collapsed;
					_originalIsChecked = autoUpdate.IsChecked;
					autoUpdate.IsChecked = false;
					SelectTab("OUTPUT");
				}
			}
			else
			{
				if (oldSize.Width < _breakPointWidth)
				{
					runPane.Visibility = Visibility.Visible;
					autoUpdate.IsChecked = _originalIsChecked ?? true;
				}
				SelectTab(null); // ensure to restore the splitter
			}
		}

		private async void LogoClicked(object sender, RoutedEventArgs e)
		{
			await Launcher.LaunchUriAsync(new Uri("http://platform.uno/"));
		}

		private void ShowXaml(object sender, RoutedEventArgs e) => SelectTab("XAML");
		private void ShowData(object sender, RoutedEventArgs e) => SelectTab("DATA");

		private void ShowOutput(object sender, RoutedEventArgs e)
		{
			SelectTab("OUTPUT");
			LaunchUpdate();
		} 

		private void SelectTab(string pane)
		{
			this.Log().Info($"Going to tab {pane ?? "none"}");

			switch (pane)
			{
				case "XAML":
#if !__WASM__
					MainSplitView.IsPaneOpen = false;
					MainSplitView.DisplayMode = SplitViewDisplayMode.Overlay; 
					openPaneButton.Visibility = Visibility.Visible;
#endif
					codePaneColumn.Width = new GridLength(1, GridUnitType.Star);
					splitterColumn.Width = new GridLength(0);
					previewColumn.Width = new GridLength(0);
					tabsPaneRow.Height = new GridLength(1, GridUnitType.Auto);
					xamlPaneRow.Height = new GridLength(1, GridUnitType.Star);
					corePaneRow.Height = new GridLength(0);
					break;


				case "DATA":
#if !__WASM__
					MainSplitView.IsPaneOpen = false;
					MainSplitView.DisplayMode = SplitViewDisplayMode.Overlay; 
					openPaneButton.Visibility = Visibility.Visible;
#endif
					codePaneColumn.Width = new GridLength(1, GridUnitType.Star);
					splitterColumn.Width = new GridLength(0);
					previewColumn.Width = new GridLength(0);
					tabsPaneRow.Height = new GridLength(1, GridUnitType.Auto);
					xamlPaneRow.Height = new GridLength(0);
					corePaneRow.Height = new GridLength(1, GridUnitType.Star); 
					break;

				case "OUTPUT":
#if !__WASM__
					MainSplitView.IsPaneOpen = false;
					MainSplitView.DisplayMode = SplitViewDisplayMode.Overlay; 
					openPaneButton.Visibility = Visibility.Visible;
#endif
					codePaneColumn.Width = new GridLength(0);
					splitterColumn.Width = new GridLength(0);
					previewColumn.Width = new GridLength(1, GridUnitType.Star);
					tabsPaneRow.Height = new GridLength(1, GridUnitType.Auto);
					xamlPaneRow.Height = new GridLength(1, GridUnitType.Star);
					corePaneRow.Height = new GridLength(0);
					break;

				default:
#if !__WASM__
					MainSplitView.IsPaneOpen = true;
					MainSplitView.DisplayMode = SplitViewDisplayMode.CompactInline;
					openPaneButton.Visibility = Visibility.Collapsed;
#endif
					codePaneColumn.Width = new GridLength(1, GridUnitType.Star);
					splitterColumn.Width = new GridLength(10);
					previewColumn.Width = new GridLength(1, GridUnitType.Star);
					tabsPaneRow.Height = new GridLength(0, GridUnitType.Pixel);
					xamlPaneRow.Height = new GridLength(1, GridUnitType.Star);
					corePaneRow.Height = new GridLength(1, GridUnitType.Star);
					break;
			}
		}
	}
}
