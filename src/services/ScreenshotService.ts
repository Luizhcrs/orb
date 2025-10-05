import { desktopCapturer, nativeImage, screen } from 'electron';
import { ScreenshotConfig, ScreenshotArea, ScreenshotResult, DEFAULT_SCREENSHOT_CONFIG } from '../types/ScreenshotTypes';

export class ScreenshotService {
  private config: ScreenshotConfig;

  constructor(config: ScreenshotConfig = DEFAULT_SCREENSHOT_CONFIG) {
    this.config = config;
  }

  /**
   * Captura a tela inteira
   */
  async captureFullScreen(): Promise<ScreenshotResult> {
    try {
      console.log('📸 Capturando tela inteira...');
      
      const primaryDisplay = screen.getPrimaryDisplay();
      const { width, height } = primaryDisplay.bounds;
      
      return await this.captureArea({
        x: 0,
        y: 0,
        width,
        height
      });
    } catch (error) {
      console.error('❌ Erro ao capturar tela inteira:', error);
      return {
        success: false,
        error: `Erro ao capturar tela: ${error}`,
        timestamp: Date.now()
      };
    }
  }

  /**
   * Captura uma área específica da tela
   */
  async captureArea(area: ScreenshotArea): Promise<ScreenshotResult> {
    try {
      console.log(`📸 Capturando área: ${area.x},${area.y} ${area.width}x${area.height}`);
      
      const sources = await desktopCapturer.getSources({
        types: ['screen'],
        thumbnailSize: { width: area.width, height: area.height }
      });
      
      if (sources.length === 0) {
        throw new Error('Nenhuma fonte de captura encontrada');
      }
      
      const thumbnail = sources[0].thumbnail;
      const imageData = thumbnail.toDataURL();
      
      console.log('✅ Screenshot capturado com sucesso');
      
      return {
        success: true,
        imageData,
        timestamp: Date.now()
      };
    } catch (error) {
      console.error('❌ Erro ao capturar área:', error);
      return {
        success: false,
        error: `Erro ao capturar área: ${error}`,
        timestamp: Date.now()
      };
    }
  }

  /**
   * Converte uma NativeImage para base64
   */
  imageToBase64(image: Electron.NativeImage): string {
    return image.toDataURL();
  }

  /**
   * Cria uma imagem vazia como fallback
   */
  createEmptyImage(): string {
    const emptyImage = nativeImage.createEmpty();
    return this.imageToBase64(emptyImage);
  }

  /**
   * Atualiza a configuração do serviço
   */
  updateConfig(newConfig: Partial<ScreenshotConfig>): void {
    this.config = { ...this.config, ...newConfig };
  }

  /**
   * Obtém a configuração atual
   */
  getConfig(): ScreenshotConfig {
    return { ...this.config };
  }
}
