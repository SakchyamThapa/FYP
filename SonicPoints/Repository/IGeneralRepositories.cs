#region Assembly RepoBaseModelCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\LEGION\.nuget\packages\repobasemodelcore\1.0.0\lib\net7.0\RepoBaseModelCore.dll
// Decompiled with ICSharpCode.Decompiler 8.1.1.7464
#endregion

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace RepoBaseModelCore;

public interface IGeneralRepositories<Entity, PKType> : IDisposable where Entity : class
{
    bool Include(string RelTables);

    bool Include(string[] RelTables);

    IQueryable<Entity> FilterDeleted();

    IQueryable<Entity> FilterActive();

    IQueryable<Entity> GetList(bool IncludeDeleted = false);

    IQueryable<Entity> GetListWithInactive(bool IncludeDeleted = false);

    IQueryable<Entity> FindBy(Expression<Func<Entity, bool>> predicate);

    Task<Entity> FindAsync(PKType id);

    Entity FirstOrDefault();

    Entity GetDetail(PKType id);

    bool AddDefaultsInsert(Entity Data);

    bool Insert(Entity Data);

    bool AddDefaultsUpdate(Entity Data);

    bool Update(Entity Data);

    bool Save();

    bool Delete(Entity Data);

    Task<Entity> FirstOrDefaultAsync();

    Task<Entity> GetDetailAsync(PKType id);

    Task<bool> SaveAsync();
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
