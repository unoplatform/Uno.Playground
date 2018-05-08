using System;
using System.Collections.Generic;
using System.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Uno.UI.Demo.Behaviors
{
    public static class ItemsWrapGridBehavior
    {
		public static double GetMaxItemSize(DependencyObject obj)
		{
			return (double)obj.GetValue(MaxItemSizeProperty);
		}

		public static void SetMaxItemSize(DependencyObject obj, double value)
		{
			obj.SetValue(MaxItemSizeProperty, value);
		}

		// Using a DependencyProperty as the backing store for MaxItemSize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty MaxItemSizeProperty =
			DependencyProperty.RegisterAttached("MaxItemSize", typeof(double), typeof(ItemsWrapGridBehavior), new PropertyMetadata(0d, OnMaxItemSizeChanged));

		private static void OnMaxItemSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var itemsWrapGrid = d as ItemsWrapGrid;
			UpdateItemSize(itemsWrapGrid);
			Window.Current.SizeChanged += (_, __) => UpdateItemSize(itemsWrapGrid);
		}

		private static void UpdateItemSize(ItemsWrapGrid itemsWrapGrid)
		{
			var availableWidth = Window.Current.Bounds.Width;
			var maxItemSize = GetMaxItemSize(itemsWrapGrid);
			var horizontalItems = Math.Ceiling(availableWidth / maxItemSize);
			var itemSize = Math.Floor(availableWidth / horizontalItems);
			itemsWrapGrid.ItemWidth = itemSize;
			itemsWrapGrid.ItemHeight = itemSize;
		}
	}
}
