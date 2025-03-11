namespace ArtGallery.Models.GeneralService
{
    public class GeneralService
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public object Data {  get; set; }
    }
}
