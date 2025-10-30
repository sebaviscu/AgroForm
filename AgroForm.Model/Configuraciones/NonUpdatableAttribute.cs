using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgroForm.Model.Configuracion
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
    public class NonUpdatableAttribute : Attribute
    {
        public NonUpdatableAttribute() { }
    }
}
