export interface ScreenshotConfig {
  quality: number;
  format: 'png' | 'jpeg';
  compressionLevel: number;
}

export const DEFAULT_SCREENSHOT_CONFIG: ScreenshotConfig = {
  quality: 90,
  format: 'png',
  compressionLevel: 6
};

export interface ScreenshotArea {
  x: number;
  y: number;
  width: number;
  height: number;
}

export interface ScreenshotResult {
  success: boolean;
  imageData?: string;
  error?: string;
  timestamp: number;
}
