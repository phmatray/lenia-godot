# Contributing to Lenia

We welcome contributions to the Lenia project! This document provides guidelines for contributing to make the process smooth and efficient for everyone.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Coding Standards](#coding-standards)
- [Testing](#testing)
- [Documentation](#documentation)
- [Submitting Changes](#submitting-changes)
- [Issue Guidelines](#issue-guidelines)

## Code of Conduct

This project and everyone participating in it is governed by our commitment to creating a welcoming and inclusive environment. By participating, you are expected to uphold high standards of respectful communication and collaboration.

## Getting Started

### Prerequisites

- [Godot 4.4+](https://godotengine.org/download) with C# support
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Git for version control
- A code editor (Visual Studio, Visual Studio Code, or Rider recommended)

### Development Setup

1. **Fork the repository** on GitHub
2. **Clone your fork** locally:
   ```bash
   git clone https://github.com/yourusername/Lenia.git
   cd Lenia
   ```
3. **Add the upstream remote**:
   ```bash
   git remote add upstream https://github.com/originalowner/Lenia.git
   ```
4. **Open in Godot** and build the C# project
5. **Test the setup** by running the simulation

## Development Workflow

### Branch Management

- **Main branch**: Always stable and deployable
- **Feature branches**: Use descriptive names like `feature/gallery-improvements` or `fix/simulation-crash`
- **Hotfix branches**: For critical bug fixes, named like `hotfix/memory-leak`

### Workflow Steps

1. **Sync with upstream**:
   ```bash
   git fetch upstream
   git checkout main
   git merge upstream/main
   ```

2. **Create a feature branch**:
   ```bash
   git checkout -b feature/your-feature-name
   ```

3. **Make your changes** following the coding standards below

4. **Test thoroughly** to ensure no regressions

5. **Commit with descriptive messages**:
   ```bash
   git commit -m "Add gallery search functionality
   
   - Implement real-time search filtering
   - Add search input styling
   - Update gallery refresh logic"
   ```

6. **Push to your fork**:
   ```bash
   git push origin feature/your-feature-name
   ```

7. **Create a Pull Request** with a clear description

## Coding Standards

### C# Guidelines

#### Naming Conventions
```csharp
// Classes: PascalCase
public class LeniaSimulation

// Methods: PascalCase
public void UpdateSimulation()

// Properties: PascalCase
public float DeltaTime { get; set; }

// Private fields: camelCase
private float[,] currentGrid;

// Constants: PascalCase
private const int DefaultGridSize = 128;
```

#### Code Organization
- Keep methods focused and under 50 lines when possible
- Use meaningful names that describe the purpose
- Add XML documentation for public APIs:
  ```csharp
  /// <summary>
  /// Updates the simulation by one time step using the current parameters.
  /// </summary>
  /// <param name="deltaTime">The time step for this update</param>
  public void UpdateSimulation(float deltaTime)
  ```

#### Error Handling
```csharp
// Use try-catch for operations that might fail
try
{
    var image = Image.LoadFromFile(imagePath);
    // Process image...
}
catch (Exception e)
{
    GD.PrintErr($"Failed to load image: {e.Message}");
    return null;
}
```

### Godot Scene Guidelines

#### Node Structure
- Use descriptive node names: `HeaderBar`, `SimulationCanvas`, `GalleryGrid`
- Keep scene hierarchies shallow when possible
- Group related nodes under common parents

#### Styling
- Use consistent margins and spacing (8px, 16px multiples)
- Apply StyleBoxFlat for custom UI elements
- Use the established color scheme:
  ```csharp
  // Background colors
  var bgColor = new Color(0.05f, 0.08f, 0.12f, 1.0f);
  var panelColor = new Color(0.12f, 0.15f, 0.22f, 0.95f);
  var sidebarColor = new Color(0.08f, 0.1f, 0.15f, 0.95f);
  ```

### Performance Considerations

- Use `Parallel.For` for computationally intensive loops
- Cache expensive calculations (like kernel offsets)
- Avoid frequent memory allocations in hot paths
- Profile performance-critical sections

## Testing

### Manual Testing
Before submitting a PR, test these scenarios:
- [ ] Application starts without errors
- [ ] All UI panels display correctly
- [ ] Simulation runs smoothly at different speeds
- [ ] Screenshot capture works
- [ ] Gallery browsing and search function
- [ ] Parameter adjustments update simulation
- [ ] Different patterns can be loaded

### Performance Testing
- Test with different grid sizes (64x64 to 512x512)
- Verify FPS remains acceptable (>30 FPS) during simulation
- Check memory usage doesn't grow over time

## Documentation

### Code Comments
- Add comments for complex algorithms
- Explain "why" not "what" in comments
- Document mathematical formulas with references:
  ```csharp
  // Lenia growth function: G(u) = 2*exp(-((u-μ)/σ)²/2) - 1
  // Reference: https://arxiv.org/abs/1812.05433
  private float GrowthFunction(float u)
  ```

### README Updates
- Update feature lists when adding new functionality
- Add screenshots for visual changes
- Update installation instructions if dependencies change

## Submitting Changes

### Pull Request Guidelines

#### Title Format
- Use descriptive titles: "Add gallery search functionality" not "Update gallery"
- Start with action verbs: Add, Fix, Update, Remove, Refactor

#### Description Template
```markdown
## Summary
Brief description of what this PR accomplishes.

## Changes Made
- Bullet point list of specific changes
- Include both code and visual changes
- Mention any breaking changes

## Testing
- [ ] Manual testing completed
- [ ] Performance impact assessed
- [ ] No regressions introduced

## Screenshots
(If applicable, add before/after screenshots)

## Related Issues
Fixes #123, Closes #456
```

#### Review Process
- All PRs require at least one review
- Address all review comments before merging
- Maintain a clean commit history (squash if needed)
- Ensure CI checks pass

### Commit Message Format
```
<type>: <short description>

<optional longer description>

<optional footer>
```

Types:
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `perf`: Performance improvements
- `test`: Adding or updating tests

## Issue Guidelines

### Bug Reports
Include:
- **Steps to reproduce** the issue
- **Expected behavior** vs **actual behavior**
- **Environment details**: OS, Godot version, .NET version
- **Screenshots or logs** if applicable

### Feature Requests
Include:
- **Problem description**: What problem does this solve?
- **Proposed solution**: How should it work?
- **Alternatives considered**: Other approaches you've thought about
- **Additional context**: Screenshots, mockups, examples

### Issue Labels
- `bug`: Something isn't working
- `enhancement`: New feature or improvement
- `documentation`: Documentation needs
- `good first issue`: Good for newcomers
- `help wanted`: Community help requested
- `performance`: Performance-related issues

## Architecture Guidelines

### Adding New Features

1. **Plan the architecture** before coding
2. **Follow existing patterns** in the codebase
3. **Consider performance impact** early
4. **Design for extensibility** when reasonable

### UI Components
- Create reusable components for common patterns
- Use signals for component communication
- Keep UI logic separate from simulation logic
- Follow the established theme and styling

### Simulation Extensions
- Maintain backward compatibility when possible
- Document algorithm changes with academic references
- Consider parameter ranges and edge cases
- Test with various grid sizes and parameters

## Getting Help

- **Discord/Community**: [Add link if available]
- **GitHub Discussions**: For questions and design discussions
- **Issues**: For bug reports and feature requests
- **Code Review**: Tag maintainers for complex PRs

## Recognition

Contributors will be acknowledged in:
- README.md contributors section
- Release notes for significant contributions
- Special recognition for outstanding contributions

Thank you for contributing to Lenia! Your efforts help make this project better for everyone.