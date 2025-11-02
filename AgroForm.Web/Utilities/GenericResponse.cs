namespace AgroForm.Web.Utilities
{
    public class GenericResponse<TObject>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TObject? Object { get; set; }
        public List<TObject>? ListObject { get; set; }
    }
}
