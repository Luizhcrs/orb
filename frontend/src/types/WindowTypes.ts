import { BrowserWindow } from 'electron';

export interface WindowConfig {
  width: number;
  height: number;
  minWidth?: number;
  minHeight?: number;
  x?: number;
  y?: number;
  frame?: boolean;
  transparent?: boolean;
  alwaysOnTop?: boolean;
  skipTaskbar?: boolean;
  resizable?: boolean;
  movable?: boolean;
  minimizable?: boolean;
  maximizable?: boolean;
  closable?: boolean;
  focusable?: boolean;
  show?: boolean;
}

export interface OrbWindowConfig extends WindowConfig {
  width: 90;
  height: 90;
  x: 0;
  y: 0;
  frame: false;
  transparent: true;
  alwaysOnTop: true;
  skipTaskbar: true;
  resizable: false;
  movable: false;
  minimizable: false;
  maximizable: false;
  closable: false;
  focusable: true;
  show: false;
}

export interface ChatWindowConfig extends WindowConfig {
  width: 380;
  height: 512; // 480 + 32px da custom title bar
  frame: false;
  transparent: true;
  alwaysOnTop: true;
  skipTaskbar: true;
  resizable: false;
  movable: true;
  minimizable: false;
  maximizable: false;
  closable: false;
  focusable: true;
  show: false;
}

export interface ConfigWindowConfig extends WindowConfig {
  width: 800;
  height: 700;
  minWidth: 600;
  minHeight: 500;
  frame: false;
  transparent: true;
  alwaysOnTop: true;
  skipTaskbar: false;
  resizable: true;
  movable: true;
  minimizable: true;
  maximizable: true;
  closable: true;
  focusable: true;
  show: false;
}

export interface WindowManagerState {
  orbWindow: BrowserWindow | null;
  chatWindow: BrowserWindow | null;
  configWindow: BrowserWindow | null;
  isChatOpen: boolean;
  isOrbVisible: boolean;
  capturedImage: string | null;
  isChatExpanded: boolean; // 🔥 FIX: Rastrear estado de expansão
}
