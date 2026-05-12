using AgroForm.Model;

namespace AgroForm.Web.Models.Configuracion
{
    public class CultivoConfigVM
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? Orden { get; set; }
        public bool Activo { get; set; }
        public string? Color { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsVisible { get; set; }
        public List<EstadoFenologicoConfigVM> EstadosFenologicos { get; set; } = new();
    }

    public class EstadoFenologicoConfigVM
    {
        public int Id { get; set; }
        public int IdCultivo { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
    }

    public class CatalogoConfigVM
    {
        public int Id { get; set; }
        public EnumClass.TipoCatalogoEnum Tipo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public bool IsGlobal { get; set; }
        public bool IsVisible { get; set; }
        public string TipoDisplay => GetTipoDisplayName();

        private string GetTipoDisplayName()
        {
            return Tipo switch
            {
                EnumClass.TipoCatalogoEnum.Plaga => "Plaga",
                EnumClass.TipoCatalogoEnum.Maleza => "Maleza",
                EnumClass.TipoCatalogoEnum.Enfermedad => "Enfermedad",
                EnumClass.TipoCatalogoEnum.TipoFertilizante => "Tipo Fertilizante",
                EnumClass.TipoCatalogoEnum.Nutriente => "Nutriente",
                EnumClass.TipoCatalogoEnum.ProductoAgroquimico => "Producto Agroquímico",
                EnumClass.TipoCatalogoEnum.MetodoSiembra => "Método de Siembra",
                EnumClass.TipoCatalogoEnum.MetodoRiego => "Método de Riego",
                EnumClass.TipoCatalogoEnum.MetodoAplicacion => "Método de Aplicación",
                EnumClass.TipoCatalogoEnum.Maquinaria => "Maquinaria",
                EnumClass.TipoCatalogoEnum.FuenteAgua => "Fuente de Agua",
                EnumClass.TipoCatalogoEnum.Laboratorio => "Laboratorio",
                EnumClass.TipoCatalogoEnum.Otro => "Otro",
                _ => Tipo.ToString()
            };
        }
    }
}
