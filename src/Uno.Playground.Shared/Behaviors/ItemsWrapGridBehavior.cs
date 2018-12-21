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
			// Create a weak reference to the 
			var weakItemsWrapGrid = new WeakReference(d);

			UpdateItemSize(weakItemsWrapGrid.Target as ItemsWrapGrid);

			WindowSizeChangedEventHandler handler = null;

			handler = (_, __) =>
			{
				if (weakItemsWrapGrid.Target is ItemsWrapGrid itemsWrapGrid)
				{
					UpdateItemSize(itemsWrapGrid);
				}
				else
				{
					// Unregister the delegate if the target instance is not available anymore.
					Window.Current.SizeChanged -= handler;
				}
			};

			// The Uno ItemsWrapGrid is "virtual" and does not yet provide a parent for
			// which we can get the size.
			// This behavior assumes that the GridView will fill up the screen's width.
			Window.Current.SizeChanged += handler;
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
