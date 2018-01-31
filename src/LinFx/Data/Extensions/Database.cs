﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using LinFx.Data.Extensions.Mapper;
using LinFx.Data.Extensions.Sql;
using LinFx.Data.Filters.Query;
using Dapper;

namespace LinFx.Data.Extensions
{
	public interface IDatabase : IDisposable
    {
        IDbConnection Connection { get; }
		int? CommandTimeout { get; set; }
		void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
        void Commit();
        void Rollback();
        void RunInTransaction(Action action);
        T RunInTransaction<T>(Func<T> func);
		bool HasActiveTransaction { get; }
		IDbTransaction Transaction { get; }

		void Insert<TEntity>(TEntity[] entities, int? commandTimeout = null) where TEntity : class;
		dynamic Insert<TEntity>(TEntity entity, int? commandTimeout = null) where TEntity : class;
		bool Update<T>(T entity, int? commandTimeout = null) where T : class;
        bool Delete<T>(T entity, int? commandTimeout = null) where T : class;
        bool Delete<TEntity>(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null) where TEntity : class;

		int Count<TEntity>(Expression<Func<TEntity, bool>> predicate = null) where TEntity : class;
		TEntity Get<TEntity>(dynamic id, int? commandTimeout = null) where TEntity : class;
		TEntity Get<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;
		IEnumerable<TEntity> Select<TEntity>(Expression<Func<TEntity, bool>> predicate = null, Paging paging = null, params Expression<Func<TEntity, object>>[] sort) where TEntity : class;
		IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, uint firstResult, uint maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class;
        IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, uint firstResult, uint maxResults, int? commandTimeout, bool buffered) where T : class;
        IMultipleResultReader GetMultiple(MultiplePredicate predicate, int? commandTimeout = null);

