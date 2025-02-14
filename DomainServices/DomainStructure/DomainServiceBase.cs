using System.Linq.Expressions;
using System.Reflection;

using Bronya.Dtos;
using Bronya.Entities;

using Buratino.DI;
using Buratino.Entities.Abstractions;
using Buratino.Repositories.RepositoryStructure;
using Buratino.Xtensions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public class DomainServiceBase<T> : IDomainService<T> where T : IEntityBase
    {
        public virtual IRepository<T> Repository { get; set; }
        public virtual Account Account { get; set; }

        protected DomainServiceBase()
        {
            Repository = Container.GetRepository<T>();
            //DInject();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null)
        {
            return Repository.GetAll(filter);
        }

        public virtual QueryableSession<T> GetAllQuery()
        {
            return Repository.GetAllQuery();
        }

        public virtual T Get(Guid id)
        {
            return Repository.Get(id);
        }

        public virtual T Save(T entity)
        {
            return entity.Id != Guid.Empty
                ? Repository.Update(entity)
                : Repository.Insert(entity);
        }

        public virtual T CascadeSave(T entity)
        {
            foreach (var item in entity.GetType().GetProperties())
            {
                if (item.PropertyType.IsImplementationOfClass(typeof(IEntityBase)))
                {
                    object value = item.GetValue(entity);
                    if (value == null)
                    {
                        continue;
                    }

                    var subDomain = Container.GetDomainService(item.PropertyType, Account);
                    subDomain.InvokeMethod("Save", new object[] { value });
                }
            }
            return Save(entity);
        }

        public virtual bool Delete(T entity)
        {
            return Delete(entity.Id);
        }

        public virtual bool Delete(Guid id)
        {
            return Repository.Delete(id);
        }

        protected void DInject()
        {
            if (!Container.IsReady)
                throw new ArgumentNullException("Платформа еще не запустилась. Этот метод можно вызывать после окончания конфигурации платформы.");
            var allProps = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var allowedProps = allProps.Where(x => x.PropertyType.IsInterface /*&& x.DeclaringType == GetType()*/ && x.GetValue(this) == null).ToArray();
            var res = allowedProps.ActionAll(x => x.SetValue(this, Container._serviceProvider.GetService(x.PropertyType)));
        }
    }
}
