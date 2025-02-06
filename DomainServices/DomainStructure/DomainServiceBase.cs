using System.Reflection;

using Buratino.DI;
using Buratino.Entities.Abstractions;
using Buratino.Repositories.RepositoryStructure;
using Buratino.Xtensions;

namespace Buratino.Models.DomainService.DomainStructure
{
    public class DomainServiceBase<T> : IDomainService<T> where T : IEntityBase
    {
        public virtual IRepository<T> Repository { get; set; }

        protected DomainServiceBase()
        {
            Repository = Container.GetRepository<T>();
            //DInject();
        }

        public virtual IQueryable<T> GetAll()
        {
            return Repository.GetAll();
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

                    var subDomain = Container.GetDomainService(item.PropertyType);
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
