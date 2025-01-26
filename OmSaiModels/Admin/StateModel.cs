﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OmSaiModels.Admin
{
    public  class StateModel
    {
        [Key]
        public int StateId { get; set; }
        [Required]
        [StringLength(100)]
        public string StateName { get; set; }
    }
}
