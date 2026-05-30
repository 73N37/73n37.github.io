# Blueprint: Design Document (V1 Prototype)

This design document establishes the visual systems and premium interface styling for the Command Center.

## 1. Color Palette Tokens
To create a stunning first impression and reflect a luxury estate rental aesthetic, we enforce two curated modes:

### Cream/Slate/Gold Light Mode (Branded)
- **Background**: `#FCFAF6` (Warm linen cream)
- **Surface**: `#FFFFFF` (Pure white card surfaces)
- **Primary Text**: `#1A2332` (Deep dark slate)
- **Accent Highlight**: `#D4AF37` (Metallic luxury gold)
- **Support Teal**: `#0D9488` (Lakeside pavilion highlights)
- **Border**: `1px solid rgba(212, 175, 55, 0.15)`

### Obsidian Dark Mode
- **Background**: `#070A13` (Obsidian dark space)
- **Surface**: `#0E1326` (Translucent navy-slate card panels)
- **Primary Text**: `#F1F5F9` (Frost silver)
- **Accent Highlight**: `#F59E0B` (Amber gold)
- **Card Shadows**: `0 8px 30px rgba(0, 0, 0, 0.4)`

## 2. Micro-Animations & Interactivity
- **Optimistic Drag Hover**: Cards change border-left highlight to active pulsing gold when hovered.
- **BFF Disconnections**: Renders the persistent Gold warning header that glides down from the top using a custom CSS animation:
  ```css
  @@keyframes slide-down {
    from { transform: translateY(-100%); }
    to { transform: translateY(0); }
  }
  ```
- **Ghost Loading State**: While Graph or e-conomic sync executes, cards show an optimistic blur overlay with a soft glowing circular progress bar.
