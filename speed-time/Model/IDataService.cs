﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    internal interface IDataService
    {
        Task SaveSettings();
        Task LoadSettings();
    }
}
