using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using Windows.UI.ViewManagement;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Markup;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Uno.Logging;
using Uno.UI.Demo.Behaviors;
using Uno.UI.Behaviors;

using Monaco.Languages;
using Monaco.Editor;
using Monaco;

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

		private bool _isMobileFormFactor = false;
		private bool _isDirty = false;
		private bool _isEditorInitialized = false;
		private bool _codeChangePending = false;
		private DateTimeOffset _lastChange;

		public Playground()
		{
			MeasureStartup();

			this.InitializeComponent();

#if false // simulate monaco slow initialization
			// Keeps Monaco out of the tree for 5s, so the editor status message stays visible.
			var editorHost = (Border)xamlText.Parent;
			editorHost.Child = null;
			_ = DelayEditor();
			async Task DelayEditor()
			{
				await Task.Delay(5000);
				editorHost.Child = xamlText;
			}
#elif false // simulate monaco initialization failure
			// Keeps Monaco out of the tree so it never loads, and after 5s raises a fake startup error
			// through the real InternalException handler.
			((Border)xamlText.Parent).Child = null;
			_ = SimulateEditorError();
			async Task SimulateEditorError()
			{
				await Task.Delay(5000);
				var internalException = typeof(EditorHostBase).GetField(nameof(EditorHostBase.InternalException), BindingFlags.Instance | BindingFlags.NonPublic);
				(internalException?.GetValue(xamlText) as Delegate)?.DynamicInvoke(
					xamlText,
					new PlatformNotSupportedException(
						"(simulated) WebKitGTK runtime library not found. Monaco Editor requires WebKitGTK on Linux.\n\n" +
						"Install it with:\n" +
						"  Ubuntu 24.04+: sudo apt install libgtk-3-0t64 libwebkit2gtk-4.1-0\n" +
						"  Ubuntu 22.04:  sudo apt install libwebkit2gtk-4.0-37"));
			}
#endif

			Loaded += Playground_Loaded;

			xamlText.PropertyChanged += OnPropertyChanged;
			xamlText.EditorLoaded += OnEditorLoaded;

			// The Monaco backend fails silently otherwise: a failed presenter leaves an empty pane.
			xamlText.InternalException += (snd, exception) =>
			{
				this.Log().Error("Monaco editor backend failed.", exception);

				DispatcherQueue.TryEnqueue(() =>
				{
					// Once the editor is up, these are non-fatal script errors: the log is enough.
					if (!xamlText.IsEditorLoaded)
					{
						editorStatusErrors.Text += (editorStatusErrors.Text.Length > 0 ? "\n" : "") + exception.Message;
					}
				});
			};

#if __WASM__
			// Eagerly create material theme
			_ = new Uno.Material.MaterialTheme();
#endif

			jsonDataContext.TextChanged += OnDataContextTextChanged;

			content.SizeChanged += (object? snd, SizeChangedEventArgs args) =>
			{
				resolution.Text = $"{content.ActualWidth}x{content.ActualHeight}";
			};

			var abc = DynamicAnimation.StoryboardProperty.ToString();
			DynamicAnimation.GetStoryboard(this);
			DynamicAnimation.SetStoryboard(this, null!);

			SizeChanged += (object? snd, SizeChangedEventArgs e) => RestoreCodePaneSize(e.PreviousSize, e.NewSize);

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
			samplesCombobox.PlaceholderText = "Loading Snippets...";
			samplesCombobox.IsEnabled = false;

			await Task.Delay(50);

			try
			{
				SampleCategory[] categories;
				using (var httpClient = CreateHttp())
				{
					var response = await httpClient.GetAsync(BaseApiUrl);
					var responsePayload = await response.Content.ReadAsStringAsync();
					categories = JsonConvert.DeserializeObject<List<SampleCategory>>(responsePayload)!.ToArray();
				}

				var samplesForDisplay = categories
					.SelectMany(cat => cat.Samples.Select(s => new SampleForDisplay(cat, s)))
					.ToArray();

				samplesCombobox.ItemsSource = samplesForDisplay;
				samplesCombobox.PlaceholderText = "Snippets";
				samplesCombobox.IsEnabled = true;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				samplesCombobox.PlaceholderText = "[Error loading samples]";
			}
		}

		private void MeasureStartup()
		{
			var sw = Stopwatch.StartNew();
			_ = Dispatcher.RunIdleAsync(_ => { Console.WriteLine($"First idle dispatch: {sw.Elapsed}"); });
		}

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

