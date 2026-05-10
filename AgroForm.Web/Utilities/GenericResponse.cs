namespace AgroForm.Web.Utilities
{
    public class GenericResponse<TObject>
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public TObject? Object { get; set; }
        public List<TObject>? ListObject { get; set; }

        public override string ToString()
        {
            var result = $"GenericResponse<{typeof(TObject).Name}>: Success={Success}";
            
            if (!string.IsNullOrEmpty(Message))
                result += $", Message=\"{Message}\"";
            
            if (Object != null)
                result += $", Object={Object}";
            
            if (ListObject != null)
                result += $", ListObject.Count={ListObject.Count}";
            
            return result;
        }
    }
}
