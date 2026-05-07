# Contributing to Island Illusion Palace

欢迎贡献代码！请阅读以下指南来了解如何参与项目开发。

## 如何贡献

### 1. 提交 Issue

如果您发现了 bug 或者有功能建议，请先提交 Issue：

- **Bug Report**: 提供详细的问题描述、复现步骤和截图
- **Feature Request**: 描述您想要的功能以及它如何改善游戏

### 2. Fork 项目

1. Fork 本项目到您的 GitHub 账号
2. Clone 您的 fork 到本地

### 3. 创建分支

```bash
git checkout -b feature/your-feature-name
# 或者
git checkout -b fix/your-bug-fix
```

### 4. 编写代码

请遵循以下编码规范：

- **命名规范**: 使用 PascalCase 命名类和方法，使用 camelCase 命名变量
- **代码风格**: 保持与现有代码一致的风格
- **注释**: 为复杂逻辑添加必要的注释
- **测试**: 如果添加了新功能，请编写相应的测试

### 5. 提交代码

```bash
git add .
git commit -m "feat: 添加新功能描述"
git push origin feature/your-feature-name
```

提交信息格式：

- `feat`: 新功能
- `fix`: Bug 修复
- `docs`: 文档更新
- `style`: 代码风格调整
- `refactor`: 代码重构
- `test`: 测试更新

### 6. 创建 Pull Request

在 GitHub 上创建 Pull Request，并描述您的更改内容。

## 代码规范

### C# 规范

- 使用 Unity 官方推荐的代码风格
- 使用 `var` 代替显式类型声明（当类型明显时）
- 避免使用 `static` 变量，除非必要
- 使用属性（Property）代替公共字段

### 资源规范

- 资源命名使用 PascalCase
- 使用文件夹组织资源
- 添加必要的注释说明资源用途

## 开发环境

- Unity 2022.x
- Visual Studio 2022 或 Rider
- Git

## 联系方式

如有问题，请通过 Issue 联系我们。

感谢您的贡献！