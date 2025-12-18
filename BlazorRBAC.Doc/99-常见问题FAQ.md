# 常见问题 FAQ

> 学习过程中可能遇到的问题和解决方案

---

## 🔧 环境与配置

### Q1：PostgreSQL 连接失败？

**错误信息**：`Connection refused` 或 `could not connect to server`

**解决方案**：
1. 检查 PostgreSQL 服务是否运行
   ```bash
   # Windows
   pg_ctl status
   
   # Linux/Mac
   sudo systemctl status postgresql
   ```
2. 检查端口是否正确（默认 5432）
3. 检查防火墙设置
4. 验证连接字符串中的用户名密码

### Q2：FreeSql 自动同步表失败？

**问题**：`UseAutoSyncStructure(true)` 不生效

**解决方案**：
1. 确保实体类有 `[Table]` 特性
2. 手动调用 `CodeFirst.SyncStructure<T>()`
3. 检查数据库用户是否有 CREATE TABLE 权限

### Q3：NuGet 包安装慢？

**解决方案**：
配置国内源（nuget.org 镜像）
```bash
dotnet nuget add source https://nuget.cdn.azure.cn/v3/index.json -n azure
```

---

## 🔐 认证相关

### Q4：JWT Token 验证失败？

**错误信息**：`IDX10503: Signature validation failed`

**原因**：
- SecretKey 不一致（API 和配置文件）
- SecretKey 长度不够（至少32位）

**解决方案**：
```csharp
// 确保 appsettings.json 和 JwtService 使用相同的 Key
"SecretKey": "YourSuperSecretKey12345678901234567890"  // 至少32字符
```

### Q5：Blazor Server 获取不到用户信息？

**问题**：`AuthenticationState` 总是匿名用户

**解决方案**：
1. 检查 Token 是否正确存储在 LocalStorage
2. 确认 `CustomAuthStateProvider` 注册正确
3. 使用浏览器开发者工具查看 Storage

### Q6：密码加密后无法验证？

**问题**：`BCrypt.Verify()` 总是返回 false

**原因**：
- 使用了不同的 BCrypt 库
- 哈希值被截断（数据库字段太短）

**解决方案**：
```sql
-- 确保字段长度足够
password_hash VARCHAR(200)  -- BCrypt 哈希长度为 60，但留足空间
```

---

## 🎨 UI 相关

### Q7：MudBlazor 组件不显示？

**问题**：页面空白或样式错误

**解决方案**：
1. 确认 `_Host.cshtml` 引入了 MudBlazor CSS/JS
   ```html
   <link href="_content/MudBlazor/MudBlazor.min.css" rel="stylesheet" />
   <script src="_content/MudBlazor/MudBlazor.min.js"></script>
   ```
2. 检查 `Program.cs` 是否添加了 `AddMudServices()`

### Q8：动态菜单不刷新？

**问题**：添加新菜单后，前端看不到

**解决方案**：
1. 清除浏览器缓存
2. 重新登录（Token 中不包含菜单信息，需要重新请求）
3. 使用 `StateHasChanged()` 强制刷新组件

---

## 🐛 数据库相关

### Q9：外键约束错误？

**错误信息**：`violates foreign key constraint`

**原因**：
- 删除数据时，有其他表引用
- 插入数据时，外键 ID 不存在

**解决方案**：
```sql
-- 使用 ON DELETE CASCADE
FOREIGN KEY (user_id) REFERENCES sys_user(id) ON DELETE CASCADE
```

### Q10：菜单树查询太慢？

**问题**：用户多了后，菜单查询变慢

**解决方案**：
1. 添加索引
   ```sql
   CREATE INDEX idx_menu_parent ON sys_menu(parent_id);
   CREATE INDEX idx_user_role ON sys_user_role(user_id);
   ```
2. 使用缓存（Redis 或内存缓存）

---

## 🚀 部署相关

### Q11：生产环境连接字符串怎么配置？

**解决方案**：
使用环境变量或 User Secrets

```bash
# 开发环境
dotnet user-secrets set "ConnectionStrings:Default" "Host=..."

# 生产环境（Docker）
docker run -e ConnectionStrings__Default="Host=..."
```

### Q12：Blazor Server SignalR 连接失败？

**问题**：控制台报错 `Failed to start the connection`

**解决方案**：
1. 检查防火墙/负载均衡配置
2. 启用 WebSocket
   ```csharp
   app.UseWebSockets();
   app.MapBlazorHub();
   ```

---

## 💡 最佳实践

### Q13：如何管理大量菜单？

**建议**：
- 使用菜单编码规范：`module-function-page`
- 定期清理无用菜单
- 考虑实现"菜单组"概念

### Q14：如何处理并发登录？

**方案**：
1. **单设备登录**：新 Token 使旧 Token 失效
2. **多设备登录**：Token 中加入设备信息

### Q15：如何实现"记住我"功能？

**实现**：
```csharp
// 生成长期有效的 RefreshToken（7天）
var refreshToken = GenerateRefreshToken(userId);
await _localStorage.SetAsync("refreshToken", refreshToken);
```

---

## 📝 调试技巧

### 使用 Spectre.Console 美化输出

```csharp
using Spectre.Console;

// 表格输出
var table = new Table();
table.AddColumn("用户名");
table.AddColumn("角色");
table.AddRow("admin", "SuperAdmin");
AnsiConsole.Write(table);

// 进度条
await AnsiConsole.Progress()
    .StartAsync(async ctx =>
    {
        var task = ctx.AddTask("初始化数据...");
        await SeedData();
        task.Increment(100);
    });
```

### 使用 Serilog 记录关键信息

```csharp
Log.Information("用户 {Username} 登录成功", username);
Log.Warning("权限验证失败：{UserId} 访问 {Resource}", userId, resource);
Log.Error(ex, "数据库连接失败");
```

---

## 🔍 更多帮助

### 官方文档
- [Blazor](https://learn.microsoft.com/zh-cn/aspnet/core/blazor/)
- [FreeSql](https://freesql.net/)
- [MudBlazor](https://mudblazor.com/)

### 社区资源
- GitHub Issues
- Stack Overflow
- 中文开发者社区

---

[⬅️ 返回目录](./README.md)
