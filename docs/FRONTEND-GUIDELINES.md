# Frontend Style & Design Guidelines

> Based on [Vercel Web Interface Guidelines](https://github.com/vercel-labs/web-interface-guidelines)
> Skill source: `C:\Users\tuand\OneDrive\Desktop\Skill\SKILL.md`

This document defines the mandatory frontend standards for the Task Manager project. **All React components must comply.**

---

## 1. Accessibility (A11y)

- Icon-only buttons → `aria-label` required
- Form controls → `<label>` or `aria-label`
- Interactive elements → keyboard handlers (`onKeyDown`/`onKeyUp`)
- `<button>` for actions, `<a>`/`<Link>` for navigation — **never `<div onClick>`**
- Images → `alt` (or `alt=""` if decorative)
- Decorative icons → `aria-hidden="true"`
- Async updates (toasts, validation) → `aria-live="polite"`
- Semantic HTML first (`<button>`, `<a>`, `<label>`, `<table>`) before ARIA
- Headings hierarchical `<h1>`–`<h6>`; include skip link for main content
- `scroll-margin-top` on heading anchors

## 2. Focus States

- All interactive elements → visible focus: `focus-visible:ring-*` or equivalent
- **Never** `outline-none` / `outline: none` without focus replacement
- Use `:focus-visible` over `:focus` (avoid focus ring on click)
- Group focus with `:focus-within` for compound controls (e.g., search bar)

## 3. Forms

- Inputs → `autocomplete` + meaningful `name`
- Correct `type` (`email`, `tel`, `url`, `number`) and `inputmode`
- **Never** block paste (`onPaste` + `preventDefault`)
- Labels clickable (`htmlFor` or wrapping control)
- Disable spellcheck on emails, codes, usernames (`spellCheck={false}`)
- Checkboxes/radios: label + control share single hit target (no dead zones)
- Submit button stays enabled until request starts; spinner during request
- Errors inline next to fields; focus first error on submit
- Placeholders end with `…` and show example pattern
- `autocomplete="off"` on non-auth fields
- Warn before navigation with unsaved changes (`beforeunload` or router guard)

## 4. Animation

- Honor `prefers-reduced-motion` (provide reduced variant or disable)
- Animate `transform`/`opacity` only (compositor-friendly)
- **Never** `transition: all` — list properties explicitly
  ```css
  /* ❌ Bad */
  transition: all 0.2s;
  /* ✅ Good */
  transition: transform 0.2s, opacity 0.2s;
  ```
- Set correct `transform-origin`
- SVG: transforms on `<g>` wrapper with `transform-box: fill-box; transform-origin: center`
- Animations interruptible — respond to user input mid-animation

### Kanban Drag & Drop Animations
- Use `transform` for card movement (GPU-accelerated)
- `will-change: transform` on draggable cards
- Drop animation < 200ms
- Respect `prefers-reduced-motion` — instant snap with no animation

## 5. Typography

- `…` not `...`
- Curly quotes `"` `"` not straight `"`
- Non-breaking spaces: `10&nbsp;MB`, `⌘&nbsp;K`
- Loading states end with `…`: `"Loading…"`, `"Saving…"`
- `font-variant-numeric: tabular-nums` for number columns (task counts, dates)
- Use `text-wrap: balance` or `text-pretty` on headings

## 6. Content Handling

- Text containers handle long content: `truncate`, `line-clamp-*`, or `break-words`
- Flex children → `min-w-0` to allow text truncation
- Handle empty states — don't render broken UI for empty strings/arrays
- User-generated content: anticipate short, average, and very long inputs

### Task-Specific
- Task titles: `line-clamp-2` in card view, full in detail view
- Description: `line-clamp-3` in list, expandable
- Tags: horizontal scroll or wrap with overflow handling
- Empty project → show friendly illustration + CTA

## 7. Images

- `<img>` → explicit `width` and `height` (prevents CLS)
- Below-fold images → `loading="lazy"`
- Above-fold critical images → `priority` or `fetchpriority="high"`
- Avatars: fixed dimensions, `object-cover`, fallback initials

## 8. Performance

- Large lists (>50 items) → virtualize (`@tanstack/virtual` or `virtua`)
- No layout reads in render (`getBoundingClientRect`, `offsetHeight`, etc.)
- Batch DOM reads/writes; avoid interleaving
- Prefer uncontrolled inputs; controlled inputs must be cheap per keystroke
- Add `<link rel="preconnect">` for API domain
- Critical fonts: `<link rel="preload" as="font">` with `font-display: swap`

### Task List Virtualization
- Task list view: virtualize when >50 tasks
- Kanban columns: virtualize when >30 cards per column

## 9. Navigation & State

- **URL reflects state** — filters, tabs, pagination, expanded panels in query params
- Links use `<a>`/`<Link>` (Cmd/Ctrl+click, middle-click support)
- Deep-link all stateful UI (`useState` → consider URL sync via `nuqs`)
- Destructive actions → confirmation modal or undo window — **never immediate**

### Task Manager URL Patterns
```
/tasks                        → All tasks (default: list view)
/tasks?view=kanban            → Kanban board
/tasks?status=todo,inprogress → Filtered
/tasks?priority=high,urgent   → Priority filter
/tasks?project=abc123         → Project filter
/tasks?q=search+term          → Search
/tasks/abc123                 → Task detail (modal over list)
/projects                     → Project list
/projects/abc123              → Project detail
/projects/abc123/tasks        → Project tasks
```

## 10. Touch & Interaction

- `touch-action: manipulation` (prevents double-tap zoom delay)
- `-webkit-tap-highlight-color` set intentionally
- `overscroll-behavior: contain` in modals/drawers/sheets
- During Kanban drag: disable text selection, `inert` on dragged elements
- `autoFocus` sparingly — desktop only, single primary input; avoid on mobile

## 11. Safe Areas & Layout

- Full-bleed layouts → `env(safe-area-inset-*)` for notches
- Avoid unwanted scrollbars: `overflow-x-hidden`, fix content overflow
- Flex/grid over JS measurement for layout

### App Layout
```
┌─────────────────────────────────────────┐
│  Header (fixed, h-14)                   │
├───────────┬─────────────────────────────┤
│  Sidebar  │  Main Content               │
│  (w-64,   │  (flex-1, overflow-y-auto)  │
│  fixed)   │                             │
│           │                             │
│  - Tasks  │  ┌─ Toolbar ──────────────┐ │
│  - Inbox  │  │ Search │ Filter │ View │ │
│  - Today  │  └────────────────────────┘ │
│  - Done   │                             │
│  ─────    │  ┌─ Content ──────────────┐ │
│  Projects │  │  (List or Kanban)      │ │
│  - Proj1  │  │                        │ │
│  - Proj2  │  │                        │ │
│           │  └────────────────────────┘ │
└───────────┴─────────────────────────────┘
```

## 12. Dark Mode & Theming

- `color-scheme: dark` on `<html>` for dark themes
- `<meta name="theme-color">` matches page background
- Native `<select>`: explicit `background-color` and `color`
- Tailwind: use `dark:` variant consistently

### Color Palette (Tailwind Config)
```js
// Todoist-inspired, minimal
colors: {
  primary: {
    50: '#fef2f2',   // Lightest
    500: '#ef4444',  // Brand red (like Todoist)
    600: '#dc2626',  // Hover
    700: '#b91c1c',  // Active
  },
  surface: {
    light: '#ffffff',
    dark: '#1a1a2e',
  },
  sidebar: {
    light: '#fafafa',
    dark: '#16213e',
  },
  // Priority colors
  priority: {
    urgent: '#dc2626',  // Red
    high: '#f59e0b',    // Amber
    medium: '#3b82f6',  // Blue
    low: '#6b7280',     // Gray
  },
  // Status colors (Kanban)
  status: {
    todo: '#6b7280',
    inProgress: '#3b82f6',
    review: '#f59e0b',
    done: '#10b981',
  }
}
```

## 13. Locale & i18n

- Dates/times → `Intl.DateTimeFormat` (not hardcoded formats)
- Numbers/currency → `Intl.NumberFormat`
- Detect language via `Accept-Language` / `navigator.languages`, not IP
- Relative dates: `date-fns/formatDistanceToNow` or `Intl.RelativeTimeFormat`

### Date Display Rules
- Today → "Today"
- Yesterday → "Yesterday"
- This week → "Monday", "Tuesday"
- Older → "Mar 15" (same year) or "Mar 15, 2025" (diff year)
- Overdue → red text, "Overdue · Mar 10"

## 14. Hydration Safety

- Inputs with `value` → need `onChange` (or use `defaultValue` for uncontrolled)
- Date/time rendering → guard against hydration mismatch
- `suppressHydrationWarning` only where truly needed

## 15. Hover & Interactive States

- Buttons/links → `hover:` state (visual feedback)
- Interactive states increase contrast: hover/active/focus more prominent
- Task cards: subtle `hover:shadow-md` + `hover:border-primary-200`
- Kanban columns: `hover:bg-gray-50` highlight on drag over

## 16. Content & Copy

- Active voice: "Create Task" not "A task will be created"
- Title Case for headings/buttons (Chicago style)
- Numerals for counts: "8 tasks" not "eight tasks"
- Specific button labels: "Create Task" not "Submit", "Save Changes" not "OK"
- Error messages include fix/next step, not just problem
- Second person; avoid first person
- `&` over "and" where space-constrained

## 17. Anti-Patterns Checklist

**Flag during code review:**

- [ ] `user-scalable=no` or `maximum-scale=1` (disabling zoom)
- [ ] `onPaste` with `preventDefault`
- [ ] `transition: all`
- [ ] `outline-none` without `focus-visible` replacement
- [ ] Inline `onClick` navigation without `<a>`
- [ ] `<div>` or `<span>` with click handlers (should be `<button>`)
- [ ] Images without dimensions
- [ ] Large arrays `.map()` without virtualization
- [ ] Form inputs without labels
- [ ] Icon buttons without `aria-label`
- [ ] Hardcoded date/number formats (use `Intl.*`)
- [ ] `autoFocus` without clear justification

---

## 18. Component Library

### Base Components (build first)

```
components/ui/
├── Button.tsx        → variants: primary, secondary, ghost, danger
├── Input.tsx         → with label, error, helper text
├── Select.tsx        → custom dropdown with keyboard nav
├── Checkbox.tsx      → with label, indeterminate state
├── Modal.tsx         → focus trap, overscroll-behavior, Esc to close
├── Badge.tsx         → for tags, priority, status
├── Avatar.tsx        → image + initials fallback
├── Tooltip.tsx       → accessible, keyboard-triggered
├── Dropdown.tsx      → menu with keyboard nav
├── Toast.tsx         → aria-live, auto-dismiss, action button
├── Spinner.tsx       → respects prefers-reduced-motion
├── EmptyState.tsx    → illustration + message + CTA
├── ConfirmDialog.tsx → for destructive actions
└── SkipLink.tsx      → skip to main content
```

### Naming Convention
- Components: PascalCase (`TaskCard.tsx`)
- Hooks: camelCase with `use` prefix (`useAuth.ts`)
- Utils: camelCase (`formatDate.ts`)
- Types: PascalCase, suffix with type (`TaskDto.ts`, `CreateTaskRequest.ts`)
- CSS: Tailwind utility classes (no CSS modules)

---

*All frontend code will be reviewed against these guidelines using the web-design-guidelines skill.*
