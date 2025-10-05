export interface MousePosition {
  x: number;
  y: number;
}

export interface HotCornerConfig {
  x: number;
  y: number;
  width: number;
  height: number;
  showDelay: number;
  hideDelay: number;
}

export const DEFAULT_HOT_CORNER: HotCornerConfig = {
  x: 0,
  y: 0,
  width: 3,
  height: 3,
  showDelay: 1000, // 1 segundo para começar a detectar
  hideDelay: 200   // 200ms para esconder
};

export interface MouseDetectorState {
  isDetecting: boolean;
  isOrbVisible: boolean;
  mouseCheckInterval: NodeJS.Timeout | null;
  hideTimeout: NodeJS.Timeout | null;
  lastMousePosition: MousePosition | null;
}
