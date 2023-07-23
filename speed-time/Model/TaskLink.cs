using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace DSaladin.SpeedTime.Model
{
    public class TaskLink : INotifyPropertyChanged
    {
        private string taskCheck;
        public string TaskCheck
        {
            get => taskCheck;
            set
            {
                taskCheck = value;
                NotifyPropertyChanged();
            }
        }

        private string taskLinkUri;
        public string TaskLinkUri
        {
            get => taskLinkUri;
            set
            {
                taskLinkUri = value;
                NotifyPropertyChanged();
            }
        }

        private bool containsNumbers;
        public bool ContainsNumbers
        {
            get => containsNumbers;
            set
            {
                containsNumbers = value;
                NotifyPropertyChanged();
            }
        }

        private bool containsLetters;
        public bool ContainsLetters
        {
            get => containsLetters; set
            {
                containsLetters = value;
                NotifyPropertyChanged();
            }
        }

        public TaskLink(string taskCheck, string taskLinkUri, bool containsNumbers, bool containsLetters)
        {
            TaskCheck = taskCheck;
            TaskLinkUri = taskLinkUri;
            ContainsNumbers = containsNumbers;
            ContainsLetters = containsLetters;
        }

        private string GetRegex()
        {
            string check = TaskCheck;

            if (ContainsNumbers && ContainsLetters)
                check += "[A-Za-z0-9]+";
            else if (ContainsNumbers && !ContainsLetters)
                check += "[0-9]+";
            else if (!ContainsNumbers && ContainsLetters)
                check += "[A-Za-z]+";

            return check;
        }

        public string GetLink(string text)
        {
            Match match = Regex.Match(text, GetRegex());
            if (!match.Success)
                return "";

            return string.Format(TaskLinkUri, match.Value);
        }

        public static TaskLink Empty()
        {
            return new("", "", false, false);
        }

        public static TaskLink? ContainsAny(string text, List<TaskLink> taskLinks)
        {
            foreach (TaskLink taskLink in taskLinks)
            {
                if (Regex.IsMatch(text, taskLink.GetRegex()))
                    return taskLink;
            }

            return null;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
