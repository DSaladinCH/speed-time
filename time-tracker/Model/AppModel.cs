using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker.Model
{
    internal static class AppModel
    {
        public static IDataService DataService { get; set; } = new PropertyDataService();
    }
}
