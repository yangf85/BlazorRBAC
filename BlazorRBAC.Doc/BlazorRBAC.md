# BlazorRBAC - Blazor Server 权限管理系统

> 基于 Blazor Server 的 RBAC（Role-Based Access Control）权限管理系统

**技术栈**: .NET 9 · Blazor Server · PostgreSQL · FreeSql · MudBlazor · JWT

[快速开始](#-快速开始) · [学习文档](./docs/00-项目总览.md) · [常见问题](./docs/99-常见问题FAQ.md)

---

## 📋 项目简介

BlazorRBAC 是一个完整的、生产级的权限管理系统学习项目，实现了：

- ✅ **基于角色的访问控制（RBAC）**
- ✅ **页面级权限验证**
- ✅ **动态菜单生成**
- ✅ **JWT Token 认证**
- ✅ **第三方登录（微信）**
- ✅ **现代化UI（MudBlazor）**

### 适合谁？

- 🎓 .NET 初学者想学习权限系统
- 💼 Blazor 开发者需要RBAC参考实现
- 🚀 企业项目需要权限管理模块

---

## 🛠️ 技术栈

| 分类 | 技术 |
|------|------|
| **运行时** | .NET 9 / C# 14 |
| **UI框架** | Blazor Server + MudBlazor |
| **数据库** | PostgreSQL 15+ |
| **ORM** | FreeSql |
| **认证** | JWT Bearer + BCrypt |
| **验证** | FluentValidation |
| **映射** | Mapster |
| **日志** | Serilog |
| **API文档** | Scalar |

---

## 🚀 快速开始

### 1. 环境准备

```bash
# 安装 .NET 9 SDK
https://dotnet.microsoft.com/download/dotnet/9.0

# 安装 PostgreSQL 15+
https://www.postgresql.org/download/

# 验证安装
dotnet --version
psql --version
```

### 2. 克隆项目

```bash
git clone https://github.com/your-repo/BlazorRBAC.git
cd BlazorRBAC
```

### 3. 配置数据库

```sql
-- 创建数据库
CREATE DATABASE blazor_rbac;
```

修改连接字符串 `src/BlazorRBAC.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=blazor_rbac;Username=postgres;Password=你的密码"
  }
}
```

### 4. 运行项目

```bash
# 还原包
dotnet restore

# 运行 API（会自动创建表和初始数据）
cd src/BlazorRBAC.Api
dotnet run

# 另开终端运行 Web
cd src/BlazorRBAC.Web
dotnet run
```

### 5. 访问系统

- **API文档**: https://localhost:5001/scalar/v1
- **Web界面**: https://localhost:5002

**默认账号**: admin / admin123

---

## 📁 项目结构

```
BlazorRBAC/
├── src/
│   ├── BlazorRBAC.Domain/          # 领域层（实体、枚举）
│   ├── BlazorRBAC.Application/     # 应用层（DTO、Service、验证）
│   ├── BlazorRBAC.Infrastructure/  # 基础设施（ORM、JWT、第三方）
│   ├── BlazorRBAC.Api/             # WebAPI层
│   └── BlazorRBAC.Web/             # Blazor Server层
├── docs/                           # 学习文档
└── README.md
```

---

## 📚 学习路径

### 10天学习计划（每天2小时）

| 天数 | 主题 | 文档 |
|-----|------|------|
| Day 1 | 环境准备 + 数据库设计 | [Day01](./docs/01-Day01-环境与数据库.md) |
| Day 2 | 项目搭建 + FreeSql配置 | [Day02](./docs/02-Day02-项目搭建.md) |
| Day 3 | 实体设计 + 数据初始化 | [Day03](./docs/03-Day03-实体与初始化.md) |
| Day 4 | 认证与JWT | [Day04](./docs/04-Day04-认证与JWT.md) |
| Day 5 | 权限验证 | [Day05](./docs/05-Day05-权限验证.md) |
| Day 6 | Blazor认证集成 | [Day06](./docs/06-Day06-Blazor认证.md) |
| Day 7 | 动态菜单UI | [Day07](./docs/07-Day07-动态菜单UI.md) |
| Day 8 | 后台管理CRUD | [Day08](./docs/08-Day08-后台管理.md) |
| Day 9 | 微信登录 | [Day09](./docs/09-Day09-微信登录.md) |
| Day 10 | 优化完善 | [Day10](./docs/10-Day10-优化完善.md) |

**推荐学习路径**:
1. 📖 从 [项目总览](./docs/00-项目总览.md) 开始
2. 💻 按Day 1-10顺序学习
3. ❓ 遇到问题查看 [FAQ](./docs/99-常见问题FAQ.md)

---

## 🎯 核心功能

### 1. 用户认证

```csharp
// 本地登录
POST /api/auth/login
{
  "username": "admin",
  "password": "admin123"
}

// 返回Token
{
  "token": "eyJhbGc...",
  "refreshToken": "...",
  "expireTime": "2025-12-17T11:00:00Z"
}
```

### 2. 动态菜单

```csharp
// 根据用户权限返回菜单树
GET /api/menu/current-user

[
  {
    "id": 1,
    "menuName": "系统管理",
    "icon": "mdi-cog",
    "children": [
      {
        "menuName": "用户管理",
        "routePath": "/system/user"
      }
    ]
  }
]
```

### 3. 权限验证

```csharp
[Authorize(Policy = "Permission")]
[Permission("system:user:view")]
public async Task<IActionResult> GetUsers()
{
    // 只有拥有权限的用户能访问
}
```

---

## 🔐 数据库设计

### 核心5张表

```
sys_user (用户表)
  ↓
sys_user_role (用户角色关联)
  ↓
sys_role (角色表)
  ↓
sys_role_menu (角色菜单关联)
  ↓
sys_menu (菜单/权限表)
```

详细设计见: [Day 1 - 数据库设计](./docs/01-Day01-环境与数据库.md)

---

## 🎓 核心概念

### RBAC模型

- **用户（User）**: 登录系统的人
- **角色（Role）**: 一组权限的集合（管理员、普通用户）
- **权限（Permission）**: 能访问的资源（菜单=权限）

### 设计决策

1. **菜单即权限** - 一个菜单项 = 一个权限点
2. **3级菜单** - 模块 → 功能组 → 页面
3. **超级管理员** - 通过 `IsSystem` 字段跳过权限检查
4. **第三方登录** - 支持微信OAuth

---

## 🤝 贡献指南

欢迎提交 Issue 和 Pull Request！

### 开发规范

- 遵循 C# 命名约定
- 代码注释完整
- 提交前运行测试

---

## 📄 License

本项目采用 MIT License

---

## 🙏 致谢

感谢以下开源项目：

- [FreeSql](https://github.com/dotnetcore/FreeSql) - ORM框架
- [MudBlazor](https://github.com/MudBlazor/MudBlazor) - UI组件库
- [Serilog](https://github.com/serilog/serilog) - 日志框架
- [Mapster](https://github.com/MapsterMapper/Mapster) - 对象映射

---

**Made with ❤️ for .NET Developers**

[⬆ 回到顶部](#blazorrbac---blazor-server-权限管理系统)
