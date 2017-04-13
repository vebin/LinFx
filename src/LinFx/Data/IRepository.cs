﻿using LinFx.Domain.Entities;
using System.Threading.Tasks;

namespace LinFx.Data
{
    /// <summary>
    /// This interface is implemented by all repositories to ensure implementation of fixed methods.
    /// </summary>
    /// <typeparam name="TEntity">Main Entity type this repository works on</typeparam>
    /// <typeparam name="TPrimaryKey">Primary key type of the entity</typeparam>
    public interface IRepository<TEntity, TPrimaryKey> where TEntity : IEntity<TPrimaryKey>
    {
        #region 查询

        /// <summary>
        /// Gets an entity with given primary key.
        /// </summary>
        /// <param name="id">Primary key of the entity to get</param>
        /// <returns>Entity</returns>
        Task<TEntity> GetAsync(TPrimaryKey id);

        /// <summary>
        /// Gets an entity with given primary key or null if not found.
        /// </summary>
        /// <param name="id">Primary key of the entity to get</param>
        /// <returns>Entity or null</returns>
        Task<TEntity> FirstOrDefaultAsync(TPrimaryKey id);

        #endregion

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="item"></param>
        void InsertAsync(TEntity item);

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="item"></param>
        void UpdateAsync(TEntity item);

        /// <summary>
        /// Deletes an entity by primary key.
        /// </summary>
        /// <param name="id"></param>
        void DeleteAsync(TPrimaryKey id);

        /// <summary>
        /// Deletes an entity.
        /// </summary>
        /// <param name="item"></param>
        void DeleteAsync(TEntity item);
    }

    /// <summary>
    //     A shortcut of IRepository`2 for most used primary key
    //     type (System.String).
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public interface IRepository<TEntity> : IRepository<TEntity, string> where TEntity : IEntity<string>
    {
    }
}