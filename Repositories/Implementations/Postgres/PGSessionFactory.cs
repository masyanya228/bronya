﻿using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernate;

using NHibernate.Cfg;
using NHibernate.Dialect;
using Buratino.Xtensions;
using Bronya.Maps.NHibMaps;

namespace Bronya.Repositories.Implementations.Postgres
{
    public class PGSessionFactory : IPGSessionFactory
    {
        private ISessionFactory sessionFactory;

        public ISessionFactory SessionFactory
        {
            get
            {
                sessionFactory ??= CreateSessionFactory();

                return sessionFactory;
            }
            set => sessionFactory = value;
        }

        public IConfiguration Configuration { get; }
        public string Host { get; }
        public int Port { get; }
        public string Database { get; }
        public string Username { get; }
        public string Password { get; }

        public PGSessionFactory(IConfiguration configuration)
        {
            Configuration = configuration;
            Host = Configuration.GetValue("host", "localhost");
            Port = Configuration.GetValue("port", 5433);
            Database = Configuration.GetValue("database", "bronya_thegreenplace");
            Username = Configuration.GetValue("username", "postgres");
            Password = Configuration.GetValue("password", "postgres");
            Console.Title += $"[{Host}:{Port} DB:{Database}]";
        }

        private ISessionFactory CreateSessionFactory()
        {
            var db = Fluently
                .Configure()
                    .Database(
                        PostgreSQLConfiguration.Standard
                        .ConnectionString(c =>
                        {
                            c.Host(Host)
                            .Port(Port)
                            .Database(Database)
                            .Username(Username)
                            .Password(Password);
                        })
                        .Dialect<PostgreSQL82Dialect>());

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
                using var file = new FileStream(@"update.sql", FileMode.Append, FileAccess.Write);
                using var sw = new StreamWriter(file);
                sw.Write(x);
                sw.Close();
            };
            var update = new SchemaUpdate(configuration);
            update.Execute(updateExport, true);
        }
    }
}
