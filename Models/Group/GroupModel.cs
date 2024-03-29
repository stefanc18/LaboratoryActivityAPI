﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using LaboratoryActivityAPI.Models.Student;
using LaboratoryActivityAPI.Models.Lab;

namespace LaboratoryActivityAPI.Models.Group
{
    public class GroupModel
    {
        [Key]
        public int GroupId { get; set; }

        [Column(TypeName = "nvarchar(100)")]
        public string Name { get; set; }

        [Column()]
        public int NumberOfStudents { get; set; }

        public virtual ICollection<StudentModel> Students { get; set; }
        public virtual ICollection<LabModel> Labs { get; set; }
    }
}
