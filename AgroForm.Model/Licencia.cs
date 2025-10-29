using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model
{
    public class Licencia
    {
        public int Id { get; set; }
        public DateTime? RegistrationDate { get; set; }

        public string? RazonSocial { get; set; }
        public string? NombreContacto { get; set; }
        public string? NumeroContacto { get; set; }
    }
}
