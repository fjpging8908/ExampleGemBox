namespace ExapleGembox.Data.Dto
{
    public class FileDto
    {
        public string FileName { get; set; }

        public byte[] FileContent { get; set; }

        public string ContentType { get; set; }
    }
}