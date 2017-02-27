using SiteParser.DAL.Models;
using System;
using System.Collections.Generic;

namespace SiteParser.DAL.Interfaces
{
    public interface IRepository<T>
         where T : Entity
    {
        void AddRange(IEnumerable<T> entities);

        List<T> GetRange(Func<T, bool> condition);

        void Add(T entity);

        bool TryAdd(T entity);

        T Get(string name);

        void Delete(int id);

        void DeleteAll();
    }
}
