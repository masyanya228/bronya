using Bronya.Entities;
using Bronya.Entities.Abstractions;
using Bronya.Services;

using System.Linq.Expressions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public class PersistentDomainService<T> : DomainServiceBase<T> where T : PersistentEntity
    {
        /// <summary>
        /// Убирает из выборки удаленные объекты
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public override IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            //if (filter != null)
            //{
            //    Expression<Func<T, bool>> notDeleted = x => x.IsDeleted == false;
            //    var notDeletedC = notDeleted.Compile();
            //    var filterC = filter.Compile();
            //    filter = c => notDeletedC(c) && filterC(c);
            //}
            return Repository.GetAll(filter).Where(x => !x.IsDeleted);
        }

        /// <summary>
        /// Мягкое удаление с указанием аккаунта и метки времени
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override bool Delete(T entity)
        {
            entity.IsDeleted = true;
            entity.DeletedStamp = new TimeService().GetNow();
            entity.WhoDeleted = Account;
            return base.Save(entity) != default;
        }

        /// <summary>
        /// Мягкое удаление
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public override bool Delete(Guid guid)
        {
            return Delete(base.Get(guid));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override T Save(T entity)
        {
            if (entity.Id != Guid.Empty)
            {
                entity.WhoUpdated = Account;
                entity.UpdatedStamp = new TimeService().GetNow();
                return base.Save(entity);
            }
            else
            {
                entity.Account = Account;
                entity.TimeStamp = new TimeService().GetNow();
                return base.Save(entity);
            }
        }
    }
}
