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

## Day 3 - 实体与数据初始化

### Q1: 主键冲突错误（duplicate key）？

**错误信息**：
```
23505: duplicate key value violates unique constraint "Role_pkey"
```

**原因**：BaseEntity 的 Id 字段没有配置为自增

**错误代码**：
```csharp
[Column(Position = 1)]
public int Id { get; set; }  // ❌ 缺少 IsIdentity
```

**解决方案**：
```csharp
[Column(Position = 1, IsIdentity = true)]  // ✅ 添加自增标识
public int Id { get; set; }
```

**验证方法**：
```sql
-- 查看表结构
\d "User"

-- 应该看到：
-- "Id" SERIAL PRIMARY KEY
```

---

### Q2: 为什么要用 int 而不是 long 作为主键？

**对比**：
| 类型 | 范围 | 占用 | 场景 |
|------|------|------|------|
| int | 21亿 | 4字节 | RBAC系统（推荐）|
| long | 922亿亿 | 8字节 | 日志表、订单表 |

**推荐 int 的理由**：
- ✅ RBAC 系统永远用不到 21 亿条记录
- ✅ 索引体积小 50%，查询更快
- ✅ 节省存储和传输成本

**什么时候用 long**：
- 增长极快的日志表
- 大型电商订单表
- IoT 设备数据表

---

### Q3: 是否需要给 LoginType 使用枚举？

**字符串 vs 枚举对比**：

| 方案 | 优点 | 缺点 |
|------|------|------|
| 字符串 | 灵活，易扩展 | 易拼写错误，无智能提示 |
| 枚举（存整数） | 类型安全，性能好 | 数据库不直观（存1、2、3）|
| 枚举（存字符串）| 兼顾两者 | 需配置 MapType |

**推荐方案**：枚举 + 存字符串
```csharp
// 1. 定义枚举
public enum LoginType { Local, WeChat }

// 2. 实体中配置
[Column(MapType = typeof(string))]
public LoginType LoginType { get; set; }

// 数据库存储："Local" 或 "WeChat"
```

---

### Q4: 是否需要给实体类添加 Sys 前缀？

**对比**：
```csharp
// 方案A：带前缀
public class SysUser : BaseEntity { }  // → 表名: SysUser

// 方案B：不带前缀
[Table(Name = "sys_user")]
public class User : BaseEntity { }  // → 表名: sys_user
```

**推荐方案B（去掉前缀）**：
- ✅ 代码简洁（User vs SysUser）
- ✅ 符合领域模型命名习惯
- ✅ 通过 [Table] 特性映射表名

**适合用前缀的场景**：
- 多个模块有同名实体（如 User 和 ProductUser）
- 需要明确区分系统表和业务表

---

### Q5: Menu 表是否需要 MenuLevel 字段？

**原设计**：固定 3 级菜单，用 MenuLevel 字段标识

**改进方案**：去掉 MenuLevel，用 Parent/Children 导航属性

```csharp
// 原方案（冗余）
public int MenuLevel { get; set; }  // 1/2/3

// 改进方案（灵活）
[Navigate(nameof(ParentId))]
public Menu? Parent { get; set; }

[Navigate(nameof(ParentId))]
public List<Menu> Children { get; set; } = new();
```

**优势**：
- ✅ 支持无限层级（数据库设计灵活）
- ✅ 业务层约束 3 级（代码控制）
- ✅ FreeSql 原生支持树形查询（AsTreeCte）
- ✅ MudBlazor 通过递归组件渲染，不依赖 Level

---

### Q6: 是否需要给表添加索引？

**推荐添加的索引**：

**必需索引**（⭐⭐⭐）：
```csharp
User:     [Index("uk_username", nameof(Username), true)]
Role:     [Index("uk_role_code", nameof(RoleCode), true)]
Menu:     [Index("uk_menu_code", nameof(MenuCode), true)]
          [Index("idx_parent_id", nameof(ParentId), false)]
UserRole: [Index("uk_user_role", "UserId,RoleId", true)]
RoleMenu: [Index("uk_role_menu", "RoleId,MenuId", true)]
```

**可选索引**（⭐⭐）：
```csharp
User: [Index("idx_email", nameof(Email), false)]
```

**索引命名规范**：
- 唯一索引：`uk_` 前缀（unique key）
- 普通索引：`idx_` 前缀（index）

---

### Q7: 是否需要添加 CreatedBy/UpdatedBy 字段？

**对比**：

| 字段 | 适用场景 | 复杂度 |
|------|---------|--------|
| CreatedAt/UpdatedAt | 所有项目（推荐）| 简单 |
| CreatedBy/UpdatedBy | 强审计需求 | 中等 |

**推荐方案**：只记录时间戳

