using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace DSaladin.SpeedTime.Behavior
{
    internal class ScrollToSelectedIndex: Behavior<ListView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
            base.OnDetaching();
        }

        private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
        {
            if (sender is null || sender is not ListView listView || listView.Items.Count == 0 || listView.SelectedIndex == -1)
                return;

            listView.ScrollIntoView(listView.Items.GetItemAt(listView.SelectedIndex));
        }
    }
}
