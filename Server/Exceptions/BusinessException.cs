using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace blazorTest.Server.Exceptions
{
    /// <summary>
    /// ビジネス例外の基底クラス。
    /// </summary>
    /// <remarks>
    /// ビジネスロジック上の例外を扱う場合に使います。
    /// ユーザーに明確に伝えるべき異常はこの例外を継承してthrowします。
    /// </remarks>
    public abstract class BusinessException : Exception
    {
    }
}
