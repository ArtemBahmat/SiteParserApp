using SiteParserCore.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteParserCore.Interfaces
{
    public interface IRepository2<T>
         where T : Entity
    {
        void Add(T entity);

        T Get(string name);
    }
}
