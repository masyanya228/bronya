namespace Bronya.Dtos
{
    /// <summary>
    /// Для работы кэша стримов в TG Api
    /// </summary>
    public class StreamFileIdDto
    {
        public string FileId { get; set; }

        public StreamFileIdDto(string fileId)
        {
            FileId = fileId;
        }
    }
}
