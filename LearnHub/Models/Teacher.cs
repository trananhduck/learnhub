﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace LearnHub.Models
{
    public class Teacher
    {
        [Key, ForeignKey("User")]
        public string Username { get; set; }
        public string CitizenID { get; set; }
        public int? Salary { get; set; }
        public DateTime? DateOfJoining { get; set; }
        public string FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDay { get; set; }
        public string Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Ethnicity { get; set; }
        public string? Religion { get; set; }
        public double? Coefficient { get; set; } // hệ số lương
        public string? Specialization { get; set; }

        //Navigation Properties 
        public User User { get; set; }
        public ICollection<Classroom> Classrooms { get; set; }
        public ICollection<TeachingAssignment> TeachingAssignments { get; set; }
        public ICollection<Document> Documents { get; set; }
        public ICollection<Exercise> Exercises { get; set; }
    }
}
