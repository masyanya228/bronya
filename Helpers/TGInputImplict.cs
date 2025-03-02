using Bronya.Caching.Structure;
using Bronya.Dtos;

using Telegram.Bot.Types;
using Telegram.Bot.Types.InputFiles;

namespace Bronya.Helpers
{
    public class TGInputImplict
    {
        public Stream Stream { get; set; }

        public string FileId { get; set; }

        public TGInputImplict(Stream stream)
        {
            Stream = stream;
        }

        public TGInputImplict(string fileId)
        {
            FileId = fileId;
        }

        public static implicit operator TGInputImplict(Stream stream) =>
            stream is null ? default : new(stream);

        public static implicit operator TGInputImplict(string value) =>
            value is null ? default : new(value);

        public InputOnlineFile GetInputOnlineFile(ICacheService<StreamFileIdDto> streamFilesCacheService, string hash)
        {
            CheckInCache(streamFilesCacheService, hash);
            return FileId != default ? FileId : Stream;
        }

        public InputMedia GetInputMedia(ICacheService<StreamFileIdDto> streamFilesCacheService, string hash)
        {
            CheckInCache(streamFilesCacheService, hash);
            return FileId != default ? FileId : new InputMedia(Stream, "Картинка");
        }

        private void CheckInCache(ICacheService<StreamFileIdDto> streamFilesCacheService, string hash)
        {
            if (Stream != default && hash != default)
            {
                var exist = streamFilesCacheService.Get(null, hash);
                FileId = exist?.FileId;
                Stream.Position = 0;
            }
        }
    }
}
