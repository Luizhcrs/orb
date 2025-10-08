import { screen } from 'electron';
import { MousePosition, HotCornerConfig, MouseDetectorState, DEFAULT_HOT_CORNER } from '../types/MouseTypes';

export class MouseDetector {
  private config: HotCornerConfig;
  private state: MouseDetectorState;
  private onShowOrb: () => void;
  private onHideOrb: () => void;

  constructor(
    config: HotCornerConfig = DEFAULT_HOT_CORNER,
    onShowOrb: () => void,
    onHideOrb: () => void
  ) {
    this.config = config;
    this.onShowOrb = onShowOrb;
    this.onHideOrb = onHideOrb;
    this.state = {
      isDetecting: false,
      isOrbVisible: false,
      mouseCheckInterval: null,
      hideTimeout: null,
      lastMousePosition: null
    };
  }

  /**
   * Inicia a detecção de mouse para hot corner
   */
  startDetection(): void {
    if (this.state.isDetecting) {
      console.log('⚠️ Detecção de mouse já está ativa');
      return;
    }

    console.log('🖱️ Iniciando detecção de mouse...');
    this.state.isDetecting = true;

    // Aguardar um pouco antes de começar para evitar aparecer na inicialização
    setTimeout(() => {
      this.state.mouseCheckInterval = setInterval(() => {
        this.checkMousePosition();
      }, 100); // Verificar a cada 100ms
    }, this.config.showDelay);
  }

  /**
   * Para a detecção de mouse
   */
  stopDetection(): void {
    if (!this.state.isDetecting) {
      return;
    }

    console.log('🛑 Parando detecção de mouse...');
    this.state.isDetecting = false;

    if (this.state.mouseCheckInterval) {
      clearInterval(this.state.mouseCheckInterval);
      this.state.mouseCheckInterval = null;
    }

    if (this.state.hideTimeout) {
      clearTimeout(this.state.hideTimeout);
      this.state.hideTimeout = null;
    }
  }

  /**
   * ⚡ OTIMIZAÇÃO: Pausa a detecção temporariamente (sem resetar estado)
   */
  pauseDetection(): void {
    if (this.state.mouseCheckInterval) {
      console.log('⏸️ Pausando detecção de mouse (Orb visível)...');
      clearInterval(this.state.mouseCheckInterval);
      this.state.mouseCheckInterval = null;
    }
  }

  /**
   * ⚡ OTIMIZAÇÃO: Retoma a detecção após pausa
   */
  resumeDetection(): void {
    if (!this.state.mouseCheckInterval && this.state.isDetecting) {
      console.log('▶️ Retomando detecção de mouse...');
      this.state.mouseCheckInterval = setInterval(() => {
        this.checkMousePosition();
      }, 100);
    }
  }

  /**
   * Verifica a posição do mouse e gerencia visibilidade do orb
   */
  private checkMousePosition(): void {
    try {
      const cursor = screen.getCursorScreenPoint();
      const isInHotCorner = this.isInHotCorner(cursor);
      const isOverOrb = this.isMouseOverOrb(cursor);

      // Verificar se o mouse realmente se moveu significativamente
      const hasMouseMoved = this.hasMouseMovedSignificantly(cursor);
      this.state.lastMousePosition = cursor;

      // Debug: log da posição do mouse e estado (reduzido)
      if (this.state.isOrbVisible && hasMouseMoved) {
        console.log(`🖱️ Mouse: (${cursor.x}, ${cursor.y}) | HotCorner: ${isInHotCorner} | OverOrb: ${isOverOrb} | OrbVisible: ${this.state.isOrbVisible}`);
      }

      // Se o mouse está sobre o ORB, cancelar timeout de esconder
      if (isOverOrb && this.state.isOrbVisible) {
        if (this.state.hideTimeout) {
          console.log('🖱️ Mouse sobre o ORB, cancelando timer de esconder...');
          clearTimeout(this.state.hideTimeout);
          this.state.hideTimeout = null;
        }
        return; // Não fazer nada enquanto mouse estiver sobre o ORB
      }

      if (isInHotCorner && !this.state.isOrbVisible) {
        this.showOrb();
      } else if (!isInHotCorner && !isOverOrb && this.state.isOrbVisible && hasMouseMoved) {
        console.log('🖱️ Mouse saiu do hot corner e não está sobre o ORB, agendando esconder orb...');
        this.scheduleHideOrb();
      }
    } catch (error) {
      console.error('❌ Erro ao verificar posição do mouse:', error);
    }
  }

