using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Andux.Core.TenantTesting.Application.Services.Request
{
    /// <summary>
    /// 修改用户请求
    /// </summary>
    public class UpdateUserRequest
    {
        public string Name { get; set; } = default!;
    }
}
