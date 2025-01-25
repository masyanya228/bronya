using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate;

using NHibernate.Cfg;
using NHibernate.Dialect;
using Buratino.Xtensions;
using Buratino.Maps.NHibMaps;

namespace Buratino.Repositories.Implementations.Postgres
{
    public class PGSessionFactory : IPGSessionFactory
    {
        private ISessionFactory sessionFactory;

        public ISessionFactory SessionFactory
        {
            get
            {
                if (sessionFactory == null)
                    sessionFactory = CreateSessionFactory();

                return sessionFactory;
            }
            set => sessionFactory = value;
        }

        private ISessionFactory CreateSessionFactory()
        {
            var db = Fluently
                .Configure()
                    .Database(
                        PostgreSQLConfiguration.Standard
                        .ConnectionString(c =>
                            c.Host("localhost")
                            .Port(5433)
                            .Database("bronya_thegreenplace")
                            .Username("postgres")
                            .Password("007007Qq"))
                        .Dialect<PostgreSQL82Dialect>()
                        .ShowSql());

            var mappings = typeof(INHMap).GetImplementations();
            foreach (var item in mappings)
            {
                db.Mappings(x => x.FluentMappings.Add(item));
            }

            return db.ExposeConfiguration(TreatConfiguration)
                .BuildSessionFactory();
        }

        private void TreatConfiguration(Configuration configuration)
        {
            Action<string> updateExport = x =>
            {
                using (var file = new FileStream(@"update.sql", FileMode.Append, FileAccess.Write))
                using (var sw = new StreamWriter(file))
                {
                    sw.Write(x);
                    sw.Close();
                }
            };
            var update = new SchemaUpdate(configuration);
            update.Execute(updateExport, true);
        }
    }
}
