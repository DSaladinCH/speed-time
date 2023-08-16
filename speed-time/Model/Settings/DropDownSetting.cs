using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DSaladin.SpeedTime.Model.Settings
{
    internal class DropDownSetting : BindingSetting<object>
    {
        public IEnumerable<object> List
        {
            get { return (IEnumerable<object>)GetValue(ListProperty); }
            set
            {
                SetValue(ListProperty, value);
                NotifyPropertyChanged();
            }
        }

        public string DisplayMemberPath
        {
            get { return (string)GetValue(DisplayMemberPathProperty); }
            set
            {
                SetValue(DisplayMemberPathProperty, value);
                NotifyPropertyChanged();
            }
        }

        public string SelectedValuePath
        {
            get { return (string)GetValue(SelectedValuePathProperty); }
            set
            {
                SetValue(SelectedValuePathProperty, value);
                NotifyPropertyChanged();
            }
        }

        public static readonly DependencyProperty ListProperty =
            DependencyProperty.Register(nameof(List), typeof(IEnumerable), typeof(DropDownSetting));

        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(DropDownSetting),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(DropDownSetting),
                new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    }
}
