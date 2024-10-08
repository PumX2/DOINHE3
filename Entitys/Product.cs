﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DOINHE.Entitys
{
    public class Product
    {
        public int Id { get; set; }

        public string? ProductName { get; set; } 

        public int? CategoryId { get; set; }
        public int? UserId { get; set; }

        public DateTime? DateTimeStart { get; set; }

        public DateTime? DateTimeEnd { get; set; }

        public DateTime? CreateDate { get; set; }

        public double? Price { get; set; }

        public string? Address { get; set; }

        public string? Description { get; set; }

        public bool? StatusIsBuy { get; set; }

        public bool? StatusIsApprove { get; set; }

        public byte[]? ImgDescription { get; set; }

        public string? Key { get; set; }

        public byte[]? ImgKey { get; set; }

        public virtual User? Users { get; set; }
        public virtual Category? Categories { get; set; }
        public virtual ICollection<Order>? Orders { get; set; }

        public static implicit operator Product?(string? v)
        {
            throw new NotImplementedException();
        }
    }
}
