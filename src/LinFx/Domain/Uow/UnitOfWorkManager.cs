﻿using LinFx.Unity;
using System.Transactions;

namespace LinFx.Domain.Uow
{
	/// <summary>
	/// Unit of work manager.
	/// Used to begin and control a unit of work.
	/// </summary>
	public interface IUnitOfWorkManager
    {
        /// <summary>
        /// Gets currently active unit of work (or null if not exists).
        /// </summary>
        IActiveUnitOfWork Current { get; }
        /// <summary>
        /// Begins a new unit of work.
        /// </summary>
        /// <returns>A handle to be able to complete the unit of work</returns>
        IUnitOfWorkCompleteHandle Begin();
        /// <summary>
        /// Begins a new unit of work.
        /// </summary>
        /// <returns>A handle to be able to complete the unit of work</returns>
        IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope);
        /// <summary>
        /// Begins a new unit of work.
        /// </summary>
        /// <returns>A handle to be able to complete the unit of work</returns>
        IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options);
    }

    public class UnitOfWorkManager : IUnitOfWorkManager
    {
		private readonly IUnity _iocResolver;
		private readonly ICurrentUnitOfWorkProvider _currentUnitOfWorkProvider;
		private readonly IUnitOfWorkDefaultOptions _defaultOptions;

		public UnitOfWorkManager(
			IUnity iocResolver,
			ICurrentUnitOfWorkProvider currentUnitOfWorkProvider,
			IUnitOfWorkDefaultOptions defaultOptions)
		{
			_iocResolver = iocResolver;
			_currentUnitOfWorkProvider = currentUnitOfWorkProvider;
			_defaultOptions = defaultOptions;
		}

		public IActiveUnitOfWork Current
        {
            get { return _currentUnitOfWorkProvider.Current; }
        }

		public IUnitOfWorkCompleteHandle Begin()
		{
			return Begin(new UnitOfWorkOptions());
		}

		public IUnitOfWorkCompleteHandle Begin(TransactionScopeOption scope)
		{
			return Begin(new UnitOfWorkOptions { Scope = scope });
		}

		public IUnitOfWorkCompleteHandle Begin(UnitOfWorkOptions options)
		{
			//options.FillDefaultsForNonProvidedOptions(_defaultOptions);

			var outerUow = _currentUnitOfWorkProvider.Current;

			if(options.Scope == TransactionScopeOption.Required && outerUow != null)
			{
				return new InnerUnitOfWorkCompleteHandle();
			}

			var uow = _iocResolver.Resolve<IUnitOfWork>();

			uow.Completed += (sender, args) =>
			{
				_currentUnitOfWorkProvider.Current = null;
			};

			uow.Failed += (sender, args) =>
			{
				_currentUnitOfWorkProvider.Current = null;
			};

			uow.Disposed += (sender, args) =>
			{
				//_iocResolver.Release(uow);
			};

			//Inherit filters from outer UOW
			if(outerUow != null)
			{
				//options.FillOuterUowFiltersForNonProvidedOptions(outerUow.Filters.ToList());
			}

			uow.Begin(options);

			//Inherit tenant from outer UOW
			if(outerUow != null)
			{
				//uow.SetTenantId(outerUow.GetTenantId(), false);
			}

			_currentUnitOfWorkProvider.Current = uow;

			return uow;
		}
	}
}
