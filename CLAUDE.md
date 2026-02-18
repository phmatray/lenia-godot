# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a Godot 4.4 project implementing Lenia, a continuous cellular automaton system, using C# and .NET 8.0. The project creates beautiful, organic patterns through mathematical simulation of artificial life.

## Build and Development Commands

### Building
```bash
# Build the C# project
dotnet build

# Run with Godot (requires Godot in PATH)
godot --path . res://lenia.tscn
```

### Development in Godot Editor
- Open `project.godot` in Godot Engine 4.4+
- Build C# project when prompted
- Main scene is `menu.tscn` (not `lenia.tscn` directly)
- Press F5 to run

## Architecture Overview

### Core Components

**LeniaSimulation.cs** (`Scripts/LeniaSimulation.cs:1`) - The heart of the application
- Manages the continuous cellular automaton grid and computation
- Handles parallel processing using `Parallel.For` for performance
- Implements kernel convolution and growth function calculations
- Exports configurable parameters (growth, kernel radius, delta time, etc.)

**LeniaMainUI.cs** (`Scripts/LeniaMainUI.cs:1`) - Main UI orchestrator
- Coordinates all UI components (HeaderBar, LeftToolbar, SimulationCanvas, RightSidebar, StatusBar)
- Connects signals between components
- Initializes components with simulation reference

### UI Architecture
The UI follows a modular, signal-based design:
- **HeaderBar**: Play/pause controls, speed slider, reset functionality
- **LeftToolbar**: Tool selection (paint/erase brushes)
- **SimulationCanvas**: Interactive canvas for viewing and manipulating simulation
- **RightSidebar**: Parameter controls and pattern presets
- **StatusBar**: Performance monitoring and status information
- **Gallery**: Screenshot capture and browsing system

### Scene Structure
- `menu.tscn` - Entry point and menu system
- `lenia.tscn` - Main simulation scene
- `gallery.tscn` - Screenshot gallery interface
- Component scenes: `header_bar.tscn`, `left_toolbar.tscn`, `right_sidebar.tscn`, etc.

### Key Patterns
- **Signal-based communication**: Components communicate via Godot signals rather than direct references
- **Modular initialization**: Each UI component has an `Initialize(simulation)` method
- **Parallel processing**: Grid updates use `Parallel.For` for performance
- **Resource management**: Custom ImageTexture handling for real-time visualization

### Color Mapping and Visualization
**ColorMapper.cs** handles multiple visualization schemes:
- Heat, Grayscale, Rainbow, and Plasma color schemes
- Real-time pixel buffer manipulation for performance
- RGB format with direct byte array manipulation

### Gallery System
**Gallery.cs** implements a sophisticated screenshot system:
- Canvas-only image capture (excludes UI elements)
- Metadata embedding with simulation parameters
- Grid/list view with dynamic thumbnails
- Search and filtering capabilities
- Batch export functionality

## Fun Features Added

### Interactive Audio System
**AudioFeedback.cs** - Generative audio that responds to simulation state
- Real-time audio synthesis based on population density and growth patterns
- Different tones for birth, death, growth, and decay events
- Paint interaction sounds for immediate user feedback
- Musical patterns for different preset selections
- Configurable volume and audio enable/disable

### Enhanced Pattern Library
**PatternLibrary.cs** - Comprehensive pattern collection with categories
- **Classic**: Traditional patterns like Orbium
- **Creatures**: Self-propelling life forms with different behaviors
- **Oscillators**: Patterns that pulse and breathe in place
- **Generators**: Sources that create waves and emanations
- **Experimental**: Chaotic and unpredictable patterns
- **User Saved**: Custom patterns saved by users
- Search and difficulty filtering
- Visual previews and detailed descriptions
- Parameter presets for each pattern

### Particle Visual Effects
**ParticleEffects.cs** - Dynamic visual feedback system
- Birth explosions with radiating particles
- Death implosions with converging effects
- Growth sparkles and decay falling particles
- Movement trails for creature tracking
- Paint interaction feedback
- Configurable intensity and particle limits
- Memory-efficient particle pooling system

### Time-lapse Recording
**TimelapseRecorder.cs** - Capture and export simulation evolution
- Multiple quality presets (Fast/Balanced/High/Custom)
- Frame-by-frame image sequence export
- Metadata tracking with simulation parameters
- HTML preview generation with thumbnail grid
- FFmpeg command file generation for video creation
- Configurable duration limits and frame rates

### Interactive Tutorial System
**TutorialSystem.cs** - Guided learning experiences
- **First Time**: Welcome tutorial for new users
- **Features**: Tool and interface explanations
- **Advanced**: Power user techniques
- **Challenges**: Goal-oriented learning tasks
- Step-by-step guidance with visual highlights
- Progress tracking and completion rewards
- Contextual hints and auto-advancement options

### Challenge & Achievement System
**ChallengeSystem.cs** - Competitive and goal-oriented gameplay
- **Survival**: Keep patterns alive for extended periods
- **Speed**: Achieve goals as quickly as possible
- **Efficiency**: Optimize with minimal parameters
- **Discovery**: Find specific pattern behaviors
- **Creativity**: Design aesthetically pleasing configurations
- **Optimization**: Maximize/minimize specific metrics
- Difficulty progression from Beginner to Master
- Leaderboards and personal best tracking
- Achievement system with unlock conditions
- Reward points and progression mechanics

## Important Notes

### Target Framework
- .NET 8.0 with `EnableDynamicLoading=true`
- Godot.NET.Sdk/4.4.1

