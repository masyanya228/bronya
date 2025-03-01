namespace Bronya.Dtos
{
    public class StreamFileIdDto
    {
        public string FileId { get; set; }

        public StreamFileIdDto(string fileId)
        {
            FileId = fileId;
        }
    }
}
