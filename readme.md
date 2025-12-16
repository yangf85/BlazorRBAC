# BlazorRBAC - Blazor Server 权限管理系统

## 📋 项目概述

基于 Blazor Server 的 RBAC（Role-Based Access Control）权限管理系统，实现页面级权限控制和动态菜单生成。

## 🎯 核心功能

- ✅ 用户注册/登录（本地 + 微信第三方登录）
- ✅ 基于角色的权限管理（RBAC）
- ✅ 动态菜单生成（3级菜单）
- ✅ 页面级权限验证
- ✅ 超级管理员支持
- ✅ JWT Token 认证

## 🛠️ 技术栈

| 分类 | 技术 | 说明 |
|------|------|------|
| **框架** | Blazor Server | UI框架 |
| | .NET 9 | 运行时 |
| | C# 14 | 编程语言 |
| **UI组件** | MudBlazor | UI组件库 |
| **数据库** | PostgreSQL | 关系型数据库 |
| **ORM** | FreeSql | 数据访问 |
| **验证** | FluentValidation | 数据验证 |
| **映射** | Mapster | 对象映射 |
| **认证** | JWT Bearer | Token认证 |
| | BCrypt.Net-Next | 密码加密 |
| **调试** | Spectre.Console | 控制台输出 |
| | Scalar | API文档 |
| **日志** | Serilog | 日志记录 |
| **HTTP** | WebApiClientCore | HTTP客户端 |

## 📊 数据库设计

### 核心5张表

```
1. User          - 用户表
2. Role          - 角色表
3. Menu          - 菜单/权限表（菜单即权限）
4. UserRole      - 用户角色关联表
5. RoleMenu      - 角色菜单关联表
```

### 关系图

```
用户 ←→ 用户角色 ←→ 角色 ←→ 角色菜单 ←→ 菜单
```

### 数据库创建方式

**✅ 使用 FreeSql CodeFirst 自动同步**

- 无需手动写 SQL 脚本
- FreeSql 根据实体类自动创建表结构
- 启动时自动检测并创建/更新所有表
- 配置 `UseAutoSyncStructure(true)` 即可

**初始数据通过代码种子服务（SeedDataService）初始化**
- 超级管理员账号
- 默认角色（SuperAdmin、Admin、User）
- 基础菜单结构

## 🏗️ 系统架构

### 项目结构

```
BlazorRBAC.sln
├── BlazorRBAC.Domain          # 领域层（实体、枚举）
├── BlazorRBAC.Application     # 应用层（DTO、Service、验证器）
├── BlazorRBAC.Infrastructure  # 基础设施（FreeSql、JWT、微信登录）
├── BlazorRBAC.Api             # WebAPI层（控制器、过滤器）
└── BlazorRBAC.Web             # Blazor Server（页面、组件、认证）
```

### 认证流程

```
用户登录 
  ↓
验证用户名密码/微信授权
  ↓
生成 JWT Token + RefreshToken
  ↓
Blazor Server 验证 Token
  ↓
获取用户权限（角色+菜单）
  ↓
生成动态菜单
  ↓
页面访问权限验证
```

### FreeSql CodeFirst 优势

本项目采用 **FreeSql CodeFirst** 模式管理数据库：

**✅ 核心优势：**
- **无需手写SQL**：实体类即表结构定义
- **自动同步**：启动时自动检测并创建/更新表
- **类型安全**：编译时检查，避免SQL拼写错误
- **迁移友好**：修改实体类属性，自动更新表结构
- **跨数据库**：切换数据库只需改配置，无需改代码

**对比传统方式：**

| 方式 | 传统 SQL 脚本 | FreeSql CodeFirst |
|------|--------------|-------------------|
| 建表 | 手写 CREATE TABLE | 定义实体类 |
| 修改表 | 手写 ALTER TABLE | 修改实体类属性 |
| 类型检查 | ❌ 运行时才发现错误 | ✅ 编译时检查 |
| 多数据库 | ❌ 需要为每个库写SQL | ✅ 自动适配 |
| 学习成本 | 需要掌握SQL语法 | 只需要C#知识 |

## 📅 学习计划（10天，每天2小时）

