using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BookShoppingProject.DataAccess.Repository.IRepository
{
   public interface IRepository<T> where T:class
    {
        T Get(int Id);

        IEnumerable<T> GetAll(
            Expression<Func<T,bool>> filter=null,
            Func<IQueryable<T>,IOrderedQueryable<T>> orderBy=null,
            string includeProperties=null

            );

        T FirstorDefault(
            Expression<Func<T,bool>> filter=null,
            string includeProperties=null
            );

        void Add(T entity);
        void Remove(T entity);
        void Remove(int Id);

        void RemoveRange(IEnumerable<T> entity);

    }
}
