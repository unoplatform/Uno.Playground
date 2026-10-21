using Uno.Logging;
using Windows.UI.Core;
using Microsoft.UI.Xaml.Media.Animation;

namespace Uno.UI.Demo.Behaviors
{
	[Bindable]
	public static class DynamicAnimation
    {
#if !WINAPPSDK
		[Preserve]
#endif
		public static readonly DependencyProperty StoryboardProperty = DependencyProperty.RegisterAttached(
			"Storyboard", typeof(Storyboard), typeof(DependencyObject), new PropertyMetadata(default(Storyboard), OnStoryBoardChanged));

		private static void OnStoryBoardChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			(e.OldValue as Storyboard)?.Stop();
			if (e.NewValue is Storyboard storyboard)
			{
				foreach (var timeline in storyboard.Children)
				{
					Storyboard.SetTarget(timeline, d);
				}

				_ = d.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
				{
					try
					{
						storyboard.Begin();
					}
					catch (Exception err)
					{
						typeof(DynamicAnimation).Log().Error("Failed to start storyboard.", err);
					}
				});
			}
		}

#if !WINAPPSDK
		[Preserve]
#endif
		public static Storyboard GetStoryboard(DependencyObject target)
			=> (Storyboard) target.GetValue(StoryboardProperty);

#if !WINAPPSDK
		[Preserve]
#endif
		public static void SetStoryboard(DependencyObject target, Storyboard value)
			=> target.SetValue(StoryboardProperty, value);
	}
}