  /**
   * Verifica se o mouse se moveu significativamente (debounce)
   */
  private hasMouseMovedSignificantly(currentPosition: MousePosition): boolean {
    if (!this.state.lastMousePosition) {
      return true;
    }

    const deltaX = Math.abs(currentPosition.x - this.state.lastMousePosition.x);
    const deltaY = Math.abs(currentPosition.y - this.state.lastMousePosition.y);
    const threshold = 5; // 5 pixels de tolerância

    return deltaX > threshold || deltaY > threshold;
  }

  /**
   * Verifica se o cursor está na área do hot corner
   */
  private isInHotCorner(position: MousePosition): boolean {
    return (
      position.x >= this.config.x &&
      position.x <= this.config.x + this.config.width &&
      position.y >= this.config.y &&
      position.y <= this.config.y + this.config.height
    );
  }

  /**
   * Verifica se o mouse está sobre o ORB
   */
  private isMouseOverOrb(position: MousePosition): boolean {
    if (!this.state.isOrbVisible) {
      return false;
    }

    // Obter posição do ORB window (assumindo que está no canto superior esquerdo)
    // O ORB tem 90x90px, então vamos verificar uma área um pouco maior para margem de erro
    const orbArea = {
      x: 0,
      y: 0,
      width: 100,
      height: 100
    };

    return (
      position.x >= orbArea.x &&
      position.x <= orbArea.x + orbArea.width &&
      position.y >= orbArea.y &&
      position.y <= orbArea.y + orbArea.height
    );
  }

  /**
   * Mostra o orb imediatamente
   */
  showOrb(): void {
    if (this.state.isOrbVisible) {
      return;
    }

    // Cancelar timeout de esconder se existir
    if (this.state.hideTimeout) {
      clearTimeout(this.state.hideTimeout);
      this.state.hideTimeout = null;
    }

    this.state.isOrbVisible = true;
    this.onShowOrb();
    console.log('👁️ Orb mostrado via hot corner');
    
    // Agendar esconder automaticamente após 5 segundos
    this.scheduleHideOrb();
  }

  /**
   * Agenda o esconder do orb após delay
   */
  private scheduleHideOrb(): void {
    if (!this.state.isOrbVisible) {
      return;
    }

    // Se já existe um timeout ativo, não re-agendar
    if (this.state.hideTimeout) {
      console.log('⏰ Timeout já ativo, não re-agendando...');
      return;
    }

    console.log(`⏰ Agendando esconder orb em ${this.config.hideDelay}ms...`);
    this.state.hideTimeout = setTimeout(() => {
      console.log('⏰ Timeout executado, escondendo orb...');
      this.state.hideTimeout = null; // Limpar referência
      this.hideOrb();
    }, this.config.hideDelay);
  }

  /**
   * Esconde o orb imediatamente
   */
  hideOrb(): void {
    if (!this.state.isOrbVisible) {
      return;
    }

    if (this.state.hideTimeout) {
      clearTimeout(this.state.hideTimeout);
      this.state.hideTimeout = null;
    }

    this.state.isOrbVisible = false;
    this.onHideOrb();
    console.log('👁️‍🗨️ Orb escondido via hot corner');
  }

  /**
   * Força o orb a ficar visível (usado quando chat está aberto)
   */
  forceShowOrb(): void {
    this.state.isOrbVisible = true;
    this.onShowOrb();
    console.log('👁️ Orb forçado a aparecer');
  }

  /**
   * Força o orb a ficar escondido
   */
  forceHideOrb(): void {
    this.state.isOrbVisible = false;
    this.onHideOrb();
    console.log('👁️‍🗨️ Orb forçado a esconder');
  }

  /**
   * Atualiza a configuração do hot corner
   */
  updateConfig(newConfig: Partial<HotCornerConfig>): void {
    this.config = { ...this.config, ...newConfig };
    console.log('⚙️ Configuração do hot corner atualizada');
  }

  /**
   * Obtém o estado atual
   */
  getState(): MouseDetectorState {
    return { ...this.state };
  }

  /**
   * Obtém a configuração atual
   */
  getConfig(): HotCornerConfig {
    return { ...this.config };
  }

  /**
   * Limpa recursos (chamado no cleanup)
   */
  cleanup(): void {
    this.stopDetection();
  }
}
