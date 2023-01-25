﻿using DSaladin.FancyPotato.DSWindows;
using DSaladin.TimeTracker.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DSaladin.TimeTracker
{
    /// <summary>
    /// Interaction logic for QuickTimeTracker.xaml
    /// </summary>
    public partial class QuickTimeTracker : DSWindow
    {
        public QuickTimeTracker(DSViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        internal static TrackTime? Open(TrackTime? lastTrackTime)
        {
            QuickTimeTrackerViewModel viewModel = new(lastTrackTime);
            QuickTimeTracker quickTimeTracker = new(viewModel);
            quickTimeTracker.ShowDialog();

            if (string.IsNullOrEmpty(viewModel.WorkTitle))
                return null;

            TrackTime trackTime = new(DateTime.Now, viewModel.WorkTitle, viewModel.IsBreak.Value);
            trackTime.IsAFK = viewModel.IsAFK.Value;
            return trackTime;
        }
    }
}
