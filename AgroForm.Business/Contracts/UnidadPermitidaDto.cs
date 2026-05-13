namespace AgroForm.Business.Contracts
{
    public class UnidadPermitidaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Sigla { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public decimal FactorConversion { get; set; }
        public bool EsPredeterminado { get; set; }
    }
}
