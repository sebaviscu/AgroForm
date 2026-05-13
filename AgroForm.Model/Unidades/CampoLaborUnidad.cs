namespace AgroForm.Model.Unidades
{
    public class CampoLaborUnidad
    {
        public int Id { get; set; }
        public int IdTipoActividad { get; set; }
        public string NombreCampo { get; set; } = string.Empty;
        public string NombrePropiedad { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public bool Requerido { get; set; } = false;
        public int? Orden { get; set; }
        public bool Activo { get; set; } = true;

        public TipoActividad TipoActividad { get; set; } = null!;
        public ICollection<CampoLaborUnidadPermitida> UnidadesPermitidas { get; set; } = new List<CampoLaborUnidadPermitida>();
    }
}
