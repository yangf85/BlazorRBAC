# 常见问题 FAQ

## Day 2 - 项目搭建

### Q1: API 和 Web 项目都要配置 HTTPS 吗？

**问题**：开发环境是否需要 HTTPS？

**答案**：推荐都配置 HTTPS
- ✅ 开发生产环境一致
- ✅ 避免混合内容错误（HTTPS 调用 HTTP 会被阻止）
- ✅ Blazor Server 的 SignalR 在 HTTPS 下更稳定

**配置方式**：
```json
// launchSettings.json
"applicationUrl": "https://localhost:7129;http://localhost:5129"
```

**信任证书**：
```bash
dotnet dev-certs https --trust
```

---

### Q2: 启动时浏览器没有自动打开？

**原因**：`launchSettings.json` 中 `launchBrowser: false`

**解决**：修改配置
```json
{
  "https": {
    "launchBrowser": true,
    "launchUrl": "scalar/v1",  // 指定启动页面
    "applicationUrl": "https://localhost:7129"
  }
}
```

---

### Q3: PostgreSQL 连接失败，密码错误？

**错误信息**：
```
No password has been provided but the backend requires one
```

**解决方法**：

**方法1**：尝试常见密码
- `postgres`
- `123456`
- `admin`

**方法2**：pgAdmin 中查看保存的密码
- 右键服务器 → 属性 → Connection

**方法3**：重置密码（修改 pg_hba.conf）
```
# 临时改为 trust
host    all    all    127.0.0.1/32    trust

# 重启服务，用 psql 修改密码
ALTER USER postgres PASSWORD 'newpassword';

# 改回 scram-sha-256，再次重启
```

---

### Q4: 文件夹命名与 NuGet 包冲突？

**问题**：创建 `FreeSql` 文件夹导致命名空间混淆

**错误示例**：
```
Infrastructure/
└── FreeSql/          ← 和 FreeSql NuGet 包冲突
    └── FreeSqlSetup.cs
```

**正确做法**：使用语义化命名
```
Infrastructure/
└── Database/         ← 避免冲突，语义清晰
    └── FreeSqlSetup.cs
```

**规则**：避免用 NuGet 包同名作为文件夹名

---

### Q5: FluentValidation.AspNetCore 已过时？

**问题**：安装时提示包已废弃

**原因**：FluentValidation 11.0+ 不再需要 AspNetCore 扩展包

**正确安装**：
```
✅ FluentValidation
✅ FluentValidation.DependencyInjectionExtensions
❌ FluentValidation.AspNetCore (不需要)
```

**手动注册验证器**：
```csharp
services.AddValidatorsFromAssemblyContaining<LoginValidator>();
```

---

### Q6: Scalar API 文档主题不生效？

**问题**：配置主题后界面没变化

**可能原因**：
- Scalar.AspNetCore 版本不支持
- 配置方式不正确

**简化方案**：使用默认主题
```csharp
app.MapScalarApiReference(options =>
{
    options.WithTitle("BlazorRBAC API");
    // 主题功能可选，不影响核心功能
});
```

---

### Q7: StackOverflowException 无限递归？

**错误代码**：
```csharp
public static WebApplication UseSerilogRequestLogging(this WebApplication app)
{
    app.UseSerilogRequestLogging();  // ← 调用了自己！
    return app;
}
```

**原因**：方法名和 Serilog 扩展方法重名，导致递归

**解决方案1**：改方法名
```csharp
public static WebApplication UseRequestLogging(this WebApplication app)
{
    app.UseSerilogRequestLogging();  // ← 调用 Serilog 的方法
    return app;
}
```

**解决方案2**：不包装，直接使用
```csharp
// Program.cs 中直接调用 Serilog 提供的方法
app.UseSerilogRequestLogging();
```

---

### Q8: 访问 https://localhost:7129/ 显示 404？

**原因**：根路径没有配置任何端点

**解决**：访问具体的 API 路径
```
✅ https://localhost:7129/api/test/ping
✅ https://localhost:7129/scalar/v1
❌ https://localhost:7129/
```

---

### Q9: 扩展方法太多，Program.cs 会不会越来越乱？

**不会**：这正是扩展方法模式的优势

**架构原则**：
- ServiceCollectionExtensions：服务注册（`Add*`）
- ApplicationBuilderExtensions：中间件配置（`Use*`）
- 专用扩展类：如 SerilogExtensions

**Program.cs 始终保持简洁**：
```csharp
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddApiDocumentation();
app.UseApiDocumentation();
```

---

## 通用问题

### Q10: 如何查看项目使用的端口？

**方法1**：查看 `launchSettings.json`
```json
"applicationUrl": "https://localhost:7129;http://localhost:5129"
```

**方法2**：查看控制台输出
```
Now listening on: https://localhost:7129
```

---

### Q11: logs 文件夹需要手动创建吗？

**不需要**：Serilog 第一次写入时会自动创建

**日志位置**：
```
BlazorRBAC.Api/
└── logs/
    └── app-20241218.log
```

---

### Q12: 为什么不用顶级语句（Top-level statements）？

**原因**：保持传统的 Main 方法结构更清晰

**顶级语句**：
```csharp
var builder = WebApplication.CreateBuilder(args);
// ...
```

**Main 方法**：
```csharp
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        // ...
    }
}
```

两种都可以，看个人偏好。本项目使用 Main 方法。

---

## 调试技巧

### 1. 查看 Serilog 日志
```
控制台：实时查看
文件：logs/app-yyyyMMdd.log
```

### 2. 测试 API 接口
```
Scalar：https://localhost:7129/scalar/v1
直接访问：https://localhost:7129/api/test/ping
```

### 3. 检查数据库连接
```csharp
// TestController
[HttpGet("db-connection")]
public IActionResult TestDatabaseConnection()
{
    var version = _fsql.Ado.ExecuteScalar("SELECT version()");
    return Ok(version);
}
```

---

**更新日期**：2024-12-18  
**适用范围**：Day 2 项目搭建阶段