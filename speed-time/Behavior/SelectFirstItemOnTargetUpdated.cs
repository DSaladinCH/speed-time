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
    internal class SelectFirstItemOnTargetUpdated : Behavior<ListView>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.TargetUpdated += OnTargetUpdated;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.TargetUpdated -= OnTargetUpdated;
            base.OnDetaching();
        }

        private void OnTargetUpdated(object? sender, DataTransferEventArgs e)
        {
            if (sender is null || sender is not ListView listView || listView.Items.Count == 0)
                return;

            listView.SetCurrentValue(System.Windows.Controls.Primitives.Selector.SelectedIndexProperty, 0);
            listView.ScrollIntoView(listView.Items.GetItemAt(0));
        }
    }
}
