﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearnHub.Models
{
    public class Classroom
    {
        public Guid Id { get; }
        public string Name { get; }
        public int StudentNumber { get;  }
        //fk gradeid
        //fk teacherid
   }
}
