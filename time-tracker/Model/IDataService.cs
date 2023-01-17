﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.TimeTracker.Model
{
    internal interface IDataService
    {
        Task SaveTrackedTimes(List<TrackTime> trackedTimes);
        Task<List<TrackTime>> LoadTrackedTimes();
    }
}