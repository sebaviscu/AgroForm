namespace AgroForm.Business.Contracts
{
    public class CampoUnidadConfigDto
    {
        public string NombreCampo { get; set; } = string.Empty;
        public string NombrePropiedad { get; set; } = string.Empty;
        public string Etiqueta { get; set; } = string.Empty;
        public bool Requerido { get; set; }
        public int? IdUnidadPredeterminada { get; set; }
        public List<UnidadPermitidaDto> Unidades { get; set; } = new();
    }
}