### Performance Considerations
- Kernel caching with pre-computed offset arrays
- Parallel grid processing for large simulations
- Efficient pixel buffer manipulation for rendering
- Memory management with object pooling for UI and particles

### Custom User Directory
The project uses a custom user directory (`config/custom_user_dir_name="Lenia"`) for storing screenshots, recordings, patterns, and user data.

### Integration Points
The new features are fully integrated into the existing UI:

**LeniaMainUI.cs** - Main orchestrator enhanced with:
- Automatic initialization of all fun feature systems
- Signal-based communication between features
- Public methods for accessing features from other components
- Real-time audio updates in `_Process()` method
- Global hotkey handling for feature access

**HeaderBar.cs** - Enhanced toolbar with new buttons:
- 📚 Pattern Library button - opens pattern browser
- 🎓 Tutorial button - launches learning system  
- 🏆 Challenge button - opens competitive modes
- 🎬 Record button - starts/stops time-lapse recording
- 🔊 Audio toggle - enables/disables sound feedback
- ✨ Particle toggle - enables/disables visual effects

**RightSidebar.cs** - New "Fun Features" section with:
- Audio volume control slider
- Particle intensity adjustment
- Recording status information
- Quick access buttons for all features

**StatusBar.cs** - Enhanced with:
- Recording status messages
- Feature activity notifications
- Temporary status updates

### Feature Interactions
- **Audio**: Responds to simulation population changes, user interactions, and pattern selections
- **Particles**: Visual feedback for births, deaths, growth, decay, and user painting
- **Patterns**: Automatic parameter adjustment when loading preset configurations
- **Tutorials**: Context-aware guidance with visual highlighting
- **Challenges**: Real-time simulation analysis for goal completion
- **Recording**: Metadata embedding with simulation parameters and HTML preview generation

### Usage Hotkeys
- **Pattern Library**: Access via header button or programmatic calls
- **Audio Control**: Toggle button in header, volume slider in sidebar
- **Particle Control**: Toggle button in header, intensity slider in sidebar
- **Recording**: Start/stop via header button with visual feedback
- **F1**: Show keyboard shortcuts overlay
- **F2**: Restart onboarding tutorial
- **Escape**: Close overlays and dialogs

## Modern UI/UX System

### Design System
**ModernTheme.cs** (`Scripts/UI/ModernTheme.cs`) - Centralized theming
- **Glassmorphic design**: Semi-transparent panels with blur effects
- **Neumorphic buttons**: Soft shadows and depth for modern look
- **Color palette**: Dark theme with blue accents, proper contrast ratios
- **Consistent spacing**: 4px grid system for all UI elements
- **Typography**: Clear hierarchy with shadow effects for legibility

### Animation System
**AnimationSystem.cs** (`Scripts/UI/AnimationSystem.cs`) - Smooth transitions
- **Entrance animations**: FadeIn, SlideFrom, FadeScale, Bounce effects
- **Interactive feedback**: Hover effects, ripple animations, pulse/shake
- **Performance optimized**: Uses Godot's tween system efficiently
- **Customizable timing**: Easy/linear/bounce easing functions

### UI Components

#### Rich Tooltips
**RichTooltip.cs** (`Scripts/UI/RichTooltip.cs`)
- Multi-content support (title, description, icon, shortcuts)
- Smart positioning to stay on screen
- Smooth fade animations
- Helper utilities for easy integration

#### Modern Buttons
**ModernButton.cs** (`Scripts/UI/ModernButton.cs`)
- Material Design ripple effects on click
- Multiple variants (Primary, Secondary, Success, Warning, Danger, Ghost)
- Icon support with proper theming
- Hover and press animations

#### Keyboard Shortcuts Overlay
**KeyboardShortcutsOverlay.cs** (`Scripts/UI/KeyboardShortcutsOverlay.cs`)
- F1 to toggle display
- Categorized shortcuts (Simulation, Drawing, View, Parameters, Advanced)
- Visual key badges with modern styling
- Dimmed background with glassmorphic panel

#### Onboarding Flow
**OnboardingFlow.cs** (`Scripts/UI/OnboardingFlow.cs`)
- 7-step guided tutorial for new users
- Spotlight effect highlighting UI elements
- Progress indicators and navigation
- Persistent state (won't show again once completed)
- Skip option for experienced users

### Accessibility Features
**AccessibilityManager.cs** (`Scripts/UI/AccessibilityManager.cs`)
- **High Contrast Mode**: Black/white theme with clear borders
- **Reduced Motion**: Simplified animations for motion sensitivity
- **Large Text Mode**: Scalable fonts (1.0x - 2.0x)
- **Screen Reader Support**: Proper announcements and ARIA roles
- **Keyboard Navigation**: Enhanced focus indicators
- **Persistent Settings**: Saves user preferences

### UI Initialization
**UIInitializer.cs** (`Scripts/UI/UIInitializer.cs`)
- Automatic enhancement of existing UI elements
- Staggered entrance animations on startup
- Global keyboard shortcut setup
- Modern theme application to all components

### Visual Enhancements
- **Hover Effects**: All interactive elements have smooth hover transitions
- **Focus Indicators**: Clear visual feedback for keyboard navigation
- **Loading States**: Skeleton screens and progress indicators
- **Error Feedback**: Shake animations and color highlights
- **Success Feedback**: Pulse animations and color changes

### Best Practices
- All animations respect accessibility settings
- Components follow single responsibility principle
- Consistent use of signals for loose coupling
- Responsive design adapts to window size
- Performance optimized with object pooling