namespace AgroForm.Model.Unidades
{
    public class CampoLaborUnidadPermitida
    {
        public int Id { get; set; }
        public int IdCampoLaborUnidad { get; set; }
        public int IdUnidadMedida { get; set; }
        public bool EsPredeterminado { get; set; }
        public int Orden { get; set; }

        public CampoLaborUnidad CampoLaborUnidad { get; set; } = null!;
        public UnidadMedida UnidadMedida { get; set; } = null!;
    }
}
