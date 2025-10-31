namespace AgroForm.Web.Models
{
    public class ProveedorVM : EntityBaseWithLicenciaVM
    {
        public string RazonSocial { get; set; } = string.Empty;
        public string? NombreFantasia { get; set; }
        public string? CUIT { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Provincia { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? NombreContacto { get; set; }
        public string? Observaciones { get; set; }

        public bool Activo { get; set; } = true;
    }
}
