﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LearnHub.Models
{
    public class Subject : IAdminId
    {
        public string Id { get; set; }
        public string Name { get; set; }

        public int? LessonNumber { get; set; }

        public string? GradeId { get; set; }

        public string? MajorId { get; set; }
        public string? AdminId { get; set; }
        public Admin Admin { get; set; }

        //Navigation Properties
        public Grade Grade { get; set; }
        public Major Major { get; set; }
        public ICollection<TeachingAssignment> TeachingAssignments { get; set; }
       
        public ICollection<ExamSchedule> ExamSchedules { get; set; }
        public ICollection<Score> SubjectResults { get; set; }
    }
}
