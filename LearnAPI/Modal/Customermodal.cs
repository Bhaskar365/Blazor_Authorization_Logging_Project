﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearnAPI.Modal
{
    public class Customermodal
    {
        [StringLength(50)]
        [Unicode(false)]
        public string Code { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string Name { get; set; } = null!;

        [StringLength(50)]
        [Unicode(false)]
        public string? Email { get; set; }

        [StringLength(50)]
        [Unicode(false)]
        public string? Phone { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal? CreditLimit { get; set; }

        public bool? IsActive { get; set; }

        public int? Taxcode { get; set; }

        public string? Statusname { get; set; }
    }
}
