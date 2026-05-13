namespace AgroForm.Model.Unidades
{
    public class UnidadMedida
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;
        public CategoriaUnidad Categoria { get; set; }
        public DimensionBase DimensionBase { get; set; }
        public decimal FactorConversion { get; set; } = 1m;
        public int? Orden { get; set; }
        public bool Activo { get; set; } = true;
    }
}