#if __WASM__
			void OnNavigatedToFragment(object? snd, Wasm.NewFragmentEventArgs newFragmentEventArgs)
			{
				var t = LoadSample(newFragmentEventArgs.Fragment);
			}
			Wasm.FragmentNavigationHandler.NavigatedToFragment += OnNavigatedToFragment;

			var fragment = Wasm.FragmentNavigationHandler.CurrentFragment;
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
			var sample = samplesCombobox.SelectedItem as SampleForDisplay;
			if (sample != null)
			{
				await LoadSample(sample.Id);
			}
		}

		private async void Playground_Loaded(object sender, RoutedEventArgs e)
		{
			SetDarkLightToggleInitialState();

			// Not tied to the editor: if Monaco can't start (no WebView), snippets still load in the preview.
			await LoadSamples();
		}

		private void SetDarkLightToggleInitialState()
		{
			// Initialize the toggle to the current theme.
			var root = global::Microsoft.UI.Xaml.Window.Current!.Content as FrameworkElement;

			var settings = new UISettings();
			var systemBackground = settings.GetColorValue(UIColorType.Background);
			darkThemeCheckBox.IsChecked =
				systemBackground.R < 100 &&
				systemBackground.G < 100 &&
				systemBackground.B < 100;
		}

		[GeneratedRegex(@"(/\*([^*]|[\r\n]|(\*+([^*/]|[\r\n])))*\*+/)|(//.*)")]
		private static partial Regex CommentStripperRegex();

		private void OnDataContextTextChanged(object sender, TextChangedEventArgs e)
		{
			try
			{
				_registrations.ForEach(r => r.PropertyChanged -= OnExpandoChanged);
				_registrations.Clear();
				_currentError = null;

				var jsonData = jsonDataContext.Text;
				var uncommentedJsonData = CommentStripperRegex().Replace(jsonData, "");
				var data = JsonConvert.DeserializeObject<ExpandoObject>(uncommentedJsonData);

				DataContext = data;

				ClearError();

				if (data is not null)
				{
					var allSub = data
						.Flatten(i => (i as IDictionary<string, object>)?.Values.OfType<ExpandoObject>())
						.OfType<INotifyPropertyChanged>();

					_registrations.AddRange(allSub);

					foreach (var sub in allSub)
					{
						sub.PropertyChanged += OnExpandoChanged;
					}
				}
			}
			catch (Exception ex)
			{
				ShowError(ex);
			}
		}

		private void OnExpandoChanged(object? sender, PropertyChangedEventArgs e)
		{
			var data = JsonConvert.SerializeObject(DataContext);

			jsonDataContext.Text = data;
		}

		private async void OnEditorLoaded(object sender, RoutedEventArgs e)
		{
			// Before the guard below: a retry can succeed after an error was listed.
			editorStatus.Visibility = Visibility.Collapsed;
			editorStatusErrors.Text = "";

			// Raised again when the desktop backend re-bootstraps Monaco in the same page, where the
			// completion provider is still registered: registering it again would duplicate every item.
			if (_isEditorInitialized)
			{
				return;
			}

			_isEditorInitialized = true;

			// Must run once the editor is initialized: registrations made before that are dropped.
			await xamlText.Languages.RegisterCompletionItemProviderAsync("xml", new XamlLanguageProvider());
		}

		private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
		{
			Console.WriteLine("Property: " + e.PropertyName);
			if (e.PropertyName == "Text")
			{
				OnTextChanged(sender, null!);
			}
		}


		private void OnTextChanged(object? sender, TextChangedEventArgs? e)
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

			await Task.Delay(25); // give time to semi-transparent to appear

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

		/// When parsing XAML we need to prepend a Grid element (see GetXamlInput) which adds lines to the start
		/// of the XAML, causing any errors to be off by that many lines
		private int GetXamlPrefixLineCount => 3;



		private string GetXamlInput()
		{
			var ns = new[] {
				("behaviors", "using:Uno.UI.Demo.Behaviors"),
				("utu", "using:Uno.Toolkit.UI"),
				("muxc", "using:Microsoft.UI.Xaml.Controls"),
				("um", "using:Uno.Material.Extensions"),
				("cwuc", "using:CommunityToolkit.WinUI.UI.Controls"),
				("cwd", "using:CommunityToolkit.WinUI.DeveloperTools"),
				("cwl", "using:CommunityToolkit.WinUI.Lottie"),
				("x", "http://schemas.microsoft.com/winfx/2006/xaml"),
				("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006"),
				("d", "http://schemas.microsoft.com/expression/blend/2008")
			};

			var nsTags = string.Join(" ", ns.Select(v => $"xmlns:{v.Item1}=\"{v.Item2}\""));

			// The snippet starts its own line at column 0, so error positions only shift by GetXamlPrefixLineCount lines
			return
				$@"<Grid
					xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
					{nsTags}>
{xamlText.Text}
</Grid>";
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
				id = JsonConvert.DeserializeObject<string>(responsePayload)!;
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
			var url = new System.Uri($"{PlaygroundUrl}#{System.Uri.EscapeDataString(id)}");
			if (clipboard)
			{
				var clipboardData = new DataPackage();
				clipboardData.SetText(url.ToString());
				Clipboard.SetContent(clipboardData);
			}

			link.NavigateUri = url;
			link.Inlines.Clear();
			link.Inlines.Add(new Run { Text = id });
			linkBlock.Visibility = Visibility.Visible;

#if __WASM__
			Wasm.FragmentNavigationHandler.CurrentFragment = "#" + id;
#endif
		}

		private static readonly Regex _lineEndingRegex = new Regex(@"\r\n|\n\r|\n|\r");
		private Exception? _currentError;
		private bool _isLoadingSample;
		private CancellationTokenSource? _loadSampleCts;
		private bool? _autoUpdateBeforeLoad;

		private static string UniformizeLineEndings(string xamlTextText)
		{
			return _lineEndingRegex.Replace(xamlTextText, "\r\n");
		}

		private async Task LoadSample(string id)
		{
			// Loads can overlap (hash change, snippet pick): the newest one wins and older ones drop their results.
			_loadSampleCts?.Cancel();
			var ct = (_loadSampleCts = new CancellationTokenSource()).Token;

			// Captured by the first of overlapping loads only: later ones would see the false it set.
			_autoUpdateBeforeLoad ??= autoUpdate.IsChecked;

			using (var httpClient = CreateHttp())
			{
				try
				{
					autoUpdate.IsChecked = false;
					xamlText.Text = "<!-- loading... -->";
					jsonDataContext.Text = "/* loading... */";

					var response = await httpClient.GetAsync($"{BaseApiUrl}/{System.Uri.EscapeDataString(id)}", ct);
					var responsePayload = await response.Content.ReadAsStringAsync(ct);
					var json = JObject.Parse(responsePayload);

					_isLoadingSample = true;
					ClearError();
					ct.ThrowIfCancellationRequested();

					var xaml = ((string?)json["Xaml"]) ?? "<!-- empty xaml -->";
					var data = ((string?)json["Data"]) ?? "// empty";
					xamlText.Text = UniformizeLineEndings(xaml);
					jsonDataContext.Text = UniformizeLineEndings(data);

					await Task.Delay(10, ct);

					SetCodeDirtyState();

					await Update();
					ct.ThrowIfCancellationRequested();

					SetLink(id);
				}
				catch (OperationCanceledException) when (ct.IsCancellationRequested)
				{
					// Superseded by a newer load, which owns the editor, the link and the cleanup below.
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
					if (!ct.IsCancellationRequested)
					{
						Analytics.ReportPageView($"snippet/{id}", "");

						_isLoadingSample = false;
						ShowError(_currentError);

						autoUpdate.IsChecked = _autoUpdateBeforeLoad;
						_autoUpdateBeforeLoad = null;
					}
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

		private void AutoUpdate_OnChecked(object sender, RoutedEventArgs e)
		{
			LaunchUpdate();
		}

		private void DarkThemeChanged(object sender, RoutedEventArgs e)
		{
			var isChecked = darkThemeCheckBox.IsChecked;
			if (Window.Current!.Content is FrameworkElement windowRoot)
			{
				if (isChecked == true)
				{
					windowRoot.RequestedTheme = ElementTheme.Dark;
					xamlText.RequestedTheme = ElementTheme.Dark;
				}
				else
				{
					windowRoot.RequestedTheme = ElementTheme.Light;
					xamlText.RequestedTheme = ElementTheme.Light;					
				}
			}
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
			ClearMarkers();
		}

		private void ShowError(Exception? error)
		{
			_currentError = error;
			if (error != null && !_isLoadingSample)
			{
				errorText.Text = error.Message;
				errorBorder.Visibility = Visibility.Visible;

				// The editor pushes the whole Markers list to Monaco on every change: keep only the current error in it
				ClearMarkers();
				if (error is System.Xml.XmlException xamlError)
				{
					// Clamped so an error in the prepended Grid still lands in the editor
					var line = (uint)Math.Max(1, xamlError.LineNumber - GetXamlPrefixLineCount);
					xamlText.Markers.Add(
						new MarkerData()
						{
							Code = "0000",
							Message = error.Message,
							Severity = MarkerSeverity.Error,
							Source = "Origin",
							StartLineNumber = line,
							StartColumn = (uint)xamlError.LinePosition,
							EndLineNumber = line,
							EndColumn = (uint)xamlError.LinePosition + 5
						});
				}
			}

		}

		private void ClearMarkers()
		{
			// Clear() notifies even when empty, and each notification is a round-trip to Monaco
			if (xamlText.Markers.Count > 0)
			{
				xamlText.Markers.Clear();
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
			var splitter = (UIElement)sender;
			if (!splitter.CapturePointer(e.Pointer))
			{
				return;
			}

			var capturedWidth = codePane.ActualWidth;
			var capturedPoint = e.GetCurrentPoint(this).Position;
			var maxWidth = Math.Max(0, mainContent.ActualWidth - splitterColumn.ActualWidth);

			splitter.PointerMoved += Move;
			splitter.PointerReleased += End;
			splitter.PointerCaptureLost += End;

			splitter.Opacity = .5;

			// Resize the column live rather than moving the splitter with a RenderTransform: Monaco is a
			// native element, and Uno controls transformed over one neither clip nor blend correctly.
			void Move(object o, PointerRoutedEventArgs args)
			{
				var width = capturedWidth + args.GetCurrentPoint(this).Position.X - capturedPoint.X;
				codePaneColumn.Width = new GridLength(Math.Clamp(width, 0, maxWidth));
			}

			void End(object o, PointerRoutedEventArgs args)
			{
				splitter.PointerMoved -= Move;
				splitter.PointerReleased -= End;
				splitter.PointerCaptureLost -= End;

				splitter.Opacity = 1;
			}
		}

		private const int _breakPointWidth = 720;

		private bool? _originalIsChecked = null;
		private void RestoreCodePaneSize(Size oldSize, Size newSize)
		{
			_isMobileFormFactor = newSize.Width < _breakPointWidth;
			if (_isMobileFormFactor)
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
					previewPane.Visibility = Visibility.Visible;
					SelectTab("XAML");
				}
			}
		}

		private async void LogoClicked(object sender, RoutedEventArgs e)
		{
			await Launcher.LaunchUriAsync(new System.Uri("http://platform.uno/"));
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
					dataContextPane.Visibility = Visibility.Collapsed;
					xamlRadioButton.IsChecked = true;
					if (_isMobileFormFactor)
					{
						previewPane.Visibility = Visibility.Collapsed;
					}
					break;

				case "DATA":
					dataContextPane.Visibility = Visibility.Visible;
					if (_isMobileFormFactor)
					{
						previewPane.Visibility = Visibility.Collapsed;
					}
					break;

				case "OUTPUT":
					dataContextPane.Visibility = Visibility.Collapsed;
					previewPane.Visibility = Visibility.Visible;
					outputRadioButton.IsChecked = true;
					break;

				default:
					break;
			}
		}
	}

	public class XamlLanguageProvider : CompletionItemProvider
	{
		public string[] TriggerCharacters => new string[] { "<" };

		public async Task<CompletionList> ProvideCompletionItemsAsync(IModel document, Position position, CompletionContext context)
		{
			var textUntilPosition = await document.GetValueInRangeAsync(new Monaco.Range(1, 1, position.LineNumber, position.Column));

			if (textUntilPosition is not null && textUntilPosition.EndsWith("boo"))
			{
				return new CompletionList()
				{
					Suggestions = new[]
					{
						new CompletionItem("booyah", "booyah", CompletionItemKind.Folder),
						new CompletionItem("booboo", "booboo", CompletionItemKind.File),
					}
				};
			}
			else if (context.TriggerKind == CompletionTriggerKind.TriggerCharacter)
			{
				return new CompletionList()
				{
					Suggestions = new[]
					{
						new CompletionItem("TextBlock", "TextBlock>\n\t$0\n</TextBlock", CompletionItemKind.Snippet)
						{
							InsertTextRules = CompletionItemInsertTextRule.InsertAsSnippet
						},
					}
				};
			}

			return new CompletionList()
			{
				Suggestions = new CompletionItem[0]
			};
		}

		public Task<CompletionItem> ResolveCompletionItemAsync(IModel model, CompletionItem item)
			=> Task.FromResult(item);
	}
}
