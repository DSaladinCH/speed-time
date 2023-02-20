﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Interop;
using DSaladin.FancyPotato.DSWindows;

namespace DSaladin.SpeedTime.ViewModel
{
    public partial class QuickTimeTrackerViewModel : DSViewModel
    {
        private string workTitle = "";
        public string WorkTitle
        {
            get => workTitle;
            set
            {
                workTitle = value;
                NotifyPropertyChanged();
            }
        }

        private bool? isBreak = false;
        public bool? IsBreak
        {
            get => isBreak;
            set
            {
                isBreak = value;
                if (IsBreak == true)
                    IsAFK = false;
                NotifyPropertyChanged();
            }
        }

        private bool? isAFK = false;
        public bool? IsAFK
        {
            get => isAFK;
            set
            {
                isAFK = value;
                if (IsAFK == true)
                    IsBreak = false;
                NotifyPropertyChanged();
            }
        }

        private TrackTime? lastTrackTime = default;
        public TrackTime? LastTrackTime
        {
            get => lastTrackTime;
            set
            {
                lastTrackTime = value;
                NotifyPropertyChanged();
            }
        }

        public QuickTimeTrackerViewModel(TrackTime? lastTrackTime)
        {
            LastTrackTime = lastTrackTime;
        }

        public override void WindowContentRendered(object? sender, EventArgs eventArgs)
        {
            DSWindow window = (DSWindow)sender!;
            SetForegroundWindow(new WindowInteropHelper(window).Handle);
            window.KeyUp += QuickTimeTracker_KeyUp;
        }
        
        [LibraryImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static partial bool SetForegroundWindow(IntPtr hWnd);

        private void QuickTimeTracker_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                WorkTitle = "";
                (sender as Window)!.Close();
                return;
            }

            if (e.Key == Key.Enter)
            {
                (sender as Window)!.Close();
                return;
            }

            if (e.KeyboardDevice.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.B)
                {
                    IsBreak = !IsBreak;
                    return;
                }

                if (e.Key == Key.F)
                {
                    IsAFK = !isAFK;
                    return;
                }
            }
        }
    }
}