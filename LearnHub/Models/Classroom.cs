﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LearnHub.Models
{
    public class Classroom : DomainObject
    {
        public string Name { get; set; }
        public int? Capacity { get; set; }
        public Guid? GradeId { get; set; }
        public Guid? TeacherInChargeId { get; set; }
        //Navigation Properties
        public Grade Grade { get; set; }
        public Teacher TeacherInCharge { get; set; }
        public ICollection<StudentPlacement> StudentPlacements { get; set; }
        public ICollection<TeachingAssignment> TeachingAssignments { get; set; }
        public ICollection<Document> Documents { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<ExamSchedule> ExamSchedules { get; set; }
        public ICollection<Exercise> Exercises { get; set; }

    }
}