﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ItemChanger.Placements
{
    /// <summary>
    /// Interface which indicates that placement expects all items to share a common cost.
    /// </summary>
    public interface ISingleCostPlacement
    {
        Cost Cost { get; set; }
    }
}
