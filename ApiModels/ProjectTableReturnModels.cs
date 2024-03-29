﻿using MVP.Date.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MVP.ApiModels
{
    public class ProjectTableReturnModels
    {
        public List<Project> projects { get; set; }
        public List<Tasks> completed { get; set; }
        public List<Tasks> today { get; set; }
        public List<Tasks> future { get; set; }
    }
}