| 天数 | 主题 | 核心内容 |
|-----|------|---------|
| Day 1 | 环境准备 + RBAC概念 | .NET 9安装、PostgreSQL配置、RBAC原理 |
| Day 2 | 项目搭建 + FreeSql配置 | Solution创建、NuGet包安装、数据库连接测试 |
| Day 3 | 实体与自动建表 | 5个实体类、CodeFirst自动同步、种子数据服务 |
| Day 4 | 认证与JWT | 注册、登录、BCrypt加密、JWT生成/验证 |
| Day 5 | 权限验证 | 授权策略、权限检查、动态菜单查询 |
| Day 6 | Blazor认证集成 | AuthenticationStateProvider |
| Day 7 | 动态菜单UI | MudBlazor菜单树、路由守卫 |
| Day 8 | 后台管理 | 用户/角色/菜单CRUD |
| Day 9 | 微信登录 | OAuth流程、用户绑定 |
| Day 10 | 优化完善 | RefreshToken、安全加固、错误处理 |

## 🔑 关键设计决策

### 1. 菜单即权限
- 一个菜单项 = 一个权限点 = 一个路由
- 简化模型，易于理解

### 2. 固定3级菜单
- 一级：模块（系统管理）
- 二级：功能组（用户管理）
- 三级：页面（用户列表）

### 3. 超级管理员
- 在角色表添加 `IsSystem` 字段
- 系统角色跳过权限检查

### 4. 第三方登录
- 支持微信登录
- 用户表添加：`login_type`、`external_id`、`external_email`

### 5. 暂不实现
- ❌ 缓存层（Redis）
- ❌ 测试框架
- ❌ 按钮级权限
- ❌ 数据权限（行级）

## 📁 文档组织

```
/BlazorRBAC-Docs/
├── 00-总文档.md                    # 本文档
├── 01-环境与RBAC概念.md            # Day 1
├── 02-项目搭建与FreeSql.md         # Day 2
├── 03-实体与自动建表.md            # Day 3
├── 04-认证与JWT.md                 # Day 4
├── 05-权限验证.md                  # Day 5
├── 06-Blazor认证.md                # Day 6
├── 07-动态菜单UI.md                # Day 7
├── 08-后台管理.md                  # Day 8
├── 09-微信登录.md                  # Day 9
├── 10-优化完善.md                  # Day 10
└── 99-常见问题.md                  # 附录
```

## 🚀 快速开始

### 1. 环境要求
- .NET 9 SDK
- PostgreSQL 15+（只需创建空数据库）
- Visual Studio 2022 / VS Code + C# Dev Kit

### 2. 数据库准备
只需创建空数据库 `blazor_rbac`，表结构由 FreeSql 自动生成

### 3. 核心NuGet包
- **ORM**：FreeSql.Provider.PostgreSQL（支持 CodeFirst 自动建表）
- **认证**：Microsoft.AspNetCore.Authentication.JwtBearer、BCrypt.Net-Next
- **验证与映射**：FluentValidation、Mapster
- **UI**：MudBlazor
- **工具**：Serilog.AspNetCore、Spectre.Console、Scalar.AspNetCore
- **HTTP客户端**：WebApiClientCore

### 4. 配置数据库连接
在 `appsettings.json` 中配置 PostgreSQL 连接字符串

### 5. 启动项目
运行项目后，FreeSql 会自动：
- ✅ 检测数据库表是否存在
- ✅ 自动创建5张表
- ✅ 运行种子数据服务，创建初始管理员

## 📚 学习建议

### 新手路径
1. 先完成 Day 1-5（核心RBAC）
2. 验证权限系统工作正常
3. 再学习 Day 6-8（Blazor集成）
4. Day 9-10 作为扩展内容

### 调试技巧
- 使用 Spectre.Console 美化控制台输出
- Serilog 记录关键操作日志
- Scalar 测试 API 接口

### 常见问题
- Blazor Server 的 JWT 传递（SignalR）
- FreeSql 的实体导航属性配置
- 微信登录的回调处理

## 🔒 安全建议

1. **密码安全**：使用 BCrypt 加密，迭代次数 ≥ 10
2. **Token安全**：
   - AccessToken 有效期短（15-30分钟）
   - RefreshToken 有效期长（7天）
   - Token 存储在 HttpOnly Cookie
3. **HTTPS**：生产环境强制使用
4. **SQL注入**：使用 FreeSql 参数化查询

## 📞 后续扩展

学完基础后可以添加：
- Redis 缓存（权限、菜单）
- 按钮级权限控制
- 数据权限（组织架构）
- 操作日志审计
- 单元测试 + 集成测试

---

**当前版本**：v1.0  
**更新日期**：2025-12-16  
**适用人群**：.NET 初学者、Blazor 新手、RBAC 学习者