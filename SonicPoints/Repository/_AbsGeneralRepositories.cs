#region Assembly RepoBaseModelCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\LEGION\.nuget\packages\repobasemodelcore\1.0.0\lib\net7.0\RepoBaseModelCore.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace RepoBaseModelCore;

public abstract class _AbsGeneralRepositories<DBContextName, DBModelName, PrimaryKeyType> : IGeneralRepositories<DBModelName, PrimaryKeyType>, IDisposable where DBContextName : DbContext where DBModelName : class
{
    protected DBContextName _entities;

    protected DbSet<DBModelName> dbSet;

    protected string[] RelList;

    protected IQueryable<DBModelName> _Query;

    public _AbsGeneralRepositories(DBContextName context)
    {
        _entities = context;
        dbSet = _entities.Set<DBModelName>();
        _Query = dbSet.AsQueryable();
    }

    public virtual DBModelName FirstOrDefault()
    {
        return _Query.FirstOrDefault();
    }

    public virtual DBModelName GetDetail(PrimaryKeyType id)
    {
        return dbSet.Find(id);
    }

    public virtual IQueryable<DBModelName> GetList(bool IncludeDeleted = false)
    {
        GetListWithInactive(IncludeDeleted);
        FilterActive();
        return _Query;
    }

    public virtual IQueryable<DBModelName> GetListWithInactive(bool IncludeDeleted = false)
    {
        if (!IncludeDeleted)
        {
            FilterDeleted();
        }

        return _Query;
    }

    public virtual bool AddDefaultsInsert(DBModelName Data)
    {
        AddDefaultsUpdate(Data);
        return true;
    }

    public virtual bool Insert(DBModelName Data)
    {
        AddDefaultsInsert(Data);
        dbSet.Add(Data);
        return true;
    }

    public virtual bool Save()
    {
        _entities.SaveChanges();
        return true;
    }

    public virtual bool AddDefaultsUpdate(DBModelName Data)
    {
        return true;
    }

    public virtual bool Update(DBModelName Data)
    {
        AddDefaultsUpdate(Data);
        _entities.Entry(Data).State = EntityState.Modified;
        return true;
    }

    public virtual bool Delete(DBModelName Data)
    {
        _entities.Entry(Data).State = EntityState.Modified;
        return true;
    }

    public virtual IQueryable<DBModelName> FindBy(Expression<Func<DBModelName, bool>> predicate)
    {
        return _Query.Where(predicate);
    }

    public virtual async Task<DBModelName> FindAsync(PrimaryKeyType id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual void Dispose()
    {
    }

    public virtual bool Include(string RelTables)
    {
        _Query.Include(RelTables);
        return true;
    }

    public virtual bool Include(string[] RelTables)
    {
        foreach (string relTables in RelTables)
        {
            Include(relTables);
        }

        return true;
    }

    public virtual IQueryable<DBModelName> FilterDeleted()
    {
        return _Query;
    }

    public virtual IQueryable<DBModelName> FilterActive()
    {
        return _Query;
    }

    public virtual async Task<DBModelName> FirstOrDefaultAsync()
    {
        return await _Query.FirstOrDefaultAsync();
    }

    public virtual async Task<DBModelName> GetDetailAsync(PrimaryKeyType id)
    {
        return await dbSet.FindAsync(id);
    }

    public virtual async Task<bool> SaveAsync()
    {
        await _entities.SaveChangesAsync();
        return true;
    }
}
#if false // Decompilation log
'337' items in cache
------------------
Resolve: 'System.Runtime, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Runtime.dll'
------------------
Resolve: 'System.Security.Cryptography, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Security.Cryptography, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Security.Cryptography.dll'
------------------
Resolve: 'System.Linq.Expressions, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq.Expressions, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Linq.Expressions.dll'
------------------
Resolve: 'Microsoft.EntityFrameworkCore, Version=7.0.1.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.EntityFrameworkCore, Version=8.0.8.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
WARN: Version mismatch. Expected: '7.0.1.0', Got: '8.0.8.0'
Load from: 'C:\Users\LEGION\.nuget\packages\microsoft.entityframeworkcore\8.0.8\lib\net8.0\Microsoft.EntityFrameworkCore.dll'
------------------
Resolve: 'System.Collections, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Collections, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Collections.dll'
------------------
Resolve: 'System.Linq, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Linq.dll'
------------------
Resolve: 'System.Linq.Queryable, Version=7.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Linq.Queryable, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Linq.Queryable.dll'
------------------
Resolve: 'Microsoft.EntityFrameworkCore.Abstractions, Version=8.0.8.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Found single assembly: 'Microsoft.EntityFrameworkCore.Abstractions, Version=8.0.8.0, Culture=neutral, PublicKeyToken=adb9793829ddae60'
Load from: 'C:\Users\LEGION\.nuget\packages\microsoft.entityframeworkcore.abstractions\8.0.8\lib\net8.0\Microsoft.EntityFrameworkCore.Abstractions.dll'
------------------
Resolve: 'System.Runtime.InteropServices, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.InteropServices, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Runtime.InteropServices.dll'
------------------
Resolve: 'System.Runtime.CompilerServices.Unsafe, Version=7.0.0.0, Culture=neutral, PublicKeyToken=null'
Found single assembly: 'System.Runtime.CompilerServices.Unsafe, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
WARN: Version mismatch. Expected: '7.0.0.0', Got: '8.0.0.0'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Runtime.CompilerServices.Unsafe.dll'
------------------
Resolve: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Found single assembly: 'System.Runtime, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Load from: 'C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\8.0.11\ref\net8.0\System.Runtime.dll'
#endif
