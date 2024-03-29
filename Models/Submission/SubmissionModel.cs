﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LaboratoryActivityAPI.Models.Student;
using LaboratoryActivityAPI.Models.Assignment;

namespace LaboratoryActivityAPI.Models.Submission
{
    public class SubmissionModel
    {
        [Key]
        public int SubmissionId { get; set; }

        [Column()]
        public int AssignmentId { get; set; }

        [Column()]
        public string StudentId { get; set; }

        [Column(TypeName = "nvarchar(300)")]
        public string Link { get; set; }

        [Column(TypeName = "nvarchar(500)")]
        public string Comment { get; set; }

        [Column()]
        public int Grade { get; set; }

        [Column()]
        public DateTime SubmissionDate { get; set; }

        [ForeignKey("AssignmentId")]
        public virtual AssignmentModel Assignment { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentModel Student { get; set; }
    }
}
