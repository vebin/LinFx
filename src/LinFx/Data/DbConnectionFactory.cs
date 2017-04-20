﻿using System;
using System.Data;

namespace LinFx.Data
{
    public class DbConnectionFactory : IDbConnectionFactory, IDisposable
    {
        private DbConnection _connection;

        private DbConnection Connection => _connection ?? (_connection = new DbConnection(this));

        public DbConnectionFactory(string connectionString, IDbProvider provider)
        {
            ConnectionString = connectionString;
            DbProvider = provider;
        }

        public IDbProvider DbProvider { get; set; }

        public string ConnectionString { get; set; }

        public virtual IDbConnection OpenDbConnection()
        {
            var connection = CreateDbConnection();
            connection.Open();
            return connection;
        }

        public virtual IDbConnection CreateDbConnection()
        {
            if (ConnectionString == null)
                throw new ArgumentNullException("ConnectionString");

            var connection = new DbConnection(this);
            return connection;
        }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }

    public static class DbConnectionFactoryExtensions
    {
        /// <summary>
        /// Alias for OpenDbConnection
        /// </summary>
        public static IDbConnection Open(this IDbConnectionFactory factory)
        {
            return factory.OpenDbConnection();
        }
    }
}