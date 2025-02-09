﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAppRestaurante.Models.Models
{
    public class BaseResponseModel
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; } = string.Empty;
        public object Data { get; set; } = new();
    }
}
