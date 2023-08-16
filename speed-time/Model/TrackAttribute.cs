﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DSaladin.SpeedTime.Model
{
    public class TrackAttribute
    {
        public int TrackTimeId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

        public virtual TrackTime TrackTime { get; set; }
    }
}
