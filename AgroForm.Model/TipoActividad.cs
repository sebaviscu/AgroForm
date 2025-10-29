﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class TipoActividad : EntityBase
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Icono { get; set; }

        public ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
    }
}
