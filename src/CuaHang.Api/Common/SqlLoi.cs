using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CuaHang.Api.Common;

public static class SqlLoi
{
    /// <summary>2601: vi phạm chỉ mục duy nhất. 2627: vi phạm ràng buộc duy nhất.</summary>
    public static bool LaViPhamChiMucDuyNhat(DbUpdateException ex)
        => ex.InnerException is SqlException { Number: 2601 or 2627 };
}
