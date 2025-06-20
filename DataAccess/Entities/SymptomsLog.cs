﻿using DataAccess.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Entities
{
    public class SymptomsLog: BaseEntity
    {
        public DateTime Date { get; set; } 
        public required List<Symptom> Symptoms { get; set; }
    }
}
