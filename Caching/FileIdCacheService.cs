using Bronya.Caching.Structure;
using Bronya.Dtos;

namespace Bronya.Caching
{
    /// <summary>
    /// Пара Hash:FileId
    /// </summary>
    public class FileIdCacheService : InMemoryCacheService<StreamFileIdDto>
    {
        public FileIdCacheService()
        {
            TimeExpiration = TimeSpan.FromMinutes(1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public override object Remove(StreamFileIdDto hash)
        {
            return Cache.Remove(hash.FileId);
        }

        public override string GetKey(StreamFileIdDto fileId)//todo
        {
            return fileId.FileId;
        }
    }
}