        void ClearCache();
        Guid GetNextGuid();
        IClassMapper GetMap<T>() where T : class;
    }

    public class Database : IDatabase
    {
        private readonly IDapperImplementor _dapper;
		private IDbTransaction _transaction;
		private static QueryFilterExecuter _dapperFilterExecuter = new QueryFilterExecuter();

        public IDbConnection Connection { get; private set; }

        public Database(IDbConnection connection, ISqlGenerator sqlGenerator)
        {
            _dapper = new DapperImplementor(sqlGenerator);
            Connection = connection;

            if (Connection.State != ConnectionState.Open)
                Connection.Open();
        }

        public bool HasActiveTransaction
        {
            get { return _transaction != null; }
        }

		public IDbTransaction Transaction
		{
			get { return _transaction; }
		}

		public int? CommandTimeout { get; set; }

		public void Dispose()
        {
            if (Connection.State != ConnectionState.Closed)
            {
                if (_transaction != null)
                    _transaction.Rollback();
                Connection.Close();
            }
        }

        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            _transaction = Connection.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void Rollback()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        public void RunInTransaction(Action action)
        {
            BeginTransaction();
            try
            {
                action();
                Commit();
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                    Rollback();

                throw ex;
            }
        }

        public T RunInTransaction<T>(Func<T> func)
        {
            BeginTransaction();
            try
            {
                T result = func();
                Commit();
                return result;
            }
            catch (Exception ex)
            {
                if (HasActiveTransaction)
                    Rollback();

                throw ex;
            }
        }

		public TEntity Get<TEntity>(dynamic id, int? commandTimeout) where TEntity : class
		{
			return _dapper.Get<TEntity>(Connection, id, _transaction, commandTimeout);
		}

		public TEntity Get<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
		{
			return Select(predicate, null).FirstOrDefault();
		}

		public void Insert<T>(T[] entities, int? commandTimeout) where T : class
		{
			_dapper.Insert(Connection, entities, _transaction, commandTimeout);
		}

		public dynamic Insert<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Insert(Connection, entity, _transaction, commandTimeout);
        }

		public bool Update<TEntity>(TEntity entity, int? commandTimeout) where TEntity : class
        {
            return _dapper.Update(Connection, entity, _transaction, commandTimeout);
        }

        public bool Delete<T>(T entity, int? commandTimeout) where T : class
        {
            return _dapper.Delete(Connection, entity, _transaction, commandTimeout);
        }

		public bool Delete<TEntity>(Expression<Func<TEntity, bool>> predicate, int? commandTimeout) where TEntity : class
		{
			var filteredPredicate = _dapperFilterExecuter.ExecuteFilter(predicate);
			return _dapper.Delete<TEntity>(Connection, filteredPredicate, _transaction, commandTimeout);
		}

		public IEnumerable<TEntity> Select<TEntity>(Expression<Func<TEntity, bool>> predicate, Paging paging, params Expression<Func<TEntity, object>>[] sort) where TEntity : class
		{
			var filteredPredicate = _dapperFilterExecuter.ExecuteFilter(predicate);
			return _dapper.Select<TEntity>(Connection, filteredPredicate, paging, sort.ToSorting());
		}

		public IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, uint firstResult, uint maxResults, IDbTransaction transaction, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, transaction, commandTimeout, buffered);
        }

        public IEnumerable<T> GetSet<T>(object predicate, IList<ISort> sort, uint firstResult, uint maxResults, int? commandTimeout, bool buffered) where T : class
        {
            return _dapper.GetSet<T>(Connection, predicate, sort, firstResult, maxResults, _transaction, commandTimeout, buffered);
        }

        public int Count<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
			var filteredPredicate = _dapperFilterExecuter.ExecuteFilter(predicate);
			return _dapper.Count<TEntity>(Connection, filteredPredicate, _transaction);
        }

        public IMultipleResultReader GetMultiple(MultiplePredicate predicate, int? commandTimeout)
        {
            return _dapper.GetMultiple(Connection, predicate, _transaction, commandTimeout);
        }

        public void ClearCache()
        {
            _dapper.SqlGenerator.Configuration.ClearCache();
        }

        public Guid GetNextGuid()
        {
            return _dapper.SqlGenerator.Configuration.GetNextGuid();
        }

        public IClassMapper GetMap<T>() where T : class
        {
            return _dapper.SqlGenerator.Configuration.GetMap<T>();
        }
	}

	public static class DatabaseExtensions
	{
        public static T FirstOrDefault<T>(this IDatabase db, Expression<Func<T, bool>> predicate = null) where T : class
        {
            return db.Select(predicate).FirstOrDefault();
        }

        public static int Execute(this IDatabase db, string sql, object param, CommandType? commandType = null)
		{
			return db.Connection.Execute(sql, param, db.Transaction, db.CommandTimeout, commandType);
		}

		public static T ExecuteScalar<T>(this IDatabase db, string sql, object param = null, CommandType? commandType = null)
		{
			return db.Connection.ExecuteScalar<T>(sql, param, db.Transaction, db.CommandTimeout, commandType);
		}

		public static IEnumerable<T> Query<T>(this IDatabase db, string sql, object param = null, CommandType? commandType = null)
		{
			return db.Connection.Query<T>(sql, param, db.Transaction, true, db.CommandTimeout, commandType);
		}

		public static IEnumerable<T> Query<T1, T2, T>(this IDatabase db, string sql, Func<T1, T2, T> map, object param = null, CommandType? commandType = null, string splitOn = "Id")
		{
			return db.Connection.Query(sql, map, param, db.Transaction, splitOn: splitOn);
		}

		public static IEnumerable<T> Query<T1, T2, T3, T>(this IDatabase db, string sql, Func<T1, T2, T3, T> map, object param = null, CommandType? commandType = null, string splitOn = "Id")
		{
			return db.Connection.Query(sql, map, param, db.Transaction, splitOn: splitOn);
		}

		public static T QueryFirstOrDefault<T>(this IDatabase db, string sql, object param = null, CommandType? commandType = null)
		{
			return db.Connection.QueryFirstOrDefault<T>(sql, param, db.Transaction, db.CommandTimeout, commandType);
		}

		public static IMultipleResultReader QueryMultiple(this IDatabase db, string sql, object param = null, CommandType? commandType = null)
		{
			var result = db.Connection.QueryMultiple(sql, param, db.Transaction, db.CommandTimeout, commandType);
			return new GridReaderResultReader(result);
		}
	}
}