**理由**：
- ✅ RBAC 学习项目，重点不在审计系统
- ✅ 时间戳已满足 90% 的审计需求
- ✅ 如果真需要，后期加两个字段很容易

**如果需要操作人**：
```csharp
[Column(Position = 4)]
public int? CreatedById { get; set; }  // 存 UserId，不存用户名
```

---

### Q8: SeedDataService 应该放在哪个命名空间？

**问题**：是否应该统一命名空间？

**正确做法**：与 FreeSqlSetup 放在同一命名空间

```
Infrastructure/
└── Database/
    ├── FreeSqlSetup.cs        ← 命名空间: Infrastructure.Database
    └── SeedDataService.cs     ← 命名空间: Infrastructure.Database（统一）
```

**规则**：同一功能模块的类放在同一命名空间下

---

### Q9: 是否应该在 Program.cs 启动时初始化数据？

**原方案**：启动时自动初始化
```csharp
// Program.cs
using (var scope = app.Services.CreateScope())
{
    await seeder.InitializeAsync();
}
```

**缺点**：
- ❌ 每次启动都检查数据库（性能损耗）
- ❌ 不够灵活（重置数据需要手动删除后重启）
- ❌ 启动变慢
- ❌ 生产环境风险（不应该自动初始化）

**改进方案**：通过 DevController 手动触发
```csharp
[HttpPost("initial")]
public async Task<IActionResult> Initialize()
{
    await _seedService.InitializeAsync();
    return Ok(new { message = "种子数据初始化完成" });
}
```

**优势**：
- ✅ 启动快速
- ✅ 灵活可控
- ✅ 开发友好
- ✅ 生产安全（可以禁用 DevController）

---

### Q10: 表结构同步应该放在哪里？

**答案**：由 FreeSql 的 `UseAutoSyncStructure` 自动完成

```csharp
var freeSql = new FreeSqlBuilder()
    .UseConnectionString(DataType.PostgreSQL, connectionString)
    .UseAutoSyncStructure(true)  // ← 启动时自动同步
    .Build();
```

**机制**：
- 每次启动对比实体和数据库表结构
- 表不存在则创建
- 字段缺失则新增
- ⚠️ 不会删除字段（安全机制）

**结论**：
- 表结构同步：自动 ✅
- 数据初始化：手动（DevController）✅

---

### Q11: 是否需要在 AuditEntity 中添加乐观锁？

**推荐添加**：

```csharp
[Column(Position = 4, IsVersion = true)]
public int Version { get; set; }
```

**乐观锁的作用**：防止并发修改冲突

**典型场景**：
```
09:00  管理员A 读取 User(version=5)
09:01  管理员B 读取 User(version=5)
09:05  管理员A 保存，version → 6  ✅
09:10  管理员B 保存时检测到 version ≠ 5，抛异常 ❌
```

**代价**：只增加 4 字节（一个 int 字段）

**建议**：统一在 AuditEntity 中添加

---

### Q12: DevController 如何保证只在开发环境可用？

**方案1**：使用条件编译（推荐）
```csharp
#if DEBUG
[ApiController]
[Route("api/[controller]")]
public class DevController : ControllerBase { }
#endif
```

**方案2**：运行时检查
```csharp
public IActionResult Initialize()
{
    if (!_env.IsDevelopment())
        return Forbid();
    
    await _seedService.InitializeAsync();
}
```

**推荐方案1**：生产环境编译时直接去除，更安全

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

### Day 2 - 基础调试

**1. 查看 Serilog 日志**
```
控制台：实时查看
文件：logs/app-yyyyMMdd.log
```

**2. 测试 API 接口**
```
Scalar：https://localhost:7129/scalar/v1
直接访问：https://localhost:7129/api/test/ping
```

**3. 检查数据库连接**
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

### Day 3 - 数据库调试

**1. 验证表结构**
```sql
-- 查看所有表
\dt

-- 查看表结构
\d "User"

-- 应该看到 SERIAL 主键
"Id" SERIAL PRIMARY KEY
```

**2. 验证索引**
```sql
-- 查看表的索引
\di+ "uk_username"

-- 查看所有索引
SELECT indexname, indexdef FROM pg_indexes 
WHERE tablename = 'User';
```

**3. 验证数据**
```sql
-- 查看用户和角色
SELECT u."Username", r."RoleName"
FROM "UserRole" ur
JOIN "User" u ON ur."UserId" = u."Id"
JOIN "Role" r ON ur."RoleId" = r."Id";
```

**4. 测试 DevController**
```bash
# Scalar 文档测试
https://localhost:5001/scalar/v1

# 或直接调用
POST https://localhost:5001/api/dev/initial
POST https://localhost:5001/api/dev/reset
POST https://localhost:5001/api/dev/clean
```

---

**更新日期**：2024-12-19  
**适用范围**：Day 2-3 项目搭建与实体设计阶段