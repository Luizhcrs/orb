import { BrowserWindow } from 'electron';

export interface WindowConfig {
  width: number;
  height: number;
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
  height: 480;
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

export interface WindowManagerState {
  orbWindow: BrowserWindow | null;
  chatWindow: BrowserWindow | null;
  isChatOpen: boolean;
  isOrbVisible: boolean;
  capturedImage: string | null;
}
